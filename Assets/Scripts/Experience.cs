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
    [SerializeField] private GameObject currentObject;

    protected float experiencePlus;

    void Start()
    {
        player = GameObject.Find("Player");
        playerScript = player.GetComponent<Player>();
        audioSource = player.GetComponent<AudioSource>();
        experienceSound = Resources.Load<AudioClip>("Audio/ExperienceSound");
        currentObject = this.gameObject;
        CheckCurrentObject();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

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
            playerScript.playerExperience += experiencePlus;
            audioSource.PlayOneShot(experienceSound, DataPersistence.soundsVolume * 0.5f * soundAdjustment);
            Destroy(gameObject);
        }
    }
}
