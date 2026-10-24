using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DreamSlayerV2.Scenes
{
    public class ShopRestScene : IScene
    {
        private SpriteFont _font;
        private MouseState _prevMouse;
        private Texture2D _pixel;
        private readonly bool _isRest;
        private bool boughtHeal = false;
        private bool boughtInspect = false;

        public ShopRestScene() : this(false) { }

        protected ShopRestScene(bool isRest)
        {
            _isRest = isRest;
        }

        public void Initialize()
        {
            _prevMouse = Mouse.GetState();
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            var graphicsDevice = ((IGraphicsDeviceService)content.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            var ms = Mouse.GetState();
            Vector2 buyHealPos = new Vector2(200, 300);
            Vector2 buyInspectPos = new Vector2(200, 360);
            Vector2 restPos = new Vector2(200, 420);

            Rectangle rHeal = new Rectangle((int)buyHealPos.X, (int)buyHealPos.Y, 300, 40);
            Rectangle rInspect = new Rectangle((int)buyInspectPos.X, (int)buyInspectPos.Y, 300, 40);
            Rectangle rRest = new Rectangle((int)restPos.X, (int)restPos.Y, 300, 40);

            bool clicked = ms.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;

            var player = GameState.CurrentPlayer;
            if (player == null) return;

            if (!_isRest && clicked && rHeal.Contains(ms.Position) && !boughtHeal)
            {
                if (player.SoulCoins >= 20)
                {
                    player.SoulCoins -= 20;
                    boughtHeal = true;

                    // Add directly to the player's actual run deck
                    CardManager newHealCard = new CardManager(101, "Restore", 1, CardManager.CardType.Heal);
                    player.Deck.Add(newHealCard);
                }
            }

            if (!_isRest && clicked && rInspect.Contains(ms.Position) && !boughtInspect)
            {
                if (player.SoulCoins >= 50)
                {
                    player.SoulCoins -= 50;
                    boughtInspect = true;

                    // Add Inspection card directly to the player's run deck
                    CardManager newInspectCard = new CardManager(102, "Inspection", 1, CardManager.CardType.Utility);
                    player.Deck.Add(newInspectCard);
                }
            }

            if (_isRest && clicked && rRest.Contains(ms.Position))
            {
                int heal = Math.Max(1, (int)(player.MaxHP * 0.15f));
                player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + heal);
                player.Sanity = 100;
            }

            // Leave scene on Right-Click
            if (ms.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released)
            {
                SceneManager.ChangeScene(new NodeSelectScene());
            }

            _prevMouse = ms;
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(_font, _isRest ? "Rest" : "Shop", new Vector2(200, 240), Color.Yellow);
            if (_isRest)
            {
                spriteBatch.DrawString(_font, "Rest (Free) - Heal 15% MaxHP and Restore Sanity", new Vector2(200, 420), Color.LightGreen);
            }
            else
            {
                spriteBatch.DrawString(_font, $"Buy Restore Card (20 Coins) {(boughtHeal ? "- Bought" : "")}", new Vector2(200, 300), boughtHeal ? Color.Gray : Color.White);
                spriteBatch.DrawString(_font, $"Buy Inspection Card (50 Coins) {(boughtInspect ? "- Bought" : "")}", new Vector2(200, 360), boughtInspect ? Color.Gray : Color.White);
            }
            spriteBatch.DrawString(_font, "Right-click to leave", new Vector2(200, 520), Color.AntiqueWhite);

            var player = GameState.CurrentPlayer;
            if (player != null)
            {
                spriteBatch.DrawString(_font, $"Soul Coins: {player.SoulCoins}", new Vector2(50, 50), Color.Cyan);
                spriteBatch.DrawString(_font, $"Current Deck Size: {player.Deck.Count}", new Vector2(50, 80), Color.LightGray);
            }

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
    public class Shop : ShopRestScene
    {
        public Shop() : base(false) { }
    }
    public class Rest : ShopRestScene
    {
        public Rest() : base(true) { }
    }
}