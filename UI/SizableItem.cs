using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityHelpers
{
    [RequireComponent(typeof(TouchGesturesHandler))]
    public class SizableItem : MonoBehaviour, IScrollHandler
    {
        private RectTransform _selfRectTransform;
        public RectTransform SelfRectTransform { get { if (!_selfRectTransform) _selfRectTransform = GetComponent<RectTransform>(); return _selfRectTransform; } }
        private TouchGesturesHandler _touchGestures;
        public TouchGesturesHandler TouchGestures { get { if (!_touchGestures) _touchGestures = GetComponent<TouchGesturesHandler>(); return _touchGestures; } }

        public float scrollMultiplier = 32;
        public float touchMultiplier = 1;
        public float rotMultiplier = 1;

        private Vector2 orig1, orig2;
        private Vector2 current1, current2;
        public bool isPinching { get; private set; }
        private bool isFirstTouchInside;
        private bool isTouching;

        private void OnEnable()
        {
            TouchGestures.onPinch += TouchGestures_onPinch;
        }
        private void OnDisable()
        {
            TouchGestures.onPinch -= TouchGestures_onPinch;
        }

        private void TouchGestures_onPinch(Vector2 pos1, Vector2 pos2, float zoomDelta, float rotationDelta)
        {
            Zoom(touchMultiplier * zoomDelta);
            SelfRectTransform.eulerAngles += Vector3.forward * rotationDelta * rotMultiplier;
        }

        public void OnScroll(PointerEventData eventData)
        {
            Zoom(eventData.scrollDelta.y * scrollMultiplier);
        }

        private void Zoom(float amount)
        {
            SelfRectTransform.sizeDelta += Vector2.one * amount;
        }

        private void Pinched(Vector2 pos1, Vector2 pos2)
        {
            isPinching = true;
            orig1 = pos1;
            orig2 = pos2;
            current1 = pos1;
            current2 = pos2;
        }
        private void Pinching(Vector2 pos1, Vector2 pos2)
        {
            float oldDistance = Vector2.Distance(current1, current2);
            float newDistance = Vector2.Distance(pos1, pos2);
            current1 = pos1;
            current2 = pos2;

            float delta = newDistance - oldDistance;
            Zoom(delta * touchMultiplier);
        }
        private void PinchEnded()
        {
            isPinching = false;
        }
    }
}