using System.Collections.Generic;
using UnityEngine;

namespace o2f.Physics
{
    public class CollisionEventSender3D : CollisionEventSenderBase
    {
        public CollisionEnterDelegate OnCollisionEnterEvent;
        public CollisionEnterDelegate OnCollisionStayEvent;
        public CollisionEnterDelegate OnCollisionExitEvent;

        public event CollisionEventHandler CollisionEnterEvent;
        public event CollisionEventHandler CollisionStayEvent;
        public event CollisionEventHandler CollisionExitEvent;

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
        
        protected virtual void OnCollisionEnter(Collision collision)
        {
            CollisionEnterEvent?.Invoke(collision, gameObject);
            OnCollisionEnterEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            CollisionStayEvent?.Invoke(collision, gameObject);
            OnCollisionStayEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            CollisionExitEvent?.Invoke(collision, gameObject);
            OnCollisionExitEvent?.Invoke(collision, gameObject);
        }        
    }
}