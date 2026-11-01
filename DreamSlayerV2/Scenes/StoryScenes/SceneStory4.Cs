using DreamSlayerV2.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamSlayerV2.Scenes
{
    public class SceneStory4 : IScene
    {
        private readonly StoryDialogue _dialogue = new StoryDialogue(
            "As the book opened, power burst out from it. Ah... I am 100% sure it had a \"DO NOT OPEN\" tape and I missed it. Oh well.");

        public void Initialize()
        {
            _dialogue.Initialize();
            if (SceneStory1._soundInstance != null)
            {
                SceneStory1._soundInstance.Pitch = -0.2f;
            }
        }

        public void LoadContent() { }

        public void Update(GameTime gameTime)
        {
            if (_dialogue.Update(gameTime))
            {
                SceneManager.ChangeScene(new SceneStory5());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _dialogue.Draw(spriteBatch, GlobalContentManager.CaveBackground);
        }

        public void UnloadContent() { }
    }
}
