using UnityEngine;
using System.Collections;

namespace o2f.Physics
{
    public class FilteredCollisionEventSender : CollisionEventSender
    {
        [Tooltip("Object is considered for collision if it passes layer filter OR tag filter")]
        [SerializeField]
        protected LayerMask FilterLayers;
        [Tooltip("Object is considered for collision if it passes layer filter OR tag filter")]
        [SerializeField]
        protected string[] filterTags;

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

        private bool FilterObject(GameObject go)
        {
            bool hasTagOrLayer = false;

            if (((1 << go.layer) & FilterLayers.value) != 0)
                hasTagOrLayer = true;

            for (int i = 0; i < filterTags.Length; i++)
            {
                if (go.CompareTag(filterTags[i]))
                    hasTagOrLayer = true;
            }

            return hasTagOrLayer;
        }
    }
}