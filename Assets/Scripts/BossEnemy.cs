using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    // Boss enemy child script.
    private bool bossChargeMethod;
    private bool bossChargeCooldown;

    [SerializeField] private float chargeSpeed = 20f; // Speed during the charge
    [SerializeField] private float chargeDuration = 1f; // Duration of the charge
    [SerializeField] private float chargeCooldown = 10f; // Time between charges
    [SerializeField] private float chargePause = 2f; // Time to pause before charging

    protected override void Start()
    {
        base.Start();

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
        bossChargeMethod = true;
        StartCoroutine(Charge());
    }

    public override void MoveTowardsPlayer()
    {
        if (!attacked && enemyHealth > 0 && bossChargeCooldown)
        {
            // Move towards the player with the normal speed if not charging
            Vector3 moveDirection = (player.position - transform.position).normalized;
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    IEnumerator Charge()
    {
        while (bossChargeMethod)
        {
            // Stop moving and wait for pause before charge
            bossChargeCooldown = false;
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
                    bossChargeCooldown = true;
                    break; // Exit the inner while loop to stop the current charge
                }

                rb.velocity = chargeDirection * chargeSpeed;
                yield return null; // Wait until the next frame
            }

            // End the charge
            rb.velocity = Vector3.zero;

            // Wait for the cooldown period
            bossChargeCooldown = true;
            yield return new WaitForSeconds(chargeCooldown - chargeDuration); // Adjust wait time
        }
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
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[6], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
            Debug.Log("ShieldHealth: " + playerScript.shieldHealth);
        }
    }
}

