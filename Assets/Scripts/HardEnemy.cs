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
        DataPersistence.currentPlayerScore += 15 * playerScript.scoreMultiplier;
        playerScript.scoreMultiplierBase += 3;
        if (Random.value < 0.05f)
        {
            Vector3 currentPosition = transform.position;
            spawnManager.CreateHealthPotionIfNotExists(currentPosition);
        }
        return base.deathAnimation();
    }
}
