using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DreamSlayerV2.Scenes
{
    public class SceneStory3 : IScene
    {
        private float _transitionProgress;
        private bool _isTransitioning;

        private readonly StoryDialogue _dialogue = new StoryDialogue(
            "As you look at the book that dropped onto you, the cover sure is interesting. I'm sure hope there's no \"DO NOT OPEN\" tape on it.");

        public void Initialize()
        {
            _dialogue.Initialize();
            _transitionProgress = 0f;
            _isTransitioning = false;
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            if (_isTransitioning)
            {
                _transitionProgress = Math.Min(
                    1f,
                    _transitionProgress + (float)gameTime.ElapsedGameTime.TotalSeconds * 2f);

                if (_transitionProgress >= 1f)
                {
                    SceneManager.ChangeScene(new SceneStory4());
                }

                return;
            }

            if (_dialogue.Update(gameTime))
            {
                _isTransitioning = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _dialogue.Draw(spriteBatch, GlobalContentManager.SceneStoryBackground3, 1f - _transitionProgress);

            if (_transitionProgress > 0f && GlobalContentManager.Portal != null)
            {
                float portalSize = 180f + 340f * _transitionProgress;
                var portalBounds = new Rectangle(
                    (int)((1920f - portalSize) / 2f),
                    (int)((1080f - portalSize) / 2f),
                    (int)portalSize,
                    (int)portalSize);

                spriteBatch.Begin();
                spriteBatch.Draw(GlobalContentManager.Portal, portalBounds, Color.White * _transitionProgress);
                spriteBatch.End();
            }
        }

        public void UnloadContent() { }
    }
}
