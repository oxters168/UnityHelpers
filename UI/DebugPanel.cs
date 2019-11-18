using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    /// <summary>
    /// Lists the values given through the log function to the output text.
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        private static Dictionary<string, (bool, object)> debugValues = new Dictionary<string, (bool, object)>();
        public TMPro.TextMeshProUGUI output;
        private System.Text.StringBuilder builtOutput = new System.Text.StringBuilder();

        private void Update()
        {
            builtOutput.Clear();
            List<string> toBeRemoved = new List<string>();
            foreach (var debugValue in debugValues)
            {
                builtOutput.AppendLine(debugValue.Key + ": " + debugValue.Value.Item2);
                if (!debugValue.Value.Item1)
                    debugValues[debugValue.Key] = (false, debugValue.Value.Item2);
                else
                    toBeRemoved.Add(debugValue.Key);
            }
            foreach (var key in toBeRemoved)
                debugValues.Remove(key);

            output.text = builtOutput.ToString();
        }

        public static void Log(string name, object value)
        {
            debugValues[name] = (true, value);
        }
    }
}