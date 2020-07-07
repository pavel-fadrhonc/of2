using UnityEngine;

namespace of2.Audio
{
    public class WaitUntilAudioClipStarted : CustomYieldInstruction
    {
        private bool m_Wait = true;

        public void StopWaiting()
        {
            m_Wait = false;
        }

        public WaitUntilAudioClipStarted(AudioClipPlayer player)
        {
            m_Wait = player != null;
        }

        public override bool keepWaiting
        {
            get { return m_Wait; }
        }
    }
}