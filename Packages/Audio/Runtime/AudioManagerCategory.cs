﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using System.Text.RegularExpressions;
using System.Linq;
#if UNITY_EDITOR
#endif

namespace of2.Audio
{
    /// <summary>
    /// Audio hook definition (or category definition - it's used for both)
    /// </summary>
    [Serializable]
    public class AudioManagerCategory : ISerializationCallbackReceiver
    {
        //////
        /// Information about self
        //////

        [SerializeField]
        private int m_Depth = 0;
        [SerializeField]
        private string m_ID;
        [SerializeField]
        private AudioMixerGroup m_DefaultBus = null;
        [SerializeField]
        private bool m_SoundRandomization = false;
        [SerializeField]
        private bool m_PitchRandomization = false;
        [SerializeField]
        private float m_PitchRandomizationValue = 0.0f;
        [SerializeField]
        private float m_Volume = 1f;
        [SerializeField]
        private float m_StereoPan = 0f;
        [SerializeField]
        private bool m_Loop = false;
        [SerializeField]
        private float m_NextAllowedAudioDelay = -1;

        [SerializeField]
        private AudioFiltersHolder[] m_FiltersHolder;

        [SerializeField]
        private Dictionary<string, AudioManagerCategory> m_Children = new Dictionary<string, AudioManagerCategory>();

        [SerializeField]
        private AudioManagerCategory m_Parent;

        [SerializeField]
        private int m_UniqueId;

        [SerializeField]
        private int m_ParentUniqueId;

        [SerializeField]
        private int m_OrderIdx;

        [SerializeField]
        private int[] m_BlockedByCategoryIds = new int[0];

        private bool m_Foldout = false;

        //////
        /// Audio Data if this is a leaf
        //////

        [SerializeField]
        private List<string> m_AudioData;
        private int m_LastSelected = -1;
        [SerializeField]
        private List<int> m_AudioDataWeight;
        private int m_TotalWeight = -1;

        //////
        /// Getters and setters
        //////

        private void InitFiltersHolderField()
        {
            if (m_FiltersHolder == null || m_FiltersHolder.Length == 0)
            {
                m_FiltersHolder = new AudioFiltersHolder[] { new AudioFiltersHolder() };
            }
        }

        public AudioFiltersHolder AudioFilters
        {
            get { InitFiltersHolderField(); return m_FiltersHolder[0]; }
            set { InitFiltersHolderField(); m_FiltersHolder[0] = value; }
        }

        public AudioMixerGroup DefaultBus { get { return m_DefaultBus; } set { m_DefaultBus = value; } }
        public Dictionary<string, AudioManagerCategory> Children { get { return m_Children; } set { m_Children = value; } }
        public bool Foldout { get { return m_Foldout; } set { m_Foldout = value; } }
        public List<string> AudioData { get { if (m_AudioData == null) m_AudioData = new List<string>(); return m_AudioData; } set { m_AudioData = value; } }
        public List<int> AudioDataWeight { get { if (m_AudioDataWeight == null) m_AudioDataWeight = new List<int>(); return m_AudioDataWeight; } set { m_AudioDataWeight = value; } }
        public int Depth { get { return m_Depth; } set { m_Depth = value; } }
        public AudioManagerCategory Parent { get { return m_Parent; } set { m_Parent = value; if (m_Parent != null) Depth = m_Parent.Depth + 1; else Depth = -1; } }
        public string ID
        {
            get { return m_ID; }
            set
            {
                if (m_ID != value)
                {
                    m_ID = value;
                    if (m_Parent != null) m_Parent.RebuildChildren();
                }

            }
        }

        public int UniqueID { get { return m_UniqueId; } set { m_UniqueId = value; } }

        public int OrderIdx { get { return m_OrderIdx; } set { m_OrderIdx = value; } }

        public int[] BlockedByCategoryIds { get { return m_BlockedByCategoryIds; } set { m_BlockedByCategoryIds = value; } }

        public bool SoundRandomization { get { return m_SoundRandomization; } set { m_SoundRandomization = value; } }
        public bool PitchRandomization { get { return m_PitchRandomization; } set { m_PitchRandomization = value; } }
        public float Volume { get { return m_Volume; } set { m_Volume = value; } }
        public float StereoPan { get { return m_StereoPan; } set { m_StereoPan = value; } }

