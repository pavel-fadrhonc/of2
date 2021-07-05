using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using of2.Base;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;

namespace o2f.Physics
{

    public class CollisionEventSender : CollisionEventSenderBase
    {
        [Serializable]
        public class UnityEvents
        {
            public CollisionEnterDelegate OnCollisionEnterEvent;
            public CollisionEnterDelegate OnCollisionStayEvent;
            public CollisionEnterDelegate OnCollisionExitEvent;
            
            public TriggerEnterDelegate OnTriggerEnterEvent;
            public TriggerEnterDelegate OnTriggerStayEvent;
            public TriggerEnterDelegate OnTriggerExitEvent;
        
            public CollisionEnter2DDelegate OnCollisionEnter2DEvent;
            public CollisionEnter2DDelegate OnCollisionStay2DEvent;
            public CollisionEnter2DDelegate OnCollisionExit2DEvent;

            public TriggerEnter2DDelegate OnTriggerEnter2DEvent;
            public TriggerEnter2DDelegate OnTriggerStay2DEvent;
            public TriggerEnter2DDelegate OnTriggerExit2DEvent;
        }

        public UnityEvents unityEvents;

        public event CollisionEventHandler CollisionEnterEvent;
        public event CollisionEventHandler CollisionStayEvent;
        public event CollisionEventHandler CollisionExitEvent;
        
        public event CollisionEvent2DHandler CollisionEnter2DEvent;
        public event CollisionEvent2DHandler CollisionStay2DEvent;
        public event CollisionEvent2DHandler CollisionExit2DEvent;

        public event TriggerEventHandler TriggerEnterEvent;
        public event TriggerEventHandler TriggerStayEvent;
        public event TriggerEventHandler TriggerExitEvent;
        
        public event TriggerEvent2DHandler TriggerEnter2DEvent;
        public event TriggerEvent2DHandler TriggerStay2DEvent;
        public event TriggerEvent2DHandler TriggerExit2DEvent;

        private List<Collider> _colliders;
        private List<Collider2D> _colliders2D;

        protected override void InitColliders()
        {
            base.InitColliders();
            
            if (_colliders == null)
            {
                _colliders = new List<Collider>(GetComponents<Collider>());
                _colliders2D = new List<Collider2D>(GetComponents<Collider2D>());
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
            foreach (var col in _colliders2D) col.enabled = enable;
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            CollisionEnterEvent?.Invoke(collision, gameObject);
            unityEvents.OnCollisionEnterEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            CollisionStayEvent?.Invoke(collision, gameObject);
            unityEvents.OnCollisionStayEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            CollisionExitEvent?.Invoke(collision, gameObject);
            unityEvents.OnCollisionExitEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            TriggerEnterEvent?.Invoke(collider, gameObject);
            unityEvents.OnTriggerEnterEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerStay(Collider collider)
        {
            TriggerStayEvent?.Invoke(collider, gameObject);
            unityEvents.OnTriggerStayEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            TriggerExitEvent?.Invoke(collider, gameObject);
            unityEvents.OnTriggerExitEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            CollisionEnter2DEvent?.Invoke(collision, gameObject);
            unityEvents.OnCollisionEnter2DEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            CollisionStay2DEvent?.Invoke(collision, gameObject);
            unityEvents.OnCollisionStay2DEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
            CollisionExit2DEvent?.Invoke(collision, gameObject);
            unityEvents.OnCollisionExit2DEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            TriggerEnter2DEvent?.Invoke(collider, gameObject);
            unityEvents.OnTriggerEnter2DEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerStay2D(Collider2D collider)
        {
            TriggerStay2DEvent?.Invoke(collider, gameObject);
            unityEvents.OnTriggerStay2DEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            TriggerExit2DEvent?.Invoke(collider, gameObject);
            unityEvents.OnTriggerExit2DEvent?.Invoke(collider, gameObject);
        }
    }
}
