using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DreamSlayerV2.Core
{
    public static class SceneManager
    {
        private static IScene _currentScene;
        private static IScene _nextScene;
        // Queue a scene change to safely handle transitions outside tight loops
        public static void ChangeScene(IScene newScene)
        {
            _nextScene = newScene;
        }

        public static void Update(GameTime gameTime)
        {
            if (_nextScene != null)
            {
                _currentScene?.UnloadContent();
                _currentScene = _nextScene;
                _currentScene.Initialize();
                _currentScene.LoadContent();
                _nextScene = null;
            }

            _currentScene?.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            _currentScene?.Draw(spriteBatch);
        }
    }
}