using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Orion_Desktop
{
    /// <summary>Represents an audio managment instance</summary>
    internal class AudioCenter
    {
        /// <summary>Dictionary of sounds</summary>
        private static Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();

        /// <summary>Dictionary of musics</summary>
        private static Dictionary<string, Music> musics = new Dictionary<string, Music>();

        /// <summary>Loads every sound and musics of the game</summary>
        internal static void Init()
        {
            InitAudioDevice();
            LoadSounds();
            LoadMusics();

            // Set ambient volume
            SetMusicVolume("ambient", 0.5f);
        }

        /// <summary>Plays a sound</summary>
        /// <param name="key">Dictionary key of the sound</param>
        internal static void PlaySound(string key)
        {
            Raylib.PlaySound(sounds[key]);
        }

        /// <summary>Plays a sound on loop</summary>
        /// <param name="key">Dictionary key of the sound</param>
        internal static void PlaySoundLoop(string key)
        {
            if (!Raylib.IsSoundPlaying(sounds[key]))
            {
                Raylib.PlaySound(sounds[key]);
            }
        }

        /// <summary>Stops a playing sound</summary>
        /// <param name="key">Dictionary key of the sound</param>
        internal static void StopSound(string key)
        {
            Raylib.StopSound(sounds[key]);
        }

        /// <summary>Plays a selected music</summary>
        /// <param name="key">Dictionary key of the music</param>
        internal static void PlayMusic(string key)
        {
            PlayMusicStream(musics[key]);
        }

        internal static void PlayMusicLooped(string key)
        {
            if (!IsMusicPlaying(key))
            {
                PlayMusic(key);
            }
        }

        /// <summary>Updates a selected music</summary>
        /// <param name="key">Dictionary key of the music</param>
        internal static void UpdateMusic(string key)
        {
            UpdateMusicStream(musics[key]);
        }

        /// <summary>Checks if a music is already playing.</summary>
        /// <param name="key">Key of the music to check.</param>
        /// <returns><see langword="true"/> if the music is indeed playing. <see langword="false"/> otherwise.</returns>
        internal static bool IsMusicPlaying(string key)
        {
            return IsMusicStreamPlaying(musics[key]);
        }

        /// <summary>Checks if a sound is already playing.</summary>
        /// <param name="key">Key of the sound to check.</param>
        /// <returns><see langword="true"/> if the sound is indeed playing. <see langword="false"/> otherwise.</returns>
        internal static bool IsSoundPlaying(string key)
        {
            return Raylib.IsSoundPlaying(sounds[key]);
        }

        /// <summary>Sets music volume</summary>
        /// <param name="key">Dictionary key of the music</param>
        /// <param name="volume">Volume to set</param>
        internal static void SetMusicVolume(string key, float volume)
        {
            Raylib.SetMusicVolume(musics[key], volume);
        }

        /// <summary>Sets music pitch</summary>
        /// <param name="key">Dictionary key of the music</param>
        /// <param name="volume">Pitch to set</param>
        internal static void SetMusicPitch(string key, float pitch)
        {
            Raylib.SetMusicPitch(musics[key], pitch);
        }

        /// <summary>Sets music pitch</summary>
        /// <param name="key">Dictionary key of the music</param>
        /// <param name="volume">Pitch to set</param>
        internal static void SetSoundPitch(string key, float pitch)
        {
            Raylib.SetSoundPitch(sounds[key], pitch);
        }


        /// <summary>Sets sound volume</summary>
        /// <param name="key">Dictionary key of the sound</param>
        /// <param name="volume">Volume to set</param>
        internal static void SetSoundVolume(string key, float volume)
        {
            Raylib.SetSoundVolume(sounds[key], volume);
        }
        
        private static void LoadMusics()
        {
            musics = new Dictionary<string, Music>
            {
                {"ambient", LoadMusicStream("assets/audio/ambient.mp3") },
            };
        }

        private static void LoadSounds()
        {
            sounds = new Dictionary<string, Sound>()
            {
                {"button_hover", LoadSound("assets/audio/button_hover.wav") },
                {"button_click", LoadSound("assets/audio/button_click.wav") },
            };
        }
    }
}