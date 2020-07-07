﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace of2.Audio
{
    [Serializable]
    public class AudioDistortionFilterData
    {


        //
        // Summary:
        //     ///
        //     Distortion value. 0.0 to 1.0. Default = 0.5.
        //     ///
        public float DistortionLevel;

        public void SetToFilter(AudioDistortionFilter filter)
        {
            filter.distortionLevel = DistortionLevel;
        }
    }
}
