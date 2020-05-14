using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// This script is meant to be derived from for different wheel controllers
    /// </summary>
    public abstract class AbstractWheel : MonoBehaviour
    {
        public abstract bool IsGrounded();
        public abstract void SetGrip(float value);
    }
}