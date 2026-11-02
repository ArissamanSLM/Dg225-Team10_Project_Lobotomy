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
        private static float? _currentSongVolume;
        private static SoundEffectInstance _musicInstance;
        private static float? _musicInstanceVolume;
        private static SoundEffectInstance _storyMusicInstance;

        public static float MusicVolume { get; private set; } = 1f;

        public static void SetMusicVolume(float volume)
        {
            MusicVolume = MathHelper.Clamp(volume, 0f, 1f);
            MediaPlayer.Volume = _currentSongVolume ?? MusicVolume;

            if (_storyMusicInstance != null)
            {
                _storyMusicInstance.Volume = MusicVolume * StoryMusicVolumeScale;
            }

            if (_musicInstance != null)
            {
                _musicInstance.Volume = _musicInstanceVolume ?? MusicVolume;
            }
        }

        public static void PlaySong(Song song, bool restart = false, float? volumeOverride = null)
        {
            ArgumentNullException.ThrowIfNull(song);
            _currentSongVolume = volumeOverride.HasValue
                ? MathHelper.Clamp(volumeOverride.Value, 0f, 1f)
                : null;

            bool shouldPlay = restart ||
                MediaPlayer.State != MediaState.Playing ||
                !ReferenceEquals(_currentSong, song);
            if (shouldPlay)
            {
                StopLoopingMusic();
                MediaPlayer.Stop();
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = _currentSongVolume ?? MusicVolume;
                MediaPlayer.Play(song);
                _currentSong = song;
            }
            else
            {
                MediaPlayer.Volume = _currentSongVolume ?? MusicVolume;
                MediaPlayer.IsRepeating = true;
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
            _currentSongVolume = null;
            StopLoopingMusic();
        }

        public static void PlayLoopingMusic(SoundEffect soundEffect, float? volumeOverride = null)
        {
            ArgumentNullException.ThrowIfNull(soundEffect);
            StopLoopingMusic();
            MediaPlayer.Stop();
            _currentSong = null;
            _currentSongVolume = null;

            _musicInstanceVolume = volumeOverride.HasValue
                ? MathHelper.Clamp(volumeOverride.Value, 0f, 1f)
                : null;
            _musicInstance = soundEffect.CreateInstance();
            _musicInstance.Volume = _musicInstanceVolume ?? MusicVolume;
            _musicInstance.IsLooped = true;
            _musicInstance.Play();
        }

        private static void StopLoopingMusic()
        {
            _musicInstance?.Stop();
            _musicInstance = null;
            _musicInstanceVolume = null;
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
