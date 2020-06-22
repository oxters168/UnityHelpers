using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    /// <summary>
    /// Use this to pause and unpause all non kinematic rigidbodies in your scene
    /// </summary>
    public static class PhysicsPauser
    {
		private static Dictionary<Rigidbody, PhysicsSnapshot> history = new Dictionary<Rigidbody, PhysicsSnapshot>();

        public static void Pause(bool onOff)
        {
            if (onOff)
            {
                foreach (var physicsObject in Object.FindObjectsOfType<Rigidbody>())
                {
                    if (!physicsObject.isKinematic) //if something was originally kinematic don't touch it
                    {
						history[physicsObject] = new PhysicsSnapshot() { velocity = physicsObject.velocity, angularVelocity = physicsObject.angularVelocity };
                        physicsObject.isKinematic = true;
                    }
                }
            }
            else
            {
                foreach (var historyObject in history)
                {
                    historyObject.Key.isKinematic = false;
					historyObject.Key.velocity = historyObject.Value.velocity;
					historyObject.Key.angularVelocity = historyObject.Value.angularVelocity;
                }
                history.Clear();
            }
        }

		public struct PhysicsSnapshot
		{
			public Vector3 velocity;
			public Vector3 angularVelocity;
		}
    }
}
