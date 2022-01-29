using System.Collections.Generic;
using UnityEngine;

namespace o2f.Physics._2D
{
    public class CollisionEventSender2D : CollisionEventSenderBase
    {
        public Collision2DDelegate onCollision2DEnterEvent;
        public Collision2DDelegate onCollision2DStayEvent;
        public Collision2DDelegate onCollision2DExitEvent;
        
        public event Collision2DEventHandler Collision2DEnterEvent;
        public event Collision2DEventHandler Collision2DStayEvent;
        public event Collision2DEventHandler Collision2DExitEvent;
        
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
        
        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            Collision2DEnterEvent?.Invoke(collision, gameObject);
            onCollision2DEnterEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            Collision2DStayEvent?.Invoke(collision, gameObject);
            onCollision2DStayEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
            Collision2DExitEvent?.Invoke(collision, gameObject);
            Collision2DExitEvent?.Invoke(collision, gameObject);
        }        
    }
}