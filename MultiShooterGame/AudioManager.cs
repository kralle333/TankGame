using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MultiShooterGame
{
    class AudioManager
    {
        private static Dictionary<string, SoundEffect> _sfxs = new Dictionary<string, SoundEffect>();
        private static Dictionary<string, SoundEffect> _musics = new Dictionary<string, SoundEffect>();
        private static SoundEffectInstance _activeMusic = null;
        private static bool hasBeenInitialized = false;
        public static void Initialize(ContentManager contentManager)
        {
            if(!hasBeenInitialized)
            {
                string[] sfxFiles = Directory.GetFiles("Content\\Sfx");
                foreach (string file in sfxFiles)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    _sfxs[name] = contentManager.Load<SoundEffect>(name);
                }
                hasBeenInitialized = true;
            }            
        }
        public static SoundEffectInstance GetSFXInstance(string sfxName)
        {
            return _sfxs[sfxName].CreateInstance();
        }
        public static void PlaySFX(string sfxName, float volume)
        {
            SoundEffectInstance sei = _sfxs[sfxName].CreateInstance();
            sei.Volume = volume;
            sei.Play();
            sei.IsLooped = false;
        }
        public static void PlayMusic(string musicName, bool isLooped, float volume)
        {
            if (_activeMusic == null)
            {
                _activeMusic.Dispose();
            }
            _activeMusic = _musics[musicName].CreateInstance();
            _activeMusic.Play();
            _activeMusic.IsLooped = isLooped;
            _activeMusic.Volume = volume;
        }
    }
}
