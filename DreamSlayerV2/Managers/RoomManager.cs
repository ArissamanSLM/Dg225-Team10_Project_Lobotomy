using System;

namespace DreamSlayerV2
{
    public enum RoomType
    {
        Encounter,
        Elite,
        Event,
        Shop,
        Boss
    }

    public class RoomManager
    {
        private readonly Random _rng = new Random();

        public int RoomCount { get; private set; } = 0;
        public bool NightmareMode { get; set; } = false;

        // FloorDifficulty: 1 for rooms 0..12, 2 for 13..25, etc.
        public int FloorDifficulty => (RoomCount / 13) + 1;

        public RoomType CurrentRoom { get; private set; } = RoomType.Event;

        public RoomManager() { }

        // Choose the next room node and return its type. Also increments RoomCount.
        public RoomType ChooseNode()
        {
            RoomCount++;

            // Every 13th room is a Boss
            if (RoomCount % 13 == 0)
            {
                CurrentRoom = RoomType.Boss;
                return CurrentRoom;
            }

            int roll = _rng.Next(0, 101); // 0..100

            // Nightmare small forced encounter chance on early floors
            if (NightmareMode && FloorDifficulty < 5 && roll < 20)
            {
                CurrentRoom = RoomType.Encounter;
                return CurrentRoom;
            }

            // Base weights (can be tuned)
            int encounterWeight = 50;
            int eliteWeight = 10;
            int eventWeight = 20;
            int shopWeight = 20;

            // At floor 5+ increase odds for encounters and elites
            if (FloorDifficulty >= 5)
            {
                encounterWeight += 20; // more encounters
                eliteWeight += 10;     // more elites
                eventWeight = Math.Max(5, eventWeight - 15);
                shopWeight = Math.Max(5, shopWeight - 15);
            }

            int total = encounterWeight + eliteWeight + eventWeight + shopWeight;
            int pick = _rng.Next(0, total);

            if (pick < encounterWeight)
                CurrentRoom = RoomType.Encounter;
            else if (pick < encounterWeight + eliteWeight)
                CurrentRoom = RoomType.Elite;
            else if (pick < encounterWeight + eliteWeight + eventWeight)
                CurrentRoom = RoomType.Event;
            else
                CurrentRoom = RoomType.Shop;

            return CurrentRoom;
        }
    }
}
