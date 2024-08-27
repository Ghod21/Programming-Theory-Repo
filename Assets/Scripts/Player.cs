using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.EventSystems.EventTrigger;

public class Player : MonoBehaviour
{
    // Main variables
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform model;
    [SerializeField] private Animator animator; // Add a reference to the Animator component
    public AudioSource audioSource;
    [SerializeField] AudioSource runAudioSource;
    private bool isPlayingRunningSound = false;
    public AudioClip[] audioClips;
    float soundAdjustment = DataPersistence.soundAdjustment;
    public bool timeIsFrozen;

    // Move variables
    [SerializeField] GameObject dashFillArea;
    [SerializeField] public UnityEngine.UI.Image dashFillImage;
    [SerializeField] public float speed = 5;
    public float currentSpeed;
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
    public float attackRange = 3f; // Range of the attack
    float attackRangeMultiplier = 1f;
    [SerializeField] private float attackAngle = 180f; // Angle of the attack cone
    private bool isAttacking = false;
    private float attackCooldown = 0f; // Cooldown for attack
    private const float attackCooldownDuration = 0.833f; // Duration of the attack cooldown
    private int attackCount;
    private bool isAttackQueued = false; // Flag to queue attacksW

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
    public bool sprintDashTalentChosen = false;
    bool dashSprintIsOn = false;

    public bool vampireHealthTalentIsChosen = false;
    public int killsToVampire = 7;

    public bool damageAttackTalentIsChosen = false;
    public float attackRangeTalentAdd = 0f;
    [SerializeField] Transform swordTransform;
    public float swordSizeY = 1f;
    public float swordSizeMultiplier = 1f;
    public float swordSizePushAttackRangeTalentIsOn = 0f;
    public bool bleedAttackTalentIsChosen = false;
    bool hasPlayedKillSound = false;
    bool bleedSoundCooldown = false;

    // Skills variables
    public bool fireBreathTalentIsChosen = false;
    private ParticleSystem fireBreathParticle;
    public UnityEngine.UI.Image skillFillImage;
    public UnityEngine.UI.Image skillImage;
    public GameObject skillImageObject;

    public float fireBreathInitialConeRadius = 1f;
    float fireBreathMaxConeRadius = 10f;
    public float fireBreathConeAngle = 30f;
    public float fireBreathConeDurationIncrease = 1f;
    bool isUsingSpell;
    float spellCooldown = 10f;
    bool spellIsOnCooldown = false;

    [SerializeField] GameObject lightningParticles;
    [SerializeField] GameObject weaponCharge;
    ParticleSystem ParticleSystemParticles;

    public bool lightningTalentIsChosen = false;
    bool weaponIsCharged = false;

    public bool bladeVortexSkillTalentIsChosen = false;
    [SerializeField] Transform playerTransform;

    private Quaternion initialRotation;
    bool isInBladeVortex = false;
    float bladeVortexDuration = 0.5f;
    [SerializeField] ParticleSystem bladeVortexParticle;
    [SerializeField] GameObject bladeVortexParticleObject;
    float vortexSpeed;
    float vortexDuration;
    bool isNotTakingInput = false;
    public bool isTakingAoeDamage = false;





