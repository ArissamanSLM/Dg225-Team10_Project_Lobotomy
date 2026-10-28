```csharp
public override void RollNextAction(bool isSanityLow)
{
    int roll = _rng.Next(0, 100);

    if (roll < 60)
    {
        int damage = _isElite ? _rng.Next(12, 20) : _rng.Next(6, 12);
        SetNextActions(new EnemyAction(EnemyActionType.Attack, damage));
    }
    else if (roll < 85)
    {
        SetNextActions(new EnemyAction(EnemyActionType.Defend, _isElite ? 12 : 8));
    }
    else
    {
        SetNextActions(new EnemyAction(EnemyActionType.EldritchDebuff, 15));
    }
}
```
