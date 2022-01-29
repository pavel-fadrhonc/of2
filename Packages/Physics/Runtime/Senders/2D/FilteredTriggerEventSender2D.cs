using UnityEngine;

namespace o2f.Physics._2D
{
    public class FilteredTriggerEventSender2D : TriggerEventSender2D
    {
        [SerializeField] private FilteredCollisionEventSender.FilterInfo _filterInfo;

        public FilteredCollisionEventSender.FilterInfo filterInfo
        {
            get => _filterInfo;
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
            return FilteredCollisionEventSender.FilterObject(go, filterInfo);
        }        
    }
}