using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 5;
    [SerializeField] private float verticalSpeedMultiplier = 1.5f; // Adjust this multiplier for vertical speed
    [SerializeField] private float turnSpeed = 360;
    [SerializeField] private float dashSpeed = 10; // Speed during dash
    [SerializeField] private float dashDuration = 0.2f; // Duration of the dash
    [SerializeField] private Transform model;
    [SerializeField] private Animator animator; // Add a reference to the Animator component
    private Vector3 input;
    private bool isDashing = false; // Track if the player is dashing
    private float dashTime; // Track the dash time

    private void Update()
    {
        GatherInput();
        LookAtMouse();
        HandleAnimations(); // Call the method to handle animations

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing)
        {
            Dash();
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
            model.rotation = Quaternion.RotateTowards(model.rotation, targetRotation, turnSpeed * Time.deltaTime);
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
}
