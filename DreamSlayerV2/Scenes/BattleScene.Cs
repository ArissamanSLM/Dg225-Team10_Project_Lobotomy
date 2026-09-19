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

        public void Initialize() { }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
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
            
            spriteBatch.DrawString(_font, "Game Over. Press I to Retry.", new Vector2(50, 50), Color.Red);
            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}
