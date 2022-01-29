using System.Collections.Generic;
using UnityEngine;

namespace o2f.Physics._2D
{
    public class TriggerEventSender2D : CollisionEventSenderBase
    {
        public Trigger2DDelegate onTrigger2DEnterEvent;
        public Trigger2DDelegate onTrigger2DStayEvent;
        public Trigger2DDelegate onTrigger2DExitEvent;
        
        public event Trigger2DEventHandler Trigger2DEnterEvent;
        public event Trigger2DEventHandler Trigger2DStayEvent;
        public event Trigger2DEventHandler Trigger2DExitEvent;
        
        private List<Collider2D> _colliders;

        protected override void InitColliders()
        {
            base.InitColliders();
            
            if (_colliders == null)
            {
                _colliders = new List<Collider2D>(GetComponents<Collider2D>());
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
        
        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            Trigger2DEnterEvent?.Invoke(collider, gameObject);
            onTrigger2DEnterEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerStay2D(Collider2D collider)
        {
            Trigger2DStayEvent?.Invoke(collider, gameObject);
            onTrigger2DStayEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            Trigger2DExitEvent?.Invoke(collider, gameObject);
            onTrigger2DExitEvent?.Invoke(collider, gameObject);
        }           
    }
}