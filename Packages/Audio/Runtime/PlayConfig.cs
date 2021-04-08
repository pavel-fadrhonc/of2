using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace of2.Audio
{
    public class PlayConfig
    {
        public string SoundID;
        public float? Volume;
        public float? Delay;
        public bool? In3D;
        public Vector3? Position;
        public float? MinDistance;
        public float? MaxDistance;
        public AudioRolloffMode? VolumeRolloffMode;
        public float? PitchRandomisation;
        public bool? Loop;
        public bool? StartPaused;
        public Transform TrackTransform = null;
        public AudioSource ReferenceAudioSource = null;
    }
}