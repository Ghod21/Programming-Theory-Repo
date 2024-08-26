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
    AudioSource audioSourceFire;
    private float radius;


    [SerializeField] private float chargeSpeed = 20f; // Speed during the charge
    [SerializeField] private float chargeDuration = 1f; // Duration of the charge
    [SerializeField] private float chargeCooldown = 10f; // Time between charges
    [SerializeField] private float chargePause = 2f; // Time to pause before charging


    protected override void Start()
    {
        base.Start();
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

        enemyHealth = 25;
        StartCoroutine(BossSpellChangeRoutine());

    }
    int RandomSpellIndex()
    {
        return UnityEngine.Random.Range(0, 4);
    }
    int RandomSpellCooldown()
    {
        int random = UnityEngine.Random.Range(2, 5);
        return random;
    }

    IEnumerator BossSpellChangeRoutine()
    {
        while (true)
        {
            int index;
            if (fireAreasSpellIsActive)
            {
                index = UnityEngine.Random.Range(1, 4);
            }
            else
            {
                index = RandomSpellIndex();
            }

            Debug.Log($"Selected spell index: {index}");

            yield return new WaitForSeconds(RandomSpellCooldown());

            if (index == 0)
            {
                StartCoroutine(FireAreasSpawn());
            }
            else if (index == 1)
            {
                yield return StartCoroutine(ExplosionSpell());
            }
            else if (index == 2)
            {
                yield return StartCoroutine(Charge());
            } else if (index == 3)
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
            yield return new WaitForSeconds(1f);
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
                playerScript.playerHealth -= 3;
                Debug.Log("Explosion damage is done");
                return;
            }
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

            rb.velocity = chargeDirection * chargeSpeed;
            yield return null; // Wait until the next frame
        }

        // End the charge
        rb.velocity = Vector3.zero;

        // Wait for the cooldown period
        bossChargeCooldown = false;
        yield return new WaitForSeconds(chargeCooldown - chargeDuration); // Adjust wait time
    }
    protected override IEnumerator deathAnimation()
    {
        // Code to prevent 0.5 ending result when adding bonus points for killing the boss.
        float newScore = DataPersistence.currentPlayerScore * 1.5f;
        string scoreString = DataPersistence.currentPlayerScore.ToString();
        char lastDigitChar = scoreString[scoreString.Length - 1];
        if (lastDigitChar == '5')
        {
            newScore += 2.5f;
        }
        DataPersistence.currentPlayerScore = newScore;

        return base.deathAnimation();
    }
    protected override void EnemyAttack()
    {
        // Method for enemy attacks
        animator.SetBool("isAttacking", true);
        if (playerScript.isDashing == false && !playerScript.isBlockingDamage)
        {
            playerScript.playerHealth -= 5;
            playerScript.scoreMultiplierBase -= 10;
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[5], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
            Debug.Log("Health: " + playerScript.playerHealth);

        }
        else if (playerScript.isDashing == false && playerScript.isBlockingDamage)
        {
            playerScript.shieldHealth -= 5;
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
}

