using System.Collections;

public class EasyEnemy : Enemy
{
    // Easy enemy child script.
    protected override void Start()
    {
        base.Start(); // Call the Start method from the base class
        // Additional initialization code for EasyEnemy
        enemyHealth = 3;
    }
    public override void MoveTowardsPlayer()
    {
        moveSpeed = 5f;
        base.MoveTowardsPlayer();
    }
    protected override IEnumerator deathAnimation()
    {
        DataPersistence.currentPlayerScore += 5 * playerScript.scoreMultiplier;
        playerScript.scoreMultiplierBase++;
        return base.deathAnimation();
    }
}
