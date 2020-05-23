using UnityEngine;

namespace UnityHelpers
{
    public interface IGrabbable
    {
        void Grab(Grabber.GrabInfo grabberInfo, float maxGrabberForce);
        void Ungrab(Grabber.GrabInfo grabberInfo);
    }
}