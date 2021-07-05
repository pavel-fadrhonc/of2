using System;
using of2.Base;
using UltEvents;
using UnityEngine;

namespace o2f.Physics
{
    public delegate void CollisionEventHandler(Collision collision, GameObject sender);
    public delegate void CollisionEvent2DHandler(Collision2D collision, GameObject sender);
    public delegate void TriggerEventHandler(Collider collider, GameObject sender);
    public delegate void TriggerEvent2DHandler(Collider2D collider, GameObject sender);
    
    [Serializable] public class CollisionEnterDelegate : UltEvent<Collision, GameObject> {}
    [Serializable] public class TriggerEnterDelegate : UltEvent<Collider, GameObject> {}
    [Serializable] public class CollisionEnter2DDelegate : UltEvent<Collision2D, GameObject> {}
    [Serializable] public class TriggerEnter2DDelegate : UltEvent<Collider2D, GameObject> {}

    public class CollisionEventSenderBase : MonoBehaviour, IEnablable
    {
        public bool DisabledCollidersWithThis { get; set; } = true;
        
        /// <summary>
        /// Since OnDisabled does not always get called when setting MonoBehaviour.enabled = false, this is the only sure way to get the disabling / enabling functionality
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (value == enabled) return;
                
                enabled = value;
                if (value)
                    EnableCallback();
                else
                    DisableCallback();
            }
        }
        
        protected virtual void InitColliders() 
        { }
        
        protected virtual void EnableColliders(bool enable)
        {}
        
        protected virtual void EnableCallback()
        {
            if (DisabledCollidersWithThis)
            {
                InitColliders();

                EnableColliders(true);
            }
        }

        protected virtual void DisableCallback()
        {
            if (DisabledCollidersWithThis)
            {
                InitColliders();
                
                EnableColliders(false);
            }
        }
    }
}