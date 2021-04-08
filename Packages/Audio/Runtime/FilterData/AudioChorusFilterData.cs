﻿using UnityEngine;
using System;

namespace of2.Audio
{
    [Serializable]
    public class AudioChorusFilterData
    {
        //
        // Summary:
        //     ///
        //     Chorus delay in ms. 0.1 to 100.0. Default = 40.0 ms.
        //     ///
        public float delay;
        //
        // Summary:
        //     ///
        //     Chorus modulation depth. 0.0 to 1.0. Default = 0.03.
        //     ///
        public float depth;
        //
        // Summary:
        //     ///
        //     Volume of original signal to pass to output. 0.0 to 1.0. Default = 0.5.
        //     ///
        public float dryMix;
        //
        // Summary:
        //     ///
        //     Chorus modulation rate in hz. 0.0 to 20.0. Default = 0.8 hz.
        //     ///
        public float rate;
        //
        // Summary:
        //     ///
        //     Volume of 1st chorus tap. 0.0 to 1.0. Default = 0.5.
        //     ///
        public float wetMix1;
        //
        // Summary:
        //     ///
        //     Volume of 2nd chorus tap. This tap is 90 degrees out of phase of the first tap.
        //     0.0 to 1.0. Default = 0.5.
        //     ///
        public float wetMix2;
        //
        // Summary:
        //     ///
        //     Volume of 3rd chorus tap. This tap is 90 degrees out of phase of the second tap.
        //     0.0 to 1.0. Default = 0.5.
        //     ///
        public float wetMix3;

        public void SetToFilter(AudioChorusFilter filter)
        {
            filter.delay = delay;
            filter.depth = depth;
            filter.dryMix = dryMix;
            filter.rate = rate;
            filter.wetMix1 = wetMix1;
            filter.wetMix2 = wetMix2;
            filter.wetMix3 = wetMix3;
        }
    }
}