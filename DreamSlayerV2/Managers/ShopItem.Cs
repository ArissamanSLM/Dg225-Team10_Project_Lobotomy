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

        private Rectangle GetCardButtonBounds(int index) => new Rectangle(100, 290 + (index * 96), 850, 76);
        private Rectangle GetOrbButtonBounds(int index) => new Rectangle(1030, 310 + (index * 190), 800, 150);

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
                "Ultra Rare" => new CardManager(201 + index, "Mighty Defend Attack", 3, CardManager.CardType.Attack),
                "Rare" => new CardManager(150 + index, "Aura Shield", 2, CardManager.CardType.Defense),
                _ => new CardManager(100 + index, "Strike", 1, CardManager.CardType.Attack)
            };
        }

        private void GenerateOrbStock()
        {
            _availableOrbs = new List<OrbItem>
            {
                new OrbItem { 
                    Name = "Orb of Midas", 
                    Description = "+50 Soul Coins | -5% Max HP",
                    ApplyEffect = (p) => {
                        p.SoulCoins += 50;
                        p.MaxHP = Math.Max(1, (int)(p.MaxHP * 0.95f));
                        p.PlayerHP = Math.Min(p.PlayerHP, p.MaxHP);
                    }
                },
                
                new OrbItem { 
                    Name = "Orb of Preservation", 
                    Description = "+5 Defense",
                    ApplyEffect = (p) => { p.Defense += 5; } 
                }
            };
        }

        public override void Update(GameTime gameTime)
        {
            MouseState previousMouseState = _prevMouse;
            base.Update(gameTime);

            var ms = Mouse.GetState();
            bool clicked = ms.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;

            var player = GameState.CurrentPlayer;
            if (player == null) return;

            if (clicked)
            {
                // 1. ตรวจสอบคลิกซื้อการ์ด 5 ใบ
                for (int i = 0; i < _shopCards.Count; i++)
                {
                    Rectangle cardBtn = GetCardButtonBounds(i);
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
                        Rectangle orbBtn = GetOrbButtonBounds(i);
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
            SceneUi.DrawBackground(spriteBatch);
            SceneUi.DrawPanel(spriteBatch, new Rectangle(60, 160, 930, 675), Color.Gold);
            SceneUi.DrawPanel(spriteBatch, new Rectangle(1010, 160, 850, 675), Color.Cyan);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "WANDERING TRADER", new Vector2(100, 190), Color.Gold);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Cards for Sale", new Vector2(100, 245), Color.Yellow);

            MouseState mouse = Mouse.GetState();
            for (int i = 0; i < _shopCards.Count; i++)
            {
                var item = _shopCards[i];
                string statusText = item.IsSold
                    ? $"{i + 1}. {item.Card.Name} ({item.Rarity}) - SOLD"
                    : $"{i + 1}. {item.Card.Name} ({item.Rarity}) - {item.Price} Coins";
                SceneUi.DrawButton(spriteBatch, GetCardButtonBounds(i), statusText, GetCardButtonBounds(i).Contains(mouse.Position), !item.IsSold);
            }

            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Free Relic Orbs - Choose One", new Vector2(1030, 245), Color.Orange);
            for (int i = 0; i < _availableOrbs.Count; i++)
            {
                var orb = _availableOrbs[i];
                string claimStatus = orb.IsClaimed ? "CLAIMED" : orb.Name;
                string orbLabel = $"{claimStatus}\n{orb.Description}";
                bool enabled = !_orbClaimedInThisShop && !orb.IsClaimed;
                Rectangle orbBounds = GetOrbButtonBounds(i);
                SceneUi.DrawButton(spriteBatch, orbBounds, orbLabel, orbBounds.Contains(mouse.Position), enabled);
            }

            if (_messageTimer > 0f)
            {
                SceneUi.DrawWrappedText(spriteBatch, _interactionMessage, new Vector2(90, 860), 1400, Color.Orange);
            }

            SceneUi.DrawPlayerStatus(spriteBatch, GameState.CurrentPlayer);
            SceneUi.DrawButton(spriteBatch, LeaveBounds, "Return to Map", LeaveBounds.Contains(mouse.Position));

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
            else if (item.IsSold)
            {
                _interactionMessage = "That card has already been sold.";
                _messageTimer = 2.5f;
            }
            else
            {
                _interactionMessage = $"Not enough Soul Coins. The card costs {item.Price}.";
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