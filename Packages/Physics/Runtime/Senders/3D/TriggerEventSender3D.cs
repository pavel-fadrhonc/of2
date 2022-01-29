using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace o2f.Physics
{
    public class TriggerEventSender3D : CollisionEventSenderBase
    {
        [FormerlySerializedAs("onTrigger3DEvent")] [FormerlySerializedAs("onTriggerEvent")] [FormerlySerializedAs("OnTriggerEnterEvent")] public Trigger3DDelegate onTrigger3DEnterEvent;
        [FormerlySerializedAs("OnTriggerStayEvent")] public Trigger3DDelegate onTrigger3DStayEvent;
        [FormerlySerializedAs("OnTriggerExitEvent")] public Trigger3DDelegate onTrigger3DExitEvent;

        public event Trigger3DEventHandler TriggerEnterEvent;
        public event Trigger3DEventHandler TriggerStayEvent;
        public event Trigger3DEventHandler TriggerExitEvent;
        
        private List<Collider> _colliders;

        protected override void InitColliders()
        {
            base.InitColliders();
            
            if (_colliders == null)
            {
                _colliders = new List<Collider>(GetComponents<Collider>());
            }
        }
        
        private void Awake()
        {
            InitColliders();
        }

        protected override void EnableColliders(bool enable)
        {
            base.EnableColliders(enable);

            foreach (var col in _colliders) col.enabled = enable;
        }
        
        protected virtual void OnTriggerEnter(Collider collider)
        {
            TriggerEnterEvent?.Invoke(collider, gameObject);
            onTrigger3DEnterEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerStay(Collider collider)
        {
            TriggerStayEvent?.Invoke(collider, gameObject);
            onTrigger3DStayEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            TriggerExitEvent?.Invoke(collider, gameObject);
            onTrigger3DExitEvent?.Invoke(collider, gameObject);
        }        
    }
}