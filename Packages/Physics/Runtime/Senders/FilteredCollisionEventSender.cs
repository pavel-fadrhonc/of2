using System;
using UnityEngine;
using of2.Utils.TagSelector;
using UnityEngine.Rendering;

namespace o2f.Physics
{
    public class FilteredCollisionEventSender : CollisionEventSender
    {
        public enum EFilterOperation
        {
            Or,
            And
        }
        
        [SerializeField]
        protected EFilterOperation _filterOperation;
        [Tooltip("Object is considered for collision if it passes layer filter AND/OR tag filter")]
        [SerializeField]
        protected LayerMask _filterLayers;
        [Tooltip("Object is considered for collision if it passes layer filter AND/OR tag filter")]
        [SerializeField]
        [TagSelector]
        protected string[] filterTags;

        public string[] FilterTags
        {
            get => filterTags;
            set => filterTags = value;
        }

        public LayerMask FilterLayers
        {
            get => _filterLayers;
            set => _filterLayers = value;
        }

        public EFilterOperation FilterOperation
        {
            get => _filterOperation;
            set => _filterOperation = value;
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            if (FilterObject(collision.gameObject))
                base.OnCollisionEnter(collision);
        }

        protected override void OnCollisionStay(Collision collision)
        {
            if (FilterObject(collision.gameObject))
                base.OnCollisionStay(collision);
        }

        protected override void OnCollisionExit(Collision collision)
        {
            if (FilterObject(collision.gameObject))
                base.OnCollisionExit(collision);
        }

        protected override void OnTriggerEnter(Collider collider)
        {
            if (FilterObject(collider.gameObject))
                base.OnTriggerEnter(collider);
        }

        protected override void OnTriggerStay(Collider collider)
        {
            if (FilterObject(collider.gameObject))
                base.OnTriggerStay(collider);
        }

        protected override void OnTriggerExit(Collider collider)
        {
            if (FilterObject(collider.gameObject))
                base.OnTriggerExit(collider);
        }
        
        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            if (FilterObject(collision.gameObject))
                base.OnCollisionEnter2D(collision);
        }

        protected override void OnCollisionStay2D(Collision2D collision)
        {
            if (FilterObject(collision.gameObject))
                base.OnCollisionStay2D(collision);
        }

        protected override void OnCollisionExit2D(Collision2D collision)
        {
            if (FilterObject(collision.gameObject))
                base.OnCollisionExit2D(collision);
        }

        protected override void OnTriggerEnter2D(Collider2D collider)
        {
            if (FilterObject(collider.gameObject))
                base.OnTriggerEnter2D(collider);
        }

        protected override void OnTriggerStay2D(Collider2D collider)
        {
            if (FilterObject(collider.gameObject))
                base.OnTriggerStay2D(collider);
        }

        protected override void OnTriggerExit2D(Collider2D collider)
        {
            if (FilterObject(collider.gameObject))
                base.OnTriggerExit2D(collider);
        }        

        public bool FilterObject(GameObject go)
        {
            return FilterObject(go, FilterTags, _filterLayers, _filterOperation);
        }

        public static bool FilterObject(GameObject go, string[] filterTags, LayerMask filterLayers,
            EFilterOperation filterOperation)
        {
            bool hasTag = false;
            bool hasLayer = ((1 << go.layer) & filterLayers.value) != 0;

            if (filterOperation == EFilterOperation.And && hasLayer ||
                filterOperation == EFilterOperation.Or)
            {
                for (int i = 0; i < filterTags.Length; i++)
                {
                    if (go.CompareTag(filterTags[i]))
                        hasTag = true;
                }
            }

            if (filterOperation == EFilterOperation.And)
            {
                return hasTag && hasLayer;
            }
             
            return hasTag || hasLayer;            
        }

        public static bool FilterObject(GameObject go, FilterInfo filterInfo)
        {
            return FilterObject(go, filterInfo.filterTags, filterInfo.filterLayers, filterInfo.filterOperation);
        }

        [Serializable]
        public class FilterInfo
        {
            public string[] filterTags;
            public LayerMask filterLayers;
            public EFilterOperation filterOperation;
        }
    }
}