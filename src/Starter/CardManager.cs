using System.Collections.Generic;

namespace Starter;

public class CardManager
{
    public List<CardData> Deck { get; private set; } = new List<CardData>();

    public CardManager()
    {
        Deck = CardData.MakeStartingDeck();
    }

    public List<CardData> DrawInitialHand(int count)
    {
        List<CardData> hand = new List<CardData>();
        for (int i = 0; i < count && i < Deck.Count; i++)
        {
            hand.Add(Deck[i]);
        }
        return hand;
    }
}
public class CardData 
{
    public string Name { get; set; }
    public int SanityCost { get; set; }
    public int Value { get; set; } 
    public string Type { get; set; } 

    public CardData(string name, int cost, int value, string type) 
    {
        Name = name;
        SanityCost = cost;
        Value = value;
        Type = type;
    }

    public enum CardType
    {
        Attack,
        Defense,
        Heal
    }

    // Fixed: Returns a list of cards instead of multiple unreachable returns
    public static List<CardData> MakeStartingDeck()
    {
        List<CardData> deck = new List<CardData>();
        deck.Add(new CardData("Strike", 10, 5, "Attack"));
        deck.Add(new CardData("Strike", 10, 5, "Attack"));
        deck.Add(new CardData("Defend", 5, 4, "Defense"));
        deck.Add(new CardData("Defend", 5, 4, "Defense"));
        deck.Add(new CardData("Heal", 8, 2, "Heal"));
        deck.Add(new CardData("Heal", 8, 2, "Heal"));
        return deck;
    }
}
