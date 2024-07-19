using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediumEnemy : Enemy
{
    // Medium enemy child script.

    public override void MoveTowardsPlayer()
    {
        moveSpeed = 3.5f;
        base.MoveTowardsPlayer();
    }
}