    //  ....................................................................MAIN PART START................................................................
    private void Start()
    {
        vortexSpeed = dashSpeed;
        vortexDuration = dashDuration;
        initialRotation = gameObject.transform.rotation;
        ParticleSystemParticles = weaponCharge.GetComponent<ParticleSystem>();
        Transform childFireBreath = transform.Find("FireBreath");
        fireBreathParticle = childFireBreath.GetComponentInChildren<ParticleSystem>();
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
            //RunSound();
            //ChargedWeaponSoundLogic();

            if (shieldWall.activeSelf)
            {
                shieldWall.transform.Rotate(Vector3.up, shieldWallRotationSpeed * Time.deltaTime);
            }


            attackCooldown -= Time.deltaTime; // Decrease the cooldown timer

            if (Input.GetMouseButton(0) && !isAttacking && attackCooldown <= 0f && !isShielding && !isUsingSpell || Input.GetMouseButton(0) && !isAttacking && attackCooldown <= 0f && shieldAttackTalentChosen && !isUsingSpell)
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


            if (Input.GetKeyDown(KeyCode.E))
            {
                UseSpell();
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
    // Additional sounds
    void RunSound()
    {
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isMoving)
        {
            if (!isPlayingRunningSound)
            {
                runAudioSource.clip = audioClips[16];
                runAudioSource.loop = true;
                runAudioSource.volume = DataPersistence.soundAdjustment * 0.5f;
                //runAudioSource.pitch = 1.75f;
                runAudioSource.Play();
                isPlayingRunningSound = true;
            }
        }
        else
        {
            if (isPlayingRunningSound)
            {
                runAudioSource.Stop();
                isPlayingRunningSound = false;
            }
        }
    }


    //  ....................................................................MAIN PART END..................................................................
    //  ....................................................................SKILLS PART START..............................................................
    void UseSpell()
    {
        if (!isAttacking && !isDashing && !isShielding && !spellIsOnCooldown && fireBreathTalentIsChosen && !isUsingSpell)
        {
            FireBreath();
            StartCoroutine(SpellCooldownTimer());
        }
        else if (!isAttacking && !isDashing && !isShielding && !spellIsOnCooldown && lightningTalentIsChosen && !isUsingSpell && !weaponIsCharged)
        {
            StartCoroutine(ChargeWeapon());

            skillFillImage.fillAmount = 0f;
        }
        else if (!isAttacking && !isDashing && !spellIsOnCooldown && bladeVortexSkillTalentIsChosen && !spellIsOnCooldown && !isUsingSpell)
        {
            BladeVortexDash();
            StartCoroutine(SpellCooldownTimer());
        }
    }
    // Separate spells
    IEnumerator SpellCooldownTimer()
    {
        spellIsOnCooldown = true;
        skillFillImage.fillAmount = 0f; // Start fill from 0
        float updateInterval = 0.1f; // Interval between updates
        int updateCount = Mathf.CeilToInt(spellCooldown / updateInterval); // Number of updates needed
        float fillStep = 1f / updateCount; // Amount to increase fillAmount per update

        while (skillFillImage.fillAmount < 1f)
        {
            skillFillImage.fillAmount += fillStep; // Increase fillAmount value
            yield return new WaitForSeconds(updateInterval); // Wait before the next update
        }

        skillFillImage.fillAmount = 1f; // Ensure the value is 1 at the end

        spellIsOnCooldown = false;
        isUsingSpell = false;
    }
    // Blade Vortex
    private void BladeVortexDash()
    {
        if (backwardsDashTalentChosen)
        {
            dashSpeed = vortexSpeed;
            dashDuration = vortexDuration;
        }
        isUsingSpell = true;
        isDashing = true;
        isNotTakingInput = true;
        //audioSource.PlayOneShot(audioClips[2], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
        dashTime = dashDuration;

        StartCoroutine(RotatePlayerCoroutine());
        StartCoroutine(BladeVortexAttackCoroutine());
    }

    private IEnumerator RotatePlayerCoroutine()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        bladeVortexParticle.Play();
        dashTime = bladeVortexDuration;
        isInBladeVortex = true;
        float elapsedTime = 0f;
        float totalRotation = 360f * 3; // Total rotation needed (3 full rotations)
        float rotationDuration = bladeVortexDuration; // Duration of the rotation
        float dashVortexSpeed = 25f;

        // Calculate the rotation step based on the duration and speed
        float rotationStep = totalRotation / rotationDuration;

        // Store the initial rotation
        Quaternion startRotation = playerTransform.rotation;

        // ¬ычисл€ем направление рывка в сторону взгл€да игрока
        Vector3 dashDirection = playerTransform.forward;

        while (elapsedTime < rotationDuration)
        {
            // ѕеремещаем игрока в направлении взгл€да
            rb.MovePosition(rb.position + dashDirection * dashVortexSpeed * Time.deltaTime);

            // Calculate how much to rotate this frame
            float step = rotationStep * Time.deltaTime;

            // Rotate the player
            playerTransform.Rotate(0, step, 0);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Ensure we end up exactly at the final rotation
        playerTransform.rotation = startRotation * Quaternion.Euler(0, totalRotation, 0);

        // Optional delay before resetting to the initial rotation
        yield return new WaitForSeconds(0.1f);

        // Reset to the initial rotation
        playerTransform.rotation = startRotation;

        // End dashing
        isInBladeVortex = false;
        isUsingSpell = false;
        isNotTakingInput = false;
        isDashing = false;
        if (backwardsDashTalentChosen)
        {
            dashSpeed = 15f;
            dashDuration = 0.3f;
        }
    }



    private IEnumerator BladeVortexAttackCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < bladeVortexDuration)
        {
            // Perform attack action
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
            bool hitEnemy = false; // Track if any enemy was hit
            bool killed = false;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Enemy") || hitCollider.CompareTag("EnemyRange"))
                {
                    Debug.Log("Hit: " + hitCollider.name + " " + attackCount);
                    attackCount++;
                    Enemy enemy = hitCollider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        if (!damageAttackTalentIsChosen && !enemy.damagedByVortex && !enemy.isUnderDefenceAura)
                        {
                            enemy.enemyHealth--;
                            enemy.StartCoroutine(enemy.BladeVortexDamageCooldown());
                        }
                        else if (damageAttackTalentIsChosen && !enemy.damagedByVortex && !enemy.isUnderDefenceAura)
                        {
                            enemy.enemyHealth -= 1.5f;
                            enemy.StartCoroutine(enemy.BladeVortexDamageCooldown());
                        }
                        enemy.attacked = true;


                        if (enemy.enemyHealth < 1)
                        {
                            killed = true;
                        }
                        else
                        {
                            hitEnemy = true;
                        }
                        if (bleedAttackTalentIsChosen && !enemy.enemyIsBleeding)
                        {
                            StartCoroutine(BleedTalentEffect(enemy));
                            enemy.AddMaterial();
                        }
                    }

                }

            }
            if (killed)
            {
                if (!hasPlayedKillSound)
                {
                    audioSource.PlayOneShot(audioClips[3], DataPersistence.soundsVolume * 0.7f * soundAdjustment);
                    StartCoroutine(KillSoundCooldown());
                }
            }
            else if (hitEnemy)
            {
                audioSource.PlayOneShot(audioClips[0], DataPersistence.soundsVolume * 0.7f * soundAdjustment);
            }
            else
            {
                audioSource.PlayOneShot(audioClips[1], DataPersistence.soundsVolume * 0.7f * soundAdjustment);
            }

            yield return new WaitForSeconds(0.2f); // Delay between attacks
            elapsedTime += 0.2f;
        }

    }


    // Lightning

    IEnumerator ChargeWeapon()
    {
        isUsingSpell = true;
        float x = speed;
        speed *= 0.75f;
        StartCoroutine(WeaponChargeAnimation());
        yield return new WaitForSeconds(1f);
        speed = x;
        isUsingSpell = false;
    }
    IEnumerator WeaponChargeAnimation()
    {
        animator.SetBool("isChargingWeapon", true);
        yield return new WaitForSeconds(0.4f);
        StartCoroutine(SpellCooldownTimer());
        weaponCharge.SetActive(true);
        ParticleSystemParticles.Play();
        weaponIsCharged = true;
        audioSource.PlayOneShot(audioClips[12], DataPersistence.soundsVolume * 3f * soundAdjustment);
        yield return new WaitForSeconds(0.4f);
        animator.SetBool("isChargingWeapon", false);
    }
    IEnumerator ChainLightningCoroutine(Enemy initialTarget)
    {
        yield return new WaitForSeconds(0.1f);

        if (initialTarget != null && lightningTalentIsChosen && weaponIsCharged)
        {
            // Instantiate the lightning particles prefab
            GameObject lightningInstance = Instantiate(lightningParticles);

            // Get the ChainLightning component from the instantiated prefab
            ChainLightning chainLightning = lightningInstance.GetComponent<ChainLightning>();

            if (chainLightning != null)
            {
                // Set the lightningParticleObject and ParticleSystem for ChainLightning
                chainLightning.SetLightningParticleObject(lightningInstance);

                // Optionally, attach the instance to the target if needed
                lightningInstance.transform.parent = initialTarget.transform;

                // Start the chain lightning effect
                chainLightning.StartChainLightning(initialTarget, this);
            }
            else
            {
                Debug.LogError("ChainLightning component is missing from the instantiated prefab.");
            }

            // Reset weapon charge status
            weaponIsCharged = false;

            // Stop and clear original particle system
            ParticleSystemParticles.Stop();
            ParticleSystemParticles.Clear();
            weaponCharge.SetActive(false);
        }
        else
        {
            Debug.Log("Chain Lightning not activated. InitialTarget: " + (initialTarget != null) +
                ", LightningTalentIsChosen: " + lightningTalentIsChosen +
                ", SpellIsOnCooldown: " + spellIsOnCooldown +
                ", WeaponIsCharged: " + weaponIsCharged);
        }
    }
    void ChainLightningOnAttack(Enemy initialTarget)
    {
        StartCoroutine(ChainLightningCoroutine(initialTarget));
    }


    // Fire
    void FireBreath()
    {
        audioSource.PlayOneShot(audioClips[11], DataPersistence.soundsVolume * 1f * soundAdjustment);
        isUsingSpell = true;
        fireBreathParticle.Play();
        StartCoroutine(ActivateFireBreath());
        StartCoroutine(FireBreathCoolDown());
    }
    IEnumerator FireBreathCoolDown()
    {
        float x = speed;
        speed *= 0.75f;
        StartCoroutine(FireBreathAnimation());
        yield return new WaitForSeconds(1f);
        speed = x;
        yield return new WaitForSeconds(1f);
        fireBreathParticle.Stop();
        fireBreathParticle.Clear();
    }
    IEnumerator FireBreathAnimation()
    {
        animator.SetBool("isFireBreathing", true);
        yield return new WaitForSeconds(0.75f);
        isUsingSpell = false;
        animator.SetBool("isFireBreathing", false);
    }
    IEnumerator FireBreathHitCooldown(Enemy x)
    {
        yield return new WaitForSeconds(2f);
        x.enemyIsHitByFire = false;
    }
    private IEnumerator ActivateFireBreath()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fireBreathConeDurationIncrease)
        {
            float currentRadius = Mathf.Lerp(fireBreathInitialConeRadius, fireBreathMaxConeRadius, elapsedTime / fireBreathConeDurationIncrease);
            DetectEnemiesInCone(currentRadius);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        DetectEnemiesInCone(fireBreathMaxConeRadius);
    }
    private void DetectEnemiesInCone(float currentRadius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentRadius);
        bool hitEnemy = false;

        foreach (var hitCollider in hitColliders)
        {
            if (IsTargetInCone(hitCollider.transform))
            {
                if (hitCollider.CompareTag("Enemy"))
                {
                    hitEnemy = true;
                    Enemy enemy = hitCollider.GetComponent<Enemy>();

                    if (enemy != null && !enemy.enemyIsHitByFire)
                    {
                        enemy.enemyIsHitByFire = true;
                        StartCoroutine(FireBreathHitCooldown(enemy));
                        if (!enemy.isUnderDefenceAura)
                        {
                            enemy.enemyHealth--;
                        }
                        enemy.animator.SetTrigger("Attacked");
                    }
                }
            }
        }

        if (hitEnemy)
        {

        }
    }

    //  ....................................................................SKILLS PART END................................................................
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
    // Health talents start

    public IEnumerator HealthRegenTalent()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);
            playerHealth++;
            if (playerHealth > 30)
            {
                playerHealth = 30;
            }
        }
    }

    // Health talents end

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
        runAudioSource.Stop();
        isPlayingRunningSound = false;
        Time.timeScale = 0f;

    }



    //  ....................................................................DEATH PART END.................................................................
    //  ....................................................................SHIELD PART START..............................................................

    private void ShieldLogic()
    {
        float x = shieldAttackTalentChosen ? 5f : 10f;

        shieldSlider.value = Mathf.MoveTowards(shieldSlider.value, shieldHealth / x, Time.deltaTime * 100f);

        shieldWallPiecesLogic();

        if (Input.GetMouseButtonDown(1) && !shieldIsOnCooldown && !isShieldCooldownActive && !isUsingSpell)
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
    void DashSprintCheck()
    {
        if (dashSprintIsOn)
        {
            speed = 3.25f;
        }
        else
        {
            speed = 2.5f;
        }
    }
    public IEnumerator SprintDashTalent()
    {
        yield return new WaitForSeconds(0.2f);
        dashSprintIsOn = true;
        yield return new WaitForSeconds(3f);
        dashSprintIsOn = false;
    }
    private void BackwardsDashLogic()
    {
        audioSource.PlayOneShot(audioClips[2], DataPersistence.soundsVolume * 0.8f * 1.5f * soundAdjustment);
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
                if (sprintDashTalentChosen)
                {
                    StartCoroutine(SprintDashTalent());
                }
                dashIsOnCooldown = true;
            }
        }
    }

    private void DashUILogic()
    {
        UpdateDashUI();
        DashSprintCheck();
        if (backwardsDashTalentChosen && Input.GetKeyDown(KeyCode.Space) && !isDashing && !dashIsOnCooldown && !isShielding && !isUsingSpell)
        {
            BackwardsDashLogic();
        }
        else if (!backwardsDashTalentChosen && !isUsingSpell)
        {
            MainAndDoubleDashLogic();
        }
        isDoubleDashTalentChosenActivated = doubleDashTalentChosen;
        if (remainingDashes == 2 && doubleDashTalentChosen)
        {
            dashFillImage.fillAmount = 1;
        }
        else if (!doubleDashTalentChosen && !dashIsOnCooldown)
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
        }
        else
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


    private void Move()
    {
        currentSpeed = speed;

        if (isTakingAoeDamage && isShielding)
        {
            currentSpeed = speed * 0.3f;
        }
        else if (isTakingAoeDamage)
        {
            currentSpeed = speed * 0.7f;
        }
        else if (isShielding)
        {
            currentSpeed = speed / 2;
        }

        if (isDashing && !isNotTakingInput)
        {
            Vector3 movement = CalculateMovement(input) * dashSpeed;
            rb.velocity = movement;

            dashTime -= Time.deltaTime;
            if (dashTime <= 0)
            {
                isDashing = false;
            }
        }
        else if (!isNotTakingInput)
        {
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

        if (playerPlane.Raycast(ray, out float hitDist) && !isInBladeVortex)
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
        audioSource.PlayOneShot(audioClips[2], DataPersistence.soundsVolume * 0.8f * 1.5f * soundAdjustment);
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
    private void GatherInput()
    {
        if (!isDashing && !isNotTakingInput)
        {
            input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }
    }

    //  ..............................................................MOVE PART END..........................................................................

    //  ..............................................................ATTACK PART START......................................................................

    public void AttackRangeCalculation()
    {
        attackRange = (3 * attackRangeMultiplier) + attackRangeTalentAdd;
    }
    public void SwordSizeForAttackRange()
    {
        Vector3 newScale = swordTransform.localScale;
        newScale.y = (swordSizeY * swordSizeMultiplier) + swordSizePushAttackRangeTalentIsOn;
        swordTransform.localScale = newScale;
    }
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


    IEnumerator BleedTalentEffect(Enemy x)
    {
        x.enemyIsBleeding = true;
        yield return new WaitForSeconds(3);
        if (x != null)
        {
            if (!x.isUnderDefenceAura)
            {
                x.enemyHealth--;
            }

            if (x != null)
            {
                x.animator.SetTrigger("Attacked");
                x.enemyIsBleeding = false;
                BleedSound(x);
                x.RemoveMaterial();
            }
        }

    }
    void BleedSound(Enemy x)
    {
        if (x.enemyHealth <= 0 && !hasPlayedKillSound)
        {
            audioSource.PlayOneShot(audioClips[3], DataPersistence.soundsVolume * 0.7f * soundAdjustment);
            StartCoroutine(KillSoundCooldown());
        }
        else if (x.enemyHealth > 0 && !bleedSoundCooldown)
        {
            audioSource.PlayOneShot(audioClips[0], DataPersistence.soundsVolume * 0.7f * soundAdjustment);
            StartCoroutine(BleedSoundCooldown());
        }
    }
    public IEnumerator KillSoundCooldown()
    {
        hasPlayedKillSound = true;
        yield return new WaitForSeconds(0.8f);
        hasPlayedKillSound = false;
    }
    public IEnumerator BleedSoundCooldown()
    {
        bleedSoundCooldown = true;
        yield return new WaitForSeconds(0.1f);
        bleedSoundCooldown = false;
    }

    IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(0.833f / 2); // Half of the attack animation duration

        // Perform attack action at halfway through the animation
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        bool hitEnemy = false; // Track if any enemy was hit
        bool killed = false;

        Enemy initialTarget = null; // Variable to store the first enemy hit

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
                        if (!damageAttackTalentIsChosen && !enemy.isUnderDefenceAura)
                        {
                            enemy.enemyHealth--;
                        }
                        else if (damageAttackTalentIsChosen && !enemy.isUnderDefenceAura)
                        {
                            enemy.enemyHealth -= 1.5f;
                        }
                        enemy.attacked = true;

                        // Store the first enemy as the initial target for the lightning
                        if (initialTarget == null)
                        {
                            initialTarget = enemy;
                        }

                        if (enemy.enemyHealth < 1)
                        {
                            killed = true;
                        }
                        else
                        {
                            hitEnemy = true;
                        }
                        if (bleedAttackTalentIsChosen && !enemy.enemyIsBleeding)
                        {
                            StartCoroutine(BleedTalentEffect(enemy));
                            enemy.AddMaterial();
                        }
                    }
                }
            }
        }

        // Call the lightning effect on the first hit enemy, if one was hit
        if (weaponIsCharged && initialTarget != null)
        {
            ChainLightningOnAttack(initialTarget);

            //StartCoroutine(SpellCooldownTimer());
        }

        if (isShielding && shieldAttackTalentChosen)
        {
            attackToShieldCount++;
        }

        // Play the appropriate sound once after checking all colliders
        if (killed)
        {
            if (!hasPlayedKillSound)
            {
                audioSource.PlayOneShot(audioClips[3], DataPersistence.soundsVolume * 0.7f * soundAdjustment);
                StartCoroutine(KillSoundCooldown());
            }
        }
        else if (hitEnemy)
        {
            audioSource.PlayOneShot(audioClips[0], DataPersistence.soundsVolume * 0.7f * soundAdjustment);
        }
        else
        {
            audioSource.PlayOneShot(audioClips[1], DataPersistence.soundsVolume * 0.7f * soundAdjustment);
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

        isUsingSpell = false;
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
