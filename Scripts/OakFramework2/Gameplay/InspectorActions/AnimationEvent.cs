using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DefaultNamespace.Animation
{
    public class AnimationEvent : MonoBehaviour
    {
        public UnityEvent AnimationEvent0;
        public UnityEvent AnimationEvent1;
        public UnityEvent AnimationEvent2;
        public UnityEvent AnimationEvent3;
        public UnityEvent AnimationEvent4;

        private List<UnityEvent> _animationEvents;
        
        private void Awake()
        {
            _animationEvents = new List<UnityEvent>()
            {
                AnimationEvent0,
                AnimationEvent1,
                AnimationEvent2,
                AnimationEvent3,
                AnimationEvent4
            };
        }

        public void AnimationEventFunc(int index)
        {
            if (index >= _animationEvents.Count)
                return;
            
            _animationEvents[index].Invoke();
        }
    }
}