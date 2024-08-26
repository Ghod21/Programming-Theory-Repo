using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private Player playerScript;
    float soundAdjustment = DataPersistence.soundAdjustment;
    private float rotationSpeed = 60f;
    GameObject expManager;
    ExpManager expManagerScript;

    public bool reflectionTalentIsChosen;
    bool isReflected;

    private void Start()
    {
        expManager = GameObject.Find("Exp_Bar");
        expManagerScript = expManager.GetComponent<ExpManager>();
    }

    public void Initialize(Vector3 direction, float speed, Player playerScript, float soundAdjustment)
    {
        this.direction = direction;
        this.speed = speed;
        this.playerScript = playerScript;
        this.soundAdjustment = soundAdjustment;

        // Rotation for not round objects.
        // Quaternion targetRotation = Quaternion.LookRotation(direction);
        // transform.rotation = targetRotation * Quaternion.Euler(90f, 0f, 0f);

        Destroy(gameObject, 6f);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        reflectionTalentIsChosen = expManagerScript.reflectionTalentIsChosenExpManager;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("PlayerToHealthPotion"))
        {

            if (!playerScript.isDashing)
            {
                if (!playerScript.isBlockingDamage && !isReflected)
                {
                    playerScript.playerHealth -= 2;
                    playerScript.scoreMultiplierBase -= 10;
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[5], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
                    Destroy(gameObject);
                }
                else if (playerScript.isBlockingDamage && !reflectionTalentIsChosen && !isReflected)
                {
                    playerScript.shieldHealth -= 2;
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[6], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
                    Destroy(gameObject);
                } else if (playerScript.isBlockingDamage && reflectionTalentIsChosen)
                {
                    playerScript.shieldHealth--;
                    direction = -direction;
                    isReflected = true;
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[9], DataPersistence.soundsVolume * 0.6f * soundAdjustment);
                    Destroy(gameObject, 6f);
                }
            }
        }

        if (other.CompareTag("Enemy") && isReflected)
        {
            PrefabIdentifier prefabIdentifier = other.GetComponent<PrefabIdentifier>();
            if (prefabIdentifier != null)
            {
                if (prefabIdentifier.prefabName == "EnemyRangeEasy")
                {
                    EnemyRangeEasy enemyRangeEasyScript = other.transform.parent.GetComponent<EnemyRangeEasy>();

                    enemyRangeEasyScript.enemyHealth--;
                    if (enemyRangeEasyScript.enemyHealth > 0)
                    {
                        playerScript.audioSource.PlayOneShot(playerScript.audioClips[0], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
                    }
                    else
                    {
                        playerScript.audioSource.PlayOneShot(playerScript.audioClips[3], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
                    }
                    Destroy(gameObject);
                } else if (prefabIdentifier.prefabName == "EnemyRangeMedium")
                {
                    EnemyRangeMedium enemyRangeMediumScript = other.transform.parent.GetComponent<EnemyRangeMedium>();
                    enemyRangeMediumScript.enemyHealth--;
                    if (enemyRangeMediumScript.enemyHealth > 0)
                    {
                        playerScript.audioSource.PlayOneShot(playerScript.audioClips[0], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
                    }
                    else
                    {
                        playerScript.audioSource.PlayOneShot(playerScript.audioClips[3], DataPersistence.soundsVolume * 0.8f * soundAdjustment);
                    }
                    Destroy(gameObject);
                }
            }
        }
    }
}
