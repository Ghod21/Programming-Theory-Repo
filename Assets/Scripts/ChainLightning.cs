using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainLightning : MonoBehaviour
{
    private ParticleSystem lightningParticles; // Reference to the ParticleSystem
    public float detectionRadius = 5f; // Radius for detecting enemies
    public float delayBetweenJumps = 0.2f; // Delay between lightning jumps

    private List<Enemy> hitEnemies = new List<Enemy>(); // List of enemies that have been hit
    // Call this method to set the lightning particle object and get the ParticleSystem
    public void SetLightningParticleObject(GameObject lightningParticleObject)
    {
        if (lightningParticleObject != null)
        {
            lightningParticles = lightningParticleObject.GetComponent<ParticleSystem>();
            if (lightningParticles == null)
            {
                Debug.LogError("ParticleSystem component is missing from lightningParticleObject.");
            }
        }
        else
        {
            Debug.LogError("lightningParticleObject is null.");
        }
    }

    public void StartChainLightning(Enemy initialTarget, Player playerScript)
    {
        if (initialTarget == null)
        {
            Debug.LogError("Initial target is null.");
            return;
        }

        // Ensure the lightning particle object is active
        if (lightningParticles != null)
        {
            lightningParticles.gameObject.SetActive(true);
        }

        StartCoroutine(ChainLightningCoroutine(initialTarget, playerScript));
    }

    private IEnumerator ChainLightningCoroutine(Enemy currentTarget, Player playerScript)
    {
        while (currentTarget != null)
        {
            if (currentTarget == null)
            {
                Debug.LogError("Current target is null.");
                yield break; // Exit the coroutine if the target is null
            }

            // Set the lightning position to the current enemy
            transform.position = currentTarget.transform.position;

            // Activate the particles
            if (lightningParticles != null)
            {
                lightningParticles.Play();
                currentTarget.enemyHealth--;
                playerScript.audioSource.PlayOneShot(playerScript.audioClips[12], DataPersistence.soundsVolume * 2.5f * DataPersistence.soundAdjustment);
                Debug.Log("Enemy is hit by lightning");

                // Wait for 0.2 seconds
                yield return new WaitForSeconds(delayBetweenJumps);

                // Stop and clear the particles
                lightningParticles.Stop();
                lightningParticles.Clear();
            }
            else
            {
                Debug.LogError("ParticleSystem is not assigned.");
                yield break; // Exit the coroutine if ParticleSystem is not available
            }

            // Add the current enemy to the list of hit enemies
            hitEnemies.Add(currentTarget);

            // Find the nearest enemy within the radius, excluding already hit enemies
            Enemy nextTarget = FindNearestEnemy(currentTarget.transform.position);

            // If there's no next enemy, exit the loop
            if (nextTarget == null)
            {
                lightningParticles.gameObject.SetActive(false);
                break;
            }

            // Set the next enemy as the current one for the next cycle
            currentTarget = nextTarget;
        }
    }

    private Enemy FindNearestEnemy(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, detectionRadius);
        Enemy nearestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (Collider collider in hitColliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                }
            }
        }

        return nearestEnemy;
    }
}
