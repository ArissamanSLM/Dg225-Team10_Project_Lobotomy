using System;
using System.Collections.Generic;

namespace Starter;

public class PlayerManager
{
    public int Hp = 25;
    public int Sanity = 100;
    public int Defense = 0;
    public int PowerBoost = 0;
    public int DefenseBoostTurns = 0;
    public int ClickLimit = 3;
    public int HoldCard = 6;

    private readonly CardManager cardManager = new CardManager();
    public List<CardData> HandCards { get; private set; } = new List<CardData>();

    public PlayerManager()
    {
        ResetHand();
    }

    public void ResetHand()
    {
        HandCards = cardManager.DrawInitialHand(HoldCard);
        ClickLimit = 3;
    }

    public void ApplyCardEffect(CardData card)
    {
        if (card.Type == "Attack")
        {
            return;
        }
        else if (card.Type == "Defense")
        {
            GainDefense(card.Value);
            Sanity += 2;
        }
        else if (card.Type == "Heal")
        {
            Heal(card.Value);
            Sanity += 5;
        }
        else if (card.Type == "Counter")
        {
            GainDefense(3);
            Sanity += 2;
        }
        else if (card.Type == "PowerUp")
        {
            PowerBoost += card.Value;
            Sanity += 2;
        }
        else if (card.Type == "Protection")
        {
            DefenseBoostTurns = 1;
            Sanity += 2;
        }
    }

    public void GainDefense(int amount)
    {
        int blockAmount = DefenseBoostTurns > 0 ? (int)Math.Round(amount * 1.5f) : amount;
        Defense += blockAmount;

        if (DefenseBoostTurns > 0)
        {
            DefenseBoostTurns--;
        }
    }

    public void ApplyIncomingDamage(int amount)
    {
        if (Defense > 0)
        {
            int blocked = Math.Min(Defense, amount);
            Defense -= blocked;
            amount -= blocked;
        }

        if (amount > 0)
        {
            Hp -= amount;
        }
    }

    public void Heal(int amount)
    {
        Hp = Math.Min(15, Hp + amount);
    }

    public void LimitPlayerStats()
    {
        if (Hp > 15)
        {
            Hp = 15;
        }
        if (Sanity > 100)
        {
            Sanity = 100;
        }
        if (Defense < 0)
        {
            Defense = 0;
        }
    }
}
