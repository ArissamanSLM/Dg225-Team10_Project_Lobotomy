using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DreamSlayerV2.Core;   // Adjust namespace to match your SceneManager location
using DreamSlayerV2.Scenes; // Adjust namespace to match your Scene classes
using Microsoft.Xna.Framework.Content;
namespace DreamSlayerV2.Scenes
{
    public class BattleScene : IScene 
    {
         private SpriteFont _font; // Optional: add font if you want text here
        private Texture2D EnemyTexture;
        public void Initialize() { }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
            var graphicsDevice = ((IGraphicsDeviceService)content.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            EnemyTexture = new Texture2D(graphicsDevice, 1, 1);
            EnemyTexture.SetData(new[] { Color.White });
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.I))
            {
                SceneManager.ChangeScene(new TitleScene());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            
            spriteBatch.Begin();
            
            spriteBatch.DrawString(_font, "Encounter Enemy!", new Vector2(50, 50), Color.Red);
            spriteBatch.Draw(EnemyTexture, new Rectangle(100, 100, 50, 50), Color.Red); // Example enemy representation)
            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}
