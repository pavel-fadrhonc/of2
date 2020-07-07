using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace of2.Audio
{
    //Unscaled wait-for-seconds coroutine
    public class WaitForPausedAudioRealtime : CustomYieldInstruction
    {
        private float m_EndTime;
        private GlobalAudioManager _gam;
		
        public WaitForPausedAudioRealtime(GlobalAudioManager gam, float duration)
        {
            _gam = gam;
            m_EndTime = gam.PausableAudioTime + duration;
        }

        public override bool keepWaiting
        {
            get { return _gam.PausableAudioTime < m_EndTime; }
        }
    }
}