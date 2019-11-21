using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    /// <summary>
    /// This script is meant to be used on objects that have many colliders in their children
    /// that you'd like to listen to from a parent. Only attach this script to the parent.
    /// </summary>
    [ExecuteAlways]
    public class TreeCollider : MonoBehaviour
    {
        private List<TreeCollider> childTrees = new List<TreeCollider>();
        public bool debugMessages;

        public UnifiedCollisionEvent onCollided = new UnifiedCollisionEvent();
        public UnifiedCollisionEvent onTriggerEnter = new UnifiedCollisionEvent();
        public UnifiedCollisionEvent onTriggerStay = new UnifiedCollisionEvent();
        public UnifiedCollisionEvent onTriggerExit = new UnifiedCollisionEvent();
        public UnifiedCollisionEvent onCollisionEnter = new UnifiedCollisionEvent();
        public UnifiedCollisionEvent onCollisionStay = new UnifiedCollisionEvent();
        public UnifiedCollisionEvent onCollisionExit = new UnifiedCollisionEvent();

        private void Start()
        {
            InitChildren();
        }
        private void OnDestroy()
        {
            foreach (var child in childTrees)
            {
                if (child != null)
                {
                    StopListeningTo(child);
                    DestroyImmediate(child);
                }
            }
        }

        private void StartListeningTo(TreeCollider other)
        {
            other.onTriggerEnter.AddListener(OnTriggerEnter);
            other.onTriggerStay.AddListener(OnTriggerStay);
            other.onTriggerExit.AddListener(OnTriggerExit);
            other.onCollisionEnter.AddListener(OnCollisionEnter);
            other.onCollisionStay.AddListener(OnCollisionStay);
            other.onCollisionExit.AddListener(OnCollisionExit);
        }
        private void StopListeningTo(TreeCollider other)
        {
            other.onTriggerEnter.RemoveListener(OnTriggerEnter);
            other.onTriggerStay.RemoveListener(OnTriggerStay);
            other.onTriggerExit.RemoveListener(OnTriggerExit);
            other.onCollisionEnter.RemoveListener(OnCollisionEnter);
            other.onCollisionStay.RemoveListener(OnCollisionStay);
            other.onCollisionExit.RemoveListener(OnCollisionExit);
        }

        private CollisionInfo CreateCollisionInfo(Collider col, CollisionInfo.CollisionState colState)
        {
            return new CollisionInfo
            {
                isTrigger = true,
                collisionState = colState,
                sender = gameObject,
                collidedWith = col.gameObject,
                otherCollider = col,
            };
        }
        private CollisionInfo CreateCollisionInfo(Collision col, CollisionInfo.CollisionState colState)
        {
            return new CollisionInfo
            {
                isTrigger = false,
                collisionState = colState,
                sender = gameObject,
                collidedWith = col.gameObject,
                otherCollider = col.collider,
                collision = col,
            };
        }

        private void OnCollided(CollisionInfo colInfo)
        {
            if (debugMessages)
                Debug.Log(GetDebugCollisionMessage(colInfo));

            onCollided?.Invoke(colInfo);
        }
        #region 3D Trigger Events
        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEnter(CreateCollisionInfo(other, CollisionInfo.CollisionState.enter));
        }
        private void OnTriggerEnter(CollisionInfo colInfo)
        {
            onTriggerEnter?.Invoke(colInfo);
            OnCollided(colInfo);
        }
        private void OnTriggerStay(Collider other)
        {
            OnTriggerStay(CreateCollisionInfo(other, CollisionInfo.CollisionState.stay));
        }
        private void OnTriggerStay(CollisionInfo colInfo)
        {
            onTriggerStay?.Invoke(colInfo);
            OnCollided(colInfo);
        }
        private void OnTriggerExit(Collider other)
        {
            OnTriggerExit(CreateCollisionInfo(other, CollisionInfo.CollisionState.exit));
        }
        private void OnTriggerExit(CollisionInfo colInfo)
        {
            onTriggerExit?.Invoke(colInfo);
            OnCollided(colInfo);
        }
        #endregion
        #region 3D Collision Events
        private void OnCollisionEnter(Collision collision)
        {
            OnCollisionEnter(CreateCollisionInfo(collision, CollisionInfo.CollisionState.enter));
        }
        private void OnCollisionEnter(CollisionInfo colInfo)
        {
            onCollisionEnter?.Invoke(colInfo);
            OnCollided(colInfo);
        }
        private void OnCollisionStay(Collision collision)
        {
            OnCollisionStay(CreateCollisionInfo(collision, CollisionInfo.CollisionState.stay));
        }
        private void OnCollisionStay(CollisionInfo colInfo)
        {
            onCollisionStay?.Invoke(colInfo);
            OnCollided(colInfo);
        }
        private void OnCollisionExit(Collision collision)
        {
            OnCollisionExit(CreateCollisionInfo(collision, CollisionInfo.CollisionState.exit));
        }
        private void OnCollisionExit(CollisionInfo colInfo)
        {
            onCollisionExit?.Invoke(colInfo);
            OnCollided(colInfo);
        }
        #endregion

        public static string GetDebugCollisionMessage(CollisionInfo colInfo)
        {
            string whatHappened;
            if (colInfo.collisionState == CollisionInfo.CollisionState.enter)
                whatHappened = " entered ";
            else if (colInfo.collisionState == CollisionInfo.CollisionState.stay)
                whatHappened = " stayed in ";
            else if (colInfo.collisionState == CollisionInfo.CollisionState.exit)
                whatHappened = " exited ";
            else
                whatHappened = " ? ";

            return colInfo.sender.name + whatHappened + colInfo.collidedWith.name + ", isTrigger: " + colInfo.isTrigger;
        }

        private void InitChildren()
        {
            Collider[] childrenColliders = GetComponentsInChildren<Collider>(true);
            foreach (var collider in childrenColliders)
            {
                if (collider.transform.parent.GetComponentInParent<TreeCollider>().transform == transform)
                {
                    var childTreeCollider = collider.gameObject.GetComponent<TreeCollider>();
                    if (childTreeCollider == null)
                        childTreeCollider = collider.gameObject.AddComponent<TreeCollider>();

                    if (!childTrees.Contains(childTreeCollider))
                    {
                        StartListeningTo(childTreeCollider);
                        childTrees.Add(childTreeCollider);
                    }
                }
            }
        }

        public struct CollisionInfo
        {
            public enum CollisionState { enter, stay, exit, }

            public bool isTrigger { get; internal set; }
            public CollisionState collisionState { get; internal set; }
            public GameObject sender { get; internal set; }
            public GameObject collidedWith { get; internal set; }
            public Collider otherCollider { get; internal set; }
            /// <summary>
            /// If the collision is trigger this value will be null
            /// </summary>
            public Collision collision { get; internal set; }
        }

        [System.Serializable]
        public class UnifiedCollisionEvent : UnityEngine.Events.UnityEvent<CollisionInfo> { }
    }
}