using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experience : MonoBehaviour
{
    GameObject player;
    AudioSource audioSource;
    AudioClip experienceSound;
    private Player playerScript;
    [SerializeField] private float rotationSpeed = 50f;
    float soundAdjustment = DataPersistence.soundAdjustment;
    GameObject playerExpGainObject;
    public Transform playerExpGainPoint;
    float initialSpeed = 1.0f;
    float acceleration = 50.0f;
    SphereCollider sphereCollider;
    ExpManager expManager;

    bool goToPlayer;

    protected float experiencePlus;

    void Start()
    {
        expManager = FindObjectOfType<ExpManager>();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = expManager.expRangePickUp;
        player = GameObject.Find("Player");
        playerExpGainObject = GameObject.FindWithTag("ExperienceGainPosition");
        playerExpGainPoint = playerExpGainObject.GetComponent<Transform>();
        playerScript = player.GetComponent<Player>();
        audioSource = player.GetComponent<AudioSource>();
        experienceSound = Resources.Load<AudioClip>("Audio/ExperienceSound");
        CheckCurrentObject();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        if (playerExpGainPoint != null && goToPlayer)
        {
            float distance = Vector3.Distance(transform.position, playerExpGainPoint.position);

            float currentSpeed = initialSpeed + acceleration / (distance + 1); // "+1" to not divide to 0.

            Vector3 direction = (playerExpGainPoint.position - transform.position).normalized;
            transform.position += direction * currentSpeed * Time.deltaTime;
        }

    }
    private void CheckCurrentObject()
    {
        PrefabIdentifier prefabIdentifier = GetComponent<PrefabIdentifier>();
        if (prefabIdentifier != null)
        {
            if (prefabIdentifier.prefabName == "ExperienceSmall")
            {
                experiencePlus = 10;

            } else if (prefabIdentifier.prefabName == "ExperienceMedium")
            {
                experiencePlus = 25;
            } else
            {
                experiencePlus = 50;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerToHealthPotion"))
        {
            goToPlayer = true;
        }
        if (other.CompareTag("ExperienceGainPosition"))
        {
            playerScript.playerExperience += experiencePlus;
            if (experiencePlus == 10)
            {
                if (DataPersistence.easyDifficulty)
                {
                    DataPersistence.currentPlayerScore += 1 * playerScript.scoreMultiplier;
                } else
                {
                    DataPersistence.currentPlayerScore += 5 * playerScript.scoreMultiplier;
                }
                playerScript.scoreMultiplierBase++;
            } else if (experiencePlus == 25)
            {
                if (DataPersistence.easyDifficulty)
                {
                    DataPersistence.currentPlayerScore += 3 * playerScript.scoreMultiplier;
                }
                else
                {
                    DataPersistence.currentPlayerScore += 10 * playerScript.scoreMultiplier;
                }
            } else
            {
                if (DataPersistence.easyDifficulty)
                {
                    DataPersistence.currentPlayerScore += 5 * playerScript.scoreMultiplier;
                }
                else
                {
                    DataPersistence.currentPlayerScore += 20 * playerScript.scoreMultiplier;
                }
                playerScript.scoreMultiplierBase += 3;
            }

            audioSource.PlayOneShot(experienceSound, DataPersistence.soundsVolume * 0.5f * soundAdjustment);
            Destroy(gameObject);
        }
    }
}
