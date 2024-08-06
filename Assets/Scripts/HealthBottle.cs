using UnityEngine;

public class HealthBottle : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] GameObject player;
    private Player playerScript;

    private void Start()
    {
        playerScript = player.GetComponent<Player>();
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
            }
            if (playerScript.playerHealth > 30)
            {
                playerScript.playerHealth = 30;
            }
            Destroy(gameObject);
        }
    }
}
