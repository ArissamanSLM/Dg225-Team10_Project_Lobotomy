using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;



namespace DreamSlayerV2.Core
{
    public static class UpgradeOrbPermanent
    {
        public static int RebirthCoins { get; set; } = 0;

        // Upgrade Levels (Total Orbs Level)
        public static int MaxHPLevel { get; set; } = 0;      // Max 12 (Star 3, Orb 3)
        public static int StartCoinsLevel { get; set; } = 0; // Max 9  (Star 2, Orb 3)

        // Features Toggles & Unlocks
        public static bool UnlockDiceMinigame { get; set; } = false; // Cost: 300 Coins
        public static bool EnergyAltarUnlocked { get; set; } = false; // Cost: 500 Coins
        public static DateTime LastAltarUse { get; set; } = DateTime.MinValue;

        // Special Charms Challenge & Nightmare Mode
        public static bool CharmsUnlocked { get; set; } = false; // Cost: 5,000 Coins (Super Expensive!)
        public static bool NightmareModeUnlocked { get; set; } = false; // Cost: 1,000 Coins (Req Depths 10)

        // Calculate Cost based on level
        public static int GetUpgradeCost(int currentLevel)
        {
            return (int)(50 * Math.Pow(1.5, currentLevel));
        }

        // Convert Level to Display String (e.g., Level 5 -> "Star 1, Orb 2")
        public static string GetStarOrbText(int level)
        {
            if (level <= 0) return "Empty (0/0)";
            int star = (level - 1) / 3;
            int orb = ((level - 1) % 3) + 1;
            return $"Star {star}, Orb {orb}";
        }

        // --- Bonus Calculation Methods ---
        public static int GetMaxHPBonus() => MaxHPLevel; // +1 HP per Orb
        public static int GetStartingCoinsBonus() => StartCoinsLevel * 10; // +10 Coins per Orb

        // Altar Cooldown check (24 Hours)
        public static bool CanUseEnergyAltar()
        {
            return EnergyAltarUnlocked && (DateTime.Now - LastAltarUse).TotalHours >= 24;
        }

        public static void UseEnergyAltar(PlayerController player)
        {
            if (CanUseEnergyAltar() && player != null)
            {
                player.MaxHP += 10;
                player.PlayerHP += 10;
                player.Sanity = Math.Min(100, player.Sanity + 20);
                LastAltarUse = DateTime.Now;
            }
        }
    }
}