using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Main enemy parent script.
    [SerializeField] protected BoxCollider boundaryCollider; // Collider defining the boundary
    [SerializeField] protected SphereCollider exclusionZone; // Collider defining where enemies should not spawn
    [SerializeField] protected Animator animator;
    [SerializeField] protected Player playerScript;
    private float desiredY = 1.4f; // Desired height for the position
    protected Transform player; // Reference to the player's Transform
    protected Rigidbody rb; // Reference to the Rigidbody component
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected float attackRange = 2.0f; // Distance within which the enemy will attack
    protected bool isAttacking = false; // To prevent multiple attack calls
    public int enemyHealth;
    public bool attacked;
    bool deathAnimationDone = false;
    protected float soundAdjustment = 0.6f;
    int prefabIndex;

    [SerializeField] private GameObject currentObject;

    protected SpawnManager spawnManager;

    protected virtual void Start()
    {
        // Find and assign the BoxCollider with the "MapBox" tag
        boundaryCollider = GameObject.FindWithTag("MapBox").GetComponent<BoxCollider>();
        // Find and assign the SphereCollider with the "ExclusionZone" tag
        exclusionZone = GameObject.FindWithTag("Player").GetComponent<SphereCollider>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        // Find the Player object and get the Player script
        GameObject playerObject = GameObject.Find("Player");
        playerScript = playerObject.GetComponent<Player>();
        spawnManager = FindObjectOfType<SpawnManager>();
        currentObject = this.gameObject;
    }

    private void Update()
    {
        LookAtPlayer(); // Ensure the enemy always faces the player
        CheckBoundary();
        CheckAttackRange(); // Check if the enemy is within attack range
        rb.velocity = Vector3.zero; // Reset velocity to avoid residual movement
        rb.angularVelocity = Vector3.zero; // Reset angular velocity to avoid rotation issues
        if (enemyHealth <= 0 && !deathAnimationDone)
        {
            StartCoroutine(deathAnimation());
            deathAnimationDone = true;
        }
        if (attacked && enemyHealth > 0)
        {
            animator.SetTrigger("Attacked");
            attacked = false;
        }
    }


    private void FixedUpdate()
    {
        if (!isAttacking && !animator.GetBool("isAttacking")) // Only move if not attacking
        {
            MoveTowardsPlayer(); // Move the enemy towards the player
        }
    }

    private void LookAtPlayer()
    {
        if (!attacked && enemyHealth > 0)
        {
            Vector3 direction = (player.position - transform.position).normalized; // Calculate direction to the player
            Quaternion lookRotation = Quaternion.LookRotation(direction); // Calculate the rotation to look at the player
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // Smoothly rotate towards the player
        }
    }

    public virtual void MoveTowardsPlayer()
    {
        if (!attacked && enemyHealth > 0)
        {
            Vector3 moveDirection = (player.position - transform.position).normalized; // Calculate direction towards the player
            rb.velocity = moveDirection * moveSpeed; // Use velocity to move the enemy towards the player
        }
    }


    private void CheckBoundary()
    {
        Vector3 enemyPosition = transform.position;
        Vector3 closestPointInBounds = boundaryCollider.ClosestPoint(enemyPosition);

        // If enemy is outside the boundary, teleport them to the closest valid point
        if (enemyPosition != closestPointInBounds)
        {
            Vector3 newPosition = GetClosestValidPoint(closestPointInBounds);
            newPosition.y = desiredY; // Set the desired height
            transform.position = newPosition;
        }
    }

    private Vector3 GetClosestValidPoint(Vector3 startPoint)
    {
        // Check if the startPoint is inside the exclusion zone
        if (IsInsideExclusionZone(startPoint))
        {
            // Find the closest point on the exclusion zone boundary
            Vector3 closestPointOnExclusion = exclusionZone.ClosestPoint(startPoint);
            // Calculate direction to move away from the exclusion zone
            Vector3 directionAwayFromExclusion = (startPoint - closestPointOnExclusion).normalized;
            // Move the point to the boundary of the exclusion zone
            return closestPointOnExclusion + directionAwayFromExclusion * exclusionZone.radius;
        }

        return startPoint;
    }

    private bool IsInsideExclusionZone(Vector3 point)
    {
        Vector3 closestPoint = exclusionZone.ClosestPoint(point);
        return Vector3.Distance(point, closestPoint) < Mathf.Epsilon;
    }

    protected virtual void CheckAttackRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            isAttacking = true;
            StartCoroutine(Attack());
        }
    }


    private IEnumerator Attack()
    {
        if (enemyHealth > 0 && !deathAnimationDone)
        {
            // Attack logic
            EnemyAttack();

            // Wait for attack cooldown
            yield return new WaitForSeconds(0.6f);

            isAttacking = false;
            animator.SetBool("isAttacking", false);
            animator.SetTrigger("Idle");
        }
    }



    protected virtual void EnemyAttack()
    {
        // Method for enemy attacks
        animator.SetBool("isAttacking", true);
        if (playerScript.isDashing == false && !playerScript.isBlockingDamage)
        {
            playerScript.playerHealth--;
            playerScript.scoreMultiplierBase -= 10;
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[5], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
            Debug.Log("Health: " + playerScript.playerHealth);

        } else if (playerScript.isDashing == false && playerScript.isBlockingDamage)
        {
            playerScript.shieldHealth--;
            playerScript.audioSource.PlayOneShot(playerScript.audioClips[6], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
            Debug.Log("ShieldHealth: " + playerScript.shieldHealth);
        }
    }

    protected virtual IEnumerator deathAnimation()
    {
        PrefabIdentifier prefabIdentifier = GetComponent<PrefabIdentifier>();
        if (prefabIdentifier.prefabName != "boss")
        {
            ExperienceSpawnOnDeath();
        }
        animator.SetTrigger("Dead");
        yield return new WaitForSeconds(1.067f);
        Destroy(gameObject);
    }
    void ExperienceSpawnOnDeath()
    {
        Vector3 position = transform.position + new Vector3(0, 0.5f, 0);
        spawnManager.CreateExperienceAtPosition(position, CheckPrefabType());
    }
    private int CheckPrefabType()
    {
        PrefabIdentifier prefabIdentifier = GetComponent<PrefabIdentifier>();
        if (prefabIdentifier != null)
        {
            if (prefabIdentifier.prefabName == "easy")
            {
                prefabIndex = 0;
            } else if (prefabIdentifier.prefabName == "medium")
            {
                prefabIndex = 1;
            } else if (prefabIdentifier.prefabName == "hard")
            {
                prefabIndex = 2;
            }
        }
        return prefabIndex;
    }
}
