using UnityEngine;

namespace UnityHelpers
{
    public static class QuaternionHelpers
    {
        /// <summary>
        /// Determines whether the quaternion is safe for interpolation or use with transform.rotation.
        /// </summary>
        /// <returns><c>false</c> if using the quaternion in Quaternion.Lerp() will result in an error (eg. NaN values or zero-length quaternion).</returns>
        /// <param name="quaternion">Quaternion.</param>
        public static bool IsValid(this Quaternion quaternion)
        {
            bool isNaN = float.IsNaN(quaternion.x + quaternion.y + quaternion.z + quaternion.w);

            bool isZero = quaternion.x == 0 && quaternion.y == 0 && quaternion.z == 0 && quaternion.w == 0;

            return !(isNaN || isZero);
        }
        /// <summary>
        /// Turns all NaN values to 0
        /// </summary>
        /// <param name="quaternion">The quaternion in question</param>
        /// <returns>The fixed quaternion</returns>
        public static Quaternion FixNaN(this Quaternion quaternion)
        {
            if (float.IsNaN(quaternion.x))
                quaternion.x = 0;
            if (float.IsNaN(quaternion.y))
                quaternion.y = 0;
            if (float.IsNaN(quaternion.z))
                quaternion.z = 0;
            if (float.IsNaN(quaternion.w))
                quaternion.w = 0;

            return quaternion;
        }
        /// <summary>
        /// Sets the x, y, z, and w values' decimal places
        /// </summary>
        /// <param name="quaternion">The original quaternion rotation</param>
        /// <param name="places">Number of decimal places to keep</param>
        /// <returns>The quaternion with it's values' decimal places adjusted</returns>
        public static Quaternion SetDecimalPlaces(this Quaternion quaternion, uint places)
        {
            return new Quaternion(MathHelpers.SetDecimalPlaces(quaternion.x, places), MathHelpers.SetDecimalPlaces(quaternion.y, places), MathHelpers.SetDecimalPlaces(quaternion.z, places), MathHelpers.SetDecimalPlaces(quaternion.w, places));
        }
    }
}