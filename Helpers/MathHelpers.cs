using UnityEngine;

namespace UnityHelpers
{
    public static class MathHelpers
    {
        /// <summary>
        /// Converts a value that's in meters per second to kilometers per hour by multiplying (mps * MPS_TO_KMH = kmh).
        /// </summary>
        public const float MPS_TO_KMH = 3.6f;
        /// <summary>
        /// Converts a value that's in meters per second to miles per hour by multiplying (mps * MPS_TO_MPH = mph).
        /// </summary>
        public const float MPS_TO_MPH = 2.237f;
        /// <summary>
        /// Converts a value that's in meters to miles by multiplying (meters * METERS_TO_MILES = miles).
        /// </summary>
        public const float METERS_TO_MILES = 0.00062150404f;
        /// <summary>
        /// Converts a value that's in meters to kilometers by multiplying (meters * METERS_TO_KILOS = kilometers).
        /// </summary>
        public const float METERS_TO_KILOS = 0.001f;

        /// <summary>
        /// Returns an odd number given an index (0 => 1, 1 => 3, 2 => 5...)
        /// </summary>
        /// <param name="index">The index of the odd number</param>
        /// <returns>An odd number</returns>
        public static int GetOddNumber(int index)
        {
            return index != 0 ? Mathf.RoundToInt((index - Mathf.Sign(index)) * 2 + Mathf.Sign(index)) : 0;
        }

        /// <summary>
        /// Sets the decimal places of the value.
        /// </summary>
        /// <param name="value">The original value</param>
        /// <param name="places">Number of decimal places to keep</param>
        /// <returns>The value with only the number of decimal places specified</returns>
        public static float SetDecimalPlaces(float value, uint places)
        {
            if (places > 0)
            {
                for (int i = 0; i < places; i++)
                    value *= 10;
                value = (int)value;
                for (int i = 0; i < places; i++)
                    value /= 10;
            }
            else
                value = (int)value;

            return value;
        }

        /// <summary>
        /// Gets the number line direction between a and b (a - b). -1 for "left", 1 for "right", and 0 for equal.
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <returns>Direction on number line</returns>
        public static int GetDirection(float a, float b)
        {
            float aRounded = SetDecimalPlaces(a, 5);
            float bRounded = SetDecimalPlaces(b, 5);
            return Mathf.Abs(aRounded - bRounded) > Mathf.Epsilon ? Mathf.RoundToInt(Mathf.Sign(aRounded - bRounded)) : 0;
        }
    }
}