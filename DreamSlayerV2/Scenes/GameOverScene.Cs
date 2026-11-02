using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework.Input;
using System;

namespace DreamSlayerV2.Scenes
{
    public class GameOverScene : IScene
    {

        private int _nodesPassed;
        private int _enemiesCleared;
        private int _cardsPlayed;
        private bool _isVictory;
        private int _floorsPassed;
        private int _finalScore;
        private bool _isdefeat;
        private int _rebirthCoinsEarned;

        public GameOverScene(bool isVictory, bool isDefeat)
        {
            _nodesPassed = GameState.NodesVisited;
            _enemiesCleared = GameState.EnemiesDefeated;
            _cardsPlayed = GameState.CardsPlayed;
            _isVictory = isVictory; 
            _isdefeat = isDefeat;

            _floorsPassed = GameState.DepthsCleared;
            _finalScore = GameState.CalculateScore(_isVictory);
            if (_finalScore > UpgradeOrbPermanent.BestScore)
            {
                UpgradeOrbPermanent.BestScore = _finalScore;
            }

            _rebirthCoinsEarned = CalculateRebirthCoins(
                _nodesPassed,
                _floorsPassed,
                GameState.CurrentPlayer.SoulCoins,
                _isVictory);
            UpgradeOrbPermanent.RebirthCoins += _rebirthCoinsEarned;
            GameSaveManager.SavePermanentProgress();
            GameSaveManager.DeleteSave();
        }
                public static int CalculateRebirthCoins(int stagesCleared, int bossesDefeated, int soulCoinsEarned, bool isVictory)
        {
            int baseCoins = (stagesCleared * 15) + (bossesDefeated * 50) + (soulCoinsEarned / 10);
            int victoryBonus = isVictory ? 250 : 0;
            return baseCoins + victoryBonus;
        }
        public void Initialize() { }

        public void LoadContent()
        {
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
            SceneUi.DrawBackground(spriteBatch);
            SceneUi.DrawPanel(spriteBatch, new Rectangle(300, 120, 1320, 660), Color.MediumPurple);

            string titleText = _isVictory ? "Depth Escaped" : "Game Over";
            Color titleColor = _isVictory ? Color.Gold : Color.Red;
            Vector2 titleSize = GlobalContentManager.DefaultFont.MeasureString(titleText);
            spriteBatch.DrawString(GlobalContentManager.DefaultFont, titleText, new Vector2((1920 - titleSize.X) / 2f, 145), titleColor);

            int baseScore = (_nodesPassed * 100) + (_enemiesCleared * 250) +
                (_cardsPlayed * 10) + (_floorsPassed * 1000);
            int survivalScore = _isVictory
                ? (Math.Max(0, GameState.CurrentPlayer.PlayerHP) * 20) +
                  (Math.Max(0, GameState.CurrentPlayer.Sanity) * 10)
                : 0;
            int defeatPenalty = _isdefeat ? 100 : 0;
            bool nightmare = GameState.CurrentPlayer.IsNightmareMode;
            string breakdown =
                $"Nodes visited ({_nodesPassed}): +{_nodesPassed * 100}\n" +
                $"Enemies defeated ({_enemiesCleared}): +{_enemiesCleared * 250}\n" +
                $"Cards played ({_cardsPlayed}): +{_cardsPlayed * 10}\n" +
                $"Depths cleared ({_floorsPassed}): +{_floorsPassed * 1000}\n" +
                (_isVictory ? $"Escape bonus (HP + Sanity): +{survivalScore}\n" : "") +
                (_isdefeat ? "Defeat penalty: -100\n" : "") +
                (nightmare ? $"Nightmare multiplier: x2 ({baseScore + survivalScore - defeatPenalty} x 2)\n" : "") +
                $"FINAL SCORE: {_finalScore}\n" +
                $"Best Score: {UpgradeOrbPermanent.BestScore}\n" +
                $"Ascended Coins: {UpgradeOrbPermanent.AscendedCoins}\n" +
                $"Rebirth Coins earned: +{_rebirthCoinsEarned}";

            spriteBatch.DrawString(GlobalContentManager.DefaultFont, breakdown, new Vector2(420, 230), Color.White);

            spriteBatch.DrawString(GlobalContentManager.DefaultFont, "Press 'I' to return to Title Screen", new Vector2(760, 850), Color.Yellow);

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}