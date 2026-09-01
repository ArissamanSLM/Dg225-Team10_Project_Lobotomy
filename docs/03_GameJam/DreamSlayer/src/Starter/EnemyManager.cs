using static Microsoft.Xna.Framework.MathHelper;


namespace Starter;

public class EnemyManager
{
    public int EnemyHp { get; set; } = 50;
    public int BossHp { get; set; } = 200;
    public int EnemyActionCount { get; set; } = 1;
    public int BossActionCount { get; set; } = 2;
    
    public void MonsterAction(PlayerManager playerManager)
    {
        Random rand = new Random();
        int enemyDamage = 6 * rand.Next(1, 4); // Random damage between 6 and 18
        if (EnemyActionCount == 0)
        {
            playerManager.ApplyIncomingDamage(enemyDamage);
            EnemyActionCount = 1;
        }
        else
        {
            EnemyActionCount--;
        }
    }

    public void BossAction(PlayerManager playerManager)
    {
        Random rand = new Random();
        int bossDamage = 10 * rand.Next(1, 4); // Random damage between 10 and 30
        if (BossActionCount == 0)
        {
            playerManager.ApplyIncomingDamage(bossDamage);
            BossActionCount = 2;
        }
        else
        {
            BossActionCount--;
        }
    }
}
