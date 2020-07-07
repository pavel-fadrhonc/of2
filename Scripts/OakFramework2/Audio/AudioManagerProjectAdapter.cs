using System;
using of2.AssetManagement;
using of2.Audio;
using UnityEngine;
using Zenject;

namespace of2.Audio
{
    public interface IAudioManagerHelper
    {
        string AudioEnumToStringId(int enumAsInt);
    }
    
    [Serializable]
    public class HelperPlayConfig
    {
        public int soundEnum;
        public float volume = 1f;
        public float delay = 0f;
    }

    public interface IAudioManager
    {
        AudioClipHandle PlayAudio(int soundIdEnum);
        AudioClipHandle PlayAudio(HelperPlayConfig config);
        AudioClipHandle PlayAudio(string soundIdEnumName);
        AudioClipHandle PlayAudio(int soundIdEnum, AudioClipHandle oldHandle);
        void PlayAudioQueued(int soundIdEnum);
        AudioClipHandle GetAudio(int soundIdEnum);
        AudioClipHandle GetAudio(int soundIdEnum, Transform target);
        AudioClipHandle PlayAudio(PlayConfig c);
        AudioClipHandle StopAudio(AudioClipHandle clipHandler);
        AudioClipHandle StopAudioFadeout(AudioClipHandle clipHandler, float fadeOut);
        
        void SetMixerParameter(string paramName, float value);
    }
    
    /// <summary>
    /// Connects project specific script (AudioList with project specific hooks) with general AudioManager from plugins
    /// </summary>
    public class AudioManagerProjectAdapter : MonoBehaviour, IAudioManager
    {
        private IAudioManagerHelper _audioManagerHelper;
        private IAssetManager _assetManager;
        private GlobalAudioManager _gam;

        private void Awake()
        {
            _gam = GetComponentInChildren<GlobalAudioManager>();
        }
        
        [Inject]
        public void Construct(IAudioManagerHelper audioManagerHelper)
        {
            _audioManagerHelper = audioManagerHelper;
        }

        public AudioClipHandle PlayAudio(string soundIdEnumName)
        {
            return PlayAudio(new PlayConfig { SoundID = soundIdEnumName });
        }

        public void PlayAudioQueued(int soundIdEnum)
        {
            if (soundIdEnum == -1)
                return;
            
            _gam.PlayQueuedAudio(new PlayConfig { SoundID = _audioManagerHelper.AudioEnumToStringId(soundIdEnum) });
        }
    
        public void SetMixerParameter(string paramName, float value)
        {
            _gam.SetMixerParameter(paramName, value);
        }
    
        public AudioClipHandle PlayAudio(int soundIdEnum, AudioClipHandle oldHandle)
        {
            StopAudio(oldHandle);
            return PlayAudio(new PlayConfig { SoundID = _audioManagerHelper.AudioEnumToStringId(soundIdEnum) });
        }
    
        public AudioClipHandle PlayAudio(int soundIdEnum)
        {
            return PlayAudio(new PlayConfig { SoundID = _audioManagerHelper.AudioEnumToStringId(soundIdEnum) });
        }
    
        public AudioClipHandle PlayAudio(HelperPlayConfig config)
        {
            if (config.soundEnum == -1)
                return null;
            
            var playConfig = new PlayConfig()
            {
                SoundID = _audioManagerHelper.AudioEnumToStringId(config.soundEnum),
                Volume = config.volume,
                Delay = config.delay
            };

            return PlayAudio(playConfig);
        }
    
        public AudioClipHandle GetAudio(int soundIdEnum)
        {
            return GetAudio(soundIdEnum, null);
        }
    
        public AudioClipHandle GetAudio(int soundIdEnum, Transform target)
        {
            return PlayAudio(new PlayConfig { SoundID = _audioManagerHelper.AudioEnumToStringId(soundIdEnum), StartPaused = true, Target = target });
        }

        public AudioClipHandle PlayAudio(PlayConfig c)
        {
            return _gam.PlayAudio(c);
        }
    
        public AudioClipHandle StopAudio(AudioClipHandle clipHandler)
        {
            if (clipHandler == null)
            {
                D.AudioWarning("Trying to stop null AudioClipHandle!");
                return null;
            }
    
            clipHandler.StopAndRelease();
            return null;
        }
    
        public AudioClipHandle StopAudioFadeout(AudioClipHandle clipHandler, float fadeOut)
        {
            if (clipHandler == null)
            {
                D.AudioWarning("Trying to stop null AudioClipHandle!");
                return null;
            }
    
            clipHandler.ChangeVolume(AnimationCurve.EaseInOut(0, 1, fadeOut, 0), fadeOut, true);
            return null;
        }                
    }
}