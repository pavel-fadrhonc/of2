﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace of2.Audio
{
    [Serializable]
    public class AudioFiltersHolder
    {
        public AudioChorusFilterData Chorus;
        public AudioDistortionFilterData Distortion;
        public AudioEchoFilterData Echo;
        public AudioFadeInFilterData FadeIn;
        public AudioFadeOutFilterData FadeOut;
        public AudioHighPassFilterData HighPass;
        public AudioLowPassFilterData LowPass;
        public AudioReverbFilterData Reverb;
    }
}