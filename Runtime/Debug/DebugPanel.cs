﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityHelpers
{
    /// <summary>
    /// Lists the values given through the log function to the output text.
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        public string emptyMessage = "Nothing to show";

        private static Dictionary<string, (float, float, object)> debugValues = new Dictionary<string, (float, float, object)>();

        #if (TextMeshPro)
        public TMPro.TextMeshProUGUI output;
        #else
        public UnityEngine.UI.Text output;
        #endif
        private System.Text.StringBuilder builtOutput = new System.Text.StringBuilder();

        [Space(10), Tooltip("If set to true, will only output from filtered values")]
        public bool filterValues;
        [Tooltip("Will only show values that have the given keys (semicolon separated)")]
        public string keysFilter;
        [Tooltip("Will use the given object names as filters")]
        public GameObject[] filterFromName;

        private void Update()
        {
            List<string> toBeRemoved = new List<string>();
            foreach (var debugValue in debugValues)
            {
                if ((Time.time - debugValue.Value.Item2) > debugValue.Value.Item1)
                    toBeRemoved.Add(debugValue.Key);
            }
            foreach (var key in toBeRemoved)
                debugValues.Remove(key);

            var builtOutput = GetOutput();
            if (string.IsNullOrEmpty(builtOutput))
                builtOutput = emptyMessage;
            output.text = builtOutput;
        }
        
        public string GetOutput()
        {
            builtOutput.Clear();
            foreach (var debugValue in debugValues)
                if (!filterValues || IsFiltered(debugValue.Key))
                    builtOutput.AppendLine(debugValue.Key + ": " + debugValue.Value.Item3);
            
            return builtOutput.ToString();
        }
        public bool IsFiltered(string key)
        {
            string[] keys = keysFilter.Split(';');
            int keyIndex = System.Array.IndexOf(keys, key);
            bool isName = filterFromName.Select(obj => obj.name).Contains(key);
            return keyIndex >= 0 || isName;
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