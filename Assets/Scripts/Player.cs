using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 5;
    [SerializeField] private float verticalSpeedMultiplier = 1.5f; // Adjust this multiplier for vertical speed
    [SerializeField] private float turnSpeed = 360;
    [SerializeField] private Transform model;
    private Vector3 input;

    private void Update()
    {
        GatherInput();
        Look();
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
        // Calculate movement direction in isometric space
        Vector3 movement = CalculateMovement(input);
        rb.MovePosition(transform.position + movement);
    }

    private Vector3 CalculateMovement(Vector3 inputDirection)
    {
        // Convert input direction to isometric view
        Vector3 isoDirection = inputDirection.ToIso();

        // Adjust speed based on the isometric view
        float adjustedSpeed = (inputDirection.z != 0) ? speed * verticalSpeedMultiplier : speed; // Increase vertical speed

        return isoDirection * adjustedSpeed * Time.deltaTime;
    }

    private void Look()
    {
        if (input == Vector3.zero) return;

        // Calculate rotation to face the movement direction in isometric space
        Quaternion targetRotation = Quaternion.LookRotation(CalculateMovement(input), Vector3.up);
        model.rotation = Quaternion.RotateTowards(model.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}