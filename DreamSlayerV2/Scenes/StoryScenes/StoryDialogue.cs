using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DreamSlayerV2.Core;
using System;

namespace DreamSlayerV2.Scenes
{
    internal sealed class StoryDialogue
    {
        private const float CharacterInterval = 0.03f;
        private static readonly Vector2 DialoguePosition = new Vector2(70, 760);
        private static readonly Vector2 PromptPosition = new Vector2(1500, 980);

        private readonly string _text;
        private MouseState _previousMouseState;
        private float _typingTimer;
        private int _visibleCharacters;

        public StoryDialogue(string text)
        {
            _text = text;
        }

        public string DisplayedText => _text.Substring(0, _visibleCharacters);
        public bool IsTypingFinished => _visibleCharacters >= _text.Length;

        public void Initialize()
        {
            _previousMouseState = Mouse.GetState();
        }

        public bool Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();
            bool clicked = currentMouseState.LeftButton == ButtonState.Pressed &&
                           _previousMouseState.LeftButton == ButtonState.Released;
            _previousMouseState = currentMouseState;

            if (!IsTypingFinished)
            {
                if (clicked)
                {
                    _visibleCharacters = _text.Length;
                    _typingTimer = 0f;
                    return false;
                }

                _typingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                while (_typingTimer >= CharacterInterval && !IsTypingFinished)
                {
                    _typingTimer -= CharacterInterval;
                    _visibleCharacters++;
                }

                return false;
            }

            return clicked;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D background, float opacity = 1f)
        {
            spriteBatch.Begin();
            if (background != null)
            {
                spriteBatch.Draw(background, new Rectangle(0, 0, 1920, 1080), Color.White * opacity);
            }

            SpriteFont font = GlobalContentManager.DefaultFont;
            Vector2 textSize = font.MeasureString(DisplayedText);
            Rectangle dialogueBounds = new Rectangle(
                (int)DialoguePosition.X - 20,
                (int)DialoguePosition.Y - 20,
                Math.Min((int)textSize.X + 40, 1780),
                (int)textSize.Y + 40);

            spriteBatch.Draw(GlobalContentManager.WhitePixel, dialogueBounds, Color.Black * (0.7f * opacity));
            spriteBatch.DrawString(font, DisplayedText, DialoguePosition, Color.SkyBlue * opacity);

            if (IsTypingFinished)
            {
                MouseState mouseState = Mouse.GetState();
                string prompt = "[Click to Continue]";
                Vector2 promptSize = font.MeasureString(prompt);
                Rectangle promptBounds = new Rectangle(
                    (int)PromptPosition.X,
                    (int)PromptPosition.Y,
                    (int)promptSize.X,
                    (int)promptSize.Y);
                Color promptColor = promptBounds.Contains(mouseState.Position) ? Color.LightYellow : Color.Yellow;
                promptColor *= opacity;
                spriteBatch.DrawString(font, prompt, PromptPosition, promptColor);
            }

            spriteBatch.End();
        }
    }
}
