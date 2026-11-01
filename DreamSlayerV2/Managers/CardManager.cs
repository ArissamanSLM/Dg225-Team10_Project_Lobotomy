using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DreamSlayerV2.Core;
using DreamSlayerV2.Scenes;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace DreamSlayerV2
{
    public class CardManager
    {
        public enum CardType { Attack, Defense, Heal, Utility, Special }
        public enum CardColorType { Red, Blue, Green, Yellow, Purple }
        public enum CardRarity { Common, Uncommon, Rare, UltraRare, Special }
        public enum HazardSubtype { None, Slime, Rock, Curse, Energy }
        public Texture2D PlusSymbolTexture => GlobalContentManager.PlusCard;
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

            bool isPlusCard = Name.EndsWith("+", StringComparison.Ordinal);
            if (isPlusCard)
            {
                Name = Name[..^1];
            }

            CardReader();

            if (isPlusCard)
            {
                Name += "+";
                CardReaderPlus();
            }
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
            Description = "Combines this card and every Strike in hand into a powerful burst.";
            Does = $"Deals 6 damage for every Strike in hand plus this card, then discards them all. Cost: {Cost * 10} Sanity.";
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
            Description = "Consolidates this card and every Defend in hand into one burst of armor.";
            Does = $"Gains 6 Defense for every Defend in hand plus this card, then discards them all. Cost: {Cost * 10} Sanity.";
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
            Does = $"Deal 50 damage, lock Alpha until the encounter ends, permanently remove Omega, and leave your Sanity at 20.";
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
            Description = "Empower your attacks for the current turn.";
            Does = $"Gain +1 base damage to Attack cards until the end of this turn. Cost: {Cost * 10} Sanity.";
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
            Does = $"Add 1 Beta Card to your deck. Cost: {Cost * 10} Sanity.";
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
            Does = $"Heal 20 Sanity (1 Use per Encounter) and remove from the deck. Cost: {Cost * 0} Sanity.";
            break;

        case "Aura Heal":
            CardID = 11; // (Col 4, Row 3)
            Rarity = CardRarity.Rare;
            Description = "Channel powerful aura energy to restore body and mind.";
            Does = $"Heal 30 sanity and 15 Hp (1 Use per Encounter) and remove from the deck. Cost: {Cost * 0} Sanity.";
            break;

        case "Beta":
            CardID = 15; // (Col 4, Row 4) -> รูป Beta (β) อยู่ที่นี่!
            Rarity = CardRarity.UltraRare;
            Description = "Channel deep cosmic forces to replenish mind energy.";
            Does = $"Restore 100 Sanity, add 1 Omega Card to your deck, and permanently remove Beta. Cost: {Cost * 0} Sanity.";
            break;
        }
        }
        public void increaseInHandDamage(int amount)
        {
            InHandDamage += amount;
            Does = $"Deals {InHandDamage} damage. Cost: {Cost * 10} Sanity.";
        }
        public void CardReaderPlus()
        {
            switch (Name)
            {
                case "Strike+":
                    Rarity = CardRarity.Common;
                    Description = "Upgraded of Strike making more damage.";
                    Does = $"Deals 9 damage. Cost: {Cost * 10} Sanity.";
                    break;
                case "Defend+":
                    Rarity = CardRarity.Common;
                    Description = "Upgraded of Defend making more Defense.";
                    Does = $"Gains 10 Defense. Cost: {Cost * 10} Sanity.";
                    break;
                case "Inspection+":
                    Rarity = CardRarity.Common;
                    Description = "Upgraded of Inspection making more Draw without losing sanity.";
                    Does = $"Draw 3 Cards. Cost: {Cost * 0} Sanity.";
                    break;
                case "Piece+":
                    Description = "An upgraded strike that hits a weak point.";
                    Does = $"Deals 10 damage. Cost: {Cost * 10} Sanity.";
                    break;
                case "All in One+":
                    Description = "Combines this card and every Strike or Strike+ in hand into a powerful burst.";
                    Does = $"Deals 8 damage for every Strike or Strike+ in hand plus this card. Cost: {Cost * 10} Sanity.";
                    break;
                case "Mighty Defend Attack+":
                    Description = "Converts your Defense into an even stronger attack.";
                    Does = "Deals 120% of your current Defense. Leaves your Sanity at 35.";
                    break;
                case "No Fight+":
                    Description = "Combines this card and every Defend or Defend+ in hand into a stronger burst of armor.";
                    Does = $"Gains 10 Defense for every Defend or Defend+ in hand plus this card. Cost: {Cost * 10} Sanity.";
                    break;
                case "Mirror Shield+":
                    Description = "Doubles your existing protection and adds an upgraded barrier.";
                    Does = $"Doubles current Defense and adds 20 Defense. Cost: {Cost * 10} Sanity.";
                    break;
                case "Omega+":
                    Description = "Unleashes an upgraded cosmic strike.";
                    Does = "Deals 65 damage and leaves your Sanity at 35.";
                    break;
                case "Powerup+":
                    Description = "Empower your attacks with a bonus that fades over time.";
                    Does = $"Gain +3 base damage to Attack cards, decreasing by 1 at the end of each turn until it reaches 0. Cost: {Cost * 10} Sanity.";
                    break;
                case "Restore+":
                case "Heal+":
                    Rarity = CardRarity.Common;
                    Description = "Upgraded of Restore making more Heal.";
                    Does = $"Heal 12 HP and 10 Sanity. Cost: {Cost * 10} Sanity.";
                    break;
                case "Focus+":
                    Description = "Restore more Sanity and draw a card.";
                    Does = $"Restore 35 Sanity and draw 1 card. Cost: {Cost * 10} Sanity.";
                    break;
                case "Aura Heal+":
                    Description = "Channel powerful aura energy to restore body and mind.";
                    Does = "Restore 50 Sanity and heal 25 HP. Removes this card from the hand.";
                    break;
            }
        }
    }
}