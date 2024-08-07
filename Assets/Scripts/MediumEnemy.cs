using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediumEnemy : Enemy
{
    private SpawnManager spawnManager; // Variable to store the reference to SpawnManager

    protected override void Start()
    {
        base.Start(); // Call the Start method from the base class

        // Additional initialization code for MediumEnemy
        enemyHealth = 5;

        // Find the SpawnManager in the scene and store the reference
        spawnManager = FindObjectOfType<SpawnManager>();
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
        if (Random.value < 0.01f)
        {
            Vector3 currentPosition = transform.position;
            spawnManager.CreateHealthPotionIfNotExists(currentPosition);
        }
        return base.deathAnimation();
    }
}
