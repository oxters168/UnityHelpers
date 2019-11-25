using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    public class FocusCameraController : BaseCameraController
    {
        private Camera attachedCamera;
        private List<Transform> targets = new List<Transform>();
        [Tooltip("This is the normal of the targets that will be used to help calculate the direction of the camera")]
        /// <summary>
        /// This is the normal of the targets that will be used to help calculate the direction of the camera
        /// </summary>
        public Vector3 targetNormal = Vector3.up;

        private void Start()
        {
            attachedCamera = GetComponentInChildren<Camera>();
            Debug.Assert(attachedCamera != null, "FocusCameraController: Could not find camera component");
        }
        private void Update()
        {
            if (targets.Count > 0)
            {
                Bounds totalBounds = targets[0].GetTotalBounds();
                for (int i = 1; i < targets.Count; i++)
                    totalBounds = totalBounds.Combine(targets[i].GetTotalBounds());

                Vector3 startPosition = targets[0].position;
                Vector3 endPosition = targets[targets.Count - 1].position;
                Vector3 connectingLine = (endPosition - startPosition).Planar(targetNormal);
                Vector3 cameraDirection = Vector3.Cross(targetNormal, connectingLine.normalized);
                float cameraDistance = attachedCamera.PerspectiveDistanceFromWidth((totalBounds.max - totalBounds.min).magnitude);
                transform.forward = -cameraDirection;
                transform.position = totalBounds.center + cameraDirection * cameraDistance;
            }
        }

        protected override void ApplyInput()
        {
            //throw new System.NotImplementedException();
        }
        public void AddTarget(Transform target)
        {
            targets.Add(target);
        }
        public void RemoveTarget(Transform target)
        {
            targets.Remove(target);
        }
        public void ClearTargets()
        {
            targets.Clear();
        }
    }
}