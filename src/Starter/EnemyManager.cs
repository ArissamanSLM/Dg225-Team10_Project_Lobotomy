namespace Starter;

public class EnemyManager
{
    public int EnemyHp { get; set; } = 30;
    public int BossHp { get; set; } = 120;
    public int EnemyActionCount { get; set; } = 2;
    public int BossActionCount { get; set; } = 3;

    public void MonsterAction(ref int playerHp)
    {
        int enemyDamage = 3;
        if (EnemyActionCount == 0)
        {
            playerHp -= enemyDamage;
            EnemyActionCount = 2;
        }
        else
        {
            EnemyActionCount--;
        }
    }

    public void BossAction(ref int playerHp)
    {
        int bossDamage = 5;
        if (BossActionCount == 0)
        {
            playerHp -= bossDamage;
            BossActionCount = 3;
        }
        else
        {
            BossActionCount--;
        }
    }
}
