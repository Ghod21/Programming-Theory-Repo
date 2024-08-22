using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenceAura : MonoBehaviour
{
    public bool auraIsOn = true;
    SphereCollider sphereCollider;

    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        auraIsOn = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (auraIsOn)
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null && !enemy.isHardEnemy)
            {
                enemy.isUnderDefenceAura = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (auraIsOn)
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null && !enemy.isHardEnemy)
            {
                enemy.isUnderDefenceAura = false;
            }
        }
    }

    public void DisableDefenceAuraForAllEnemies()
    {
        if (sphereCollider == null)
        {
            Debug.LogError("SphereCollider component is missing.");
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereCollider.radius * 3);

        foreach (Collider hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            if (enemy != null && !enemy.isHardEnemy)
            {
                enemy.isUnderDefenceAura = false;
            }
        }
    }
}
