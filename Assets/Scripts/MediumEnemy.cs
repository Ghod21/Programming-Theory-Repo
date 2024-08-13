using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediumEnemy : Enemy
{

    protected override void Start()
    {
        base.Start(); // Call the Start method from the base class

        // Additional initialization code for MediumEnemy
        enemyHealth = 5;
    }

    public override void MoveTowardsPlayer()
    {
        moveSpeed = 4f;
        base.MoveTowardsPlayer();
    }

    protected override IEnumerator deathAnimation()
    {
        DataPersistence.currentPlayerScore += 10 * playerScript.scoreMultiplier;
        playerScript.scoreMultiplierBase += 2;
        return base.deathAnimation();
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
