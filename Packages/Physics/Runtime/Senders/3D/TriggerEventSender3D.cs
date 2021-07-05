using System.Collections.Generic;
using UnityEngine;

namespace o2f.Physics
{
    public class TriggerEventSender3D : CollisionEventSenderBase
    {
        public TriggerEnterDelegate OnTriggerEnterEvent;
        public TriggerEnterDelegate OnTriggerStayEvent;
        public TriggerEnterDelegate OnTriggerExitEvent;

        public event TriggerEventHandler TriggerEnterEvent;
        public event TriggerEventHandler TriggerStayEvent;
        public event TriggerEventHandler TriggerExitEvent;
        
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
            OnTriggerEnterEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerStay(Collider collider)
        {
            TriggerStayEvent?.Invoke(collider, gameObject);
            OnTriggerStayEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            TriggerExitEvent?.Invoke(collider, gameObject);
            OnTriggerExitEvent?.Invoke(collider, gameObject);
        }        
    }
}