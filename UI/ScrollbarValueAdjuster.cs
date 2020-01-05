using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UnityHelpers
{
    public class ScrollbarValueAdjuster : MonoBehaviour
    {
        private Scrollbar scrollbar;
        private bool scrollbarErrored;

        [Tooltip("Scrollbar scrolls to nearest bookmark")]
        public bool magneticBookmarks;
        public float magneticScrollSpeed = 0.1f;
        [Tooltip("Automatically calculated magnetic bookmarks on scrollbar")]
        public uint equalSplits;
        [Tooltip("Manually placed magnetic bookmarks in scrollbar (between 0 and 1 inclusive)")]
        public float[] manualSplits;

        private float prevScrollbarValue = 0;
        private float lastScrollTime = float.MinValue;
        public float scrollCooldown = 1;

        void Update()
        {
            if (scrollbar == null)
            {
                scrollbar = GetComponent<Scrollbar>();
                if (scrollbar != null)
                    prevScrollbarValue = scrollbar.value;
            }

            if (scrollbar != null)
            {
                scrollbarErrored = false;
                if (magneticBookmarks)
                {
                    List<float> allBookmarks = new List<float>(manualSplits);
                    if (equalSplits > 1)
                        for (int i = 0; i < equalSplits; i++)
                            allBookmarks.Add((float)i / (equalSplits - 1));

                    float magneticValue = allBookmarks.Count > 0 ? allBookmarks[0] : scrollbar.value;
                    foreach (float currentBookmark in allBookmarks)
                        if (Mathf.Abs(currentBookmark - scrollbar.value) < Mathf.Abs(magneticValue - scrollbar.value))
                            magneticValue = currentBookmark;

                    int magneticScrollDirection = MathHelpers.GetDirection(magneticValue, scrollbar.value);
                    int currentScrollDirection = MathHelpers.GetDirection(scrollbar.value, prevScrollbarValue);
                    //Debug.Log("CV: " + scrollbar.value + " MV: " + magneticValue + " PV: " + prevScrollbarValue + " MSD: " + magneticScrollDirection + " CSD: " + currentScrollDirection);
                    if (currentScrollDirection != 0 && currentScrollDirection != magneticScrollDirection)
                        lastScrollTime = Time.time;
                    else if (Time.time - lastScrollTime >= scrollCooldown)
                        scrollbar.value += magneticScrollDirection * Mathf.Min(magneticScrollSpeed, Mathf.Abs(magneticValue - scrollbar.value));

                    prevScrollbarValue = scrollbar.value;
                }
            }
            else if (!scrollbarErrored)
            {
                Debug.LogError("ScrollbarValueAdjuster(" + transform.name + "): Could not find scrollbar component, this script should be attached to the same object that has the scrollbar component");
                scrollbarErrored = true;
            }
        }
    }
}