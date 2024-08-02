using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies; // Array to hold the enemy prefabs
    [SerializeField] private BoxCollider spawnArea; // The BoxCollider that defines the spawn area
    [SerializeField] private SphereCollider exclusionZone; // The SphereCollider where enemies should not spawn
    private int numberOfEnemies; // Number of enemies to spawn
    [SerializeField] private GameObject mainManager;
    private MainManager mainManagerScript;
    private float spawnHeight = 1.4f; // Desired spawn height
    public int waveDifficulty;
    private bool startSpawn;
    private float spawnTime = 3;
    private float decreaseRate = 2f / 600f;
    private int difficultyMeter;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            startSpawn = true;
            StartCoroutine(Spawner());
            numberOfEnemies = 1;
            spawnBoss();
        }    
        mainManagerScript = mainManager.GetComponent<MainManager>();
    }

    private void Update()
    {
        spawnTime -= decreaseRate * Time.deltaTime;
        if (spawnTime < 1f)
        {
            spawnTime = 1f;
        }
        difficultyMeter = mainManagerScript.difficultyMeter / 60;
        if(difficultyMeter < 1)
        {
            difficultyMeter = 1;
        } else if(difficultyMeter > 10)
        {
            difficultyMeter = 10;
        }
        waveDifficulty = Mathf.Clamp(difficultyMeter, 1, 10); // Adjust waveDifficulty based on difficultyMeter
    }

    IEnumerator Spawner()
    {
        while (startSpawn)
        {
            yield return new WaitForSeconds(spawnTime);
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPosition = GetRandomPointInBounds(spawnArea.bounds, exclusionZone);
            GameObject enemy = ChooseEnemyBasedOnDifficulty();
            GameObject instantiatedEnemy = Instantiate(enemy, spawnPosition, Quaternion.identity);
            instantiatedEnemy.transform.position = new Vector3(instantiatedEnemy.transform.position.x, spawnHeight, instantiatedEnemy.transform.position.z);
        }
    }

    private void spawnBoss()
    {
        Vector3 spawnPosition = GetRandomPointInBounds(spawnArea.bounds, exclusionZone);
        GameObject enemy = enemies[3];
        GameObject instantiatedEnemy = Instantiate(enemy, spawnPosition, Quaternion.identity);
        instantiatedEnemy.transform.position = new Vector3(instantiatedEnemy.transform.position.x, spawnHeight, instantiatedEnemy.transform.position.z);
    }

    private GameObject ChooseEnemyBasedOnDifficulty()
    {
        float[] probabilities = GetProbabilitiesForDifficulty(waveDifficulty);
        float randomValue = Random.value; // Random value between 0 and 1

        float cumulativeProbability = 0f;
        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue < cumulativeProbability)
            {
                return enemies[i];
            }
        }

        // Fallback in case of floating point precision issues
        return enemies[enemies.Length - 1];
    }

    private float[] GetProbabilitiesForDifficulty(int difficulty)
    {
        // Define probabilities based on difficulty
        switch (difficulty)
        {
            case 1:
                return new float[] { 1f, 0f, 0f }; // 100% chance for first enemy
            case 2:
                return new float[] { 0.95f, 0.05f, 0f };
            case 3:
                return new float[] { 0.9f, 0.1f, 0f };
            case 4:
                return new float[] { 0.85f, 0.15f, 0f };
            case 5:
                return new float[] { 0.8f, 0.19f, 0.01f };
            case 6:
                return new float[] { 0.75f, 0.23f, 0.02f };
            case 7:
                return new float[] { 0.7f, 0.27f, 0.03f };
            case 8:
                return new float[] { 0.65f, 0.31f, 0.04f };
            case 9:
                return new float[] { 0.6f, 0.35f, 0.05f };
            default:
                // Define fallback probabilities for higher difficulties
                return new float[] { 0.5f, 0.4f, 0.1f }; // Example: 50%, 40%, 10%
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
