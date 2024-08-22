using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediumEnemy : Enemy
{

    private bool chargeCooldownBool;

    private readonly float chargeSpeed = 12f; // Speed during the charge
    private readonly float chargeDuration = 0.7f; // Duration of the charge
    private readonly float chargeCooldown = 15f; // Time between charges
    private readonly float chargePause = 1f; // Time to pause before charging

    protected override void Start()
    {
        base.Start(); // Call the Start method from the base class

        // Additional initialization code for MediumEnemy
        enemyHealth = 5;
        StartCoroutine(Charge());
    }

    public override void MoveTowardsPlayer()
    {
        moveSpeed = 4f;
        if (!attacked && enemyHealth > 0 && chargeCooldownBool)
        {
            // Move towards the player with the normal speed if not charging
            Vector3 moveDirection = (player.position - transform.position).normalized;
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    protected override IEnumerator deathAnimation()
    {
        DataPersistence.currentPlayerScore += 10 * playerScript.scoreMultiplier;
        playerScript.scoreMultiplierBase += 2;
        if (Random.value < 0.02f && expManagerScript.HealthPotionsTalentIsChosenExpManager)
        {
            Vector3 currentPosition = transform.position;
            spawnManager.CreateHealthPotionIfNotExists(currentPosition);
        }
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

            while (Time.time < chargeEndTime)
            {
                if (isAttacking) // Check if the boss is attacking
                {
                    rb.velocity = Vector3.zero; // Stop movement if attacking
                    yield return new WaitForSeconds(chargePause); // Wait before next charge
                    chargeCooldownBool = true;
                    break; // Exit the inner while loop to stop the current charge
                }

                rb.velocity = chargeDirection * chargeSpeed;
                yield return null; // Wait until the next frame
            }

            // End the charge
            rb.velocity = Vector3.zero;

            // Wait for the cooldown period
            chargeCooldownBool = true;
            yield return new WaitForSeconds(chargeCooldown - chargeDuration); // Adjust wait time
        }
    }

    protected override void EnemyAttack()
    {
        // Method for enemy attacks
        animator.SetBool("isAttacking", true);
        if (playerScript.isDashing == false && !playerScript.isBlockingDamage)
        {
            playerScript.playerHealth -= 2;
            playerScript.scoreMultiplierBase -= 10;
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[5], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
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
