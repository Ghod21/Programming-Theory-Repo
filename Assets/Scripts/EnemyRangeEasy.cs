using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangeEasy : Enemy
{
    private GameObject projectilePrefab;
    private float projectileSpeed = 5f;
    private float attackInterval = 3f;

    private bool isInAttackRange = false;

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
        return base.deathAnimation();
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        while (enemyHealth > 0)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                EnemyAttack();
                StartCoroutine(AttackCooldown());
            }
            yield return new WaitForSeconds(attackInterval);
            isAttacking = false;
        }
    }

    protected override void CheckAttackRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            isInAttackRange = true;
            rb.velocity = Vector3.zero;
            attackRange = 100f;
        }
        else
        {
            isInAttackRange = false;
        }
    }

    protected override void EnemyAttack()
    {
        animator.SetBool("isAttacking", true);

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Offset to spawn the projectile slightly in front of the enemy
        Vector3 spawnPosition = transform.position + directionToPlayer * 1.0f;

        // Add offset to the y position to make the projectile appear slightly higher
        spawnPosition.y += 0.5f; // Adjust this value to control the height offset

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        projectile.GetComponent<Projectile>().Initialize(directionToPlayer, projectileSpeed, playerScript, soundAdjustment);
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        animator.SetBool("isAttacking", false);
        animator.SetTrigger("Idle");
    }

    public override void MoveTowardsPlayer()
    {
        if (!isAttacking && !isInAttackRange)
        {
            Vector3 moveDirection = (player.position - transform.position).normalized;
            rb.velocity = moveDirection * moveSpeed;
        }
    }
}
