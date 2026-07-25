using System;

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

    public void Shoproom(ref int coinSoul, ref int hp, ref int sanity)
    {
        if (coinSoul >= 20)
        {
            coinSoul -= 20;
            hp += 10;
            sanity += 20;
        }
        else if (coinSoul >= 10)
        {
            coinSoul -= 10;
            hp += 5;
            sanity += 10;
        }
    }
}
