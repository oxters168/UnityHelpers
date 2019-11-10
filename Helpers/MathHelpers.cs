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
    }
}