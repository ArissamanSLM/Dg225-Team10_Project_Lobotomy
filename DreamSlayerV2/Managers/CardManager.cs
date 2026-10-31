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
        public int Cost { get; set; } // Orb amount; each Orb costs 10 Sanity.
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
    // กำหนด CardID ตามพิกัด (Col, Row) บน Sprite Sheet 4x4 
    // สูตรคำนวณตำแหน่ง: CardID = (Row - 1) * 4 + (Col - 1)
    switch (Name)
    {
        // --- COLUMN 1 ---
        case "Strike":
            CardID = 0; // (Col 1, Row 1)
            Rarity = CardRarity.Common;
            InHandDamage = 6;
            Description = "Basic attack standard strike.";
            Does = $"Deals {InHandDamage} damage. Cost: {Cost * 10} Sanity.";
            break;

        case "All in One":
            CardID = 4; // (Col 1, Row 2)
            Rarity = CardRarity.Rare;
            Description = "Combines every Strike card in hand into a powerful burst.";
            Does = $"Deals 6 damage for every Strike in hand, then discards all Strikes. Cost: {Cost * 10} Sanity.";
            break;

        case "Mighty Defend Attack":
            CardID = 8; // (Col 1, Row 3)
            Rarity = CardRarity.UltraRare;
            Description = "Converts your remaining Defense into offensive force.";
            Does = $"Deals damage equal to your current Shield. Leaves your Sanity at 20.";
            break;

        case "Piece":
            CardID = 12; // (Col 1, Row 4)
            Rarity = CardRarity.Rare;
            Description = "Target an enemy to expose weak points.";
            Does = $"Deals 8 damage and applies 1 Valuable (+50% DMG taken). Cost: {Cost * 10} Sanity.";
            break;

        // --- COLUMN 2 ---
        case "Defend":
            CardID = 1; // (Col 2, Row 1)
            Rarity = CardRarity.Common;
            Description = "Standard defensive stance.";
            Does = $"Gains 6 Defense. Cost: {Cost * 10} Sanity.";
            break;

        case "No Fight":
            CardID = 5; // (Col 2, Row 2)
            Rarity = CardRarity.Rare;
            Description = "Consolidates all defensive maneuvers into one burst of armor.";
            Does = $"Gains 6 Defense for every Defend card in hand, then discards all Defends. Cost: {Cost * 10} Sanity.";
            break;

        case "Mirror Shield":
            CardID = 9; // (Col 2, Row 3)
            Rarity = CardRarity.UltraRare;
            Description = "Doubles existing protection and adds a heavy bonus barrier.";
            Does = $"Doubles current Defense and adds +10 Defense. Cost: {Cost * 10} Sanity.";
            break;

        case "Omega":
            CardID = 13; // (Col 2, Row 4) -> รูป Omega (Ω) อยู่ที่นี่!
            Rarity = CardRarity.UltraRare;
            Description = "Unleash the ultimate cosmic strike.";
            Does = $"Deal 50 Damage to Enemy and Next Card do double Effects. Removes card. Leaves your Sanity at 20.";
            break;

        // --- COLUMN 3 ---
        case "Inspection":
            CardID = 2; // (Col 3, Row 1)
            Rarity = CardRarity.Common;
            Description = "Analyze the battle to draw new tactical options.";
            Does = $"Draw 1 Card. Cost: {Cost * 10} Sanity.";
            break;

        case "Powerup":
            CardID = 6; // (Col 3, Row 2)
            Rarity = CardRarity.Rare;
            Description = "Empower all attack techniques for the rest of the encounter.";
            Does = $"Gain +1 Base Damage to all Attack cards permanently this battle. Cost: {Cost * 10} Sanity.";
            break;

        case "Clock":
            CardID = 10; // (Col 3, Row 3)
            Rarity = CardRarity.Rare;
            Description = "Store accumulated energy and unleash a multiplied strike.";
            Does = $"Stores turn damage and releases with 1.2x extra damage at turn end. Cost: {Cost * 10} Sanity.";
            break;

        case "Alpha":
            CardID = 14; // (Col 3, Row 4) -> รูป Alpha (α) อยู่ที่นี่!
            Rarity = CardRarity.Rare;
            Description = "The first step toward ancient cosmic knowledge.";
            Does = $"Gain 1 Beta Card and remove from the deck. Cost: {Cost * 10} Sanity.";
            break;

        // --- COLUMN 4 ---
        case "Restore":
            CardID = 3; // (Col 4, Row 1)
            Rarity = CardRarity.Common;
            Description = "Apply continuous mental recovery.";
            
            break;

        case "Heal":
            CardID = 3; // (Col 4, Row 1)
            Rarity = CardRarity.Common;
            Description = "Comman healing magic in this Star";
            Does = $"Heal 6 Hp to yourself. Cost: {Cost * 10} Sanity.";
            break;

        case "Focus":
            CardID = 7; // (Col 4, Row 2)
            Rarity = CardRarity.UltraRare;
            Description = "Clear your mind and regain focused Sanity.";
            Does = $"Heal 20 Sanity (1 Use per Encounter) and remove from the deck. Cost: {Cost * 10} Sanity.";
            break;

        case "Aura Heal":
            CardID = 11; // (Col 4, Row 3)
            Rarity = CardRarity.Rare;
            Description = "Channel powerful aura energy to restore body and mind.";
            Does = $"Heal 30 sanity and 15 Hp (1 Use per Encounter) and remove from the deck. Cost: {Cost * 10} Sanity.";
            break;

        case "Beta":
            CardID = 15; // (Col 4, Row 4) -> รูป Beta (β) อยู่ที่นี่!
            Rarity = CardRarity.UltraRare;
            Description = "Channel deep cosmic forces to replenish mind energy.";
            Does = $"Restore 100 Sanity And gain 1 Omega Card And Remove Beta Card from the deck. Cost: {Cost * 10} Sanity.";
            break;
    }
}
    }
}