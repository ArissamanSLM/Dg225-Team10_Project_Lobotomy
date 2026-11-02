using DreamSlayerV2.Core;
using DreamSlayerV2.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DreamSlayerV2.Scenes
{
    public class SceneStory1 : IScene
    {
        private readonly StoryDialogue _dialogue = new StoryDialogue(
            "I was a newly mage accepted in this magic school. Cleaning in the library as I were punish for having the lowest score in the test. Very stupid punishment if you think.");

        public static SoundEffectInstance _soundInstance;

        public void Initialize()
        {
            _dialogue.Initialize();
        }

        public void LoadContent()
        {
            _soundInstance = MusicManager.PlayLoopingStoryMusic(GlobalContentManager.TitleMusicEffect);
        }

        public void Update(GameTime gameTime)
        {
            if (_dialogue.Update(gameTime))
            {
                SceneManager.ChangeScene(new SceneStory2());
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _dialogue.Draw(spriteBatch, GlobalContentManager.SceneStoryBackground1);
        }

        public void UnloadContent() { }
    }
}
