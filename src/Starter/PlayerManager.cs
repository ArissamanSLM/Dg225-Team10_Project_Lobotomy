using System.Collections.Generic;

namespace Starter;

public class PlayerManager
{
    public int Hp = 15;
    public int Sanity = 100;
    public int ClickLimit = 6;
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
        ClickLimit = 6;
    }

    public void ApplyCardEffect(CardData card)
    {
        if (card.Type == "Attack")
        {
            return;
        }
        else if (card.Type == "Defense")
        {
            Hp += card.Value;
            Sanity += 2;
        }
        else if (card.Type == "Heal")
        {
            Hp += card.Value;
            Sanity += 5;
        }
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
    }
}