        [SerializeField]
        private int m_EnabledFilters = (int)EAudioFilter.None;

        public void UpdateFilterHolder()
        {
            if (m_EnabledFilters == 0)
            {
                m_FiltersHolder = null;
            }
        }

        // Filters aka Audio Source Effects
        public bool UseChorusFilter
        {
            get { return (m_EnabledFilters & (int)EAudioFilter.Chorus) != 0; }
            set { m_EnabledFilters = m_EnabledFilters & ~((int)EAudioFilter.Chorus) | (value ? (int)EAudioFilter.Chorus : 0); }
        }

        public bool UseDistortionFilter
        {
            get { return (m_EnabledFilters & (int)EAudioFilter.Distortion) != 0; }
            set { m_EnabledFilters = m_EnabledFilters & ~((int)EAudioFilter.Distortion) | (value ? (int)EAudioFilter.Distortion : 0); }
        }

        public bool UseEchoFilter
        {
            get { return (m_EnabledFilters & (int)EAudioFilter.Echo) != 0; }
            set { m_EnabledFilters = m_EnabledFilters & ~((int)EAudioFilter.Echo) | (value ? (int)EAudioFilter.Echo : 0); }
        }

        public bool UseHighPassFilter
        {
            get { return (m_EnabledFilters & (int)EAudioFilter.HighPass) != 0; }
            set { m_EnabledFilters = m_EnabledFilters & ~((int)EAudioFilter.HighPass) | (value ? (int)EAudioFilter.HighPass : 0); }
        }

        public bool UseLowPassFilter
        {
            get { return (m_EnabledFilters & (int)EAudioFilter.LowPass) != 0; }
            set { m_EnabledFilters = m_EnabledFilters & ~((int)EAudioFilter.LowPass) | (value ? (int)EAudioFilter.LowPass : 0); }
        }

        public bool UseReverbFilter
        {
            get { return (m_EnabledFilters & (int)EAudioFilter.Reverb) != 0; }
            set { m_EnabledFilters = m_EnabledFilters & ~((int)EAudioFilter.Reverb) | (value ? (int)EAudioFilter.Reverb : 0); }
        }

        public bool UseFadeOut
        {
            get { return (m_EnabledFilters & (int)EAudioFilter.FadeOut) != 0; }
            set { m_EnabledFilters = m_EnabledFilters & ~((int)EAudioFilter.FadeOut) | (value ? (int)EAudioFilter.FadeOut : 0); }
        }

        public bool UseFadeIn
        {
            get { return (m_EnabledFilters & (int)EAudioFilter.FadeIn) != 0; }
            set { m_EnabledFilters = m_EnabledFilters & ~((int)EAudioFilter.FadeIn) | (value ? (int)EAudioFilter.FadeIn : 0); }
        }

        public AudioManagerCategory(string id, int uniqueId)
        {
            this.ID = id;
            this.UniqueID = uniqueId;
        }


        public string RandomWeightedAudioData()
        {
            // Invalid data
            if (AudioData.Count <= 1)
                return GetNextOrderedAudioData();
            // If needed init AudioDataWeigt
            if (AudioDataWeight.Count <= 1 || m_AudioData.Count != AudioDataWeight.Count)
            {
                AudioDataWeight.Clear();
                for (int i = 0; i < m_AudioData.Count; i++)
                {
                    AudioDataWeight.Add(1);
                }
            }

            // Check first weight calculations
            if (m_TotalWeight == -1)
            {
                m_TotalWeight = 0;
                foreach (int weight in AudioDataWeight)
                {
                    m_TotalWeight += weight;
                }
            }

            // Ajust weight of last selected
            if (m_LastSelected != -1)
            {
                m_TotalWeight -= AudioDataWeight[m_LastSelected];
                AudioDataWeight[m_LastSelected] = 0;
            }

            int cumulative = 0;
            int diceRoll = UnityEngine.Random.Range(0, m_TotalWeight);
            for (int i = 0; i < AudioDataWeight.Count; i++)
            {
                cumulative += m_AudioDataWeight[i];
                if (diceRoll < cumulative)
                {
                    if (m_LastSelected != -1)
                        AudioDataWeight[m_LastSelected] = 1;
                    m_LastSelected = i;
                    return AudioData[i];
                }
            }
            if (m_LastSelected != -1)
                AudioDataWeight[m_LastSelected] = 1;
            return GetNextOrderedAudioData();
        }

