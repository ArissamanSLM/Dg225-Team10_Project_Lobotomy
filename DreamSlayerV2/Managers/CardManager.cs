using System;

namespace DreamSlayerV2
{
    public class CardManager
    {
        public enum CardType { Attack, Defense, Utility, Status }
        public enum HazardSubtype { None, Slime, Rock, Curse, Energy }

        public int CardID { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public CardType Type { get; set; }
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

            ConfigureHazardRules();
            CardReader();
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

        public void AddHazard(HazardSubtype hazard)
        {
            Hazard = hazard;
            ConfigureHazardRules();
        }

        public void CardReader()
        {
            switch (Type)
            {
                case CardType.Attack:
                    Name = "Strike";
                    Description = "This is an attack card. It can be used to deal damage to enemies.";
                    Does = "Deals " + InHandDamage + " damage.";
                    break;
                case CardType.Defense:
                    Name = "Defend";
                    Description = "This is a defense card. It can be used to reduce damage taken from enemies.";
                    Does = "Reduces incoming damage by 5.";
                    break;
                case CardType.Utility:
                    Name = "Utility Card";
                    Description = "This is a utility card. It can be used for various supportive actions.";
                    Does = "Performs utility action.";
                    break;
                case CardType.Status:
                    if (Hazard == HazardSubtype.Slime)
                    {
                        Name = "Slime";
                        Description = "A sticky slime hazard obstructing your hand.";
                        Does = $"Deals {InHandDamage} damage in hand.";
                    }
                    else
                    {
                        Name = "Status Card";
                        Description = "This is a status card. It can be used to apply or remove status effects.";
                        Does = "Applies status effect.";
                    }
                    break;
            }
        }
    }
}