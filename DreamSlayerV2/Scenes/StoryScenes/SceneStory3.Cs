using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DreamSlayerV2.Scenes
{
    public class SceneStory3 : IScene
    {
        private SpriteFont _font;
        private string promptText = "Click Here to Continue...";
        private Vector2 promptPos = new Vector2(100, 600);
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
            MouseState currentMouseState = Mouse.GetState();

            Vector2 textSize = _font.MeasureString(promptText);
            Rectangle promptBounds = new Rectangle((int)promptPos.X, (int)promptPos.Y, (int)textSize.X, (int)textSize.Y);

            bool isHovered = promptBounds.Contains(currentMouseState.Position);
            bool isClicked = isHovered && currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;

            if (isClicked)
            {
                SceneManager.ChangeScene(new SceneStory4());
            }

            _previousMouseState = currentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(_font, "Yet, a persistent restlessness gnaws at your mind.\nYou gaze toward the looming peaks of the tower in the distance,\nwondering what lies beyond the horizon.", new Vector2(50, 50), Color.Red);

            Vector2 textSize = _font.MeasureString(promptText);
            Rectangle promptBounds = new Rectangle((int)promptPos.X, (int)promptPos.Y, (int)textSize.X, (int)textSize.Y);
            Color promptColor = promptBounds.Contains(Mouse.GetState().Position) ? Color.LightYellow : Color.Yellow;

            spriteBatch.DrawString(_font, promptText, promptPos, promptColor);
            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}