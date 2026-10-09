using Microsoft.Xna.Framework;
using System;

namespace DreamSlayerV2
{
    public class CardManager
    {
        public enum CardType { Attack, Defense, Heal, Utility, Special }
        public enum CardColorType { Red, Blue, Green, Yellow, Purple }
        public enum CardRarity { Common, Uncommon, Rare, UltraRare, Special }
        public enum HazardSubtype { None, Slime, Rock, Curse, Energy }

        public int CardID { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; } // Sanity cost
        public CardType Type { get; set; }
        public CardColorType CardColor { get; set; }
        public CardRarity Rarity { get; set; } = CardRarity.Common;
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

        // Automatically assign card frame color based on type
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
                case CardType.Special:
                    CardColor = CardColorType.Purple;
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
                    switch (Name)
                    {
                        case "Strike":
                            Rarity = CardRarity.Common;
                            InHandDamage = 6;
                            Description = "Basic attack standard strike.";
                            Does = $"Deals {InHandDamage} damage. Cost: {Cost} Sanity.";
                            break;

                        case "Piece":
                            Rarity = CardRarity.Uncommon;
                            InHandDamage = 8;
                            Description = "Target an enemy to expose weak points.";
                            Does = $"Deals {InHandDamage} damage and applies 1 Valuable (+50% DMG taken). Cost: {Cost} Sanity.";
                            break;

                        case "All in One":
                            Rarity = CardRarity.Rare;
                            Description = "Combines every Strike card in hand into a powerful burst.";
                            Does = $"Deals 6 damage for every Strike in hand, then discards all Strikes. Cost: {Cost} Sanity.";
                            break;

                        case "Mighty Defend Attack":
                            Rarity = CardRarity.UltraRare;
                            Description = "Converts your remaining Defense into offensive force.";
                            Does = $"Deals damage equal to your current Shield. Sets your Sanity to 0.";
                            break;
                    }
                    break;

                case CardType.Defense:
                    switch (Name)
                    {
                        case "Defend":
                            Rarity = CardRarity.Common;
                            Description = "Standard defensive stance.";
                            Does = $"Gains 6 Defense. Cost: {Cost} Sanity.";
                            break;

                        case "No Fight":
                            Rarity = CardRarity.Rare;
                            Description = "Consolidates all defensive maneuvers into one burst of armor.";
                            Does = $"Gains 6 Defense for every Defend card in hand, then discards all Defends. Cost: {Cost} Sanity.";
                            break;

                        case "Mirror Shield":
                            Rarity = CardRarity.UltraRare;
                            Description = "Doubles existing protection and adds a heavy bonus barrier.";
                            Does = $"Doubles current Defense and adds +10 Defense. Cost: {Cost} Sanity.";
                            break;
                    }
                    break;

                case CardType.Utility:
                    switch (Name)
                    {
                        case "Inspection":
                            Rarity = CardRarity.Common;
                            Description = "Analyze the battle to draw new tactical options.";
                            Does = $"Draw 1 Card. Cost: {Cost} Sanity.";
                            break;

                        case "Powerup":
                            Rarity = CardRarity.Rare;
                            Description = "Empower all attack techniques for the rest of the encounter.";
                            Does = $"Gain +1 Base Damage to all Attack cards permanently this battle. Cost: {Cost} Sanity.";
                            break;

                        case "Clock":
                            Rarity = CardRarity.UltraRare;
                            Description = "Store accumulated energy and unleash a multiplied strike.";
                            Does = $"Stores turn damage and releases with 1.2x extra damage at turn end. Cost: {Cost} Sanity.";
                            break;
                    }
                    break;

                case CardType.Heal:
                    switch (Name)
                    {
                        case "Restore":
                            Rarity = CardRarity.Uncommon;
                            Description = "Apply continuous mental recovery.";
                            Does = $"Heals 4 HP per turn for 2 turns. Cost: {Cost} Sanity.";
                            break;

                        case "Focus":
                            Rarity = CardRarity.UltraRare;
                            Description = "Clear your mind and regain focused Sanity.";
                            Does = $"Restore 20 Sanity. Exhausts (1 use per encounter). Cost: {Cost} Sanity.";
                            break;

                        case "Aura Heal":
                            Rarity = CardRarity.UltraRare;
                            Description = "Channel powerful aura energy to restore body and mind.";
                            Does = $"Restore 30 Sanity and 15 HP. Exhausts (1 use per encounter). Cost: {Cost} Sanity.";
                            break;
                    }
                    break;

                case CardType.Special:
                    switch (Name)
                    {

                        case "Alpha":
                            Rarity = CardRarity.Rare;
                            Description = "The first step toward ancient cosmic knowledge.";
                            Does = $"Add 1 Beta card to hand. Exhausts. Cost: {Cost} Sanity.";
                            break;

                        case "Beta":
                            Rarity = CardRarity.UltraRare;
                            Description = "Channel deep cosmic forces to replenish mind energy.";
                            Does = $"Restore 100 Sanity and add 1 Omega card to hand. Exhausts. Cost: {Cost} Sanity.";
                            break;

                        case "Omega":
                            Rarity = CardRarity.Special;
                            Description = "Unleash the ultimate cosmic strike.";
                            Does = $"Deal 50 Damage to ALL enemies and apply 3 Valuable. Exhausts. Cost: {Cost} Sanity.";
                            break;
                    }
                    break;
            }
        }
    }
}