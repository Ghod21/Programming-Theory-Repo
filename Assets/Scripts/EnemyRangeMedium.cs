using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyRangeMedium : Enemy
{
    private GameObject projectilePrefab;
    private float projectileSpeed = 5f;
    private float attackInterval = 4f;
    private SpawnManager spawnManager;

    private bool isInAttackRange = false;

    protected override void Start()
    {
        projectilePrefab = Resources.Load<GameObject>("Prefabs/ProjectileRangeEnemy");
        base.Start();
        StartCoroutine(AttackRoutine());

        spawnManager = FindObjectOfType<SpawnManager>();
    }
    protected override IEnumerator deathAnimation()
    {
        DataPersistence.currentPlayerScore += 10 * playerScript.scoreMultiplier;
        playerScript.scoreMultiplierBase += 2;
        if (Random.value < 0.05f)
        {
            Vector3 currentPosition = transform.position;
            spawnManager.CreateHealthPotionIfNotExists(currentPosition);
        }
        return base.deathAnimation();
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

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(1f);
        while (enemyHealth > 0)
        {
            if (!isAttacking)
            {
                isAttacking = true;
                StartCoroutine(EnemyAttackToAnimation());
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                StartCoroutine(EnemySecondAttackToAnimation());
            }
            yield return new WaitForSeconds(attackInterval);
            isAttacking = false;
        }
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
    private IEnumerator EnemySecondAttackToAnimation()
    {
        animator.SetBool("isAttacking", true);

        EnemyAttack();

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        StartCoroutine(AttackCooldown());
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

