using UnityEngine;

namespace UnityHelpers
{
    public interface IGrabbable
    {
        void Grab(Transform grabber, float maxGrabberForce);
        void Ungrab(Transform grabber);
    }
}