        public string GetNextOrderedAudioData()
        {
            m_LastSelected++;
            m_LastSelected %= AudioData.Count();
            return AudioData[m_LastSelected];
        }

        /// <summary>
        /// Check if the current node is a leaf (aka has no children)
        /// </summary>
        /// <returns>True if it is a leaf, false if it isn't a leaf</returns>
        public bool IsLeaf()
        {
            return (m_Children == null || m_Children.Count == 0);
        }

        /// <summary>
        /// Check if any of the brothers has a given id
        /// </summary>
        /// <param name="id">ID to check</param>
        /// <returns>True if the ID was found in at least one brother, false if it wasn't found</returns>       
        private bool IDInBrothers(string id)
        {
            if (m_Parent == null) return false;
            if (m_Parent.CountIDInChildren(id) > 0) return true;
            return false;
        }

        /// <summary>
        /// Count the amount of children that already have the current ID (with or without a number suffix).
        /// </summary>
        /// <param name="id">The ID to check</param>
        /// <returns>The amount of children with the given ID</returns>
        public int CountIDInChildren(string id)
        {
            int amount = 0;
            /* D.AudioLog("Current amount: "+ amount);
            D.AudioLog("Current initial ID: " + id);
            D.AudioLog(Children != null);*/
            if (Children != null && Children.Count > 0)
            {
                //D.AudioLog(Children.Count);
                List<AudioManagerCategory> buffer = Children.Values.ToList();
                foreach (AudioManagerCategory entry in buffer)
                {
                    string realID = new Regex(@"\d+$").Replace(entry.ID, ""); // just getting the count number out of the end of the ID string to see if they have the same root
                    if (realID == id)
                    {
                        /* 
                        D.AudioLog("Current entry ID: " + entry.ID);
                        D.AudioLog("Current real ID: " + realID);
                        */
                        amount++;
                    }
                }
            }
            //D.AudioLog("Current final amount: " + amount);
            return amount;
        }

        public void RebuildChildren()
        {
            List<AudioManagerCategory> buffer = Children.Values.ToList();
            Children.Clear();
            foreach (AudioManagerCategory entry in buffer)
            {
                string id = entry.ID;
                int count = 0;
                while (Children.ContainsKey(id))
                {
                    id = entry.ID + count.ToString();
                    count++;
                }
                Children.Add(id, entry);
            }
        }

        /// <summary>
        /// Get the child with a given ID
        /// </summary>
        /// <param name="id">the id to search for</param>
        /// <returns>The child with given IDor null if none is found</returns>
        public AudioManagerCategory GetChild(string id)
        {
            if (m_Children.ContainsKey(id))
                return this.Children[id];
            return null;
        }

        /// <summary>
        /// Get a list of all immediate children that are current leafs
        /// </summary>
        /// <returns>A valid list with all children that are leafs. If there is no leaf children the list will be empty.</returns>
        public List<AudioManagerCategory> GetLeafChildren()
        {
            List<AudioManagerCategory> m_Children;
            m_Children = new List<AudioManagerCategory>();
            if (Children != null && Children.Count > 0)
                foreach (KeyValuePair<string, AudioManagerCategory> entry in Children)
                    if (entry.Value.IsLeaf()) m_Children.Add(entry.Value);
            return m_Children;
        }


        public List<AudioManagerCategory> GetAllLeafs()
        {
            //D.AudioLog("GetAllLeafs " + ID + ", depth: " + Depth);

            var leafs = new List<AudioManagerCategory>();
            if (Children != null && Children.Count > 0)
            {
                foreach (KeyValuePair<string, AudioManagerCategory> entry in Children)
                {
                    if (entry.Value.IsLeaf())
                    {
                        leafs.Add(entry.Value);
                    }
                    else
                    {
                        var childLeafs = entry.Value.GetAllLeafs();
                        leafs = leafs.Concat(childLeafs).ToList();
                    }
                }
            }
            // D.AudioLog("GetAllLeafs " + ID + " - leafs: "+ leafs.Count + ", depth: "+ Depth);
            return leafs;
        }


