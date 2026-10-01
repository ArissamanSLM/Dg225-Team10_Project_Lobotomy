using DreamSlayerV2.Core;
using DreamSlayerV2.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DreamSlayerV2.Scenes
{
    public class RewardScene : IScene
    {
        private SpriteFont _font;
        private string promptText = "Move to Next Node";
        private Vector2 promptPos = new Vector2(100, 600);
        private MouseState _previousMouseState;

        private int _soulReward;
        private Random _rand = new Random();
        private bool _isEliteOrBoss = false;
        private string _enemyTypeLabel = "Enemy";

        // Constructor to dynamically set rewards based on what was defeated
        // roomType: 0 = Normal Encounter, 1 = Elite, 2 = Boss
        public RewardScene(int rewardTier)
        {
            if (rewardTier == 0) // Normal Encounter (20-30 Soul Coins)
            {
                _soulReward = _rand.Next(20, 31);
                _isEliteOrBoss = false;
                _enemyTypeLabel = "Enemy";
               
            }
            else if (rewardTier == 1) // Elite (50-75 Soul Coins + Relic chance for full version)
            {
                _soulReward = _rand.Next(50, 76);
                _isEliteOrBoss = true;
                _enemyTypeLabel = "Elite";
            }
            else if (rewardTier == 2) // Boss (100+ Soul Coins)
            {
                _soulReward = _rand.Next(100, 151);
                _isEliteOrBoss = true;
                _enemyTypeLabel = "Boss";
            }
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

            Vector2 textSize = _font.MeasureString(promptText);
            Rectangle promptBounds = new Rectangle((int)promptPos.X, (int)promptPos.Y, (int)textSize.X, (int)textSize.Y);

            bool isHovered = promptBounds.Contains(currentMouseState.Position);
            bool isClicked = isHovered && currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;

            if (isClicked)
            {
                SceneManager.ChangeScene(new NodeSelectScene());
                if (DreamSlayerV2.Core.GameState.CurrentPlayer != null)
                {
                    DreamSlayerV2.Core.GameState.CurrentPlayer.SoulCoins += _soulReward;
                }
            }

            _previousMouseState = currentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Draw Defeat text based on enemy tier
            spriteBatch.DrawString(_font, $"You Defeated an {_enemyTypeLabel}!", new Vector2(100, 150), Color.Purple);

            // Draw Soul Coins reward text
            spriteBatch.DrawString(_font, $"You gained +{_soulReward} Soul Coins", new Vector2(100, 220), Color.Yellow);

            // Relic reward text (Active for Elite/Boss if you want to test it, or ready for the full version update)
            if (_isEliteOrBoss)
            {
                // Full version relic hook: sprite/text placeholder
                spriteBatch.DrawString(_font, "Bonus Reward: Random Relic [Full Version Feature]", new Vector2(100, 290), Color.White);
            }

            // Draw interactive prompt to move back to node map
            Vector2 textSize = _font.MeasureString(promptText);
            Rectangle promptBounds = new Rectangle((int)promptPos.X, (int)promptPos.Y, (int)textSize.X, (int)textSize.Y);
            Color promptColor = promptBounds.Contains(Mouse.GetState().Position) ? Color.LightYellow : Color.Yellow;

            spriteBatch.DrawString(_font, promptText, promptPos, promptColor);

            spriteBatch.End();
        }

        public void UnloadContent() { }
    }
}