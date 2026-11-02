using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DreamSlayerV2.Scenes
{
    public class RewardScene : IScene
    {
        private readonly string _enemyTypeLabel;
        private readonly int _soulReward;
        private readonly CardManager _cardReward;
        private readonly bool _isDuplicateCardReward;
        private readonly int _duplicateSaleValue;
        private readonly bool _isNightmare;
        private readonly int _depth;
        private readonly Rectangle _continueBounds = new Rectangle(700, 900, 520, 80);
        private MouseState _previousMouseState;
        private bool _claimed;

        public RewardScene(int rewardTier, bool isNightmare = false, int depth = 1)
        {
            if (rewardTier < 0 || rewardTier > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardTier), "Reward tier must be normal, elite, or boss.");
            }

            _isNightmare = isNightmare;
            _depth = Math.Max(1, depth);
            _enemyTypeLabel = rewardTier switch
            {
                1 => "Elite",
                2 => "Boss",
                _ => "Enemy"
            };

            int minimumCoins = rewardTier switch { 1 => 50, 2 => 100, _ => 30 };
            int maximumCoinsExclusive = rewardTier switch { 1 => 76, 2 => 151, _ => 41 };
            float depthMultiplier = _isNightmare ? 1.5f + ((_depth - 1) * 0.25f) : 1f;
            _soulReward = (int)Math.Round(
                Random.Shared.Next(minimumCoins, maximumCoinsExclusive) *
                depthMultiplier *
                UpgradeOrbPermanent.GetSoulRewardMultiplier());

            if (rewardTier == 1)
            {
                _cardReward = CreateRewardCard(Random.Shared.Next(100) switch
                {
                    < 40 => CardManager.CardRarity.Common,
                    < 99 => CardManager.CardRarity.Rare,
                    _ => CardManager.CardRarity.UltraRare
                });
            }
            else if (rewardTier == 2)
            {
                _cardReward = CreateRewardCard(Random.Shared.Next(100) < 90
                    ? CardManager.CardRarity.Rare
                    : CardManager.CardRarity.UltraRare);
            }

            if (_cardReward != null)
            {
                PlayerController player = GameState.CurrentPlayer;
                _isDuplicateCardReward = player.OwnsUniqueCard(_cardReward);
                if (_isDuplicateCardReward)
                {
                    _duplicateSaleValue = player.GetDuplicateCardSalePrice(_cardReward);
                }
            }
        }

        public void Initialize()
        {
            _previousMouseState = Mouse.GetState();
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            bool clicked = currentMouseState.LeftButton == ButtonState.Pressed &&
                           _previousMouseState.LeftButton == ButtonState.Released;

            if (clicked && _continueBounds.Contains(currentMouseState.Position) && !_claimed)
            {
                PlayerController player = GameState.CurrentPlayer;
                player.SoulCoins += _soulReward;
                if (_cardReward != null)
                {
                    player.TryAddCard(_cardReward, _duplicateSaleValue, out _);
                }
                _claimed = true;
                SceneManager.ChangeScene(new NodeSelectScene());
            }

            _previousMouseState = currentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(GlobalContentManager.RewardScreen, new Rectangle(0, 0, 1920, 1080), Color.White);
            SceneUi.DrawPanel(spriteBatch, new Rectangle(310, 120, 1300, 740), Color.MediumPurple);

            string title = $"You defeated the {_enemyTypeLabel}!";
            Vector2 titleSize = GlobalContentManager.DefaultFont.MeasureString(title);
            spriteBatch.DrawString(
                GlobalContentManager.DefaultFont,
                title,
                new Vector2((1920 - titleSize.X) / 2f, 170),
                Color.Gold);

            string depthLabel = _isNightmare ? $"Nightmare Depth {_depth}" : $"Depth {_depth}";
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, depthLabel, new Vector2(820, 245), Color.LightCoral);
            spriteBatch.Draw(GlobalContentManager.SoulCoinIcon, new Rectangle(565, 326, 40, 40), Color.White);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"Soul Coins: +{_soulReward}", new Vector2(620, 330), Color.Yellow);

            if (_cardReward != null)
            {
                Rectangle cardBounds = new Rectangle(610, 415, 700, 300);
                SceneUi.DrawPanel(spriteBatch, cardBounds, Color.Gold);
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Card reward", new Vector2(650, 450), Color.LightGreen);
                spriteBatch.DrawString(
                    GlobalContentManager.DefaultFont,
                    $"{_cardReward.Name}  ({_cardReward.Rarity})",
                    new Vector2(650, 510),
                    Color.White);
                SceneUi.DrawWrappedText(
                    spriteBatch,
                    _cardReward.Description + "\n" + _cardReward.Does,
                    new Vector2(650, 570),
                    620,
                    Color.LightGray);
                if (_isDuplicateCardReward)
                {
                    spriteBatch.DrawString(
                        GlobalContentManager.DefaultFont,
                        $"Duplicate unique card: auto-sells for +{_duplicateSaleValue} Soul Coins.",
                        new Vector2(650, 680),
                        Color.Yellow);
                }
            }
            else
            {
                spriteBatch.DrawString(
                    GlobalContentManager.DefaultFont,
                    "Normal enemies grant Soul Coins only.",
                    new Vector2(620, 455),
                    Color.LightGray);
            }

            MouseState mouse = Mouse.GetState();
            SceneUi.DrawButton(
                spriteBatch,
                _continueBounds,
                "Claim Reward and Continue",
                _continueBounds.Contains(mouse.Position));
            spriteBatch.End();
        }

        public void UnloadContent() { }

        private static CardManager CreateRewardCard(CardManager.CardRarity rarity)
        {
            List<CardManager> candidates = new List<CardManager>
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
                new CardManager(8, "Mighty Defend Attack", 0, CardManager.CardType.Attack),
                new CardManager(9, "Mirror Shield", 1, CardManager.CardType.Defense),
                new CardManager(13, "Omega", 0, CardManager.CardType.Special),
                new CardManager(7, "Focus", 0, CardManager.CardType.Utility),
                new CardManager(15, "Beta", 0, CardManager.CardType.Special)
            };

            candidates = candidates.Where(card => card.Rarity == rarity).ToList();
            if (candidates.Count == 0)
            {
                throw new InvalidOperationException($"No reward cards are configured for {rarity} rarity.");
            }

            return candidates[Random.Shared.Next(candidates.Count)];
        }
    }
}