        /// <summary>
        /// Get a list of all children IDs that are current leafs
        /// </summary>
        /// <returns>A valid list with all children that are leafs. If there is no leaf children the list will be empty.</returns>
        public List<string> GetLeafChildrenName()
        {
            List<string> m_Children;
            m_Children = new List<string>();
            if (Children != null && Children.Count > 0)
                foreach (KeyValuePair<string, AudioManagerCategory> entry in Children)
                    if (entry.Value.IsLeaf()) m_Children.Add(entry.Value.ID);
                    else
                    {
                        entry.Value.GetLeafChildrenName().ForEach(x => m_Children.Add(x));
                    }
            return m_Children;
        }

        public Dictionary<string, AudioManagerCategory> GetLeafDictionary()
        {

            Dictionary<string, AudioManagerCategory> m_Children;
            m_Children = new Dictionary<string, AudioManagerCategory>();
            if (Children != null && Children.Count > 0)
                foreach (KeyValuePair<string, AudioManagerCategory> entry in Children)
                    if (entry.Value.IsLeaf())
                    {
                        m_Children.Add(entry.Value.ID, entry.Value);
                    }
                    else
                    {
                        entry.Value.GetLeafDictionary().ToList().ForEach(x =>
                        {
                            if (m_Children.ContainsKey(x.Key))
                            {
                                D.AudioError("Sound ID already present:" + x.Key);
                                return;
                            }
                            m_Children.Add(x.Key, x.Value);
                        });
                    }
            return m_Children;
        }

        /// <summary>
        /// Get a list of every entry in a given depth of the tree. Disregarding the parent.
        /// </summary>
        /// <param name="depth">How deep it should search. 0 is the root of the tree</param>
        /// <returns>A valid list with all the entries in that depth. If none is found the list will be empty.</returns>
        public List<AudioManagerCategory> GetDepth(int depth)
        {
            var categories = new List<AudioManagerCategory>();
            if (Depth == depth - 1)
            {
                foreach (KeyValuePair<string, AudioManagerCategory> entry in Children)
                    categories = categories.Concat(entry.Value.GetDepth(depth)).ToList();
            }
            if (Depth == depth)
            {
                categories.Add(this);
            }
            return categories;
        }



