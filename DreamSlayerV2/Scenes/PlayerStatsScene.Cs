using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DreamSlayerV2.Scenes
{
    public class PlayerStatsScene : IScene
    {
        private SpriteFont _font;
        private MouseState _previousMouseState;

        private PlayerController _player;

        // Store the scene the player came from so "Resume" can take them back smoothly
        private IScene _previousScene;

        public PlayerStatsScene(PlayerController player, IScene previousScene)
        {
            _player = player;
            _previousScene = previousScene;
        }

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

            // Define button rectangles for mouse collision detection
            Rectangle resumeButtonBounds = new Rectangle(50, 450, 200, 40);
            Rectangle quitButtonBounds = new Rectangle(50, 510, 200, 40);

            bool isLeftClicked = currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;

            if (isLeftClicked)
            {
                Point mousePoint = new Point(currentMouseState.X, currentMouseState.Y);

                // Option 1: Resume Game (Returns to whatever scene they were playing)
                if (resumeButtonBounds.Contains(mousePoint))
                {
                    if (_previousScene != null)
                    {
                        SceneManager.ChangeScene(_previousScene);
                    }
                    else
                    {
                        SceneManager.ChangeScene(new NodeSelectScene()); // Fallback
                    }
                }
                // Option 2: Quit to Title Screen
                else if (quitButtonBounds.Contains(mousePoint))
                {
                    SceneManager.ChangeScene(new TitleScene());
                }
            }

            _previousMouseState = currentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Semi-transparent background overlay effect (optional, or draw a dark tint)
            spriteBatch.DrawString(_font, "--- PAUSED / PLAYER STATS ---", new Vector2(50, 40), Color.Gold);

            if (_player != null)
            {
                // Display player stats using properties from PlayerController.cs
                string statsText =
                    $"Class: {_player.SelectedClass}\n" +
                    $"HP: {_player.PlayerHP} / {_player.MaxHP}\n" +
                    $"Sanity: {_player.Sanity}\n" +
                    $"Honor: {_player.Honor}\n" +
                    $"Level: {_player.Level}\n" +
                    $"Cards in Deck: {_player.Deck.Count}\n" +
                    $"Current Energy: {_player.Energy}";

                spriteBatch.DrawString(_font, statsText, new Vector2(50, 100), Color.White);
            }
            else
            {
                spriteBatch.DrawString(_font, "No active player data found.", new Vector2(50, 100), Color.Red);
            }

            // Draw Interactive Pause Menu Buttons with Hover Effects
            MouseState currentMouseState = Mouse.GetState();
            Point mousePoint = new Point(currentMouseState.X, currentMouseState.Y);

            Rectangle resumeButtonBounds = new Rectangle(50, 450, 200, 40);
            Rectangle quitButtonBounds = new Rectangle(50, 510, 200, 40);

            Color resumeColor = resumeButtonBounds.Contains(mousePoint) ? Color.LightGreen : Color.Green;
            Color quitColor = quitButtonBounds.Contains(mousePoint) ? Color.LightCoral : Color.IndianRed;

            spriteBatch.DrawString(_font, "> Resume Game", new Vector2(resumeButtonBounds.X, resumeButtonBounds.Y), resumeColor);
            spriteBatch.DrawString(_font, "> Quit to Title", new Vector2(quitButtonBounds.X, quitButtonBounds.Y), quitColor);

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}