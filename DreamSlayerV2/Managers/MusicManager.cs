using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace DreamSlayerV2.Managers
{
    public static class MusicManager
    {
        private const float StoryMusicVolumeScale = 0.15f;
        private static Song _currentSong;
        private static SoundEffectInstance _storyMusicInstance;

        public static float MusicVolume { get; private set; } = 1f;

        public static void SetMusicVolume(float volume)
        {
            MusicVolume = MathHelper.Clamp(volume, 0f, 1f);
            MediaPlayer.Volume = MusicVolume;

            if (_storyMusicInstance != null)
            {
                _storyMusicInstance.Volume = MusicVolume * StoryMusicVolumeScale;
            }
        }

        public static void PlaySong(Song song)
        {
            ArgumentNullException.ThrowIfNull(song);

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = MusicVolume;

            if (MediaPlayer.State != MediaState.Playing || !ReferenceEquals(_currentSong, song))
            {
                MediaPlayer.Play(song);
                _currentSong = song;
            }
        }

        public static bool IsPlayingSong(Song song)
        {
            return MediaPlayer.State == MediaState.Playing && ReferenceEquals(_currentSong, song);
        }

        public static void StopSong()
        {
            MediaPlayer.Stop();
            _currentSong = null;
        }

        public static SoundEffectInstance PlayLoopingStoryMusic(SoundEffect soundEffect)
        {
            ArgumentNullException.ThrowIfNull(soundEffect);
            StopLoopingStoryMusic(_storyMusicInstance);

            _storyMusicInstance = soundEffect.CreateInstance();
            _storyMusicInstance.Volume = MusicVolume * StoryMusicVolumeScale;
            _storyMusicInstance.Pitch = 0.10f;
            _storyMusicInstance.IsLooped = true;
            _storyMusicInstance.Play();
            return _storyMusicInstance;
        }

        public static void StopLoopingStoryMusic(SoundEffectInstance instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.Stop();
            if (ReferenceEquals(_storyMusicInstance, instance))
            {
                _storyMusicInstance = null;
            }
        }
    }
}
