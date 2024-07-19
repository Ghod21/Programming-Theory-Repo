using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Main enemy parent script.
    [SerializeField] private BoxCollider boundaryCollider; // Collider defining the boundary
    [SerializeField] private SphereCollider exclusionZone; // Collider defining where enemies should not spawn
    private float desiredY = 1.4f; // Desired height for the position
    private Transform player; // Reference to the player's Transform
    private Rigidbody rb; // Reference to the Rigidbody component
    protected float moveSpeed = 3.5f;
    [SerializeField] private float attackRange = 2.0f; // Distance within which the enemy will attack
    private bool isAttacking = false; // To prevent multiple attack calls

    private void Start()
    {
        // Find and assign the BoxCollider with the "MapBox" tag
        boundaryCollider = GameObject.FindWithTag("MapBox").GetComponent<BoxCollider>();
        // Find and assign the SphereCollider with the "ExclusionZone" tag
        exclusionZone = GameObject.FindWithTag("Player").GetComponent<SphereCollider>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        LookAtPlayer(); // Ensure the enemy always faces the player
        CheckBoundary();
        CheckAttackRange(); // Check if the enemy is within attack range
    }

    private void FixedUpdate()
    {
        if (!isAttacking) // Only move if not attacking
        {
            MoveTowardsPlayer(); // Move the enemy towards the player
        }
    }

    private void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized; // Calculate direction to the player
        Quaternion lookRotation = Quaternion.LookRotation(direction); // Calculate the rotation to look at the player
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // Smoothly rotate towards the player
    }

    public virtual void MoveTowardsPlayer()
    {
        Vector3 moveDirection = (player.position - transform.position).normalized; // Calculate direction towards the player
        rb.MovePosition(transform.position + moveDirection * moveSpeed * Time.deltaTime); // Move towards the player using Rigidbody
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

    private void OnCollisionStay(Collision collision)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void CheckAttackRange()
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
        // Attack logic
        EnemyAttack();

        // Wait for attack cooldown
        yield return new WaitForSeconds(1.0f);

        isAttacking = false;
    }

    void EnemyAttack()
    {
        // Method for enemy attacks
        // Leave this method empty for now
    }
}
