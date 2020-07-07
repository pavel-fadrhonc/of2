﻿using System.Collections;
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
        public Transform Position = null;
        public Vector3? PositionV3;
        public float? MinDistance;
        public float? MaxDistance;
        public AudioRolloffMode? VolumeRolloffMode;
        public float? PitchRandomisation;
        public bool? Loop;
        public bool? StartPaused;
        public Transform Target = null;
        public AudioSource ReferenceAudioSource = null;
        public Transform Parent = null;
    }
}