using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// This is a script to be attached to the object whose collision events you want to listen to
    /// TreeCollider is buggy, so I'm making this script for now
    /// </summary>
    public class CollisionListener : MonoBehaviour
    {
        public delegate void CollisionEvent(TreeCollider.CollisionInfo info);
        
        [SerializeField]
        public TreeCollider.UnifiedCollisionEvent onCollisionEnter;
        public event CollisionEvent OnCollisionEnterEvent;
        [SerializeField]
        public TreeCollider.UnifiedCollisionEvent onCollisionStay;
        public event CollisionEvent OnCollisionStayEvent; //Added these since unity events are null when the script was just added to an object
        [SerializeField]
        public TreeCollider.UnifiedCollisionEvent onCollisionExit;
        public event CollisionEvent OnCollisionExitEvent;
        [Space(30), SerializeField]
        public TreeCollider.UnifiedCollisionEvent onTriggerEnter;
        public event CollisionEvent OnTriggerEnterEvent;
        [SerializeField]
        public TreeCollider.UnifiedCollisionEvent onTriggerStay;
        public event CollisionEvent OnTriggerStayEvent;
        [SerializeField]
        public TreeCollider.UnifiedCollisionEvent onTriggerExit;
        public event CollisionEvent OnTriggerExitEvent;


        private void OnCollisionStay(Collision collision)
        {
            var colInfo = new TreeCollider.CollisionInfo
            {
                collidedWith = collision.gameObject,
                collision = collision,
                collisionState = TreeCollider.CollisionInfo.CollisionState.stay,
                isTrigger = false,
                otherCollider = collision.collider,
                sender = gameObject
            };
            onCollisionStay?.Invoke(colInfo);
            OnCollisionStayEvent?.Invoke(colInfo);
        }
        private void OnCollisionEnter(Collision collision)
        {
            var colInfo = new TreeCollider.CollisionInfo
            {
                collidedWith = collision.gameObject,
                collision = collision,
                collisionState = TreeCollider.CollisionInfo.CollisionState.enter,
                isTrigger = false,
                otherCollider = collision.collider,
                sender = gameObject
            };
            onCollisionEnter?.Invoke(colInfo);
            OnCollisionEnterEvent?.Invoke(colInfo);
        }
        private void OnCollisionExit(Collision collision)
        {
            var colInfo = new TreeCollider.CollisionInfo
            {
                collidedWith = collision.gameObject,
                collision = collision,
                collisionState = TreeCollider.CollisionInfo.CollisionState.exit,
                isTrigger = false,
                otherCollider = collision.collider,
                sender = gameObject
            };
            onCollisionExit?.Invoke(colInfo);
            OnCollisionExitEvent?.Invoke(colInfo);
        }
        private void OnTriggerStay(Collider other)
        {
            var colInfo = new TreeCollider.CollisionInfo
            {
                collidedWith = other.gameObject,
                collisionState = TreeCollider.CollisionInfo.CollisionState.stay,
                isTrigger = true,
                otherCollider = other,
                sender = gameObject
            };
            onTriggerStay?.Invoke(colInfo);
            OnTriggerStayEvent?.Invoke(colInfo);
        }
        private void OnTriggerEnter(Collider other)
        {
            var colInfo = new TreeCollider.CollisionInfo
            {
                collidedWith = other.gameObject,
                collisionState = TreeCollider.CollisionInfo.CollisionState.stay,
                isTrigger = true,
                otherCollider = other,
                sender = gameObject
            };
            onTriggerEnter?.Invoke(colInfo);
            OnTriggerEnterEvent?.Invoke(colInfo);
        }
        private void OnTriggerExit(Collider other)
        {
            var colInfo = new TreeCollider.CollisionInfo
            {
                collidedWith = other.gameObject,
                collisionState = TreeCollider.CollisionInfo.CollisionState.stay,
                isTrigger = true,
                otherCollider = other,
                sender = gameObject
            };
            onTriggerExit?.Invoke(colInfo);
            OnTriggerExitEvent?.Invoke(colInfo);
        }
    }
}