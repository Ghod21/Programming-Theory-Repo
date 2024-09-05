using System.Collections;
using UnityEngine;

public class HardEnemy : Enemy
{
    // Hard enemy child script.
    private GameObject defenceAuraObject;
    private DefenceAura defenceAuraScript;
    protected override void Start()
    {
        base.Start(); // Call the Start method from the base class
        // Additional initialization code for EasyEnemy
        enemyHealth = 7;
        isHardEnemy = true;
        DefenceAuraSearch();
        defenceAuraScript = defenceAuraObject.GetComponent<DefenceAura>();
    }
    protected override void Update()
    {
        base.Update();
        if (enemyHealth < 0.01f)
        {
            defenceAuraScript.auraIsOn = false;
            Destroy(defenceAuraObject, 0.5f);
        }
    }
    public override void MoveTowardsPlayer()
    {
        moveSpeed = 3f;
        base.MoveTowardsPlayer();
    }
    protected override IEnumerator deathAnimation()
    {
        isDying = true;
        //DataPersistence.currentPlayerScore += 20 * playerScript.scoreMultiplier;
        //playerScript.scoreMultiplierBase += 3;
        if (expManagerScript.HealthPotionsTalentIsChosenExpManager)
        {
            if (Random.value - healthBottleAdaptiveSpawnChance < 0.5f)
            {
                healthBottleAdaptiveSpawnChance = 0;
                Vector3 currentPosition = transform.position;
                spawnManager.CreateHealthPotionIfNotExists(currentPosition);
            }
        }
        else if (Random.value - healthBottleAdaptiveSpawnChance < 0.2f)
        {
            healthBottleAdaptiveSpawnChance = 0;
            Vector3 currentPosition = transform.position;
            spawnManager.CreateHealthPotionIfNotExists(currentPosition);
        } else
        {
            healthBottleAdaptiveSpawnChance += 0.01f;
        }

        defenceAuraScript.DisableDefenceAuraForAllEnemies();
        vampireTalentRegen();
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
                playerScript.playerHealth -= 3;
                playerScript.scoreMultiplierBase -= 10;
            }
            Debug.Log("Health: " + playerScript.playerHealth);

        }
        else if (playerScript.isDashing == false && playerScript.isBlockingDamage)
        {
            playerScript.shieldHealth -= 3;
            if (shieldDamageTalentChosen)
            {
                enemyHealth--;
            }
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[6], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
            Debug.Log("ShieldHealth: " + playerScript.shieldHealth);
        }
    }
    void DefenceAuraSearch()
    {
        string targetName = "DefenceAura";

        foreach (Transform child in transform)
        {
            if (child.name == targetName)
            {
                defenceAuraObject = child.gameObject;
                Collider collider = defenceAuraObject.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                }
                else
                {
                    Debug.LogError("Collider not found on child object with name 'DefenceAura'");
                }
                break;
            }
        }
    }

}
