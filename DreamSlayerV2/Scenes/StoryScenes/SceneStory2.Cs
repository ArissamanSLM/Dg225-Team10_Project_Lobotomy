using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamSlayerV2.Scenes
{
    public class SceneStory2 : IScene
    {
        private readonly StoryDialogue _dialogue = new StoryDialogue(
            "As I clean, I bump into one of the shelve and a book drop onto your head. It's hurt for sure...");

        public void Initialize()
        {
            _dialogue.Initialize();
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            if (_dialogue.Update(gameTime))
            {
                SceneManager.ChangeScene(new SceneStory3());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _dialogue.Draw(spriteBatch, GlobalContentManager.SceneStoryBackground2);
        }

        public void UnloadContent() { }
    }
}
