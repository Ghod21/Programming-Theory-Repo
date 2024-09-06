using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BossEnemy : Enemy
{
    // Boss enemy child script.
    private bool bossChargeCooldown;
    bool fireAreasSpellIsActive = false;
    //bool ChargeSpellIsActive = false;
    public ParticleSystem explosionBossSpell;
    SphereCollider explosionBossSpellSphereCollider;
    float distanceThreshold = 10;
    bool closeEnoughToExplode;
    //bool farEnoughToCharge;
    AudioSource audioSourceFire;
    private float radius;
    public float speedRatio;
    public float attackRangeRatio;
    float moveSpeedNew;


    [SerializeField] private float chargeSpeed = 40f; // Speed during the charge
    [SerializeField] private float chargeDuration = 0.5f; // Duration of the charge
    [SerializeField] private float chargePause = 1f; // Time to pause before charging

    [SerializeField] float bossHPRegenNumber;


    protected override void Start()
    {
        base.Start();
        speedRatio = 6 / 2.5f;
        attackRangeRatio = 2.5f / 3.2f;
        explosionBossSpell = FindParticleSystemInChildren("ExplosionBossSpell");
        explosionBossSpellSphereCollider = explosionBossSpell.GetComponent<SphereCollider>();
        audioSourceFire = spawnManager.aoeSpawnAreaObject.GetComponent<AudioSource>();
        radius = explosionBossSpellSphereCollider.radius;

        if (boundaryCollider == null)
        {
            Debug.LogError("Boundary Collider is not set.");
        }
        if (exclusionZone == null)
        {
            Debug.LogError("Exclusion Zone Collider is not set.");
        }
        if (player == null)
        {
            Debug.LogError("Player Transform is not set.");
        }
        if (rb == null)
        {
            Debug.LogError("Rigidbody is not set.");
        }
        if (playerScript == null)
        {
            Debug.LogError("Player Script is not set.");
        }

        if (DataPersistence.easyDifficulty)
        {
            enemyHealth = 100;
        }
        else
        {
            enemyHealth = 150;
        }
        enemyHealthMax = enemyHealth;
        StartCoroutine(BossSpellChangeRoutine());
        moveSpeed = moveSpeedNew;
    }

    protected override void Update()
    {
        base.Update();
        CheckDistanceToPlayer();
    }
    int RandomSpellIndex()
    {
        return UnityEngine.Random.Range(0, 4);
    }
    int RandomSpellCooldown()
    {
        int random = UnityEngine.Random.Range(4, 7);
        return random;
    }

    IEnumerator BossSpellChangeRoutine()
    {
        while (true)
        {
            int index;
            do
            {
                index = RandomSpellIndex();
            }
            while ((fireAreasSpellIsActive && index == 0) || (!closeEnoughToExplode && index == 1));

            Debug.Log($"Selected spell index: {index}");

            yield return new WaitForSeconds(RandomSpellCooldown());

            if (index == 0)
            {
                StartCoroutine(FireAreasSpawn());
            }
            else if (index == 1)
            {
                 StartCoroutine(ExplosionSpell());
            }
            else if (index == 2)
            {
                StartCoroutine(Charge());
            } 
            else if (index == 3 && CanSpawnSpell())
            {
                BossSpawnSpell();
            }

            Debug.Log($"Started spell at index: {index}");

        }
    }

    void BossSpawnSpell()
    {
        Quaternion bossRotation = Quaternion.identity;
        spawnManager.SpawnEnemiesBossSpell(bossRotation);
    }

    IEnumerator ExplosionSpell()
    {
        if (!isAttacking)
        {
            isUsingSpell = true;
            yield return new WaitForSeconds(0.5f);
            animator.ResetTrigger("JumpSpell");
            animator.SetTrigger("JumpSpell");
            yield return new WaitForSeconds(1f);
            explosionBossSpell.Play();
            Explosion();
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[17], DataPersistence.soundsVolume * 3.5f * DataPersistence.soundAdjustment);
            yield return new WaitForSeconds(0.5f);
            animator.SetTrigger("Idle");
            isUsingSpell = false;
        }

    }
    public void Explosion()
    {
        if (explosionBossSpellSphereCollider == null)
        {
            Debug.LogWarning("SphereCollider is not assigned.");
            return;
        }

        Vector3 center = explosionBossSpellSphereCollider.transform.position + explosionBossSpellSphereCollider.center;

        Collider[] colliders = Physics.OverlapSphere(center, radius);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                if (!playerScript.backwardDashIsActive)
                {
                    playerScript.playerHealth -= 5;
                }
                Debug.Log("Explosion damage is done");
                return;
            }
        }
    }
    private void CheckDistanceToPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < distanceThreshold)
        {
            closeEnoughToExplode = true;
        } else
        {
            closeEnoughToExplode = false;
        }
        if (distanceToPlayer > distanceThreshold)
        {
            //farEnoughToCharge = true;
        } else
        {
            //farEnoughToCharge = false;
        }
    }
    IEnumerator FireAreasSpawn()
    {
        fireAreasSpellIsActive = true;
        //audioSourceFire.mute = false;
        StartCoroutine(FireAreaTimer());
        while (fireAreasSpellIsActive)
        {
            spawnManager.SpawnAoEAtRandomPosition();
            yield return new WaitForSeconds(2);
        }
        yield return new WaitForSeconds(5);
        //audioSourceFire.mute = true;
    }
    IEnumerator FireAreaTimer()
    {
        yield return new WaitForSeconds(30);
        fireAreasSpellIsActive = false;
    }

    public override void MoveTowardsPlayer()
    {
        if (!attacked && enemyHealth > 0 && !bossChargeCooldown && !isUsingSpell)
        {
            // Move towards the player with the normal speed if not charging
            Vector3 moveDirection = (player.position - transform.position).normalized;
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    IEnumerator Charge()
    {
        // Stop moving and wait for pause before charge
        bossChargeCooldown = true;
        rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(chargePause); // Time to pause before charging

        // Perform the charge
        Vector3 chargeDirection = (player.position - transform.position).normalized;
        float chargeEndTime = Time.time + chargeDuration;
        if (enemyHealth < 0.001)
        {
            yield break;
        }
        playerScript.audioSource.PlayOneShot(playerScript.audioClips[19], DataPersistence.soundsVolume * 1f * DataPersistence.soundAdjustment);

        while (Time.time < chargeEndTime)
        {
            if (isAttacking) // Check if the boss is attacking
            {
                rb.velocity = Vector3.zero; // Stop movement if attacking
                yield return new WaitForSeconds(chargePause); // Wait before next charge
                bossChargeCooldown = true;
                break; // Exit the inner while loop to stop the current charge
            }
            if (enemyHealth < 0.001)
            {
                yield break;
            }
            rb.velocity = chargeDirection * chargeSpeed;
            yield return null; // Wait until the next frame
        }

        // End the charge
        rb.velocity = Vector3.zero;

        // Wait for the cooldown period
        bossChargeCooldown = false;
    }
    protected override IEnumerator deathAnimation()
    {
        isDying = true;
        // Code to prevent 0.5 ending result when adding bonus points for killing the boss.
        float newScore = DataPersistence.currentPlayerScore * 1.5f;
        string scoreString = DataPersistence.currentPlayerScore.ToString();
        char lastDigitChar = scoreString[scoreString.Length - 1];
        if (lastDigitChar == '5')
        {
            newScore += 2.5f;
        }
        DataPersistence.currentPlayerScore = newScore;

        mainManager.StartCoroutine(mainManager.Win());
        return base.deathAnimation();
    } 
    protected override void EnemyAttack()
    {
        // Method for enemy attacks
        animator.SetBool("isAttacking", true);
        if (playerScript.isDashing == false && !playerScript.isBlockingDamage)
        {
            if (!playerScript.backwardDashIsActive)
            {
                playerScript.audioSource.PlayOneShot(playerScript.audioClips[5], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
                playerScript.playerHealth -= 5;
                playerScript.scoreMultiplierBase -= 10;
            }
            Debug.Log("Health: " + playerScript.playerHealth);

        }
        else if (playerScript.isDashing == false && playerScript.isBlockingDamage)
        {
            playerScript.shieldHealth -= 4;
            if (shieldDamageTalentChosen)
            {
                enemyHealth--;
            }
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[6], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
            Debug.Log("ShieldHealth: " + playerScript.shieldHealth);
        }
    }

    private ParticleSystem FindParticleSystemInChildren(string name)
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == name)
            {
                ParticleSystem ps = child.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    return ps;
                }
            }
        }
        return null;
    }
    public IEnumerator BossHPRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds(15);
            if (enemyHealth < enemyHealthMax)
            {
                enemyHealth += bossHPRegenNumber;
            }
        }
    }

    public void UpdateEnemySpeed(float newPlayerSpeed)
    {
        moveSpeed = newPlayerSpeed * speedRatio;
    }
    public void UpdateEnemyAttackRange(float newPlayerAttackRange)
    {
        attackRange = newPlayerAttackRange * attackRangeRatio;
    }

    bool CanSpawnSpell()
    {
        int counter = 0;
        PrefabIdentifier[] allPrefabIdentifiers = FindObjectsOfType<PrefabIdentifier>();

        foreach (PrefabIdentifier prefabIdentifier in allPrefabIdentifiers)
        {
            if (prefabIdentifier.prefabName == "EnemyMedium")
            {
                counter++;
            }
        }
        if (counter < 1)
        {
            return true;
        } else
        {
            return false;
        }
    }
}

