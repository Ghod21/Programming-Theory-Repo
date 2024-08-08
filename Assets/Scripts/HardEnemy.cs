using System.Collections;
using UnityEngine;

public class HardEnemy : Enemy
{
    private SpawnManager spawnManager;
    // Hard enemy child script.
    protected override void Start()
    {
        base.Start(); // Call the Start method from the base class
        // Additional initialization code for EasyEnemy
        enemyHealth = 7;
        spawnManager = FindObjectOfType<SpawnManager>();
    }
    public override void MoveTowardsPlayer()
    {
        moveSpeed = 3f;
        base.MoveTowardsPlayer();
    }
    protected override IEnumerator deathAnimation()
    {
        DataPersistence.currentPlayerScore += 20 * playerScript.scoreMultiplier;
        playerScript.scoreMultiplierBase += 3;
        if (Random.value < 0.1f)
        {
            Vector3 currentPosition = transform.position;
            spawnManager.CreateHealthPotionIfNotExists(currentPosition);
        }
        return base.deathAnimation();
    }
    protected override void EnemyAttack()
    {
        // Method for enemy attacks
        animator.SetBool("isAttacking", true);
        if (playerScript.isDashing == false && !playerScript.isBlockingDamage)
        {
            playerScript.playerHealth -= 3;
            playerScript.scoreMultiplierBase -= 10;
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[5], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
            Debug.Log("Health: " + playerScript.playerHealth);

        }
        else if (playerScript.isDashing == false && playerScript.isBlockingDamage)
        {
            playerScript.shieldHealth -= 3;
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[6], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
            Debug.Log("ShieldHealth: " + playerScript.shieldHealth);
        }
    }
}
