using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework.Input;
using System;

namespace DreamSlayerV2.Scenes
{
    public class GameOverScene : IScene
    {
        private SpriteFont _font;

        // Run stats passed from the battle or final node
        private int _nodesPassed;
        private int _enemiesCleared;
        private int _cardsPlayed;
        private bool _isVictory;
        private bool _died;
        private int _floorsPassed;
        private int _finalScore;

        // Constructor to accept run statistics
        public GameOverScene(int nodesPassed, int enemiesCleared, int cardsPlayed, bool isVictory, bool died, int floorsPassed)
        {
            _nodesPassed = nodesPassed;
            _enemiesCleared = enemiesCleared;
            _cardsPlayed = cardsPlayed;
            _isVictory = isVictory;
            _died = died;
            _floorsPassed = floorsPassed;

            // Calculate final score using your custom rules
            _finalScore = CalculateTotalScore();
        }

        private int CalculateTotalScore()
        {
            int score = 0;
            score += _nodesPassed * 200;
            score += _enemiesCleared * 100;
            score += _cardsPlayed * 5;

            if (_isVictory)
            {
                score += 1000;
            }

            if (_died)
            {
                score -= 100;
            }

            score += _floorsPassed * 250;

            return Math.Max(0, score);
        }

        public void Initialize() { }

        public void LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("File");
        }

        public void Update(GameTime gameTime)
        {
            // Press 'I' to return to Title Scene
            if (Keyboard.GetState().IsKeyDown(Keys.I))
            {
                SceneManager.ChangeScene(new TitleScene());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            string titleText = _isVictory ? "--- DEMO VICTORY! ---" : "--- GAME OVER ---";
            Color titleColor = _isVictory ? Color.Gold : Color.Red;

            // Draw header
            spriteBatch.DrawString(_font, titleText, new Vector2(50, 40), titleColor);

            // Draw detailed breakdown matching your scoring rules
           // string breakdown =
                //$"Nodes Passed ({_nodesPassed}): +{_nodesPassed * 200}\n" +
                //$"Enemies Cleared ({_enemiesCleared}): +{_enemiesCleared * 100}\n" +
                //$"Cards Used ({_cardsPlayed}): +{_cardsPlayed * 5}\n" +
                //$"Floor Bonus ({_floorsPassed}): +{_floorsPassed * 250}\n" +
                //(_isVictory ? "Boss Cleared / Demo End: +1000\n" : "") +
                //(_died ? "Death Penalty: -100\n" : "") +
                //$"----------------------------------\n" +
                //$"FINAL SCORE: {_finalScore}";

            //spriteBatch.DrawString(_font, breakdown, new Vector2(50, 100), Color.White);

            // Prompt to restart/exit
            spriteBatch.DrawString(_font, "Press 'I' to return to Title Screen", new Vector2(50, 420), Color.Yellow);

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}