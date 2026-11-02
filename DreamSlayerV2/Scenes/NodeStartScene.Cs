using System;
using System.Collections.Generic;
using DreamSlayerV2.Core;
using DreamSlayerV2.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DreamSlayerV2.Scenes
{   
    public class NodeStartScene : IScene
    {
        private const int CardChoiceCount = 3;

        private static readonly Rectangle[] CardBounds =
        {
            new Rectangle(110, 300, 530, 510),
            new Rectangle(695, 300, 530, 510),
            new Rectangle(1280, 300, 530, 510)
        };

        private readonly Random _random = new Random();
        private readonly List<CardManager> _cardChoices = new List<CardManager>();
        private MouseState _previousMouseState;
        private PlayerController _player;
        private bool _initialized;

        public void Initialize()
        {
            if (_initialized) return;

            _previousMouseState = Mouse.GetState();
            _player = GameState.CurrentPlayer;
            if (!_player.HasStartBonus)
            {
                _player.MaxHP += 5;
                _player.HasStartBonus = true;
            }
            CreateCardChoices();
            _initialized = true;
        }

        public void LoadContent()
        {
            MusicManager.PlayLoopingMusic(GlobalContentManager.NodeMusic);
            var graphicsDevice = GlobalContentManager.GraphicsDevice;
        }

        public void Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            bool isClicked = currentMouseState.LeftButton == ButtonState.Pressed &&
                             _previousMouseState.LeftButton == ButtonState.Released;

            if (isClicked)
            {
                for (int i = 0; i < _cardChoices.Count; i++)
                {
                    if (!CardBounds[i].Contains(currentMouseState.Position)) continue;

                    _player.TryAddCard(_cardChoices[i], out _);
                    NodeSelectScene.SaveRun();
                    SceneManager.ChangeScene(new NodeSelectScene());
                    break;
                }
            }

            _previousMouseState = currentMouseState;
        }

        private void CreateCardChoices()
        {
            var cardPool = new List<CardManager>
            {
                new CardManager(10, "All in One", 1, CardManager.CardType.Attack),
                new CardManager(11, "No Fight", 1, CardManager.CardType.Defense),
                new CardManager(12, "Powerup", 1, CardManager.CardType.Utility),
                new CardManager(13, "Alpha", 1, CardManager.CardType.Special)
            };

            cardPool.RemoveAll(card => card.Rarity != CardManager.CardRarity.Rare);

            if (cardPool.Count < CardChoiceCount)
            {
                throw new InvalidOperationException(
                    $"Node start requires at least {CardChoiceCount} Rare cards, but CardReader provides {cardPool.Count}.");
            }

            for (int i = 0; i < CardChoiceCount; i++)
            {
                int selectedIndex = _random.Next(i, cardPool.Count);
                CardManager selectedCard = cardPool[selectedIndex];
                cardPool[selectedIndex] = cardPool[i];
                cardPool[i] = selectedCard;
                _cardChoices.Add(selectedCard);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            SceneUi.DrawBackground(spriteBatch);
            SceneUi.DrawPanel(spriteBatch, new Rectangle(70, 40, 1780, 960), Color.MediumPurple);

            DrawCenteredText(spriteBatch, "A GIFT BEFORE THE TOWER", 75, Color.Gold);
            DrawCenteredText(spriteBatch, "Your maximum HP increased by 5 for this run.", 145, Color.LightGreen);
            DrawCenteredText(spriteBatch, "Choose ONE Rare card. Click a card to add it to your deck.", 205, Color.White);

            MouseState mouseState = Mouse.GetState();
            for (int i = 0; i < _cardChoices.Count; i++)
            {
                CardManager card = _cardChoices[i];
                Rectangle bounds = CardBounds[i];
                bool isHovered = bounds.Contains(mouseState.Position);
                Color backgroundColor = isHovered ? new Color(64, 48, 86) : new Color(20, 25, 38);
                Color borderColor = isHovered ? Color.Gold : new Color(115, 93, 150);

                spriteBatch.Draw(GlobalContentManager.WhitePixel, bounds, backgroundColor);
                DrawBorder(spriteBatch, bounds, borderColor, 4);

                spriteBatch.DrawString(GlobalContentManager.DefaultFont, $"CARD {i + 1}", new Vector2(bounds.X + 24, bounds.Y + 22), Color.LightGray);
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, card.Name, new Vector2(bounds.X + 24, bounds.Y + 72), Color.White);
                spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X + 24, bounds.Y + 122, bounds.Width - 48, 2), Color.SlateGray);
                spriteBatch.DrawString(GlobalContentManager.DefaultFont, "RARITY: RARE", new Vector2(bounds.X + 24, bounds.Y + 142), Color.Gold);

                spriteBatch.DrawString(GlobalContentManager.DefaultFont, "CARD EFFECT", new Vector2(bounds.X + 24, bounds.Y + 205), Color.LightGreen);
                string description = card.Description + "\n\n" + card.Does;
                DrawWrappedText(
                    spriteBatch,
                    description,
                    new Vector2(bounds.X + 24, bounds.Y + 255),
                    bounds.Width - 56,
                    Color.White);

                Rectangle chooseButton = new Rectangle(bounds.X + 32, bounds.Bottom - 82, bounds.Width - 64, 52);
                SceneUi.DrawButton(spriteBatch, chooseButton, "CHOOSE THIS CARD", isHovered);
            }

            spriteBatch.End();
        }

        private void DrawCenteredText(SpriteBatch spriteBatch, string text, int y, Color color)
        {
            Vector2 size = GlobalContentManager.DefaultFont.MeasureString(text);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, text, new Vector2((1920 - size.X) / 2f, y), color);
        }

        private void DrawCenteredTextInBounds(SpriteBatch spriteBatch, string text, Rectangle bounds, int y, Color color)
        {
            Vector2 size = GlobalContentManager.DefaultFont.MeasureString(text);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, text, new Vector2(bounds.Center.X - size.X / 2f, y), color);
        }

        private void DrawWrappedText(SpriteBatch spriteBatch, string text, Vector2 position, float maxWidth, Color color)
        {
            float lineHeight = GlobalContentManager.DefaultFont.LineSpacing;
            float x = position.X;
            float y = position.Y;

            foreach (string paragraph in text.Split('\n'))
            {
                string line = string.Empty;
                foreach (string word in paragraph.Split(' '))
                {
                    string candidate = line.Length == 0 ? word : line + " " + word;
                    if (line.Length > 0 && GlobalContentManager.DefaultFont.MeasureString(candidate).X > maxWidth)
                    {
                        spriteBatch.DrawString(GlobalContentManager.DefaultFont, line, new Vector2(x, y), color);
                        y += lineHeight;
                        line = word;
                    }
                    else
                    {
                        line = candidate;
                    }
                }

                if (line.Length > 0)
                {
                    spriteBatch.DrawString(GlobalContentManager.DefaultFont, line, new Vector2(x, y), color);
                    y += lineHeight;
                }

                y += lineHeight * 0.5f;
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, int thickness)
        {
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
            spriteBatch.Draw(GlobalContentManager.WhitePixel, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
        }

        public void UnloadContent()
        {
        }
    }
}
