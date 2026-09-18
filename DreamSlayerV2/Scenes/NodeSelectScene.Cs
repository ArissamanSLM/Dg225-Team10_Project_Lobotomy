using DreamSlayerV2.Core;
using DreamSlayerV2.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace  DreamSlayerV2.Scenes
{
    public class NodeSelectScene : IScene
    {
        private SpriteFont _font;
        private Vector2 CameraPosition = Vector2.Zero;
        private Vector2 scroll_factor = new Vector2(1.0f, 1f);

        // Track the previous mouse state to calculate scroll movement
        private MouseState _previousMouseState;

        public void Initialize()
        {
            _previousMouseState = Mouse.GetState();
        }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.G))
            {
                SceneManager.ChangeScene(new GameOverScene());
            }
            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                SceneManager.ChangeScene(new TowerScene());
            }

            // 1. Get the current mouse state every frame
            MouseState currentMouseState = Mouse.GetState();

            // 2. Calculate the difference (delta) in the scroll wheel
            int scrollDelta = currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;

            // 3. Move camera based on scroll direction
            if (scrollDelta > 0)
            {
                CameraPosition.X -= 50f; // Scrolled up / forward
            }
            else if (scrollDelta < 0)
            {
                CameraPosition.X += 50f; // Scrolled down / backward
            }
            CameraPosition.X = MathHelper.Clamp(CameraPosition.X, 0f, 4000f);
            // 4. Save current state for the next frame
            _previousMouseState = currentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(_font, "Current Floor: Tower Ascent(Node Choice)", new Vector2(50, 50) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Press G to go to Game Over Scene", new Vector2(50, 100) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Press T to go to Tower Scene", new Vector2(50, 150) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Scroll to move camera left/right", new Vector2(50, 200) - CameraPosition * scroll_factor, Color.White);
            spriteBatch.DrawString(_font, "Rickroll", new Vector2(3000, 50) - CameraPosition * scroll_factor, Color.White);

            spriteBatch.End();
        }

        public void UnloadContent()
        {
            // Clear textures or memory specific to this floor when leaving
        }
    }
}