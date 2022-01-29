﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace o2f.Physics
{
    public class CollisionEventSender3D : CollisionEventSenderBase
    {
        [FormerlySerializedAs("onCollision3DEvent")] [FormerlySerializedAs("onCollisionEvent")] [FormerlySerializedAs("OnCollisionEnterEvent")] public Collision3DDelegate onCollision3DEnterEvent;
        [FormerlySerializedAs("OnCollisionStayEvent")] public Collision3DDelegate onCollision3DStayEvent;
        [FormerlySerializedAs("OnCollisionExitEvent")] public Collision3DDelegate onCollision3DExitEvent;

        public event Collision3DEventHandler CollisionEnterEvent;
        public event Collision3DEventHandler CollisionStayEvent;
        public event Collision3DEventHandler CollisionExitEvent;

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
            onCollision3DEnterEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            CollisionStayEvent?.Invoke(collision, gameObject);
            onCollision3DStayEvent?.Invoke(collision, gameObject);
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            CollisionExitEvent?.Invoke(collision, gameObject);
            onCollision3DExitEvent?.Invoke(collision, gameObject);
        }        
    }
}