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
        private bool boughtHeal = false;
        private bool boughtInspect = false;
        public List<CardManager> starterDeck = new List<CardManager>();
        public void Initialize() { _prevMouse = Mouse.GetState(); }

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

            if (clicked && rHeal.Contains(ms.Position) && !boughtHeal)
{
    if (player.SoulCoins >= 20)
    {
        player.SoulCoins -= 20;
        int heal = (int)Math.Ceiling(player.MaxHP * 0.5);
        player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + heal);
        boughtHeal = true;

        // แก้ไขตรงนี้: ใส่ ID การ์ด (เช่น 101) และเพิ่มเข้าเด็คของผู้เล่นให้ถูกต้อง
        // (ปรับชื่อตัวแปรสตาร์ทเตอร์เด็คหรือเด็คของผู้เล่นตามโครงสร้างจริงของคุณ เช่น player.Deck.Add หรือ starterDeck.Add)
        // ตัวอย่างการสร้าง CardManager ที่ถูกต้อง:
        CardManager newHealCard = new CardManager(101, "Restore", 1, CardManager.CardType.Heal);
        
        // ถ้าใช้ starterDeck เป็นลิสต์เก็บการ์ด ให้แน่ใจว่าประกาศตัวแปรและกำหนดค่าให้เรียบร้อยก่อนใช้งาน
        if (starterDeck != null)
        {
            starterDeck.Add(newHealCard);
        }
    }
}

           if (clicked && rHeal.Contains(ms.Position) && !boughtHeal)
{
    if (player.SoulCoins >= 20)
    {
        player.SoulCoins -= 20;
        int heal = (int)Math.Ceiling(player.MaxHP * 0.5);
        player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + heal);
        boughtHeal = true;

        // แก้ไขตรงนี้: ใส่ ID การ์ด (เช่น 101) และเพิ่มเข้าเด็คของผู้เล่นให้ถูกต้อง
        // (ปรับชื่อตัวแปรสตาร์ทเตอร์เด็คหรือเด็คของผู้เล่นตามโครงสร้างจริงของคุณ เช่น player.Deck.Add หรือ starterDeck.Add)
        // ตัวอย่างการสร้าง CardManager ที่ถูกต้อง:
        CardManager newHealCard = new CardManager(101, "Restore", 1, CardManager.CardType.Heal);
        
        // ถ้าใช้ starterDeck เป็นลิสต์เก็บการ์ด ให้แน่ใจว่าประกาศตัวแปรและกำหนดค่าให้เรียบร้อยก่อนใช้งาน
        if (starterDeck != null)
        {
            starterDeck.Add(newHealCard);
        }
    }
}

            if (clicked && rRest.Contains(ms.Position))
            {
                int heal = Math.Max(1, (int)(player.MaxHP * 0.15f));
                player.PlayerHP = Math.Min(player.MaxHP, player.PlayerHP + heal);
                player.Sanity = 100;
            }

            // Leave with right-click
            if (ms.RightButton == ButtonState.Pressed && _prevMouse.RightButton == ButtonState.Released)
            {
                SceneManager.ChangeScene(new NodeSelectScene());
            }

            _prevMouse = ms;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(_font, "Shop / Rest", new Vector2(200, 240), Color.Yellow);

            // Fixed the string quotes here:
            spriteBatch.DrawString(_font, $"Buy Heal (20) {(boughtHeal ? "- Bought" : "")}", new Vector2(200, 300), Color.White);
            spriteBatch.DrawString(_font, $"Buy Inspection (50) {(boughtInspect ? "- Bought" : "")}", new Vector2(200, 360), Color.White);

            spriteBatch.DrawString(_font, "Rest (Free) - Heal 15% MaxHP and Restore Sanity", new Vector2(200, 420), Color.LightGreen);
            spriteBatch.DrawString(_font, "Right-click to leave", new Vector2(200, 520), Color.AntiqueWhite);

            // Show player coins using global state
            var player = GameState.CurrentPlayer;
            if (player != null)
                spriteBatch.DrawString(_font, $"Soul Coins: {player.SoulCoins}", new Vector2(50, 50), Color.Cyan);

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}