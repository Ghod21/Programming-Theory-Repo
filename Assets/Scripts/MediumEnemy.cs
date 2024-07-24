using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediumEnemy : Enemy
{
    // Medium enemy child script.
    protected override void Start()
    {
        base.Start(); // Call the Start method from the base class
        // Additional initialization code for EasyEnemy
        enemyHealth = 5;
    }
    public override void MoveTowardsPlayer()
    {
        moveSpeed = 4f;
        base.MoveTowardsPlayer();
    }
}
