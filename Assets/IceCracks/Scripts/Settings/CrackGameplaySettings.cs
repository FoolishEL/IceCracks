using System;
using System.Collections.Generic;
using UnityEngine;

namespace IceCracks.Settings
{
    [CreateAssetMenu(menuName = "IceCracks/Settings")]
    public class CrackGameplaySettings : ScriptableObject
    {
        private const string CRACKS_ID_SELECTED = "currentSelectedCrackId";
        
        [SerializeField] private List<IceSettings> settings;

        public bool TryGetCurrentSettings(out IceSettings? currentSettings)
        {
            currentSettings = null;
            int settingsToGetId = PlayerPrefs.GetInt(CRACKS_ID_SELECTED, 0);
            if (settings.Count >= settingsToGetId + 1)
            {
                currentSettings = settings[settingsToGetId];
                return true;
            }
            return false;
        }

        public void SetCurrentSettings(int id)
        {
            PlayerPrefs.SetInt(CRACKS_ID_SELECTED, id);
            PlayerPrefs.Save();
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Editor set next")]
        private void SetNext()
        {
            int id = PlayerPrefs.GetInt(CRACKS_ID_SELECTED, 0);
            id++;
            if (id >= settings.Count)
                id = 0;
            SetCurrentSettings(id);
        }
        #endif
    }
    
    [Serializable]
    public struct IceSettings
    {
        public Material material;
        //public Color cracksColor;
        public List<AudioClip> audioSetCracks;
        public List<AudioClip> audioSetFreeze;
    }
}