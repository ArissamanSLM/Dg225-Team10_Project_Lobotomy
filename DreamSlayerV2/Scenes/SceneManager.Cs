using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DreamSlayerV2.Managers;
using DreamSlayerV2.Scenes;

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
                UpdateMusic(_currentScene);
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

        private static void UpdateMusic(IScene scene)
        {
            switch (scene)
            {
                case TitleScene:
                    MusicManager.PlaySong(GlobalContentManager.NightmareSong);
                    break;
                case NodeSelectScene:
                    MusicManager.PlayLoopingMusic(GlobalContentManager.NodeSelectMusic, volumeOverride: 1f);
                    break;
                case BossScene bossScene:
                    MusicManager.PlaySong(bossScene.IsPhase2Dialogue
                        ? GlobalContentManager.BossPhase2Song
                        : GlobalContentManager.BossPhase1Song);
                    break;
                case BattleScene battleScene when battleScene.IsEliteBattle:
                    MusicManager.PlaySong(GlobalContentManager.NightmareSong);
                    break;
                case BattleScene battleScene when battleScene.IsBossRoom:
                    if (!MusicManager.IsPlayingSong(GlobalContentManager.BossPhase1Song) &&
                        !MusicManager.IsPlayingSong(GlobalContentManager.BossPhase2Song))
                    {
                        MusicManager.PlaySong(GlobalContentManager.BossPhase1Song);
                    }
                    break;
                case SceneStory1:
                case SceneStory2:
                case SceneStory3:
                case SceneStory4:
                case SceneStory5:
                case GameOverScene:
                    MusicManager.StopSong();
                    break;
                case PlayerStatsScene:
                    break;
            }
        }
    }
}