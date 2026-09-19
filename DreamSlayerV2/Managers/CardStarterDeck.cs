using System.Collections.Generic;

namespace DreamSlayerV2
{
    public static class CardStarterDeck
    {
        public static List<CardManager> GetStarterDeck(CharacterClassV2 characterClass)
        {
            List<CardManager> starterDeck = new List<CardManager>();

            switch (characterClass)
            {
                case CharacterClassV2.Human:
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 1, "Strike", 1, CardManager.CardType.Attack));
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 4, "Defend", 1, CardManager.CardType.Defense));
                    starterDeck.Add(new CardManager(7, "Counter", 1, CardManager.CardType.Defense));
                    starterDeck.Add(new CardManager(8, "Powerup", 1, CardManager.CardType.Utility));
                    break;

                case CharacterClassV2.ShadowBind:
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 1, "Strike", 1, CardManager.CardType.Attack));
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 4, "Defend", 1, CardManager.CardType.Defense));
                    starterDeck.Add(new CardManager(7, "Shadow Step", 1, CardManager.CardType.Utility));
                    starterDeck.Add(new CardManager(8, "Shadow Clone", 2, CardManager.CardType.Utility));
                    starterDeck.Add(new CardManager(9, "Darkness Blow", 3, CardManager.CardType.Attack));
                    break;

                case CharacterClassV2.PactBinder:
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 1, "Strike", 1, CardManager.CardType.Attack));
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 4, "Defend", 1, CardManager.CardType.Defense));
                    starterDeck.Add(new CardManager(7, "Pact of Peace", 1, CardManager.CardType.Utility));
                    starterDeck.Add(new CardManager(8, "Pact of Pain", 1, CardManager.CardType.Utility));
                    starterDeck.Add(new CardManager(9, "Pact of Team", 1, CardManager.CardType.Utility));
                    starterDeck.Add(new CardManager(10, "No Laws", 2, CardManager.CardType.Utility));
                    break;

                case CharacterClassV2.TheOrbMaster:
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 1, "Strike", 1, CardManager.CardType.Attack));
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 4, "Defend", 1, CardManager.CardType.Defense));
                    starterDeck.Add(new CardManager(7, "Orb of Refresh", 1, CardManager.CardType.Utility));
                    starterDeck.Add(new CardManager(8, "Orb of Lighting", 1, CardManager.CardType.Attack));
                    starterDeck.Add(new CardManager(9, "Orb of Bubble Shield", 1, CardManager.CardType.Defense));
                    starterDeck.Add(new CardManager(10, "Orb of Power", 1, CardManager.CardType.Utility));
                    break;

                case CharacterClassV2.DreamSlayer:
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 1, "Strike", 1, CardManager.CardType.Attack));
                    for (int i = 0; i < 3; i++) starterDeck.Add(new CardManager(i + 4, "Defend", 1, CardManager.CardType.Defense));
                    starterDeck.Add(new CardManager(7, "Final Defeat Honor", 0, CardManager.CardType.Attack));
                    starterDeck.Add(new CardManager(8, "Honor Bound", 1, CardManager.CardType.Utility));
                    break;

                case CharacterClassV2.TheMixer:
                    starterDeck.Add(new CardManager(1, "Strike", 1, CardManager.CardType.Attack));
                    starterDeck.Add(new CardManager(2, "Defend", 1, CardManager.CardType.Defense));
                    starterDeck.Add(new CardManager(3, "Pact Mix", 1, CardManager.CardType.Utility));
                    starterDeck.Add(new CardManager(4, "Shadow Mix", 1, CardManager.CardType.Utility));
                    starterDeck.Add(new CardManager(5, "Orb Mix", 1, CardManager.CardType.Utility));
                    break;
            }

            return starterDeck;
        }
    }
}