        public AudioMixerGroup GetClosestBus()
        {
            if (m_DefaultBus != null)
            {
                return m_DefaultBus;
            }

            if (Parent != null)
            {
                return Parent.GetClosestBus();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Add a child to the current node. Warning: If the given node already have a parent that parent will be overwritten.
        /// </summary>
        /// <param name="item">Child to be add.</param>
        public void Add(AudioManagerCategory item)
        {
            if (item.Parent != null)
            {
                item.Parent.Children.Remove(item.ID);
            }
            item.Parent = this;
            item.DefaultBus = this.DefaultBus;
            int countInChildren = CountIDInChildren(item.ID);
            if (countInChildren > 0)
                item.m_ID = item.m_ID + countInChildren;
            if (this.Children == null) Children = new Dictionary<string, AudioManagerCategory>();//D.AudioLog("null children wtf");
            this.Children.Add(item.ID, item);
        }

        public bool RemoveFromTree()
        {
            if (Parent != null)
            {
                if (Children != null)
                    Children.Clear();
                Parent.RemoveChild(this);
                Parent = null;
                return true;
            }
            return false;
        }

        public void RemoveChild(AudioManagerCategory item)
        {
            if (Children != null)
            {
                string key = "";
                bool found = false;
                foreach (KeyValuePair<string, AudioManagerCategory> entry in Children)
                {
                    if (entry.Value.ID == item.ID)
                    {
                        found = true;
                        key = entry.Key;
                    }
                }
                if (found) Children.Remove(key);
            }
            item = null;
        }

        /// <summary>
        /// Add a child to the current node. Warning: If the given node already have a parent that parent will be overwritten.
        /// </summary>
        /// <param name="item">Child to be add.</param>
        public void ReconstructAdd(AudioManagerCategory item)
        {
            item.Parent = this;
            if (this.Children == null) Children = new Dictionary<string, AudioManagerCategory>();//D.AudioLog("null children wtf");
            if (Children.ContainsKey(item.ID))
                Children.Remove(item.ID);
            this.Children.Add(item.ID, item);
        }

        public void ReconstructClear()
        {
            m_Parent = null;
            m_Children = null;
        }

        public void NewAudioData()
        {
            AudioData.Add(null);
            AudioDataWeight.Add(1);
        }

        public void RemoveAudioData(int index)
        {
            if (AudioData.Count == 0)
            {
                return;
            };

            if (AudioData.Count == 1 && index == 0)
            {
                AudioData[index] = null;
                AudioDataWeight[index] = 1;
            }
            AudioData.RemoveAt(index);
            AudioDataWeight.RemoveAt(index);
        }

        public int Count
        {
            get { return this.Children == null ? 0 : this.Children.Count; }
        }

        public int ParentUniqueId
        {
            get
            {
                return m_ParentUniqueId;
            }

            set
            {
                m_ParentUniqueId = value;
            }
        }

        public float PitchRandomizationValue
        {
            get
            {
                return m_PitchRandomizationValue;
            }

            set
            {
                m_PitchRandomizationValue = value;
            }
        }

        public bool Loop
        {
            get
            {
                return m_Loop;
            }

            set
            {
                m_Loop = value;
            }
        }

        public float NextAllowedAudioDelay
        {
            get
            {
                return m_NextAllowedAudioDelay;
            }

            set
            {
                m_NextAllowedAudioDelay = value;
            }
        }

        public AudioManagerCategory FindCategoryWithUniqueId(int id)
        {
            if (m_UniqueId == id)
            {
                return this;
            }

            if (m_Children != null)
            {
                AudioManagerCategory found = null;
                foreach (AudioManagerCategory c in m_Children.Values)
                {
                    found = c.FindCategoryWithUniqueId(id);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        public void RenameAssetRecursive(string oldPath, string newPath, ref bool changed)
        {
            if (m_AudioData != null)
            {
                for (int i = 0; i < m_AudioData.Count; i++)
                {
                    if (m_AudioData[i] == oldPath)
                    {
                        m_AudioData[i] = newPath;
                        changed = true;
                        D.AudioLog("Updated moved audio file: " + m_AudioData[i]);
                    }
                }
            }

            if (m_Children != null)
            {
                foreach (var kv in m_Children)
                {
                    kv.Value.RenameAssetRecursive(oldPath, newPath, ref changed);
                }
            }
        }

        public void RenameAssetPathRecursive(string oldPath, string newPath, ref bool changed)
        {
            if (m_AudioData != null)
            {
                for (int i = 0; i < m_AudioData.Count; i++)
                {
                    if (m_AudioData[i].Contains(oldPath))
                    {
                        m_AudioData[i] = m_AudioData[i].Replace(oldPath, newPath);
                        changed = true;
                        D.AudioLog("Updated moved audio file: " + m_AudioData[i]);
                    }
                }
            }

            if (m_Children != null)
            {
                foreach (var kv in m_Children)
                {
                    kv.Value.RenameAssetPathRecursive(oldPath, newPath, ref changed);
                }
            }
        }

        public string EnumString
        {
            get { return m_ID.Trim().ToUpperInvariant().Replace(" ", "_"); }
        }

        public string IdPrefix
        {
            get { return m_ID.Split('_')[0]; }
        }

        public void CleanClips()
        {
            m_AudioData = null;
            m_AudioDataWeight = null;
        }

        public bool IsParentOfCategory(string categoryName)
        {
            if (ID.Equals(categoryName)) return true;
            if (Parent == null) return false;
            return Parent.IsParentOfCategory(categoryName);
        }

        public void OnBeforeSerialize()
        {
            // Delete filter object to not take space in the serialized asset
            if (m_EnabledFilters == 0)
            {
                m_FiltersHolder = null;
            }

            // if (!UseChorusFilter) m_ChorusFilter = null;
            // if (!UseDistortionFilter) m_DistortionFilter = null;
            // if (!UseEchoFilter) m_EchoFilter = null;
            // if (!UseFadeIn) m_FadeInFilter = null;
            // if (!UseFadeOut) m_FadeOutFilter = null;
            // if (!UseHighPassFilter) m_HighPassFilter = null;
            // if (!UseLowPassFilter) m_LowPassFilter = null;
            // if (!UseReverbFilter) m_ReverbFilter = null;
        }

        public void OnAfterDeserialize() { }
    }
}
