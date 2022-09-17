using System;
using UnityEngine;

namespace Various
{
    public class FollowTransform : MonoBehaviour
    {
        [Flags]         
        public enum EFollowMode
        {
            Position         = 1 << 0,
            Rotation         = 1 << 1,
            Scale            = 1 << 2
        }

        public enum EUpdateMode
        {
            Update,
            LateUpdate,
            FixedUpdate
        }

        [SerializeField] private Transform _followTransfom;
        [SerializeField] private EFollowMode _followMode;
        [SerializeField] private EUpdateMode _updateMode;

        public Transform FollowTransfom
        {
            get => _followTransfom;
            set => _followTransfom = value;
        }

        public EFollowMode FollowMode
        {
            get => _followMode;
            set => _followMode = value;
        }

        public EUpdateMode UpdateMode
        {
            get => _updateMode;
            set => _updateMode = value;
        }

        private void Update()
        {
            if (UpdateMode != EUpdateMode.Update)
                return;
            
            Sync();
        }

        private void LateUpdate()
        {
            if (UpdateMode != EUpdateMode.LateUpdate)
                return;
            
            Sync();
        }

        private void FixedUpdate()
        {
            if (UpdateMode != EUpdateMode.FixedUpdate)
                return;

            Sync();
        }

        private void Sync()
        {
            if ((_followMode & EFollowMode.Position) > 0)
            {
                transform.position = _followTransfom.position;
            }
            
            if ((_followMode & EFollowMode.Rotation) > 0)
            {
                transform.rotation = _followTransfom.rotation;
            }

            if ((_followMode & EFollowMode.Scale) > 0)
            {
                transform.localScale = _followTransfom.localScale;
            }
        }
    }
}