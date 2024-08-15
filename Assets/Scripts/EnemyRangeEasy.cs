using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangeEasy : Enemy
{
    private GameObject projectilePrefab;
    private float projectileSpeed = 5f;
    private float attackInterval = 3f;

    private bool isInAttackRange = false;
    private bool isEscaping = false;
    bool attackIsOnCooldown = false;

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
        DataPersistence.currentPlayerScore += 10 * playerScript.scoreMultiplier;
        playerScript.scoreMultiplierBase += 2;
        if (Random.value < 0.01f && expManagerScript.HealthPotionsTalentIsChosenExpManager)
        {
            Vector3 currentPosition = transform.position;
            spawnManager.CreateHealthPotionIfNotExists(currentPosition);
        }
        return base.deathAnimation();
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        while (enemyHealth > 0)
        {
            if (isInAttackRange && !isAttacking)
            {
                isAttacking = true;
                StartCoroutine(EnemyAttackToAnimation());
                attackIsOnCooldown = true;
                yield return new WaitForSeconds(attackInterval);
                attackIsOnCooldown = false;
            }
            yield return null;
        }
    }

    protected override void CheckAttackRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Determine if the enemy should escape or attack
        if (distanceToPlayer <= escapeDistance && attackIsOnCooldown)
        {
            isEscaping = true;
            isInAttackRange = false;
        }
        else if (distanceToPlayer <= attackRange)
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

            // Move towards the player if not in escape mode
            if (!isEscaping && !isInAttackRange)
            {
                moveDirection = (player.position - transform.position).normalized;
            }
            // Move away from the player if in escape mode
            else if (isEscaping)
            {
                moveDirection = (transform.position - player.position).normalized;
            }
            else
            {
                return;
            }

            rb.velocity = moveDirection * moveSpeed;
        }
    }
}
