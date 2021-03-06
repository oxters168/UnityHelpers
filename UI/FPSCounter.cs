﻿using UnityEngine;

namespace UnityHelpers
{
    public class FPSCounter : MonoBehaviour
    {
        #if (TextMeshPro)
        public TMPro.TextMeshProUGUI fpsText;
        #else
        public UnityEngine.UI.Text fpsText;
        #endif
        public int displayEveryNthFrame = 4;
        private int lastDisplayed = 0;
        private float sum = 0;

        void Update()
        {
            displayEveryNthFrame = Mathf.Clamp(displayEveryNthFrame, 1, int.MaxValue);
            sum += 1 / Time.deltaTime;
            if (++lastDisplayed % displayEveryNthFrame == 0)
            {
                fpsText.text = Mathf.FloorToInt(sum / displayEveryNthFrame).ToString();
                sum = 0;
                lastDisplayed = 0;
            }
        }
    }
}