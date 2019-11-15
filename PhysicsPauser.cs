using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    /// <summary>
    /// Use this to pause and unpause all non kinematic rigidbodies in your scene
    /// </summary>
    public static class PhysicsPauser
    {
        private static Dictionary<Rigidbody, (Vector3, Vector3)> history = new Dictionary<Rigidbody, (Vector3, Vector3)>();

        public static void Pause(bool onOff)
        {
            if (onOff)
            {
                foreach (var physicsObject in Object.FindObjectsOfType<Rigidbody>())
                {
                    if (!physicsObject.isKinematic) //if something was originally kinematic don't touch it
                    {
                        history[physicsObject] = (physicsObject.velocity, physicsObject.angularVelocity);
                        physicsObject.isKinematic = true;
                    }
                }
            }
            else
            {
                foreach (var historyObject in history)
                {
                    historyObject.Key.isKinematic = false;
                    historyObject.Key.velocity = historyObject.Value.Item1;
                    historyObject.Key.angularVelocity = historyObject.Value.Item2;
                }
                history.Clear();
            }
        }
    }
}
