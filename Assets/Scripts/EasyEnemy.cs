public class EasyEnemy : Enemy
{
    // Easy enemy child script.

    public override void MoveTowardsPlayer()
    {
        moveSpeed = 4f;
        base.MoveTowardsPlayer();
    }
}
