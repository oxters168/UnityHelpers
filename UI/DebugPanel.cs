using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityHelpers
{
    /// <summary>
    /// Lists the values given through the log function to the output text.
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        private static Dictionary<string, (float, float, object)> debugValues = new Dictionary<string, (float, float, object)>();
        public TMPro.TextMeshProUGUI output;
        private System.Text.StringBuilder builtOutput = new System.Text.StringBuilder();

        private void Update()
        {
            builtOutput.Clear();
            List<string> toBeRemoved = new List<string>();
            foreach (var debugValue in debugValues)
            {
                builtOutput.AppendLine(debugValue.Key + ": " + debugValue.Value.Item3);
                if (Time.time - debugValue.Value.Item2 > debugValue.Value.Item1)
                    toBeRemoved.Add(debugValue.Key);
            }
            foreach (var key in toBeRemoved)
                debugValues.Remove(key);

            output.text = builtOutput.ToString();
        }

        public static void Log(string name, object value)
        {
            Log(name, value, Time.fixedDeltaTime);
        }
        public static void Log(string name, object value, float showtime)
        {
            debugValues[name] = (showtime, Time.time, value);
        }
    }
}