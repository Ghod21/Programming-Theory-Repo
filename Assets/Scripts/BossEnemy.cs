using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    // Boss enemy child script.

    public override void MoveTowardsPlayer()
    {
        moveSpeed = 3f;
        base.MoveTowardsPlayer();
    }
}
