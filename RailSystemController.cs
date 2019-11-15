using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// Moves a target along rail checkpoints
    /// </summary>
    public class RailSystemController : MonoBehaviour
    {
        public Transform target;
        public float targetLerp = 5;
        public bool play;
        [Range(0, 1)]
        public float value;
        [Range(-1, 1)]
        public float speed = 0.01f;
        public bool loop;

        public bool showCheckpointsGizmos;
        public PosRotWrapper[] checkpoints;

        void Update()
        {
            if (target != null && checkpoints?.Length > 0)
            {
                if (loop && value >= 1)
                    value = 0;

                float totalLength = checkpoints.Length - (loop ? 0 : 1);

                int index = Mathf.FloorToInt(value * totalLength);
                int nextIndex = (index + (speed > 0 ? 1 : -1) + checkpoints.Length) % checkpoints.Length;
                float lerp = (value * totalLength) - index;

                target.position = Vector3.Lerp(target.position, Vector3.Lerp(checkpoints[index].position, checkpoints[nextIndex].position, checkpoints[index].transition.Evaluate(lerp)), Time.deltaTime * targetLerp);
                target.rotation = Quaternion.Lerp(target.rotation, Quaternion.Lerp(Quaternion.Euler(checkpoints[index].rotation), Quaternion.Euler(checkpoints[nextIndex].rotation), checkpoints[index].transition.Evaluate(lerp)), Time.deltaTime * targetLerp);

                if (play)
                    value = Mathf.Clamp01(value + speed);
            }
        }

        private void OnDrawGizmos()
        {
            if (showCheckpointsGizmos)
            {
                Gizmos.color = Color.green;
                if (checkpoints != null)
                    for (int i = 0; i < checkpoints.Length; i++)
                    {
                        Gizmos.matrix = Matrix4x4.TRS(checkpoints[i].position, Quaternion.Euler(checkpoints[i].rotation), Vector3.one);
                        Gizmos.DrawFrustum(Vector3.zero, Camera.main.fieldOfView, 1, 0.01f, Camera.main.aspect);
                    }
            }
        }
    }

    [System.Serializable]
    public struct PosRotWrapper
    {
        public Vector3 position;
        public Vector3 rotation;
        public AnimationCurve transition;
    }
}