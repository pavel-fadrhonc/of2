﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using OakFramework2.BaseMono;

namespace of2.Audio
{
    /// <summary>
    /// Loads audio bundles, keeps used (retained) clips in memory.
    /// We're using manual retain/release system as in. Multiple retains are supported, and mutliple releases necessary.
    /// </summary>
    public class AudioCache : of2GameObject
    {
        private class ClipCache
        {
            public AudioClip Clip;
            public int Usages;
            public string Path;
        }

        private Dictionary<string, ClipCache> m_Cache = new Dictionary<string, ClipCache>();

        private AssetBundle m_AudioBundle;

        private List<AssetBundle> m_AdditionalBundles = new List<AssetBundle>();

        public void LoadBundle(string path)
        {
            if (m_AudioBundle != null)
            {
                m_AudioBundle.Unload(false);
            }

            m_AudioBundle = AssetBundle.LoadFromFile(path);
        }

        public void LoadAdditionalBundle(AssetBundle bundle)
        {
            m_AdditionalBundles.Add(bundle);
        }

        public void UnloadAdditionalBundles()
        {

            for (int i = 0; i < m_AdditionalBundles.Count; i++)
            {
                AssetBundle b = m_AdditionalBundles[i];
                b.Unload(false);
            }

            m_AdditionalBundles.Clear();
        }

        public IEnumerator RetainClipAsync(string path)
        {
            ClipCache cache = null;
            if (m_Cache.ContainsKey(path))
            {
                //D.AudioLog("Playing cached sound! " + path);
                cache = m_Cache[path];
            }
            else
            {
                AudioClip c = null;

                // Prefer file from additional bundle (can be used for localization)
                for (int i = 0; i < m_AdditionalBundles.Count; i++)
                {
                    AssetBundle b = m_AdditionalBundles[i];
                    AssetBundleRequest r = b.LoadAssetAsync<AudioClip>(path);
                    yield return r;

                    c = (AudioClip)r.asset;

                    if (c != null)
                    {
                        break;
                    }
                }

                // Load from main bundle
                if (c == null && m_AudioBundle != null)
                {
                    AssetBundleRequest r = m_AudioBundle.LoadAssetAsync<AudioClip>(path);
                    yield return r;

                    c = (AudioClip)r.asset;
                }

                if (c == null)
                {
                    yield break;
                }

                //D.AudioLog("Caching sound! " + path);
                cache = new ClipCache();
                cache.Clip = c;
                cache.Path = path;

                m_Cache[path] = cache;
            }

            cache.Usages++;
        }

        public AudioClip RetainClip(string path)
        {
            ClipCache cache = null;
            if (m_Cache.ContainsKey(path))
            {
                //D.AudioLog("Playing cached sound! " + path);
                cache = m_Cache[path];
            }
            else
            {
                AudioClip c = null;

                // Prefer file from additional bundle (can be used for localization)
                for (int i = 0; i < m_AdditionalBundles.Count; i++)
                {
                    AssetBundle b = m_AdditionalBundles[i];
                    c = b.LoadAsset<AudioClip>(path);
                    if (c != null)
                    {
                        break;
                    }
                }

                // Load from main bundle
                if (c == null && m_AudioBundle != null)
                {
                    c = m_AudioBundle.LoadAsset<AudioClip>(path);
                }

                if (c == null)
                {
                    return null;
                }

                //D.AudioLog("Caching sound! " + path);
                cache = new ClipCache();
                cache.Clip = c;
                cache.Path = path;

                m_Cache[path] = cache;
            }

            cache.Usages++;

            return cache.Clip;
        }

        public void ReleaseClipNow(string clipPath)
        {
            if (!m_Cache.ContainsKey(clipPath))
            {
                D.AudioWarning("Clip not found for path: " + clipPath);
                return;
            }

            ReleaseClipNowInternal(m_Cache[clipPath], clipPath);
        }

        private void ReleaseClipNowInternal(ClipCache cache, string clipPath, int usages = 1)
        {
            if (cache.Path != clipPath)
            {
                D.AudioError("Trying to release incorrect clip! " + clipPath);
            }

            cache.Usages = cache.Usages - usages;
            //D.AudioLog(clipPath + " usages: " + cache.Usages);

            if (cache.Usages <= 0)
            {
                //D.AudioLog("Removing cached sound! " + cache.Path);
                m_Cache.Remove(cache.Path);
                Resources.UnloadAsset(cache.Clip);
            }
        }

        private void ClearAll()
        {
            foreach (ClipCache c in m_Cache.Values.ToList())
            {
                ReleaseClipNowInternal(c, c.Path, c.Usages);
            }
            m_Cache.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClearAll();
        }
    }
}