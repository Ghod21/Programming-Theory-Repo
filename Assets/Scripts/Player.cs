using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform model;
    [SerializeField] private Animator animator; // Add a reference to the Animator component
    [SerializeField] private float speed = 5;
    [SerializeField] private float verticalSpeedMultiplier = 1.5f; // Adjust this multiplier for vertical speed
    [SerializeField] private float dashSpeed = 10; // Speed during dash
    [SerializeField] private float dashDuration = 0.2f; // Duration of the dash
    [SerializeField] private float attackRange = 3f; // Range of the attack
    [SerializeField] private float attackAngle = 180f; // Angle of the attack cone
    [SerializeField] private string[] targetTags; // Tags of the targets
    private Vector3 input;
    public bool isDashing = false; // Track if the player is dashing
    private bool isAttacking = false;
    private bool attacked = false;
    private bool dashIsOnCooldown = false;
    private float dashTime; // Track the dash time
    private float attackCooldown = 0f; // Cooldown for attack
    private const float attackCooldownDuration = 0.833f; // Duration of the attack cooldown
    private int attackCount;

    public int playerHealth = 30;

    private bool isAttackQueued = false; // Flag to queue attacks

    private void Start()
    {
        animator.SetBool("isNotAttacking", true);
    }

    private void Update()
    {
        GatherInput();
        LookAtMouse();
        HandleAnimations(); // Call the method to handle animations
        isNotAttackingCheck(); // Check if not attacking

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && !dashIsOnCooldown)
        {
            Dash();
            dashIsOnCooldown = true;
            StartCoroutine(dashCooldown());
        }

        attackCooldown -= Time.deltaTime; // Decrease the cooldown timer

        if (Input.GetMouseButton(0) && !isAttacking && attackCooldown <= 0f)
        {
            Attack();
        }
        else if (Input.GetMouseButton(0) && isAttacking)
        {
            isAttackQueued = true; // Queue the attack if one is already in progress
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GatherInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
    }

    private void Move()
    {
        if (isDashing)
        {
            // Calculate movement direction in isometric space
            Vector3 movement = CalculateMovement(input) * dashSpeed / speed; // Adjust speed for dash
            rb.MovePosition(transform.position + movement);

            dashTime -= Time.deltaTime;
            if (dashTime <= 0)
            {
                isDashing = false;
            }
        }
        else
        {
            // Calculate movement direction in isometric space
            Vector3 movement = CalculateMovement(input);
            rb.MovePosition(transform.position + movement);
        }
    }

    private Vector3 CalculateMovement(Vector3 inputDirection)
    {
        // Convert input direction to isometric view
        Vector3 isoDirection = inputDirection.ToIso();

        // Adjust speed based on the isometric view
        float adjustedSpeed = (inputDirection.z != 0) ? speed * verticalSpeedMultiplier : speed; // Increase vertical speed

        return isoDirection * adjustedSpeed * Time.deltaTime;
    }

    private void LookAtMouse()
    {
        // Get the mouse position in world space
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (playerPlane.Raycast(ray, out float hitDist))
        {
            Vector3 targetPoint = ray.GetPoint(hitDist);
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
            model.rotation = targetRotation; // Instantly face the mouse direction
        }
    }

    private void HandleAnimations()
    {
        bool isMoving = input != Vector3.zero;
        animator.SetBool("isMoving", isMoving); // Set the "isMoving" parameter in the Animator
    }

    private void Dash()
    {
        isDashing = true;
        dashTime = dashDuration;
    }

    private void OnCollisionStay(Collision collision)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    IEnumerator dashCooldown()
    {
        yield return new WaitForSeconds(3);
        dashIsOnCooldown = false;
    }

    private void Attack()
    {
        if (isAttacking)
        {
            isAttackQueued = true; // Queue the attack if already attacking
            return;
        }

        isAttacking = true;
        attackCooldown = attackCooldownDuration; // Reset the cooldown timer
        animator.SetBool("isAttacking", true);

        // Start attack animation coroutine
        StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(0.833f / 2); // Half of the attack animation duration

        // Perform attack action at halfway through the animation
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (IsTargetInCone(hitCollider.transform))
            {
                if (hitCollider.CompareTag("Enemy"))
                {
                    // Apply damage to the target
                    Debug.Log("Hit: " + hitCollider.name + " " + attackCount);
                    attackCount++;
                    Enemy enemy = hitCollider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.enemyHealth--;
                    }
                }
            }
        }
        isAttackQueued = false;
        // Wait for the remaining half of the animation duration
        yield return new WaitForSeconds(0.833f / 2);

        // Reset attack state
        isAttacking = false;
        animator.SetBool("isAttacking", false);

        // If an attack was queued, start the next one
        if (isAttackQueued)
        {
            isAttackQueued = false;
            Attack();
        }
    }

    private bool IsTargetInCone(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(model.forward, directionToTarget);
        return angleToTarget < attackAngle / 2;
    }

    void isNotAttackingCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetBool("isNotAttacking", false);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            animator.SetBool("isNotAttacking", true);
        }
    }
}
