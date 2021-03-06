﻿using UnityEngine;

namespace UnityHelpers
{
    public interface IGrabbable
    {
        void Grab(LocalInfo grabberInfo);
        void Ungrab(LocalInfo grabberInfo);
        void UngrabAll();
        LocalInfo GetGrabber(int index);
        void SetMaxGrabbers(int amount);
        int GetMaxGrabbers();
        LocalInfo CreateLocalInfo(Grabber.GrabInfo grabInfo, float maxForce);
        bool GetLocalInfo(Grabber.GrabInfo grabInfo, out LocalInfo localInfo);
        int GetGrabCount();
    }
}