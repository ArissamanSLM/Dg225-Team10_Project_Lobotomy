using System;
using System.Collections.Generic;

namespace Starter;

public class CardManager
{
    private readonly Random _random = new Random();
    public List<CardData> Deck { get; private set; } = new List<CardData>();

    public CardManager()
    {
        Deck = CardData.MakeStartingDeck();
    }

    public List<CardData> DrawInitialHand(int count)
    {
        List<CardData> hand = new List<CardData>();
        List<CardData> shuffledDeck = new List<CardData>(Deck);

        for (int i = 0; i < count && shuffledDeck.Count > 0; i++)
        {
            int index = _random.Next(shuffledDeck.Count);
            hand.Add(shuffledDeck[index]);
            shuffledDeck.RemoveAt(index);
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

    public static List<CardData> MakeStartingDeck()
    {
        List<CardData> deck = new List<CardData>();
        deck.Add(new CardData("Strike", 10, 6, "Attack"));
        deck.Add(new CardData("Strike", 10, 6, "Attack"));
        deck.Add(new CardData("Defend", 5, 4, "Defense"));
        deck.Add(new CardData("Defend", 5, 4, "Defense"));
        deck.Add(new CardData("Heal", 8, 2, "Heal"));
        deck.Add(new CardData("Counter", 30, 6, "Counter"));
        deck.Add(new CardData("Power Up", 12, 3, "PowerUp"));
        deck.Add(new CardData("Protection", 7, 4, "Protection"));
        deck.Add(new CardData("duplicate", 20, -1 , "duplicate"));
        deck.Add(new CardData("inpection", 10, 3 , "inpection"));
        deck.Add(new CardData("Forcefield", 35, 1000 , "Forcefield"));
        return deck;
    }
}
