using System;
using System.Collections;
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


    // Move variables
    [SerializeField] GameObject dashFillArea;
    [SerializeField] UnityEngine.UI.Image dashFillImage;
    [SerializeField] private float speed = 5;
    [SerializeField] private float verticalSpeedMultiplier = 1f; // Adjust this multiplier for vertical speed
    [SerializeField] private float dashSpeed = 10; // Speed during dash
    [SerializeField] private float dashDuration = 0.2f; // Duration of the dash
    private bool dashIsOnCooldown = false;
    private float dashTime; // Track the dash time
    private float dashCooldownSeconds = 10f;
    private bool isCooldownCoroutineRunning = false;

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
    private bool isShielding;
    private bool shieldIsOnCooldown = false;
    private int attackToShieldCount = 0;
    public bool isBlockingDamage = false;
    public int shieldHealth = 10;
    public float shieldWallRotationSpeed = 30f;

    // Death and Health variables
    [SerializeField] UnityEngine.UI.Slider healthSlider;
    [SerializeField] GameObject fillArea;
    public float playerHealth = 30f;

    //  ....................................................................MAIN PART START................................................................
    private void Start()
    {
        animator.SetBool("isNotAttacking", true);
        audioSource = GetComponent<AudioSource>();
        fillArea.SetActive(true);
        shieldFillArea.SetActive(true);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            GatherInput();
            LookAtMouse();
            HandleAnimations(); // Call the method to handle animations
            isNotAttackingCheck(); // Check if not attacking
            ShieldLogic();
            HealthLogic();
            DashUILogic();

            if (Input.GetKeyDown(KeyCode.Space) && !isDashing && !dashIsOnCooldown && !isShielding)
            {
                Dash();
                dashIsOnCooldown = true;
                StartCoroutine(dashCooldown());
            }
            if (shieldWall.activeSelf)
            {
                shieldWall.transform.Rotate(Vector3.up, shieldWallRotationSpeed * Time.deltaTime);
            }


            attackCooldown -= Time.deltaTime; // Decrease the cooldown timer

            if (Input.GetMouseButton(0) && !isAttacking && attackCooldown <= 0f)
            {
                Attack();
            }
            else if (Input.GetMouseButton(0) && isAttacking)
            {
                isAttackQueued = true; // Queue the attack if one is already in progress
            }
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
    //  ....................................................................DEATH PART START...............................................................
    private void HealthLogic()
    {
        healthSlider.value = Mathf.MoveTowards(healthSlider.value, playerHealth / 30f, Time.deltaTime * 10f);
        if (playerHealth <= 0)
        {
            fillArea.SetActive(false);
            Time.timeScale = 0f;
        }
    }


    //  ....................................................................DEATH PART END.................................................................
    //  ....................................................................SHIELD PART START..............................................................

    private void ShieldLogic()
    {
        shieldSlider.value = Mathf.MoveTowards(shieldSlider.value, shieldHealth / 10f, Time.deltaTime * 10f);
        shieldWallPiecesLogic();
        if (Input.GetMouseButtonDown(1) && !shieldIsOnCooldown)
        {
            ShieldStart();
            audioSource.PlayOneShot(audioClips[4], DataPersistence.soundsVolume * 0.8f);
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
        if(isShielding)
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
        return false;
    }
    private void ShieldStart()
    {
        if (isShielding) return;
        isShielding = true;
        animator.SetBool("isShielding", true);
    }
    private void ShieldStop()
    {
        if (!isShielding) return;
        isShielding = false;
        shieldIsOnCooldown = true;
        animator.SetBool("isShielding", false);
        StartCoroutine(ShieldCooldown());
    }
    IEnumerator ShieldCooldown()
    {
        shieldFillArea.SetActive(false);
        shieldHealth = 0;
        yield return new WaitForSeconds(1f);
        shieldFillArea.SetActive(true);
        while (shieldHealth < 10)
        {
            shieldHealth++;
            if (shieldHealth == 10)
            {
                shieldIsOnCooldown = false;
            }
            yield return new WaitForSeconds(1f);
        }
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
        }
    }

    //  ....................................................................SHIELD PART END................................................................
    //  ....................................................................MOVE PART START................................................................
    private void DashUILogic()
    {
        if (!dashIsOnCooldown)
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
    }

    IEnumerator DashCooldownTimer()
    {
        isCooldownCoroutineRunning = true;
        dashFillImage.fillAmount = 0f; // Start fill from 0

        while (dashFillImage.fillAmount < 1f)
        {
            dashFillImage.fillAmount += 0.01f; // Increase fillAmount value
            yield return new WaitForSeconds(0.1f); // Wait before the next update
        }

        dashFillImage.fillAmount = 1f; // Ensure the value is 1 at the end
        isCooldownCoroutineRunning = false;
    }
private void GatherInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
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
        audioSource.PlayOneShot(audioClips[2], DataPersistence.soundsVolume * 0.8f * 2);
        dashTime = dashDuration;
    }

    private void OnCollisionStay(Collision collision)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    IEnumerator dashCooldown()
    {
        yield return new WaitForSeconds(dashCooldownSeconds);
        dashIsOnCooldown = false;
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
                if (hitCollider.CompareTag("Enemy"))
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
        if (isShielding)
        {
            attackToShieldCount++;
        }

        // Play the appropriate sound once after checking all colliders
        if (killed)
        {
            audioSource.PlayOneShot(audioClips[3], DataPersistence.soundsVolume * 0.8f);
        }
        else if (hitEnemy)
        {
            audioSource.PlayOneShot(audioClips[0], DataPersistence.soundsVolume * 0.8f);
        }
        else
        {
            audioSource.PlayOneShot(audioClips[1], DataPersistence.soundsVolume * 0.8f);
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
