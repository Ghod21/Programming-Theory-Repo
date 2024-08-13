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
    float soundAdjustment = 0.6f;
    GameObject playerExpGainObject;
    public Transform playerExpGainPoint;
    float initialSpeed = 1.0f;
    float acceleration = 50.0f;

    bool goToPlayer;

    protected float experiencePlus;

    void Start()
    {
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
                experiencePlus = 5;
            } else if (prefabIdentifier.prefabName == "ExperienceMedium")
            {
                experiencePlus = 10;
            } else
            {
                experiencePlus = 15;
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
            audioSource.PlayOneShot(experienceSound, DataPersistence.soundsVolume * 0.5f * soundAdjustment);
            Destroy(gameObject);
        }
    }
}
