using Microsoft.Xna.Framework;
using System;

namespace DreamSlayerV2
{
    public class CardManager
    {
        public enum CardType { Attack, Defense, Heal, Utility }
        public enum CardColorType { Red, Blue, Green, Yellow }
        public enum HazardSubtype { None, Slime, Rock, Curse, Energy }

        public int CardID { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; } // e.g., 10 or 20 Sanity
        public CardType Type { get; set; }
        public CardColorType CardColor { get; set; }
        public HazardSubtype Hazard { get; set; }
        public bool IsUnplayable { get; set; }
        public int InHandDamage { get; set; }
        public string Description { get; set; } = "This is a Card Prototype";
        public string Does { get; set; } = "This card does something";

        public CardManager(int cardID, string name, int cost, CardType type, HazardSubtype hazard = HazardSubtype.None)
        {
            CardID = cardID;
            Name = name;
            Cost = cost;
            Type = type;
            Hazard = hazard;

            SetCardVisuals();
            ConfigureHazardRules();
            CardReader();
        }

        // Automatically assign top-left color type based on card type or role
        private void SetCardVisuals()
        {
            switch (Type)
            {
                case CardType.Attack:
                    CardColor = CardColorType.Red;
                    break;
                case CardType.Defense:
                    CardColor = CardColorType.Blue;
                    break;
                case CardType.Heal:
                    CardColor = CardColorType.Green;
                    break;
                case CardType.Utility:
                    CardColor = CardColorType.Yellow;
                    break;
            }
        }

        private void ConfigureHazardRules()
        {
            switch (Hazard)
            {
                case HazardSubtype.Slime:
                    InHandDamage = 3;
                    IsUnplayable = true;
                    break;
                case HazardSubtype.Rock:
                case HazardSubtype.Curse:
                    IsUnplayable = true;
                    break;
            }
        }

        public void CardReader()
        {
            switch (Type)
            {
                case CardType.Attack:
                    Name = "Strike";
                    InHandDamage = 10;
                    Description = "This is an attack card. It can be used to deal damage to enemies.";
                    Does = $"Deals {InHandDamage} damage. Cost: {Cost} Sanity.";
                    break;
                case CardType.Defense:
                    Name = "Defend";
                    Description = "This is a defense card. It can be used to reduce incoming damage.";
                    Does = $"Gains 6 Defense. Cost: {Cost} Sanity.";
                    break;
                case CardType.Utility:
                    Name = "Inspection";
                    Description = "This is a utility card for drawing extra options.";
                    Does = $"Draw 1 Card. Cost: {Cost} Sanity.";
                    break;
                case CardType.Heal:
                    Name = "Restore";
                    Description = "Heals over time.";
                    Does = $"Heal 10 hp. Cost: {Cost} Sanity.";
                    break;
            }
        }
    }
}