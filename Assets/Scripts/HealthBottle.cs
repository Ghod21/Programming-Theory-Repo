using System.Collections;
using UnityEngine;

public class HealthBottle : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;
    GameObject player;
    AudioSource audioSource;
    AudioClip healthPotionSound;
    private Player playerScript;
    float soundAdjustment = 0.6f;

    private void Start()
    {
        player = GameObject.Find("Player");
        playerScript = player.GetComponent<Player>();
        audioSource = player.GetComponent<AudioSource>();
        healthPotionSound = Resources.Load<AudioClip>("Audio/HealthPotionSound1");
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerToHealthPotion"))
        {
            if (playerScript.playerHealth < 30f)
            {
                playerScript.playerHealth += 3;
                audioSource.PlayOneShot(healthPotionSound, DataPersistence.soundsVolume * 0.6f * soundAdjustment);
                if (playerScript.playerHealth > 30)
                {
                    playerScript.playerHealth = 30;
                }
                Destroy(gameObject);
            }
        }
    }
}
