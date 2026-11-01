using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DreamSlayerV2.Scenes
{
    public class ShopItem
    {
        public CardManager Card { get; set; }
        public int Price { get; set; }
        public bool IsSold { get; set; }
        public string Rarity { get; set; } // Common, Rare, Ultra Rare
    }

    public class OrbItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsClaimed { get; set; }
        public Action<PlayerController> ApplyEffect { get; set; }
    }

    public class Shop : ShopRestScene
    {
        private List<ShopItem> _shopCards = new List<ShopItem>();
        private List<OrbItem> _availableOrbs = new List<OrbItem>();
        private List<CardManager> _selectedForSacrifice = new List<CardManager>();
        
        private Random _rnd = new Random();
        private bool _orbClaimedInThisShop = false;

        public Shop() : base(false, StructureType.SafeHouse) 
        {
            GenerateShopStock();
            GenerateOrbStock();
        }

        // --- 1. STOCK GENERATION (สุ่มการ์ด 5 ใบ 48%/50%/2%) ---
        private void GenerateShopStock()
        {
            _shopCards.Clear();
            for (int i = 0; i < 5; i++)
            {
                double roll = _rnd.NextDouble();
                string rarity;
                int baseSellPrice;

                if (roll < 0.48) // 48% Common
                {
                    rarity = "Common";
                    baseSellPrice = _rnd.Next(10, 21);
                }
                else if (roll < 0.98) // 50% Rare
                {
                    rarity = "Rare";
                    baseSellPrice = _rnd.Next(30, 61);
                }
                else // 2% Ultra Rare
                {
                    rarity = "Ultra Rare";
                    baseSellPrice = _rnd.Next(120, 201);
                }

                int buyPrice = baseSellPrice * 2;
                CardManager card = GenerateCardByRarity(rarity, i);

                _shopCards.Add(new ShopItem
                {
                    Card = card,
                    Price = buyPrice,
                    IsSold = false,
                    Rarity = rarity
                });
            }
        }

        private CardManager GenerateCardByRarity(string rarity, int index)
        {
            // ตัวอย่างการสุ่มสร้างการ์ดตาม Rarity
            return rarity switch
            {
                "Ultra Rare" => new CardManager(201 + index, "Omega Destruction", 3, CardManager.CardType.Attack),
                "Rare" => new CardManager(150 + index, "Aura Shield", 2, CardManager.CardType.Defense),
                _ => new CardManager(100 + index, "Quick Strike", 1, CardManager.CardType.Attack)
            };
        }

        // --- 2. ORB GENERATION ---
        private void GenerateOrbStock()
        {
            _availableOrbs = new List<OrbItem>
            {
                new OrbItem { 
                    Name = "Orb of Midas", 
                    Description = "+50% Soul Coins Gain | -5% Max HP", 
                    ApplyEffect = (p) => { p.MaxHP = (int)(p.MaxHP * 0.95f); } 
                },
                
                
                new OrbItem { 
                    Name = "Orb of Preservation", 
                    Description = "+5 Free Defense/Turn | -1 Attack Damage", 
                    ApplyEffect = (p) => { p.Defense += 5; } 
                }
            };
        }

        // --- 3. SACRIFICE SYSTEM (3 Cards -> 1 Higher Rarity) ---
        public void SacrificeCards(PlayerController player)
        {
            if (_selectedForSacrifice.Count != 3) return;

            // ตรวจสอบว่าเป็น Rarity เดียวกันทั้ง 3 ใบหรือไม่
            string type1 = _selectedForSacrifice[0].Name; 
            
            // ลบการ์ด 3 ใบออกจาก Deck ผู้เล่น
            foreach (var card in _selectedForSacrifice)
            {
                player.Deck.Remove(card);
            }

            // สุ่มให้การ์ดใหม่ระดับสูงขึ้น (Common -> Rare, Rare -> UR)
            CardManager upgradedCard = new CardManager(300, "Sacred Awakening", 2, CardManager.CardType.Special);
            player.Deck.Add(upgradedCard);

            _selectedForSacrifice.Clear();
            _interactionMessage = "Sacrificed 3 Cards to synthesize a New High-Tier Card!";
            _messageTimer = 3.0f;
        }

        // --- 4. BUY & SELL LOGIC ---
        public void BuyCard(PlayerController player, int index)
        {
            if (index < 0 || index >= _shopCards.Count) return;
            var item = _shopCards[index];

            if (!item.IsSold && player.SoulCoins >= item.Price)
            {
                player.SoulCoins -= item.Price;
                item.IsSold = true;
                player.Deck.Add(item.Card);
                _interactionMessage = $"Bought {item.Card.Name} ({item.Rarity})!";
                _messageTimer = 2.5f;
            }
        }

        public void SellCard(PlayerController player, CardManager card)
        {
            if (!player.Deck.Contains(card)) return;

            // คำนวณราคาขายตาม Rarity
            int sellReward = card.Cost switch
            {
                3 => _rnd.Next(120, 201), // UR
                2 => _rnd.Next(30, 61),   // Rare
                _ => _rnd.Next(10, 21)    // Common
            };

            player.SoulCoins += sellReward;
            player.Deck.Remove(card);
            _interactionMessage = $"Sold {card.Name} for {sellReward} Soul Coins.";
            _messageTimer = 2.5f;
        }

        public void ClaimOrb(PlayerController player, int orbIndex)
        {
            if (_orbClaimedInThisShop || orbIndex < 0 || orbIndex >= _availableOrbs.Count) return;

            var orb = _availableOrbs[orbIndex];
            orb.ApplyEffect(player);
            orb.IsClaimed = true;
            _orbClaimedInThisShop = true;
            _interactionMessage = $"Claimed {orb.Name}!";
            _messageTimer = 3.0f;
        }
    }
}