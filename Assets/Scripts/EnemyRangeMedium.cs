using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangeMedium : Enemy
{
    private GameObject projectilePrefab;
    private float projectileSpeed = 5f;
    private float attackInterval = 4f;

    private bool isInAttackRange = false;
    private bool isEscaping = false;
    //private bool attackIsOnCooldown = false;

    // New variable to define the distance at which the enemy starts to escape
    [SerializeField] private float escapeDistance = 1.0f;

    protected override void Start()
    {
        projectilePrefab = Resources.Load<GameObject>("Prefabs/ProjectileRangeEnemy");
        base.Start();
        StartCoroutine(AttackRoutine());
    }

    protected override IEnumerator deathAnimation()
    {
        isDying = true;
        //DataPersistence.currentPlayerScore += 10 * playerScript.scoreMultiplier;
        //playerScript.scoreMultiplierBase += 2;
        if (expManagerScript.HealthPotionsTalentIsChosenExpManager)
        {
            if (Random.value - healthBottleAdaptiveSpawnChance < 0.3f)
            {
                healthBottleAdaptiveSpawnChance = 0;
                Vector3 currentPosition = transform.position;
                spawnManager.CreateHealthPotionIfNotExists(currentPosition);
            }
        }
        else if (Random.value - healthBottleAdaptiveSpawnChance < 0.15f)
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

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        while (enemyHealth > 0)
        {
            if (!isAttacking && isInAttackRange && !mainManager.win)
            {
                isAttacking = true;
                //attackIsOnCooldown = true;
                StartCoroutine(EnemyAttackToAnimation());
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                StartCoroutine(EnemySecondAttackToAnimation());
            }

             yield return new WaitForSeconds(attackInterval);
                //attackIsOnCooldown = false;
                isAttacking = false;
        }
    }

    protected override void CheckAttackRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Determine if the enemy should escape or attack
        //if (distanceToPlayer <= escapeDistance && attackIsOnCooldown)
        if (distanceToPlayer <= escapeDistance)
            {
            isEscaping = true;
            isInAttackRange = false;
        }
        else if (distanceToPlayer <= attackRange && !isEscaping)
        {
            isInAttackRange = true;
            isEscaping = false;
            rb.velocity = Vector3.zero; // Stop moving when in attack range
        }
        else
        {
            isInAttackRange = false;
            isEscaping = false;
        }
    }

    protected override void EnemyAttack()
    {
        playerScript.audioSource.PlayOneShot(playerScript.audioClips[9], DataPersistence.soundsVolume * 0.6f * soundAdjustment);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Offset to spawn the projectile slightly in front of the enemy
        Vector3 spawnPosition = transform.position + directionToPlayer * 1.0f;

        // Add offset to the y position to make the projectile appear slightly higher
        spawnPosition.y += 0.5f; // Adjust this value to control the height offset

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        projectile.GetComponent<Projectile>().Initialize(directionToPlayer, projectileSpeed, playerScript, soundAdjustment);
    }
    private IEnumerator EnemySecondAttackToAnimation()
    {
        animator.SetBool("isAttacking", true);

        EnemyAttack();

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        StartCoroutine(AttackCooldown());
    }

    private IEnumerator EnemyAttackToAnimation()
    {
        animator.SetBool("isAttacking", true);
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(animationLength * 0.3f);

        EnemyAttack();

        yield return new WaitForSeconds(animationLength * 0.7f);

        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        animator.SetBool("isAttacking", false);
        animator.SetTrigger("Idle");
        isAttacking = false;
        yield return null;
    }

    public override void MoveTowardsPlayer()
    {
        if (!isAttacking)
        {
            Vector3 moveDirection;

            if (!isEscaping && !isInAttackRange)
            {
                moveDirection = (player.position - transform.position).normalized;
            }
            else if (isEscaping)
            {
                moveDirection = (transform.position - player.position).normalized;
            }
            else
            {
                rb.velocity = Vector3.zero;
                return;
            }

            rb.velocity = moveDirection * moveSpeed;
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }
}
