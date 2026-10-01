using System;
using System.Collections.Generic;

namespace DreamSlayerV2
{
    public enum EnemyType
    {
        UnrealWolf,
        CursedFlower,
        UnrealWolfBoss
    }

    public class MonsterController
    {
        public EnemyType Type { get; private set; }
        public string MonsterName { get; set; }
        public int MonsterHP { get; set; }
        public int MaxHP { get; set; }
        public int MonsterBlock { get; set; }
        public int IntentDamage { get; set; }
        public string IntentType { get; set; }

        private readonly Random _rand = new Random();

        public MonsterController(EnemyType enemyType)
        {
            Type = enemyType;
            MonsterBlock = 0;

            // Configure stats based on EnemyType
            switch (Type)
            {
                case EnemyType.UnrealWolf:
                    MonsterName = "Unreal Wolf";
                    MaxHP = 30;
                    break;
                case EnemyType.CursedFlower:
                    MonsterName = "Cursed Flower";
                    MaxHP = 25;
                    break;
                case EnemyType.UnrealWolfBoss:
                    MonsterName = "Unreal Wolf (Boss)";
                    MaxHP = 120;
                    break;
            }

            MonsterHP = MaxHP;
            DetermineNextIntent();
        }

        public void TakeDamage(int damage)
        {
            int netDamage = damage - MonsterBlock;
            if (netDamage > 0)
            {
                MonsterBlock = 0;
                MonsterHP -= netDamage;
                if (MonsterHP < 0) MonsterHP = 0;
            }
            else
            {
                MonsterBlock -= damage;
            }
        }

        public void DetermineNextIntent()
        {
            switch (Type)
            {
                case EnemyType.UnrealWolf:
                    // 50% chance Bite, 50% chance Idle
                    if (_rand.Next(0, 2) == 0)
                    {
                        IntentType = "Bite";
                        IntentDamage = _rand.Next(1, 4) * 3; // 3, 6, or 9 damage
                    }
                    else
                    {
                        IntentType = "Idle";
                        IntentDamage = 0;
                    }
                    break;

                case EnemyType.CursedFlower:
                    // 50% chance Attack, 50% chance Summon Vine
                    if (_rand.Next(0, 2) == 0)
                    {
                        IntentType = "Thorn Attack";
                        IntentDamage = _rand.Next(2, 8) * 2; // 4 to 14 damage
                    }
                    else
                    {
                        IntentType = "Summon Vine";
                        IntentDamage = 0;
                    }
                    break;

                case EnemyType.UnrealWolfBoss:
                    IntentType = "Boss Slash";
                    IntentDamage = _rand.Next(2, 8) * 6; // 12 to 42 damage
                    break;
            }
        }

        public void PerformTurn(PlayerController player)
        {
            // Reset block at the start of enemy turn
            MonsterBlock = 0;

            if (IntentDamage > 0)
            {
                // Calculate damage against Player's Defense first
                int netDamage = IntentDamage - player.Defense;

                if (netDamage > 0)
                {
                    player.Defense = 0;
                    player.PlayerHP -= netDamage;
                }
                else
                {
                    player.Defense -= IntentDamage;
                }

                if (player.PlayerHP < 0) player.PlayerHP = 0;
            }

            // Special Skill: Summon Vine Card into hand
            if (IntentType == "Summon Vine")
            {
                AddVineToPlayerHand(player);
            }

            // Set up next intent for the following turn
            DetermineNextIntent();
        }

        private void AddVineToPlayerHand(PlayerController player)
        {
            // Find an open slot in hand array or replace available slot
            for (int i = 0; i < player.Hand.Length; i++)
            {
                if (player.Hand[i] == null)
                {
                    // CardID 999 as standard hazard card identifier
                    CardManager vineCard = new CardManager(
                        999,
                        "Vine",
                        1000000, // 1M Sanity Cost (Unplayable)
                        CardManager.CardType.Utility,
                        CardManager.HazardSubtype.Curse
                    )
                    {
                        InHandDamage = 3,
                        IsUnplayable = true,
                        Description = "A choking vine curse added by Cursed Flower.",
                        Does = "Unplayable. Deals 3 damage/defense decay while held in hand."
                    };

                    player.Hand[i] = vineCard;
                    break;
                }
            }
        }
    }
}