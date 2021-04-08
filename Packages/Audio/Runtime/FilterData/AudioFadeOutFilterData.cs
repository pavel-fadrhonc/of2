﻿
using System;
using System.Collections.Generic;
using UnityEngine;

namespace of2.Audio
{
    [Serializable]
    public class AudioFadeOutFilterData
    {
        public AnimationCurve Fade;

        [Serializable]
        public enum EFadeType
        {
            VOLUME = 0,
            PITCH = 1,
        }

        public EFadeType Type;
    }
}

