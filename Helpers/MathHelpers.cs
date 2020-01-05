using UnityEngine;

namespace UnityHelpers
{
    public static class MathHelpers
    {
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