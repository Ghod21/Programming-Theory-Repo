using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Main enemy parent script.
    [SerializeField] protected BoxCollider boundaryCollider; // Collider defining the boundary
    [SerializeField] protected SphereCollider exclusionZone; // Collider defining where enemies should not spawn
    [SerializeField] public Animator animator;
    [SerializeField] protected Player playerScript;
    protected MainManager mainManager;
    public GameObject expManager;
    public ExpManager expManagerScript;
    private float desiredY = 1.4f; // Desired height for the position
    protected Transform player; // Reference to the player's Transform
    protected Rigidbody rb; // Reference to the Rigidbody component
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected float attackRange = 2.0f; // Distance within which the enemy will attack
    protected bool isAttacking = false; // To prevent multiple attack calls
    public float enemyHealth;
    public bool attacked;
    bool deathAnimationDone = false;
    protected float soundAdjustment = DataPersistence.soundAdjustment;
    int prefabIndex;
    public bool enemyIsBleeding;
    public bool enemyIsHitByFire;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private UnityEngine.Color originalColor;
    private UnityEngine.Color newColor;
    protected bool isUsingSpell;

    public bool damagedByVortex = false;

    readonly string newColorHex = "#FFD3D3";

    public bool isUnderDefenceAura = false;
    public bool isHardEnemy = false;

    protected SpawnManager spawnManager;

    // Talents variables
    public bool shieldDamageTalentChosen = false;

    protected virtual void Start()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        expManager = GameObject.Find("Exp_Bar");
        expManagerScript = expManager.GetComponent<ExpManager>();
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
        SkinnedMeshRendererSearch();
    }

    protected virtual void Update()
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
        shieldDamageTalentChosen = expManagerScript.shieldDamageTalentChosenExpManager;
    }


    private void FixedUpdate()
    {
        if (!isAttacking && !animator.GetBool("isAttacking")) // Only move if not attacking
        {
            MoveTowardsPlayer(); // Move the enemy towards the player
        }
    }
    public IEnumerator BladeVortexDamageCooldown()
    {
        damagedByVortex = true;
        yield return new WaitForSeconds(1);
        damagedByVortex = false;
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
            if (shieldDamageTalentChosen)
            {
                if (!isUnderDefenceAura)
                {
                    enemyHealth--;
                }
                if (enemyHealth > 0)
                {
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[0], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
                } else
                {
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[3], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
                }
            } else
            {
                playerScript.audioSource.PlayOneShot(playerScript.audioClips[6], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
            }
            Debug.Log("ShieldHealth: " + playerScript.shieldHealth);
        }
    }

    protected virtual IEnumerator deathAnimation()
    {
        PrefabIdentifier prefabIdentifier = GetComponent<PrefabIdentifier>();
        if (prefabIdentifier.prefabName != "EnemyBoss")
        {
            ExperienceSpawnOnDeath();
        }
        animator.SetTrigger("Dead");
        yield return new WaitForSeconds(0.1f);
        playerScript.StartCoroutine(playerScript.KillSoundCooldown());
        yield return new WaitForSeconds(1.067f - 0.1f);
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
            if (prefabIdentifier.prefabName == "EnemyEasy" || prefabIdentifier.prefabName == "EnemyRangeEasy")
            {
                prefabIndex = 0;
            } else if (prefabIdentifier.prefabName == "EnemyMedium" || prefabIdentifier.prefabName == "EnemyRangeMedium")
            {
                prefabIndex = 1;
            } else if (prefabIdentifier.prefabName == "EnemyHard")
            {
                prefabIndex = 2;
            }
        }
        return prefabIndex;
    }
    void SkinnedMeshRendererSearch()
    {
        foreach (Transform child in transform)
        {
            SkinnedMeshRenderer renderer = child.GetComponent<SkinnedMeshRenderer>();

            if (renderer != null)
            {
                skinnedMeshRenderer = renderer;
                originalColor = skinnedMeshRenderer.material.color;
                break;
            }
        }

        if (skinnedMeshRenderer == null)
        {
            Debug.LogWarning("SkinnedMeshRenderer not found.");
        }
    }
    public void AddMaterial()
    {
        if (skinnedMeshRenderer != null)
        {
            if (UnityEngine.ColorUtility.TryParseHtmlString(newColorHex, out newColor))
            {
                skinnedMeshRenderer.material.color = newColor;
            }
            else
            {
                Debug.LogWarning("Invalid hex color string.");
            }
        }
    }

    public void RemoveMaterial()
    {
        if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.material.color = originalColor;
        }
    }

}
