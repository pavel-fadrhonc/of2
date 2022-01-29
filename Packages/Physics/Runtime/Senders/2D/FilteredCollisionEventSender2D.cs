using UnityEngine;

namespace o2f.Physics._2D
{
    public class FilteredCollisionEventSender2D : CollisionEventSender2D
    {
        [SerializeField] private FilteredCollisionEventSender.FilterInfo _filterInfo;

        public FilteredCollisionEventSender.FilterInfo FilterInfo
        {
            get => _filterInfo;
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
        
        public bool FilterObject(GameObject go)
        {
            return FilteredCollisionEventSender.FilterObject(go, FilterInfo);
        }        
    }
}