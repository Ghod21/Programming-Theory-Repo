using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // Main variables
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform model;
    [SerializeField] private Animator animator; // Add a reference to the Animator component
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    float soundAdjustment = 0.6f;
    public bool timeIsFrozen;

    // Move variables
    [SerializeField] GameObject dashFillArea;
    [SerializeField] public UnityEngine.UI.Image dashFillImage;
    [SerializeField] private float speed = 5;
    [SerializeField] private float verticalSpeedMultiplier = 1f; // Adjust this multiplier for vertical speed
    [SerializeField] public float dashSpeed = 10; // Speed during dash
    [SerializeField] public float dashDuration = 0.2f; // Duration of the dash
    public bool dashIsOnCooldown = false;
    private float dashTime; // Track the dash time
    public float dashCooldownSeconds = 10f;
    public bool isCooldownCoroutineRunning = false;

    private Vector3 input;
    public bool isDashing = false; // Track if the player is dashing

    // Attack variables
    [SerializeField] private string[] targetTags; // Tags of the targets
    [SerializeField] private float attackRange = 3f; // Range of the attack
    [SerializeField] private float attackAngle = 180f; // Angle of the attack cone
    private bool isAttacking = false;
    private float attackCooldown = 0f; // Cooldown for attack
    private const float attackCooldownDuration = 0.833f; // Duration of the attack cooldown
    private int attackCount;
    private bool isAttackQueued = false; // Flag to queue attacks

    // Shield variables
    [SerializeField] GameObject shieldWall;
    [SerializeField] GameObject shieldWallPieceOne;
    [SerializeField] GameObject shieldWallPieceTwo;
    [SerializeField] GameObject shieldWallPieceThree;
    [SerializeField] GameObject shieldFillArea;
    [SerializeField] UnityEngine.UI.Slider shieldSlider;
    public bool isShielding;
    public bool shieldIsOnCooldown = false;
    private bool isShieldCooldownActive = false;
    private int attackToShieldCount = 0;
    public bool isBlockingDamage = false;
    public float shieldHealth = 10;
    public float shieldWallRotationSpeed = 30f;

    // Death and Health variables
    [SerializeField] UnityEngine.UI.Slider healthSlider;
    [SerializeField] GameObject fillArea;
    [SerializeField] Collider boxCollider; // Assign this in the Inspector or via script
    public float playerHealth = 30f;
    public bool gameOver;

    // Score variables
    public float scoreMultiplier = 1;
    public float scoreMultiplierBase; // For adjusting scoreMultiplier based on killed enemies

    // Experience variables
    public float playerExperience = 0;

    // Talents variables
    // Shield
    public bool shieldAttackTalentChosen = false;
    bool shieldAttackTalentChosenActivated = false;
    // Dash
    public bool doubleDashTalentChosen = false;
    bool isDoubleDashTalentChosenActivated = false;
    bool isDoubleDashTalentChosenCanBeActivatedAgain = true;
    public bool backwardsDashTalentChosen = false;
    public int remainingDashes = 0; // New variable to track the remaining dashes
    public TextMeshProUGUI dashCountText; // TextMeshProUGUI to display dash count

    private List<Vector3> positionBackwardDashList = new List<Vector3>();
    private List<float> healthBackwardDashList = new List<float>();
    float updateInterval = 0.5f;
    int maxValues = 7;
    public Vector3 currentVectorForBackwardDash;
    public float currentFloatForBackwardDash;



    //  ....................................................................MAIN PART START................................................................
    private void Start()
    {
        animator.SetBool("isNotAttacking", true);
        audioSource = GetComponent<AudioSource>();
        fillArea.SetActive(true);
        shieldFillArea.SetActive(true);
        Time.timeScale = 1f;
        gameOver = false;
        playerExperience = 0;
        if (doubleDashTalentChosen)
        {
            remainingDashes = 2;
        }
        else
        {
            remainingDashes = 1;
        }
        UpdateDashUI();
        StartCoroutine(TrackBackwardsDashState());
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainScene" && !gameOver && !timeIsFrozen)
        {
            GatherInput();
            LookAtMouse();
            HandleAnimations(); // Call the method to handle animations
            isNotAttackingCheck(); // Check if not attacking
            ShieldLogic();
            HealthLogic();
            DashUILogic();
            ScoreUpdate();

            if (shieldWall.activeSelf)
            {
                shieldWall.transform.Rotate(Vector3.up, shieldWallRotationSpeed * Time.deltaTime);
            }


            attackCooldown -= Time.deltaTime; // Decrease the cooldown timer

            if (Input.GetMouseButton(0) && !isAttacking && attackCooldown <= 0f && !isShielding || Input.GetMouseButton(0) && !isAttacking && attackCooldown <= 0f && shieldAttackTalentChosen)
            {
                Attack();
            }
            else if (Input.GetMouseButton(0) && isAttacking)
            {
                isAttackQueued = true; // Queue the attack if one is already in progress
            }
            if (!shieldAttackTalentChosenActivated && shieldAttackTalentChosen)
            {
                shieldHealth = 5;
                shieldAttackTalentChosenActivated = true;
            }
        }
        if (playerHealth <= 0f)
        {
            GameOver();
        }
    }

    private void FixedUpdate()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            Move();
        }
    }
    //  ....................................................................MAIN PART END..................................................................
    //  ....................................................................SCORE PART START...............................................................

    private void ScoreUpdate()
    {
        if (scoreMultiplierBase < 10)
        {
            scoreMultiplier = 1;
        }
        else if (scoreMultiplierBase >= 5 || scoreMultiplierBase < 10)
        {
            scoreMultiplier = 2;
        }
        else if (scoreMultiplierBase >= 10 || scoreMultiplierBase < 20)
        {
            scoreMultiplier = 3;
        }
        else if (scoreMultiplierBase >= 20 || scoreMultiplierBase < 50)
        {
            scoreMultiplier = 4;
        }
        else if (scoreMultiplierBase >= 50)
        {
            scoreMultiplier = 5;
        }
    }

    //  ....................................................................SCORE PART END.................................................................
    //  ....................................................................DEATH PART START...............................................................
    private void HealthLogic()
    {
        healthSlider.value = Mathf.MoveTowards(healthSlider.value, playerHealth / 30f, Time.deltaTime * 10f);
        if (playerHealth <= 0)
        {
            fillArea.SetActive(false);
        }
    }
    private void GameOver()
    {
        gameOver = true;
        animator.SetBool("isGameOver", true);
        Time.timeScale = 0f;
    }



    //  ....................................................................DEATH PART END.................................................................
    //  ....................................................................SHIELD PART START..............................................................

    private void ShieldLogic()
    {
        float x = shieldAttackTalentChosen ? 5f : 10f;

        shieldSlider.value = Mathf.MoveTowards(shieldSlider.value, shieldHealth / x, Time.deltaTime * 100f);

        shieldWallPiecesLogic();

        if (Input.GetMouseButtonDown(1) && !shieldIsOnCooldown && !isShieldCooldownActive)
        {
            ShieldStart();
            audioSource.PlayOneShot(audioClips[4], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
        }
        else if (Input.GetMouseButtonUp(1) && isShielding)
        {
            ShieldStop();
        }

        if (attackToShieldCount == 3)
        {
            ShieldStop();
            attackToShieldCount = 0;
        }

        if (isShielding)
        {
            isBlockingDamage = IsEnemyInShieldCone();
            shieldWall.SetActive(true);
        }
        else
        {
            isBlockingDamage = false;
            shieldWall.SetActive(false);
        }

        if (shieldHealth <= 0)
        {
            ShieldStop();
        }
    }
    private bool IsEnemyInShieldCone()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (IsTargetInCone(hitCollider.transform))
            {
                if (hitCollider.CompareTag("Enemy"))
                {
                    return true;
                }
            }
        }
        Collider[] projectileColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in projectileColliders)
        {
            if (IsTargetInCone(hitCollider.transform))
            {
                if (hitCollider.CompareTag("Projectile"))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void ShieldStart()
    {
        if (isShielding) return;
        isShielding = true;
        attackToShieldCount = 0;
        animator.SetBool("isShielding", true);
    }

    private void ShieldStop()
    {
        if (!isShielding) return;
        if (!shieldIsOnCooldown && !isShieldCooldownActive)
        {
            isShielding = false;
            animator.SetBool("isShielding", false);
            shieldIsOnCooldown = true;
            isShieldCooldownActive = true; // Mark the cooldown as active
            StartCoroutine(ShieldCooldown());
        }
    }
    IEnumerator ShieldCooldown()
    {
        float x = shieldAttackTalentChosen ? 5f : 10f;
        float healthIncrement = shieldAttackTalentChosen ? 0.5f : 1f;

        if (shieldHealth == x)
        {
            yield return new WaitForSeconds(0.5f);
            shieldIsOnCooldown = false;
            isShieldCooldownActive = false;
            yield break;
        }

        yield return new WaitForSeconds(1f);

        shieldFillArea.SetActive(true);

        while (shieldHealth < x && shieldIsOnCooldown)
        {
            shieldHealth += healthIncrement;
            if (shieldHealth >= x)
            {
                shieldHealth = x;
                shieldIsOnCooldown = false;
            }
            yield return new WaitForSeconds(1f);
        }

        // Once cooldown is complete, mark it as inactive
        isShieldCooldownActive = false;
    }
    private void shieldWallPiecesLogic()
    {
        if (attackToShieldCount == 0)
        {
            shieldWallPieceOne.SetActive(true);
            shieldWallPieceTwo.SetActive(true);
            shieldWallPieceThree.SetActive(true);
        }
        if (attackToShieldCount == 1)
        {
            shieldWallPieceOne.SetActive(false);
        }
        if (attackToShieldCount == 2)
        {
            shieldWallPieceTwo.SetActive(false);
        }
        if (attackToShieldCount == 3)
        {
            shieldWallPieceThree.SetActive(false);
            shieldHealth = 0;
        }
    }

    //  ....................................................................SHIELD PART END................................................................
    //  ....................................................................MOVE PART START................................................................

    private void BackwardsDashLogic()
    {
        audioSource.PlayOneShot(audioClips[2], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
        isDashing = true;

        Vector3 targetPosition = GetPositionFrom3SecondsAgo();

        StartCoroutine(BackwardDashCoroutine(targetPosition));
    }
    private IEnumerator BackwardDashCoroutine(Vector3 targetPosition)
    {
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            float step = (elapsedTime / dashDuration);
            transform.position = Vector3.Lerp(transform.position, targetPosition, step);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        dashIsOnCooldown = true;
        playerHealth = GetHealthFrom3SecondsAgo();
        yield return new WaitForSeconds(2f);
        isDashing = false;
    }
    public IEnumerator TrackBackwardsDashState()
    {
        while (true)
        {
            currentVectorForBackwardDash = transform.position;
            currentFloatForBackwardDash = playerHealth;
            if (positionBackwardDashList.Count >= maxValues)
            {
                positionBackwardDashList.RemoveAt(0);
            }
            if (healthBackwardDashList.Count >= maxValues)
            {
                healthBackwardDashList.RemoveAt(0);
            }
            positionBackwardDashList.Add(currentVectorForBackwardDash);
            healthBackwardDashList.Add(currentFloatForBackwardDash);
            yield return new WaitForSeconds(updateInterval);
        }
    }

    float GetHealthFrom3SecondsAgo()
    {
        if (healthBackwardDashList.Count == 0) return playerHealth;
        int index = Mathf.Max(0, healthBackwardDashList.Count - maxValues);
        return healthBackwardDashList[index];
    }

    Vector3 GetPositionFrom3SecondsAgo()
    {
        if (positionBackwardDashList.Count == 0) return transform.position;
        int index = Mathf.Max(0, positionBackwardDashList.Count - maxValues);
        return positionBackwardDashList[index];
    }
    void MainAndDoubleDashLogic()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && !isShielding)
        {
            if (doubleDashTalentChosen && remainingDashes > 0)
            {
                Dash();
                remainingDashes--;
                UpdateDashUI();
            }
            else if (!doubleDashTalentChosen && !dashIsOnCooldown)
            {
                Dash();
                dashIsOnCooldown = true;
            }
        }
    }

    private void DashUILogic()
    {
        UpdateDashUI();
        if (backwardsDashTalentChosen && Input.GetKeyDown(KeyCode.Space) && !isDashing && !dashIsOnCooldown && !isShielding)
        {
            BackwardsDashLogic();
        }
        else if (!backwardsDashTalentChosen)
        {
            MainAndDoubleDashLogic();
        }
        isDoubleDashTalentChosenActivated = doubleDashTalentChosen;
        if (remainingDashes == 2 && doubleDashTalentChosen)
        {
            dashFillImage.fillAmount = 1;
        } else if(!doubleDashTalentChosen && !dashIsOnCooldown)
        {
            dashFillImage.fillAmount = 1;
        }
        else
        {
            if (!isCooldownCoroutineRunning)
            {
                StartCoroutine(DashCooldownTimer());
            }
        }
        if (remainingDashes > 2)
        {
            remainingDashes = 2;
        }
    }

    IEnumerator DashCooldownTimer()
    {
        isCooldownCoroutineRunning = true;
        dashFillImage.fillAmount = 0f; // Start fill from 0
        float x = 0.1f;
        if (!doubleDashTalentChosen)
        {
            while (dashFillImage.fillAmount < 1f)
            {
                if (isDoubleDashTalentChosenActivated && isDoubleDashTalentChosenCanBeActivatedAgain)
                {
                    isCooldownCoroutineRunning = false;
                    dashFillImage.fillAmount = 1f;
                    remainingDashes = 2;
                    isDoubleDashTalentChosenCanBeActivatedAgain = false;
                    yield break;
                }
                dashFillImage.fillAmount += x * 0.1f; // Increase fillAmount value
                yield return new WaitForSeconds(x); // Wait before the next update
            }
        } else
        {
            while (dashFillImage.fillAmount < 1f)
            {
                if (isDoubleDashTalentChosenActivated && isDoubleDashTalentChosenCanBeActivatedAgain)
                {
                    isCooldownCoroutineRunning = false;
                    dashFillImage.fillAmount = 1f;
                    remainingDashes = 2;
                    isDoubleDashTalentChosenCanBeActivatedAgain = false;
                    yield break;
                }
                dashFillImage.fillAmount += x * 0.2f; // Increase fillAmount value
                yield return new WaitForSeconds(x); // Wait before the next update
            }
            remainingDashes++;
        }
        dashFillImage.fillAmount = 1f; // Ensure the value is 1 at the end
        isCooldownCoroutineRunning = false;
        dashIsOnCooldown = false;
    }
    private void GatherInput()
    {
        if (!isDashing)
        {
            input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }
    }

    private void Move()
    {
        float currentSpeed = isShielding ? speed / 2 : speed;
        if (isDashing)
        {
            // Calculate movement direction in isometric space
            Vector3 movement = CalculateMovement(input) * dashSpeed; // Adjust speed for dash
            rb.velocity = movement;

            dashTime -= Time.deltaTime;
            if (dashTime <= 0)
            {
                isDashing = false;
            }
        }
        else
        {
            // Calculate movement direction in isometric space
            Vector3 movement = CalculateMovement(input);
            rb.velocity = movement * currentSpeed;
        }
    }

    private Vector3 CalculateMovement(Vector3 inputDirection)
    {
        // Convert input direction to isometric view
        Vector3 isoDirection = inputDirection.ToIso();

        // Adjust speed based on the isometric view
        float adjustedSpeed = (inputDirection.z != 0) ? speed * verticalSpeedMultiplier : speed; // Increase vertical speed

        return isoDirection * adjustedSpeed;
    }


    private void LookAtMouse()
    {
        // Get the mouse position in world space
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (playerPlane.Raycast(ray, out float hitDist))
        {
            Vector3 targetPoint = ray.GetPoint(hitDist);
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
            model.rotation = targetRotation; // Instantly face the mouse direction
        }
    }

    private void HandleAnimations()
    {
        bool isMoving = input != Vector3.zero;
        animator.SetBool("isMoving", isMoving); // Set the "isMoving" parameter in the Animator
    }

    private void Dash()
    {
        isDashing = true;
        audioSource.PlayOneShot(audioClips[2], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
        dashTime = dashDuration;
    }

    private void OnCollisionStay(Collision collision)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    private void UpdateDashUI()
    {
        if (dashCountText != null)
        {
            if (doubleDashTalentChosen)
            {
                dashCountText.text = remainingDashes.ToString(); // Update with remaining dashes
            }
            else
            {
                dashCountText.text = ""; // Hide dash count if not applicable
            }
        }
    }


    //  ..............................................................MOVE PART END..........................................................................

    //  ..............................................................ATTACK PART START......................................................................
    private void Attack()
    {
        if (isAttacking && Input.GetMouseButtonDown(0))
        {
            isAttackQueued = true; // Queue the attack if already attacking
            return;
        }
        isAttacking = true;
        attackCooldown = attackCooldownDuration; // Reset the cooldown timer
        if (isShielding)
        {
            animator.SetBool("isShielding", false);
        }
        animator.SetBool("isAttacking", true);

        // Start attack animation coroutine
        StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(0.833f / 2); // Half of the attack animation duration

        // Perform attack action at halfway through the animation
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        bool hitEnemy = false; // Track if any enemy was hit
        bool killed = false;

        foreach (var hitCollider in hitColliders)
        {
            if (IsTargetInCone(hitCollider.transform))
            {
                if (hitCollider.CompareTag("Enemy") || hitCollider.CompareTag("EnemyRange"))
                {
                    // Apply damage to the target
                    Debug.Log("Hit: " + hitCollider.name + " " + attackCount);
                    attackCount++;
                    Enemy enemy = hitCollider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.enemyHealth--;
                        enemy.attacked = true;
                        if (enemy.enemyHealth < 1)
                        {
                            killed = true;
                        }
                        else
                        {
                            hitEnemy = true;
                        }
                    }
                }
            }
        }
        if (isShielding && shieldAttackTalentChosen)
        {
            attackToShieldCount++;
        }

        // Play the appropriate sound once after checking all colliders
        if (killed)
        {
            audioSource.PlayOneShot(audioClips[3], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
        }
        else if (hitEnemy)
        {
            audioSource.PlayOneShot(audioClips[0], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
        }
        else
        {
            audioSource.PlayOneShot(audioClips[1], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
        }

        isAttackQueued = false;
        // Wait for the remaining half of the animation duration
        yield return new WaitForSeconds(0.833f / 2);


        // Reset attack state
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        if (isShielding)
        {
            animator.SetBool("isShielding", true);
        }


        // If an attack was queued, start the next one
        if (isAttackQueued)
        {
            isAttackQueued = false;
            Attack();
        }
    }



    private bool IsTargetInCone(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(model.forward, directionToTarget);
        return angleToTarget < attackAngle / 2;
    }

    void isNotAttackingCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetBool("isNotAttacking", false);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            animator.SetBool("isNotAttacking", true);
        }
    }
    //  ....................................................................ATTACK PART END................................................................

}
