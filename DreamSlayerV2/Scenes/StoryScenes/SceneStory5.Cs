using System;
using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamSlayerV2.Scenes
{
    public class SceneStory5 : IScene
    {
        private readonly StoryDialogue _dialogue = new StoryDialogue(
            "Before I knew it, I was in the other world. I can tell this place is really dangerous. I need to get out as soon as possible... or maybe stay a bit longer and learn as many forbidden spells as I can.");

        public void Initialize()
        {
            _dialogue.Initialize();
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            if (SceneStory1._soundInstance != null)
            {
                SceneStory1._soundInstance.Pitch = Math.Max(
                    -1f,
                    SceneStory1._soundInstance.Pitch - (float)gameTime.ElapsedGameTime.TotalSeconds * 0.048f);
            }

            if (_dialogue.Update(gameTime))
            {
                SceneManager.ChangeScene(new NodeSelectScene());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _dialogue.Draw(spriteBatch, GlobalContentManager.CaveBackground);
        }

        public void UnloadContent()
        {
            SceneStory1._soundInstance?.Stop();
        }
    }
}
