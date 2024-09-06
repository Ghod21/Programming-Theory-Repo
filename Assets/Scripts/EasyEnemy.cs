using System.Collections;
using UnityEngine;
public class EasyEnemy : Enemy
{
    //Transform objectTransform;
    // Easy enemy child script.
    protected override void Start()
    {
        base.Start(); // Call the Start method from the base class
        // Additional initialization code for EasyEnemy
        //objectTransform = GetComponent<Transform>();
        //if (spawnManager.difficultyMeter >= 3)
        //{
        //    objectTransform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        if (DataPersistence.easyDifficulty)
        {
            enemyHealth = 2;
        } else
        {
            enemyHealth = 3;
        }
        //} else
        //{
        //    objectTransform.localScale = new Vector3(1f, 1f, 1f);
        //    enemyHealth = 2;
        //}

    }
    public override void MoveTowardsPlayer()
    {
        moveSpeed = 5f;
        base.MoveTowardsPlayer();
    }
    protected override IEnumerator deathAnimation()
    {
        isDying = true;
        //DataPersistence.currentPlayerScore += 5 * playerScript.scoreMultiplier;
        //playerScript.scoreMultiplierBase++;
        if (expManagerScript.HealthPotionsTalentIsChosenExpManager)
        {
            if (Random.value - healthBottleAdaptiveSpawnChance < 0.15f)
            {
                healthBottleAdaptiveSpawnChance = 0;
                Vector3 currentPosition = transform.position;
                spawnManager.CreateHealthPotionIfNotExists(currentPosition);
            }
        } else if (Random.value - healthBottleAdaptiveSpawnChance < 0.05f)
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
}
