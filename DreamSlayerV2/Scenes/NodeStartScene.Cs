using System;
using System.Collections.Generic;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
        private SpriteFont _font;
        private Texture2D _pixelTexture;
        private MouseState _previousMouseState;
        private PlayerController _player;
        private bool _initialized;

        public void Initialize()
        {
            if (_initialized) return;

            _previousMouseState = Mouse.GetState();
            _player = GameState.CurrentPlayer;
            _player.MaxHP += 5;
            CreateCardChoices();
            _initialized = true;
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            var graphicsDevice = ((IGraphicsDeviceService)content.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
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

                    _player.Deck.Add(_cardChoices[i]);
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

            DrawCenteredText(spriteBatch, "A Gift Before the Tower", 70, Color.White);
            DrawCenteredText(spriteBatch, "Your max HP increased by 5 for this run.", 145, Color.LightGreen);
            DrawCenteredText(spriteBatch, "Choose one Rare card to add to your deck.", 205, Color.Yellow);

            MouseState mouseState = Mouse.GetState();
            for (int i = 0; i < _cardChoices.Count; i++)
            {
                CardManager card = _cardChoices[i];
                Rectangle bounds = CardBounds[i];
                bool isHovered = bounds.Contains(mouseState.Position);
                Color backgroundColor = isHovered ? new Color(70, 55, 95) : new Color(35, 35, 55);
                Color borderColor = isHovered ? Color.Gold : Color.MediumPurple;

                spriteBatch.Draw(_pixelTexture, bounds, backgroundColor);
                DrawBorder(spriteBatch, bounds, borderColor, 4);

                spriteBatch.DrawString(_font, card.Name, new Vector2(bounds.X + 24, bounds.Y + 24), Color.White);
                spriteBatch.DrawString(_font, "RARE  |  Cost: " + card.Cost, new Vector2(bounds.X + 24, bounds.Y + 76), Color.Gold);

                string description = card.Description + "\n" + card.Does;
                DrawWrappedText(
                    spriteBatch,
                    description,
                    new Vector2(bounds.X + 24, bounds.Y + 140),
                    bounds.Width - 48,
                    Color.White);

                DrawCenteredTextInBounds(spriteBatch, "Click to choose", bounds, bounds.Bottom - 50, isHovered ? Color.Gold : Color.LightGray);
            }

            spriteBatch.End();
        }

        private void DrawCenteredText(SpriteBatch spriteBatch, string text, int y, Color color)
        {
            Vector2 size = _font.MeasureString(text);
            spriteBatch.DrawString(_font, text, new Vector2((1920 - size.X) / 2f, y), color);
        }

        private void DrawCenteredTextInBounds(SpriteBatch spriteBatch, string text, Rectangle bounds, int y, Color color)
        {
            Vector2 size = _font.MeasureString(text);
            spriteBatch.DrawString(_font, text, new Vector2(bounds.Center.X - size.X / 2f, y), color);
        }

        private void DrawWrappedText(SpriteBatch spriteBatch, string text, Vector2 position, float maxWidth, Color color)
        {
            float lineHeight = _font.LineSpacing;
            float x = position.X;
            float y = position.Y;

            foreach (string paragraph in text.Split('\n'))
            {
                string line = string.Empty;
                foreach (string word in paragraph.Split(' '))
                {
                    string candidate = line.Length == 0 ? word : line + " " + word;
                    if (line.Length > 0 && _font.MeasureString(candidate).X > maxWidth)
                    {
                        spriteBatch.DrawString(_font, line, new Vector2(x, y), color);
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
                    spriteBatch.DrawString(_font, line, new Vector2(x, y), color);
                    y += lineHeight;
                }

                y += lineHeight * 0.5f;
            }
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, int thickness)
        {
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
        }

        public void UnloadContent()
        {
            _pixelTexture?.Dispose();
        }
    }
}
