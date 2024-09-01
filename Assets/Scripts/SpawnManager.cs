using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemies; // Array to hold the enemy prefabs
    [SerializeField] private BoxCollider spawnArea; // The BoxCollider that defines the spawn area
    [SerializeField] private SphereCollider exclusionZone; // The SphereCollider where enemies should not spawn
    //[SerializeField] private GameObject mainManagerScript;
    private GameObject healthPotionPrefab; // Prefab for health potion
    [SerializeField] private GameObject[] experiencePrefabs;
    private List<Vector3> recentPositions = new List<Vector3>();
    private const int maxRecentPositions = 3;
    private const float exclusionRadius = 5f;
    [SerializeField] GameObject bossObject;

    [SerializeField] GameObject[] bossSpawnSpellObjectPositions;


    [SerializeField] MainManager mainManagerScript;
    private int numberOfEnemies; // Number of enemies to spawn
    private float spawnHeight = 1.4f; // Desired spawn height
    public int waveDifficulty;
    private bool startSpawn;
    private float spawnTime = 3;
    private float decreaseRate = 2f / 600f;
    private int difficultyMeter;
    bool bossSpawned;

    int spawnCases = 21;

    [SerializeField] BoxCollider aoeSpawnArea;
    [SerializeField] GameObject aoeAreasObjects;
    [SerializeField] public GameObject aoeSpawnAreaObject;
    [SerializeField] GameObject bossStarParticleObject;
    float bossStarMoveSpeed = 10f;
    private Vector3 targetPosition = new Vector3(2.750329f, 1f, 2.416487f);
    ParticleSystem bossStarParticleSystem;
    [SerializeField] ParticleSystem bossStarFallParticleSystem;
    [SerializeField] BossEnemy bossEnemy;

    [SerializeField] AudioManager audioManager;
    [SerializeField] Player playerScript;

    public void ActivateAndMoveBossStar()
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            bossStarParticleObject.SetActive(true);
            startSpawn = false;

            StartCoroutine(MoveBossStar());
        }
    }

    IEnumerator MoveBossStar()
    {
        Vector3 startPosition = new Vector3(2.750329f, 9f, 2.416487f);

        while (bossStarParticleObject.transform.position.y > targetPosition.y)
        {
            bossStarParticleObject.transform.position = Vector3.MoveTowards(
                bossStarParticleObject.transform.position,
                targetPosition,
                bossStarMoveSpeed * Time.deltaTime
            );

            yield return null;
        }
        bossStarFallParticleSystem.Play();


        //yield return new WaitForSeconds(0.1f);
        bossObject.SetActive(true);
        bossEnemy.StartCoroutine(bossEnemy.BossHPRegen());
        mainManagerScript.BossFightUIEnable();
        bossStarParticleSystem.Stop();
        bossStarParticleSystem.Clear();
        bossStarParticleObject.SetActive(false);
        audioManager.BossMusicChangeStop();
        playerScript.audioSource.PlayOneShot(playerScript.audioClips[18], DataPersistence.soundsVolume * 0.7f * DataPersistence.soundAdjustment);
    }

    private void Start()
    {

        if (SceneManager.GetActiveScene().name == "MainScene")
        {
            bossStarParticleSystem = bossStarParticleObject.GetComponent<ParticleSystem>();
            startSpawn = true;
            StartCoroutine(Spawner());
            numberOfEnemies = 1;

            //ActivateAndMoveBossStar();
        }
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
        spawnBoss();
    }

    IEnumerator Spawner()
    {
        while (startSpawn)
        {
            if (bossSpawned)
            {
                yield break;
            }

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

    public void SpawnEnemiesBossSpell(Quaternion bossRotation)
    {
        int fixedPositionIndex = bossSpawnSpellObjectPositions.Length - 1;

        for (int i = 0; i < bossSpawnSpellObjectPositions.Length; i++)
        {
            Vector3 spawnPosition = bossSpawnSpellObjectPositions[i].transform.position;
            GameObject enemy;

            if (i == fixedPositionIndex)
            {
                enemy = enemies[1];
            }
            else
            {
                enemy = enemies[0];
            }

            GameObject instantiatedEnemy = Instantiate(enemy, spawnPosition, bossRotation);
            instantiatedEnemy.transform.position = new Vector3(instantiatedEnemy.transform.position.x, spawnHeight, instantiatedEnemy.transform.position.z);
        }
    }

    private void spawnBoss()
    {
        if (waveDifficulty == spawnCases && startSpawn)
        {
            StopCoroutine(Spawner());
            startSpawn = false;
        }
        if (!startSpawn && AreNoEnemiesExceptBoss() && !bossSpawned)
        {
            ActivateAndMoveBossStar();
            bossSpawned = true;
        }
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

    public void SpawnAoEAtRandomPosition()
    {
        if (aoeAreasObjects != null && aoeSpawnArea != null)
        {
            Vector3 spawnPosition;
            bool positionFound = false;
            int maxAttempts = 100;
            int attempt = 0;

            Quaternion spawnRotation = Quaternion.Euler(98.375f, 9.302002f, -19.33398f);

            while (!positionFound && attempt < maxAttempts)
            {
                spawnPosition = GetRandomPositionInAreaForAoE(aoeSpawnArea);

                if (IsPositionOutsideSphereColliders(spawnPosition) && IsPositionFarFromRecent(spawnPosition))
                {
                    Instantiate(aoeAreasObjects, spawnPosition, spawnRotation);
                    AddToRecentPositions(spawnPosition);
                    positionFound = true;
                }
                else
                {
                    attempt++;
                }
            }

            if (!positionFound)
            {
                Debug.LogWarning("Failed to find a suitable position outside all SphereColliders.");
            }
        }
        else
        {
            Debug.LogWarning("Prefab or fireSpawnArea is null");
        }
    }


    private Vector3 GetRandomPositionInAreaForAoE(BoxCollider area)
    {
        Vector3 center = area.transform.position + area.center;
        Vector3 size = area.size;

        float randomX = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
        float randomY = 1f;
        float randomZ = Random.Range(center.z - size.z / 2, center.z + size.z / 2);

        return new Vector3(randomX, randomY, randomZ);
    }
    private bool IsPositionOutsideSphereColliders(Vector3 position)
    {
        bool outsideSphere1 = !exclusionZone.bounds.Contains(position);

        return outsideSphere1;
    }
    private bool IsPositionFarFromRecent(Vector3 position)
    {
        foreach (Vector3 recentPosition in recentPositions)
        {
            if (Vector3.Distance(position, recentPosition) < exclusionRadius)
            {
                return false;
            }
        }
        return true;
    }

    private void AddToRecentPositions(Vector3 position)
    {
        if (recentPositions.Count >= maxRecentPositions)
        {
            recentPositions.RemoveAt(0);
        }
        recentPositions.Add(position);
    }
    private bool AreNoEnemiesExceptBoss()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (enemy.gameObject.name == "EnemyBoss")
            {
                return false;
            }
        }

        return enemies.Length == 0;
    }
}
