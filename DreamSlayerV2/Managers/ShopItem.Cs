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
        public string Rarity { get; set; }
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

        private void GenerateShopStock()
        {
            _shopCards.Clear();
            for (int i = 0; i < 5; i++)
            {
                double roll = _rnd.NextDouble();
                string rarity;
                int baseSellPrice;

                if (roll < 0.48)
                {
                    rarity = "Common";
                    baseSellPrice = _rnd.Next(10, 21);
                }
                else if (roll < 0.98)
                {
                    rarity = "Rare";
                    baseSellPrice = _rnd.Next(30, 61);
                }
                else
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
            return rarity switch
            {
                "Ultra Rare" => new CardManager(201 + index, "Omega Destruction", 3, CardManager.CardType.Attack),
                "Rare" => new CardManager(150 + index, "Aura Shield", 2, CardManager.CardType.Defense),
                _ => new CardManager(100 + index, "Quick Strike", 1, CardManager.CardType.Attack)
            };
        }

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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime); // เรียกใช้งานลอจิกพื้นฐาน (เช่น คลิกขวาออกจากฉาก)

            var ms = Mouse.GetState();
            bool clicked = ms.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;

            var player = GameState.CurrentPlayer;
            if (player == null) return;

            if (clicked)
            {
                // 1. ตรวจสอบคลิกซื้อการ์ด 5 ใบ
                for (int i = 0; i < _shopCards.Count; i++)
                {
                    Rectangle cardBtn = new Rectangle(200, 250 + (i * 45), 450, 35);
                    if (cardBtn.Contains(ms.Position))
                    {
                        BuyCard(player, i);
                        break;
                    }
                }

                // 2. ตรวจสอบคลิกรับ Orb (เลือกได้ฟรี 1 ชิ้น)
                if (!_orbClaimedInThisShop)
                {
                    for (int i = 0; i < _availableOrbs.Count; i++)
                    {
                        Rectangle orbBtn = new Rectangle(700, 250 + (i * 50), 400, 40);
                        if (orbBtn.Contains(ms.Position))
                        {
                            ClaimOrb(player, i);
                            break;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(_font, "--- WANDERING TRADER ---", new Vector2(200, 190), Color.Gold);

            // --- Draw Cards Stock (5 Cards) ---
            spriteBatch.DrawString(_font, "Cards for Sale:", new Vector2(200, 220), Color.Yellow);
            for (int i = 0; i < _shopCards.Count; i++)
            {
                var item = _shopCards[i];
                Color itemColor = item.IsSold ? Color.Gray : (item.Rarity == "Ultra Rare" ? Color.Purple : (item.Rarity == "Rare" ? Color.Cyan : Color.White));
                string statusText = item.IsSold ? "[SOLD]" : $"[{item.Price} Coins]";
                
                spriteBatch.DrawString(_font, $"{i + 1}. {item.Card.Name} ({item.Rarity}) {statusText}", new Vector2(200, 250 + (i * 45)), itemColor);
            }

            // --- Draw Orb Shop ---
            spriteBatch.DrawString(_font, "Free Relic Orbs (Pick 1):", new Vector2(700, 220), Color.Orange);
            for (int i = 0; i < _availableOrbs.Count; i++)
            {
                var orb = _availableOrbs[i];
                Color orbColor = orb.IsClaimed ? Color.Green : (_orbClaimedInThisShop ? Color.Gray : Color.LightBlue);
                string claimStatus = orb.IsClaimed ? "[CLAIMED]" : "";

                spriteBatch.DrawString(_font, $"{orb.Name} {claimStatus}\n  {orb.Description}", new Vector2(700, 250 + (i * 55)), orbColor);
            }

            if (_messageTimer > 0f)
            {
                spriteBatch.DrawString(_font, _interactionMessage, new Vector2(200, 520), Color.Orange);
            }

            spriteBatch.DrawString(_font, "Right-click to leave", new Vector2(200, 620), Color.AntiqueWhite);

            var player = GameState.CurrentPlayer;
            if (player != null)
            {
                spriteBatch.DrawString(_font, $"Soul Coins: {player.SoulCoins}", new Vector2(50, 50), Color.Cyan);
                spriteBatch.DrawString(_font, $"Current HP: {player.PlayerHP}/{player.MaxHP}", new Vector2(50, 80), Color.LightGreen);
            }

            spriteBatch.End();
        }

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

            int sellReward = card.Cost switch
            {
                3 => _rnd.Next(120, 201),
                2 => _rnd.Next(30, 61),
                _ => _rnd.Next(10, 21)
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