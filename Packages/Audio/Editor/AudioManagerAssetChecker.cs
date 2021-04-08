﻿using UnityEngine;
using UnityEditor;
using System.IO;

namespace of2.Audio
{
    [InitializeOnLoad]
    public class AudioManagerAssetChecker : UnityEditor.AssetModificationProcessor
    {

        public static AssetMoveResult OnWillMoveAsset(string oldPath, string newPath)
        {
            //D.AudioLog("Moving " + oldPath + " to " + newPath);

            string lowercase = oldPath.ToLowerInvariant();
            AudioManagerData data = null;
            bool shouldSaveData = false;

            // this skips changes by hiding folders (if we want to exclude some files from the build)
            if (!newPath.Contains("/."))
            {
                // moving whole directory, need to check a lot of stuff :(
                if (Directory.Exists(Path.Combine(Application.dataPath, oldPath.Substring("Assets/".Length))))
                {
                    data = AudioManagerData.LoadInstanceData();
                    if (data != null)
                    {
                        //D.AudioLog("It's a direcotry, checking all sounds :(");
                        data.TreeData.RenameAssetPathRecursive(oldPath, newPath, ref shouldSaveData);
                    }
                }

                // moving a file
                else if (lowercase.EndsWith(".wav") || lowercase.EndsWith(".mp3") || lowercase.EndsWith(".aif") || lowercase.EndsWith(".ogg"))
                {
                    data = AudioManagerData.LoadInstanceData();
                    if (data != null)
                    {
                        // Check all the paths
                        data.TreeData.RenameAssetRecursive(oldPath, newPath, ref shouldSaveData);
                    }
                }
            }

            if (shouldSaveData)
            {
                D.AudioLog("Updating audio manager data!");
                data.SaveTree();
#if !UNITY_2018_2_OR_NEWER
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
#endif
            }

            return AssetMoveResult.DidNotMove;
        }
    }
}