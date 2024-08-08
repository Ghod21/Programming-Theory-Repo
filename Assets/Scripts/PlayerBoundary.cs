using UnityEngine;

public class PlayerBoundary : MonoBehaviour
{
    [SerializeField] private BoxCollider boundaryCollider;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy") || other.CompareTag("EnemyRange"))
        {
            Vector3 playerPosition = other.transform.position;
            Vector3 closestPoint = boundaryCollider.ClosestPoint(playerPosition);

            if (playerPosition != closestPoint)
            {
                other.transform.position = closestPoint;
            }
        }
    }
}
