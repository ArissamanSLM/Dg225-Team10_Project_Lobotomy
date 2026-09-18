using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DreamSlayerV2.Scenes
{
    public class TowerScene : IScene
    {
        private SpriteFont _font;

        public void Initialize()
        {
            // Set up initial variables, enemy scaling, or relic states for this floor
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
        }

        public void Update(GameTime gameTime)
        {
           if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.G)) // Example: Press G to go to Game Over Scene
            {
                SceneManager.ChangeScene(new GameOverScene());
            }
           if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.N)) // Example: Press N to go to Node Select Scene
            {
                SceneManager.ChangeScene(new NodeSelectScene());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(_font, "Current Floor: Tower Ascent", new Vector2(50, 50), Color.White);
            spriteBatch.End();
        }

        public void UnloadContent()
        {
            // Clear textures or memory specific to this floor when leaving
            
        }
    }
}