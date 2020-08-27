using System.Collections.Generic;
using UnityEngine;

namespace UnityHelpers
{
	public class UnityConsoleOutput : MonoBehaviour
	{
		public static string log { get; private set; }
		List<string> myLogList = new List<string>();
		public int maxOutputLines = 50;

		public int frames = 10;
		private int framesPassed = 0;
		#if (TextMeshPro)
		public TMPro.TextMeshProUGUI outputLabel;
		#else
		public UnityEngine.UI.Text outputLabel;
		#endif

		void OnEnable ()
		{
			Application.logMessageReceived += HandleLog;
		}

		void OnDisable ()
		{
			Application.logMessageReceived -= HandleLog;
		}

		void HandleLog(string logString, string stackTrace, LogType type)
		{
			log = logString;
			string newString = "\n [" + type + "] : " + log;
			myLogList.Add(newString);
			if (type == LogType.Exception)
			{
				newString = "\n" + stackTrace;
				myLogList.Add(newString);
			}
			log = string.Empty;
			for (int i = Mathf.Max(0, myLogList.Count - maxOutputLines - 1); i < myLogList.Count; i++)
			{
				log += myLogList[i];
			}
		}

		void Update()
		{
			if (framesPassed >= frames)
			{
				framesPassed = 0;
				outputLabel.text = log;
			}
			framesPassed++;
		}
	}
}