using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DreamSlayerV2.Scenes
{
    public class Event1 : IEvent
    {
        private SpriteFont _font;
        private MouseState _previousMouseState;

        public EventTag Tag => EventTag.Lucky;
        public string DialogText => "You found a Fountain while exploring!\nYou can make a wish to receive one of three blessings:";

        private List<string> _choices = new List<string>()
        {
            "1. Toss a coin: Gain 50 Soul Coins",
            "2. Drink deeply: Heal until Max HP",
            "3. Take the Foundation Ring: Gain a buff until the end of this tower"
        };

        private Vector2 dialogPos = new Vector2(50, 50);
        private Vector2 choicesStartPos = new Vector2(50, 200);

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

            for (int i = 0; i < _choices.Count; i++)
            {
                Vector2 choiceSize = _font.MeasureString(_choices[i]);
                Vector2 choicePos = new Vector2(choicesStartPos.X, choicesStartPos.Y + (i * 50));
                Rectangle choiceBounds = new Rectangle((int)choicePos.X, (int)choicePos.Y, (int)choiceSize.X, (int)choiceSize.Y);

                bool isHovered = choiceBounds.Contains(currentMouseState.Position);
                bool isClicked = isHovered && currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;

                if (isClicked)
                {
                    ExecuteConsequence(i);
                    break;
                }
            }

            _previousMouseState = currentMouseState;
        }

        public void ExecuteConsequence(int choiceIndex)
        {
            // Note: Replace 'GameSession.Player' and 'GameSession.Items' with 
            // whatever global static reference holder your game uses for active run instances.
            // 
            // var player = GameSession.Player;
            // var itemManager = GameSession.ItemManager;

            switch (choiceIndex)
            {
                case 0:
                    // Example: player.SoulCoins += 50;
                    break;

                case 1:
                    // Heal player completely using properties from PlayerController.cs
                    // player.PlayerHP = player.MaxHP;
                    break;

                case 2:
                    // Using your ItemManager logic to grant a relic/buff for the tower run!
                    // itemManager.AddRelicToPlayer("Relic Of Max HP", 1); // Or a custom Foundation Ring definition if you added it
                    break;
            }

            // Return smoothly back to your map node selection scene
            SceneManager.ChangeScene(new NodeSelectScene());
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Draw dialog text
            spriteBatch.DrawString(_font, DialogText, dialogPos, Color.Red);

            // Draw interactive options with hover effects
            MouseState currentMouseState = Mouse.GetState();
            for (int i = 0; i < _choices.Count; i++)
            {
                Vector2 choicePos = new Vector2(choicesStartPos.X, choicesStartPos.Y + (i * 50));
                Vector2 choiceSize = _font.MeasureString(_choices[i]);
                Rectangle choiceBounds = new Rectangle((int)choicePos.X, (int)choicePos.Y, (int)choiceSize.X, (int)choiceSize.Y);

                Color choiceColor = choiceBounds.Contains(currentMouseState.Position) ? Color.LightYellow : Color.Yellow;
                spriteBatch.DrawString(_font, _choices[i], choicePos, choiceColor);
            }

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}