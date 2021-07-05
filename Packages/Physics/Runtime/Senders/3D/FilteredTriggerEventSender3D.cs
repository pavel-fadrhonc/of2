using of2.Utils.TagSelector;
using UnityEngine;

namespace o2f.Physics
{
    public class FilteredTriggerEventSender3D : TriggerEventSender3D
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
        
        public bool FilterObject(GameObject go)
        {
            return FilteredCollisionEventSender.FilterObject(go, FilterTags, _filterLayers, _filterOperation);
        }
    }
}