using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamSlayerV2.Scenes
{
    public class SceneStory3 : IScene
    {
        private readonly StoryDialogue _dialogue = new StoryDialogue(
            "As you look at the book that dropped onto you, the cover sure is interesting. I'm sure hope there's no \"DO NOT OPEN\" tape on it.");

        public void Initialize()
        {
            _dialogue.Initialize();
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            if (_dialogue.Update(gameTime))
            {
                SceneManager.ChangeScene(new SceneStory4());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _dialogue.Draw(spriteBatch, GlobalContentManager.SceneStoryBackground2);
        }

        public void UnloadContent() { }
    }
}
