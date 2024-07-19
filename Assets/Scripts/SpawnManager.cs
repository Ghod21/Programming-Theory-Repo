using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies; // Array to hold the enemy prefabs
    [SerializeField] private BoxCollider spawnArea; // The BoxCollider that defines the spawn area
    [SerializeField] private SphereCollider exclusionZone; // The SphereCollider where enemies should not spawn
    [SerializeField] private int numberOfEnemies = 5; // Number of enemies to spawn
    private float spawnHeight = 1.4f; // Desired spawn height
    public static int waveDifficulty = 3;

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPosition = GetRandomPointInBounds(spawnArea.bounds, exclusionZone);
            int randomEnemyIndex = Random.Range(0, waveDifficulty);
            GameObject enemy = Instantiate(enemies[randomEnemyIndex], spawnPosition, Quaternion.identity);
            enemy.transform.position = new Vector3(enemy.transform.position.x, spawnHeight, enemy.transform.position.z);
        }
    }

    private Vector3 GetRandomPointInBounds(Bounds bounds, SphereCollider exclusionZone)
    {
        Vector3 randomPoint;
        int attempts = 0;
        do
        {
            randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Make sure the point is inside the BoxCollider bounds
            randomPoint = bounds.ClosestPoint(randomPoint);

            // Prevent infinite loop in case of setup issues
            attempts++;
            if (attempts > 100)
            {
                Debug.LogWarning("Could not find a valid spawn point within bounds and exclusion zone.");
                break;
            }

        } while (IsInsideExclusionZone(randomPoint, exclusionZone));

        return randomPoint;
    }

    private bool IsInsideExclusionZone(Vector3 point, SphereCollider exclusionZone)
    {
        Vector3 closestPoint = exclusionZone.ClosestPoint(point);
        return Vector3.Distance(point, closestPoint) < Mathf.Epsilon;
    }
}
