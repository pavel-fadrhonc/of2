using of2.Utils.TagSelector;
using UnityEngine;

namespace o2f.Physics
{
    public class FilteredCollisionEventSender3D : CollisionEventSender3D
    {
        [SerializeField]
        protected FilteredCollisionEventSender.EFilterOperation _filterOperation;
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

        public FilteredCollisionEventSender.EFilterOperation FilterOperation
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
        
        public bool FilterObject(GameObject go)
        {
            return FilteredCollisionEventSender.FilterObject(go, FilterTags, _filterLayers, _filterOperation);
        }
    }
}