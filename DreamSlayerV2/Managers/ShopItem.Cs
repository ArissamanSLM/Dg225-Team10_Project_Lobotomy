using DreamSlayerV2.Core;
using DreamSlayerV2.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public override void LoadContent()
        {
            base.LoadContent();
            MusicManager.PlaySong(GlobalContentManager.ShopSong, restart: true);
        }

        private Rectangle GetCardButtonBounds(int index) => new Rectangle(100, 290 + (index * 96), 850, 76);
        private Rectangle GetOrbButtonBounds(int index) => new Rectangle(1030, 310 + (index * 190), 800, 150);

        private void GenerateShopStock()
        {
            _shopCards.Clear();
            for (int i = 0; i < 5; i++)
            {
                double roll = _rnd.NextDouble();
                CardManager.CardRarity rarity;

                if (roll < 0.48)
                {
                    rarity = CardManager.CardRarity.Common;
                }
                else if (roll < 0.98)
                {
                    rarity = CardManager.CardRarity.Rare;
                }
                else
                {
                    rarity = CardManager.CardRarity.UltraRare;
                }

                bool isPlusCard = _rnd.NextDouble() < 0.20;
                CardManager card = GenerateCardByRarity(rarity, isPlusCard);

                _shopCards.Add(new ShopItem
                {
                    Card = card,
                    Price = GenerateBuyPrice(card.Rarity),
                    IsSold = false,
                    Rarity = GetRarityLabel(card.Rarity)
                });
            }
        }

        private CardManager GenerateCardByRarity(CardManager.CardRarity rarity, bool isPlusCard)
        {
            List<CardManager> catalog = new List<CardManager>
            {
                new CardManager(0, "Strike", 1, CardManager.CardType.Attack),
                new CardManager(1, "Defend", 1, CardManager.CardType.Defense),
                new CardManager(2, "Inspection", 1, CardManager.CardType.Utility),
                new CardManager(3, "Heal", 1, CardManager.CardType.Heal),
                new CardManager(3, "Restore", 1, CardManager.CardType.Heal),
                new CardManager(4, "All in One", 1, CardManager.CardType.Attack),
                new CardManager(12, "Piece", 1, CardManager.CardType.Attack),
                new CardManager(5, "No Fight", 1, CardManager.CardType.Defense),
                new CardManager(6, "Powerup", 1, CardManager.CardType.Utility),
                new CardManager(10, "Clock", 1, CardManager.CardType.Utility),
                new CardManager(14, "Alpha", 1, CardManager.CardType.Special),
                new CardManager(11, "Aura Heal", 0, CardManager.CardType.Heal),
                new CardManager(8, "Mighty Defend Attack", 3, CardManager.CardType.Attack),
                new CardManager(9, "Mirror Shield", 1, CardManager.CardType.Defense),
                new CardManager(13, "Omega", 0, CardManager.CardType.Special),
                new CardManager(7, "Focus", 0, CardManager.CardType.Utility),
                new CardManager(15, "Beta", 0, CardManager.CardType.Special)
            };

            List<CardManager> matchingCards = catalog.Where(card => card.Rarity == rarity).ToList();
            if (isPlusCard)
            {
                matchingCards = matchingCards
                    .Where(card => SupportsPlusVariant(card.Name))
                    .ToList();
            }

            if (matchingCards.Count == 0)
            {
                throw new InvalidOperationException(
                    $"The shop has no {(isPlusCard ? "plus " : string.Empty)}cards configured for {rarity} rarity.");
            }

            CardManager selectedCard = matchingCards[_rnd.Next(matchingCards.Count)];
            return isPlusCard
                ? new CardManager(
                    selectedCard.CardID,
                    selectedCard.Name + "+",
                    selectedCard.Cost,
                    selectedCard.Type,
                    selectedCard.Hazard)
                : selectedCard;
        }

        private static bool SupportsPlusVariant(string cardName)
        {
            return cardName is
                "Strike" or "Defend" or "Inspection" or "Piece" or "All in One" or
                "Mighty Defend Attack" or "No Fight" or "Mirror Shield" or "Omega" or
                "Powerup" or "Clock" or "Restore" or "Heal" or "Focus" or "Aura Heal";
        }

        private int GenerateBuyPrice(CardManager.CardRarity rarity)
        {
            int baseSellPrice = rarity switch
            {
                CardManager.CardRarity.Common => _rnd.Next(10, 21),
                CardManager.CardRarity.Rare => _rnd.Next(30, 61),
                CardManager.CardRarity.UltraRare => _rnd.Next(120, 201),
                _ => throw new InvalidOperationException($"The shop cannot price {rarity} cards.")
            };

            return baseSellPrice * 2;
        }

        private static string GetRarityLabel(CardManager.CardRarity rarity)
        {
            return rarity switch
            {
                CardManager.CardRarity.UltraRare => "Ultra Rare",
                _ => rarity.ToString()
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
                bool added = player.TryAddCard(item.Card, out int duplicateSaleValue);
                _interactionMessage = added
                    ? $"Bought {item.Card.Name} ({item.Rarity})!"
                    : $"Duplicate {item.Card.Name} auto-sold for {duplicateSaleValue} Soul Coins.";
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

            int sellReward = CalculateSellPrice(card);

            player.SoulCoins += sellReward;
            player.Deck.Remove(card);
            _interactionMessage = $"Sold {card.Name} for {sellReward} Soul Coins.";
            _messageTimer = 2.5f;
        }

        public static int CalculateSellPrice(CardManager card)
        {
            ArgumentNullException.ThrowIfNull(card);
            return card.Cost switch
            {
                3 => Random.Shared.Next(120, 201),
                2 => Random.Shared.Next(30, 61),
                _ => Random.Shared.Next(10, 21)
            };
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