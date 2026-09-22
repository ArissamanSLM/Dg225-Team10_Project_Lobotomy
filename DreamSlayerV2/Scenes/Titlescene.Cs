using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace DreamSlayerV2.Scenes
{
    public class TitleScene : IScene
    {
        private Texture2D BgTitle;
        private SpriteFont _font;
        private Song _titleMusic;

        // Regular instance variables (not static)
        private string titleText = "DREAM SLAYER";
        private string startText = "Start Game";

        public void Initialize()
        {
            // Set up initial variables, enemy scaling, or relic states for this floor
        }

        public void LoadContent(ContentManager content)
        {
            // Font is loaded safely here first
            _font = content.Load<SpriteFont>("File");
            _titleMusic = content.Load<Song>("Montagem Celestial Esperanza [x90ocGRT6EE]");
            MediaPlayer.Play(_titleMusic);
           // BgTitle = content.Load<>
            MediaPlayer.IsRepeating = true;
        }

        public void Update(GameTime gameTime)
        {
            // Calculate positions dynamically inside Update/Draw so _font is ready
            Vector2 startSize = _font.MeasureString(startText);
            Vector2 startPos = new Vector2(960f - (startSize.X / 2f), 918f);

            MouseState mouse = Mouse.GetState();
            Rectangle startButtonBounds = new Rectangle((int)startPos.X, (int)startPos.Y, (int)startSize.X, (int)startSize.Y);

            if (startButtonBounds.Contains(mouse.Position) && mouse.LeftButton == ButtonState.Pressed)
            {
                SceneManager.ChangeScene(new SceneStory1());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 titleSize = _font.MeasureString(titleText);
            Vector2 titlePos = new Vector2(960f - (titleSize.X / 2f), 50f);

            Vector2 startSize = _font.MeasureString(startText);
            Vector2 startPos = new Vector2(960f - (startSize.X / 2f), 918f);

            spriteBatch.Begin();
            spriteBatch.DrawString(_font, titleText, titlePos, Color.White);
            spriteBatch.DrawString(_font, startText, startPos, Color.White);
            spriteBatch.End();
        }

        public void UnloadContent()
        {
            // Clear textures or memory specific to this floor when leaving

            

        }
    }
}