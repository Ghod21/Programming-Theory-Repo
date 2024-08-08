using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private Player playerScript;
    private float soundAdjustment;
    private float rotationSpeed = 60f;

    public void Initialize(Vector3 direction, float speed, Player playerScript, float soundAdjustment)
    {
        this.direction = direction;
        this.speed = speed;
        this.playerScript = playerScript;
        this.soundAdjustment = soundAdjustment;

        // Rotation for not round objects.
        //Quaternion targetRotation = Quaternion.LookRotation(direction);
        //transform.rotation = targetRotation * Quaternion.Euler(90f, 0f, 0f);

        Destroy(gameObject, 6f);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerToHealthPotion"))
        {

            if (!playerScript.isDashing)
            {
                if (!playerScript.isBlockingDamage)
                {
                    playerScript.playerHealth -= 2;
                    playerScript.scoreMultiplierBase -= 10;
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[5], DataPersistence.soundsVolume * 0.8f * 2 * soundAdjustment);
                }
                else
                {
                    playerScript.shieldHealth -= 2;
                    playerScript.audioSource.PlayOneShot(playerScript.audioClips[6], DataPersistence.soundsVolume * 1.2f * soundAdjustment);
                }
            }
            Destroy(gameObject);
        }
    }


}
