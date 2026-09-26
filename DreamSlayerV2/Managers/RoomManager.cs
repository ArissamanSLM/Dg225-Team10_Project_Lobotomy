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

        // Toggle this to true for your Friday Demo version (Nodes 1-7)
        public bool IsDemoMode { get; set; } = true;

        public int FloorDifficulty => (RoomCount / 13) + 1;
        public RoomType CurrentRoom { get; private set; } = RoomType.Event;

        public RoomManager() { }

        // Choose the next room node and return its type. Also increments RoomCount.
        public RoomType ChooseNode()
        {
            RoomCount++;

            // --- DEMO MODE (Strict Node 1 to 7 Structure) ---
            if (IsDemoMode)
            {
                if (RoomCount == 7)
                {
                    CurrentRoom = RoomType.Elite; // Node 7 is a forced Elite Encounter!
                    return CurrentRoom;
                }
                else if (RoomCount > 7)
                {
                    CurrentRoom = RoomType.Boss; // End of Demo / Boss transition
                    return CurrentRoom;
                }

                // For nodes 1 to 6 in the demo, pick randomly or via your event pool distribution 
                // (1 Lucky, 3 Normal, 1 Nightmare, etc.)
                int demoRoll = _rng.Next(0, 3);
                switch (demoRoll)
                {
                    case 0: CurrentRoom = RoomType.Event; break;
                    case 1: CurrentRoom = RoomType.Encounter; break;
                    case 2: CurrentRoom = RoomType.Shop; break;
                }
                return CurrentRoom;
            }

            // --- FULL VERSION MODE (Original 13-Room Cycle) ---
            if (RoomCount % 13 == 0)
            {
                CurrentRoom = RoomType.Boss;
                return CurrentRoom;
            }

            int roll = _rng.Next(0, 101);

            if (NightmareMode && FloorDifficulty < 5 && roll < 20)
            {
                CurrentRoom = RoomType.Encounter;
                return CurrentRoom;
            }

            int encounterWeight = 40;
            int eliteWeight = 5;
            int eventWeight = 40;
            int shopWeight = 15;

            if (FloorDifficulty >= 5)
            {
                encounterWeight += 20;
                eliteWeight += 10;
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