using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityHelpers
{
    public class ScrollbarValueAdjuster : MonoBehaviour
    {
        private Scrollbar scrollbar;
        private bool scrollbarErrored;
        public uint decimalPrecision = 3;

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
                    var magneticValue = GetMagneticValue();

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

        public int GetBookmarksCount()
        {
            return GetAllBookmarks().Count;
        }
        private List<float> GetAllBookmarks()
        {
            List<float> allBookmarks = new List<float>(manualSplits);
            for (int i = 0; i < allBookmarks.Count; i++)
                allBookmarks[i] = MathHelpers.SetDecimalPlaces(allBookmarks[i], decimalPrecision);

            if (equalSplits > 1)
                for (int i = 0; i < equalSplits; i++)
                {
                    float currentBookmark = MathHelpers.SetDecimalPlaces((float)i / (equalSplits - 1), decimalPrecision);
                    if (!allBookmarks.Contains(currentBookmark))
                        allBookmarks.Add(currentBookmark);
                }

            allBookmarks.Sort();

            return allBookmarks;
        }
        private float GetMagneticValue()
        {
            var allBookmarks = GetAllBookmarks();

            float magneticValue = allBookmarks.Count > 0 ? allBookmarks[0] : scrollbar.value;
            foreach (float currentBookmark in allBookmarks)
                if (Mathf.Abs(currentBookmark - scrollbar.value) < Mathf.Abs(magneticValue - scrollbar.value))
                    magneticValue = currentBookmark;
            return magneticValue;
        }
        public void GotoNextBookmark()
        {
            var bookmarks = GetAllBookmarks();
            int index = GetBookmarkIndex(scrollbar.value, bookmarks);
            if (index + 1 < bookmarks.Count)
                index++;
            scrollbar.value = bookmarks[index];
        }
        public void GotoPreviousBookmark()
        {
            var bookmarks = GetAllBookmarks();
            int index = GetBookmarkIndex(scrollbar.value, bookmarks);
            if (index - 1 >= 0)
                index--;
            scrollbar.value = bookmarks[index];
        }
        public static int GetBookmarkIndex(float value, List<float> bookmarks)
        {
            //var bookmarks = GetAllBookmarks();
            int currentIndex = -1;
            float smallestDistance = float.MaxValue;
            for (int i = 0; i < bookmarks.Count; i++)
            {
                float currentDistance = Mathf.Abs(value - bookmarks[i]);
                if (currentDistance < smallestDistance)
                {
                    currentIndex = i;
                    smallestDistance = currentDistance;
                }
            }

            return currentIndex;
        }
    }
}