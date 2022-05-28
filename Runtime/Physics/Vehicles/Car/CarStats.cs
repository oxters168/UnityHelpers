using UnityEngine;

namespace UnityHelpers
{
    [CreateAssetMenu(fileName = "CarStats", menuName = "Car/Stats", order = 1)]
    public class CarStats : ScriptableObject
    {
        public int index;

        [Tooltip("The name of the company that built the car")]
        public string companyName = "Generic";
        [Tooltip("The name of the car model itself")]
        public string modelName = "Model";
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

        [Space(10), Tooltip("This is the value that will be put into the abstract wheel, so it depends on your derivation")]
        public float grip = 3;
        [Tooltip("The maximum the tires can rotate in degrees in the local y axis when stopped")]
        public float slowWheelAngle = 33.33f;
        [Tooltip("The maximum the tires can rotate in degrees in the local y axis when going at maxForwardSpeed")]
        public float fastWheelAngle = 5f;
        [Tooltip("This graph depicts how quickly we go from slowWheelAngle to fastWheelAngle based on speed, where 0 on the x axis represents not moving and 1 on the x axis represents maxForwardSpeed")]
        public AnimationCurve wheelAngleCurve;
        [Tooltip("How much does steering cause the vehicle to slow down relative to speed")]
        public AnimationCurve percentSteerEffectsBrake;

        [Space(10), Tooltip("The amount of rays (odd index i.e. 1 = 1, 2 = 3, 3 = 5...")]
        public uint forwardRays;
        [Tooltip("The amount of rays (odd index i.e. 1 = 1, 2 = 3, 3 = 5...")]
        public uint leftRays;
        [Tooltip("The amount of rays (odd index i.e. 1 = 1, 2 = 3, 3 = 5...")]
        public uint rightRays;
        [Tooltip("The amount of rays (odd index i.e. 1 = 1, 2 = 3, 3 = 5...")]
        public uint rearRays;

        [Space(10), Tooltip("In meters")]
        public float forwardDistanceObstacleCheck = 10;
        [Tooltip("In meters")]
        public float leftDistanceObstacleCheck = 1;
        [Tooltip("In meters")]
        public float rightDistanceObstacleCheck = 1;
        [Tooltip("In meters")]
        public float rearDistanceObstacleCheck = 10;
    }
}