using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies; // Array to hold the enemy prefabs
    [SerializeField] private BoxCollider spawnArea; // The BoxCollider that defines the spawn area
    [SerializeField] private SphereCollider exclusionZone; // The SphereCollider where enemies should not spawn
    [SerializeField] private GameObject mainManager;
    private GameObject healthPotionPrefab; // Prefab for health potion
    [SerializeField] private GameObject[] experiencePrefabs;


    private MainManager mainManagerScript;
    private int numberOfEnemies; // Number of enemies to spawn
    private float spawnHeight = 1.4f; // Desired spawn height
    public int waveDifficulty;
    private bool startSpawn;
    private float spawnTime = 3;
    private float decreaseRate = 2f / 600f;
    private int difficultyMeter;
    bool bossSpawned;

    int spawnCases = 21;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            startSpawn = true;
            StartCoroutine(Spawner());
            numberOfEnemies = 1;
            // spawnBoss(); // Test for boss in the beginning
        }
        mainManagerScript = mainManager.GetComponent<MainManager>();
        healthPotionPrefab = Resources.Load<GameObject>("Prefabs/HealthPotion");
    }

    private void Update()
    {
        spawnTime -= decreaseRate * Time.deltaTime;
        if (spawnTime < 1f)
        {
            spawnTime = 1f;
        }
        difficultyMeter = mainManagerScript.difficultyMeter / 30;
        if (difficultyMeter < 1)
        {
            difficultyMeter = 1;
        }
        else if (difficultyMeter > spawnCases)
        {
            difficultyMeter = spawnCases;
        }
        waveDifficulty = Mathf.Clamp(difficultyMeter, 1, spawnCases); // Adjust waveDifficulty based on difficultyMeter
        if (waveDifficulty == spawnCases && !bossSpawned)
        {
            spawnBoss();
            bossSpawned = true;
        }
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
        GameObject enemy = enemies[5];
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
                return new float[] { 1f, 0f, 0f, 0f, 0f };
            case 2:
                return new float[] { 0.95f, 0.05f, 0f, 0f, 0f };
            case 3:
                return new float[] { 0.9f, 0.1f, 0f, 0f, 0f };
            case 4:
                return new float[] { 0.85f, 0.1f, 0.05f, 0f, 0f };
            case 5:
                return new float[] { 0.8f, 0.1f, 0.1f, 0f, 0f };
            case 6:
                return new float[] { 0.75f, 0.1f, 0.15f, 0f, 0f };
            case 7:
                return new float[] { 0.7f, 0.1f, 0.15f, 0.05f, 0f };
            case 8:
                return new float[] { 0.65f, 0.1f, 0.2f, 0.05f, 0f };
            case 9:
                return new float[] { 0.6f, 0.1f, 0.25f, 0.05f, 0f };
            case 10:
                return new float[] { 0.55f, 0.1f, 0.3f, 0.05f, 0f };
            case 11:
                return new float[] { 0.5f, 0.1f, 0.3f, 0.1f, 0.05f };
            case 12:
                return new float[] { 0.45f, 0.1f, 0.35f, 0.1f, 0.05f };
            case 13:
                return new float[] { 0.4f, 0.1f, 0.4f, 0.1f, 0.05f };
            case 14:
                return new float[] { 0.35f, 0.1f, 0.45f, 0.1f, 0.05f };
            case 15:
                return new float[] { 0.3f, 0.1f, 0.5f, 0.1f, 0.05f };
            case 16:
                return new float[] { 0.25f, 0.1f, 0.55f, 0.1f, 0.05f };
            case 17:
                return new float[] { 0.2f, 0.1f, 0.6f, 0.1f, 0.05f };
            case 18:
                return new float[] { 0.15f, 0.1f, 0.65f, 0.1f, 0.05f };
            case 19:
                return new float[] { 0.1f, 0.1f, 0.7f, 0.1f, 0.05f };
            case 20:
                return new float[] { 0.05f, 0.1f, 0.75f, 0.1f, 0.05f };
            case 21:
                return new float[] { 0.05f, 0.1f, 0.8f, 0.1f, 0.05f };
            default:
                // Define fallback probabilities for higher difficulties
                return new float[] { 0.4f, 0.3f, 0.2f, 0.05f, 0.05f }; // Example: 40%, 30%, 20%, 5%, 5%
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

    // Public function to create the health potion prefab if none exist in the scene
    public void CreateHealthPotionIfNotExists(Vector3 callingObjectPosition)
    {
        // Check if any instance of healthPotionPrefab exists in the scene
        GameObject[] existingPotions = GameObject.FindGameObjectsWithTag(healthPotionPrefab.tag);
        if (existingPotions.Length == 0)
        {
            // Set the spawn position to the calling object's position with the desired spawn height
            Vector3 spawnPosition = new Vector3(callingObjectPosition.x, spawnHeight, callingObjectPosition.z);
            Instantiate(healthPotionPrefab, spawnPosition, Quaternion.identity);
        }
    }
    public void CreateExperienceAtPosition(Vector3 position, int prefabIndex)
    {
        // Validate index
        if (prefabIndex < 0 || prefabIndex >= experiencePrefabs.Length)
        {
            Debug.LogWarning("Invalid prefab index.");
            return; // Return early if the index is invalid
        }

        // Set the spawn position to the specified position with the desired spawn height
        Vector3 spawnPosition = new Vector3(position.x, spawnHeight, position.z);

        // Create a rotation of 90 degrees around the Z axis
        Quaternion rotation = Quaternion.Euler(90, 0, 0);

        // Instantiate the prefab at the spawn position with the specified rotation
        Instantiate(experiencePrefabs[prefabIndex], spawnPosition, rotation);
    }


}
