using System.Collections;
using UnityEngine;

public class FireArea : MonoBehaviour
{
    bool isFlaming = true;
    bool isDamageCoroutineRunning = false;
    ParticleSystem fireParticleSystem;
    SphereCollider sphereCollider;
    Coroutine damageCoroutine;

    void Awake()
    {
        fireParticleSystem = GetComponent<ParticleSystem>();
        sphereCollider = GetComponent<SphereCollider>();
        StartCoroutine(DeactivateFireArea());
    }

    void OnTriggerStay(Collider other)
    {
        if (isFlaming && other.CompareTag("Player"))
        {
            Player playerScript = other.GetComponent<Player>();

            if (playerScript != null)
            {
                if (IsInsideSphereCollider(other))
                {
                    Debug.Log("Player detected inside SphereCollider.");

                    if (!isDamageCoroutineRunning)
                    {
                        damageCoroutine = StartCoroutine(DamagePlayer(playerScript));
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerToHealthPotion"))
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                isDamageCoroutineRunning = false;
            }
        }
    }

    private bool IsInsideSphereCollider(Collider other)
    {
        return sphereCollider.bounds.Contains(other.transform.position);
    }

    IEnumerator DamagePlayer(Player playerScript)
    {
        isDamageCoroutineRunning = true;

        while (isFlaming)
        {
            if (isFlaming)
            {
                Debug.Log("Applying damage to player.");
                playerScript.playerHealth--;
                playerScript.audioSource.PlayOneShot(playerScript.audioClips[14], DataPersistence.soundsVolume * 2f * DataPersistence.soundAdjustment);
                yield return new WaitForSeconds(1f);
            }
        }

        isDamageCoroutineRunning = false;
    }

    IEnumerator DeactivateFireArea()
    {

        yield return new WaitForSeconds(4f);

        var main = fireParticleSystem.main;
        main.loop = false;

        yield return new WaitForSeconds(3f);
        isFlaming = false;
        yield return new WaitForSeconds(2f);
        
        Destroy(gameObject);
    }
}
