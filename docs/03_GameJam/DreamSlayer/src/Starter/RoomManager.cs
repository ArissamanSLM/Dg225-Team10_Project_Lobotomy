using System;
using System.Collections.Generic;

namespace Starter;

public class RoomManager
{
    private int roomCount = 0;
    private int recentEncounters = 0;
    private int recentEvents = 0;

    private static readonly Random rand = new Random();

    public string GetNextBrownDoorType()
    {
        return "Encounter";
    }

    public string GetNextYellowDoorType()
    {
        return "Event";
    }

    public void RegisterRoomCompletion(string chosenType)
    {
        roomCount++;

        if (chosenType == "Encounter")
        {
            recentEncounters++;
        }
        else if (chosenType == "Event")
        {
            recentEvents++;
        }
    }

    public bool IsBossRoomReady()
    {
        return roomCount >= 5;
    }

    public void ResetRun()
    {
        roomCount = 0;
        recentEncounters = 0;
        recentEvents = 0;
    }

    public void EventRoom(ref int hp, ref int sanity, ref bool shopEvent, EventType eventType)
    {
        int roll = rand.Next(0, 100);
        if (roll < 50)
        {
            hp -= 5;
            sanity -= 10;
        }
        else if (roll > 50 && roll < 80)
        {
            hp += 10;
            sanity += 20;
        }
        else
        {
            shopEvent = true;
        }
    }

    public bool TryBuyItem(int choice, ref int coinSoul, ref int hp, ref int sanity, List<CardData> deck)
    {
        // Choice 1: Buy Card (Cost: 50 coins)
        if (choice == 1 && coinSoul >= 50)
        {
            coinSoul -= 50;
            deck.Add(new CardData("ShopCard", 12, 5, "Attack"));
            return true;
        }
        // Choice 2: Big Heal & Sanity (Cost: 20 coins)
        else if (choice == 2 && coinSoul >= 20)
        {
            coinSoul -= 20;
            hp += 10;
            sanity += 20;
            return true;
        }
        // Choice 3: Small Heal & Sanity (Cost: 10 coins)
        else if (choice == 3 && coinSoul >= 10)
        {
            coinSoul -= 10;
            hp += 5;
            sanity += 10;
            return true;
        }
        return false;
    }
    
}
