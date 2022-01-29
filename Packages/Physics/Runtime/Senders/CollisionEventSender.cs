using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using of2.Base;
using UltEvents;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace o2f.Physics
{

    public class CollisionEventSender : CollisionEventSenderBase
    {
        [Serializable]
        public class UnityEvents
        {
            [FormerlySerializedAs("onCollisionEvent")] [FormerlySerializedAs("OnCollisionEnterEvent")] public Collision3DDelegate onCollision3DEvent;
            [FormerlySerializedAs("OnCollisionStayEvent")] public Collision3DDelegate onCollision3DStayEvent;
            [FormerlySerializedAs("OnCollisionExitEvent")] public Collision3DDelegate onCollision3DExitEvent;
            
            [FormerlySerializedAs("onTriggerEvent")] [FormerlySerializedAs("OnTriggerEnterEvent")] public Trigger3DDelegate onTrigger3DEvent;
            [FormerlySerializedAs("OnTriggerStayEvent")] public Trigger3DDelegate onTrigger3DStayEvent;
            [FormerlySerializedAs("OnTriggerExitEvent")] public Trigger3DDelegate onTrigger3DExitEvent;
        
            [FormerlySerializedAs("OnCollisionEnter2DEvent")] public Collision2DDelegate onCollision2DEvent;
            public Collision2DDelegate OnCollisionStay2DEvent;
            public Collision2DDelegate OnCollisionExit2DEvent;

            [FormerlySerializedAs("OnTriggerEnter2DEvent")] public Trigger2DDelegate onTrigger2DEvent;
            public Trigger2DDelegate OnTriggerStay2DEvent;
            public Trigger2DDelegate OnTriggerExit2DEvent;
        }

        public UnityEvents unityEvents = new UnityEvents();

        public event Collision3DEventHandler CollisionEnterEvent;
        public event Collision3DEventHandler CollisionStayEvent;
        public event Collision3DEventHandler CollisionExitEvent;
        
        public event Collision2DEventHandler CollisionEnter2DEvent;
        public event Collision2DEventHandler CollisionStay2DEvent;
        public event Collision2DEventHandler CollisionExit2DEvent;

        public event Trigger3DEventHandler TriggerEnterEvent;
        public event Trigger3DEventHandler TriggerStayEvent;
        public event Trigger3DEventHandler TriggerExitEvent;
        
        public event Trigger2DEventHandler TriggerEnter2DEvent;
        public event Trigger2DEventHandler TriggerStay2DEvent;
        public event Trigger2DEventHandler TriggerExit2DEvent;

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
            unityEvents.onCollision3DEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            CollisionStayEvent?.Invoke(collision, gameObject);
            unityEvents.onCollision3DStayEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            CollisionExitEvent?.Invoke(collision, gameObject);
            unityEvents.onCollision3DExitEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            TriggerEnterEvent?.Invoke(collider, gameObject);
            unityEvents.onTrigger3DEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerStay(Collider collider)
        {
            TriggerStayEvent?.Invoke(collider, gameObject);
            unityEvents.onTrigger3DStayEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnTriggerExit(Collider collider)
        {
            TriggerExitEvent?.Invoke(collider, gameObject);
            unityEvents.onTrigger3DExitEvent?.Invoke(collider, gameObject);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            CollisionEnter2DEvent?.Invoke(collision, gameObject);
            unityEvents.onCollision2DEvent?.Invoke(collision, gameObject);
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
            unityEvents.onTrigger2DEvent?.Invoke(collider, gameObject);
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