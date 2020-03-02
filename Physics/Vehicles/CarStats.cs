using UnityEngine;

namespace UnityHelpers
{
    [CreateAssetMenu(fileName = "CarStats", menuName = "Car/Stats", order = 1)]
    public class CarStats : ScriptableObject
    {
        [Tooltip("How fast the vehicle accelerates in m/s^2 when the gas is pressed")]
        public float acceleration = 3.4f;
        [Tooltip("How fast the vehicle slows down in m/s^2 when the gas and brakes aren't pressed")]
        public float deceleration = 1.2f;
        [Tooltip("How fast the vehicle slows down in m/s^2 when the brakes are pressed")]
        public float brakeleration = 10f;
        [Tooltip("The maximum speed in m/s the vehicle can reach when driving forward")]
        public float maxForwardSpeed = 57.2f;
        [Tooltip("The maximum speed in m/s the vehicle can reach when driving backward")]
        public float maxReverseSpeed = 28.6f;
        [Tooltip("The maximum the tires can rotate in degrees in the local y axis")]
        public float maxWheelAngle = 33.33f;
    }
}