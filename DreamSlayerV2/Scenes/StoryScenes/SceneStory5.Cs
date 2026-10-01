using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace DreamSlayerV2.Scenes
{
    public class SceneStory5 : IScene
    {
        private SpriteFont _font;
        private string promptText = "Click Here to Enter the Tower...";
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
                SceneManager.ChangeScene(new NodeSelectScene());
            }

            _previousMouseState = currentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(_font, "Before I know it, I was in the other world. I can tell this place is really dangerous.\n I need to get out here as soon as possible... or maybe I could stay a bit longer and get as much forbidden spell.", new Vector2(50, 50), Color.Red);

            Vector2 textSize = _font.MeasureString(promptText);
            Rectangle promptBounds = new Rectangle((int)promptPos.X, (int)promptPos.Y, (int)textSize.X, (int)textSize.Y);
            Color promptColor = promptBounds.Contains(Mouse.GetState().Position) ? Color.LightYellow : Color.Yellow;

            spriteBatch.DrawString(_font, promptText, promptPos, promptColor);
            spriteBatch.End();
        }

        public void UnloadContent() 
        {
            MediaPlayer.Stop();
        }
    }
}