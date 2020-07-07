
using UnityEngine;

namespace of2.Audio
{
    public class AudioPreferences : ScriptableObject
    {
        private static AudioPreferences m_LoadedInstance;

        public static AudioPreferences Instance
        {
            get
            {
                if (m_LoadedInstance == null)
                {
                    m_LoadedInstance = ScriptableObjectHelper.LoadOrCreateScriptableObjectInResources<AudioPreferences>("AudioPreferences");
                }
                return m_LoadedInstance;
            }
        }

        public string AudioManagerProjectSpecificFolder = "Assets/of2/Audio/";
        public string AudioManagerDataPrefabName = "AudioManagerData";
        public string AudioEnumName = "AudioList";
        
        public const string AudioManagerInstallerPrefabName = "AudioManagerInstallerPrefab";
        public const string ProjectSpecificAudioManagerInstallerName = "ProjectSpecificAudioManagerInstaller";
        
//        public string AudioManagerDataPath = "Assets/Global/Audio/AudioManagerData.prefab";
//
//        public string AudioEnumPath = "Assets/Global/Audio/Scripts/AudioList.cs";

        public string MissingSoundTriggerId = "GEN_Missing_Sound";

        public bool PlayMissingSound = false;

        public AudioSource3DData Default3DSettings;

        public string AudioManagerDataPrefabPath => AudioManagerProjectSpecificFolder + AudioManagerDataPrefabName + ".prefab";
        
        void Reset()
        {
            Default3DSettings = new AudioSource3DData(1f, 100f, 20000f, 1f, AudioRolloffMode.Logarithmic, 0f, 1f, true, true);
        }
    }
}