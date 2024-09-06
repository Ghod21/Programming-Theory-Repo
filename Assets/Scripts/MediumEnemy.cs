using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MediumEnemy : Enemy
{

    private bool chargeCooldownBool;

    private readonly float chargeSpeed = 12f; // Speed during the charge
    private readonly float chargeDuration = 0.8f; // Duration of the charge
    private readonly float chargeMaxCooldown = 8f; // Time between charges
    private readonly float chargePause = 1f; // Time to pause before charging
    bool isAbleToWalkBeforeFirstCharge = true;

    protected override void Start()
    {
        base.Start(); // Call the Start method from the base class

        // Additional initialization code for MediumEnemy
        if (DataPersistence.easyDifficulty)
        {
            enemyHealth = 4;
        }
        else
        {
            enemyHealth = 5;
        }
        StartCoroutine(StartChargeDelay());
    }

    IEnumerator StartChargeDelay()
    {
        yield return new WaitForSeconds(Random.Range(1f,3f));
        StartCoroutine(Charge());
        isAbleToWalkBeforeFirstCharge = false;
    }

    public override void MoveTowardsPlayer()
    {
        moveSpeed = 4f;
        if (!attacked && enemyHealth > 0 && chargeCooldownBool || !attacked && enemyHealth > 0 && isAbleToWalkBeforeFirstCharge)
        {
            // Move towards the player with the normal speed if not charging
            Vector3 moveDirection = (player.position - transform.position).normalized;
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    protected override IEnumerator deathAnimation()
    {
        isDying = true;
        //DataPersistence.currentPlayerScore += 10 * playerScript.scoreMultiplier;
        //playerScript.scoreMultiplierBase += 2;
        if (expManagerScript.HealthPotionsTalentIsChosenExpManager)
        {
            if (Random.value - healthBottleAdaptiveSpawnChance < 0.15f)
            {
                healthBottleAdaptiveSpawnChance = 0;
                Vector3 currentPosition = transform.position;
                spawnManager.CreateHealthPotionIfNotExists(currentPosition);
            }
        }
        else if (Random.value - healthBottleAdaptiveSpawnChance < 0.1f)
        {
            healthBottleAdaptiveSpawnChance = 0;
            Vector3 currentPosition = transform.position;
            spawnManager.CreateHealthPotionIfNotExists(currentPosition);
        }
        else
        {
            healthBottleAdaptiveSpawnChance += 0.01f;
        }
        vampireTalentRegen();
        return base.deathAnimation();
    }
        IEnumerator Charge()
    {
        while (true)
        {
            // Stop moving and wait for pause before charge
            chargeCooldownBool = false;
            rb.velocity = Vector3.zero;
            yield return new WaitForSeconds(chargePause); // Time to pause before charging

            // Perform the charge
            Vector3 chargeDirection = (player.position - transform.position).normalized;
            float chargeEndTime = Time.time + chargeDuration;
            if (enemyHealth < 0.01)
            {
                chargeCooldownBool = true;
                yield break;
            }
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[19], DataPersistence.soundsVolume * 1f * DataPersistence.soundAdjustment);

       
            while (Time.time < chargeEndTime)
            {
                if (isAttacking) // Check if the boss is attacking
                {
                    rb.velocity = Vector3.zero; // Stop movement if attacking
                    yield return new WaitForSeconds(chargePause); // Wait before next charge
                    chargeCooldownBool = true;
                    break; // Exit the inner while loop to stop the current charge
                }
                if (enemyHealth < 0.001)
                {
                    chargeCooldownBool = true;
                    yield break;
                }
                rb.velocity = chargeDirection * chargeSpeed;
                yield return null; // Wait until the next frame
            }

            // End the charge
            rb.velocity = Vector3.zero;

            // Wait for the cooldown period
            chargeCooldownBool = true;
            yield return new WaitForSeconds(UnityEngine.Random.Range(2, chargeMaxCooldown) - chargeDuration); // Adjust wait time
        }
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
                playerScript.playerHealth -= 2;
                playerScript.scoreMultiplierBase -= 10;
            }
            Debug.Log("Health: " + playerScript.playerHealth);

        }
        else if (playerScript.isDashing == false && playerScript.isBlockingDamage)
        {
            playerScript.shieldHealth -= 2;
            if (shieldDamageTalentChosen)
            {
                enemyHealth--;
            }
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[6], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
            Debug.Log("ShieldHealth: " + playerScript.shieldHealth);
        }
    }
}
