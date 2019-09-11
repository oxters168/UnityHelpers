using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityHelpers
{
    [RequireComponent(typeof(TouchGesturesHandler))]
    public class SizableItem : MonoBehaviour, IScrollHandler
    {
        private Canvas canvas;

        private RectTransform _selfRectTransform;
        public RectTransform SelfRectTransform { get { if (!_selfRectTransform) _selfRectTransform = GetComponent<RectTransform>(); return _selfRectTransform; } }
        private TouchGesturesHandler _touchGestures;
        public TouchGesturesHandler TouchGestures { get { if (!_touchGestures) _touchGestures = GetComponent<TouchGesturesHandler>(); return _touchGestures; } }

        public float scrollMultiplier = 32;
        public float touchMultiplier = 1;
        public float rotMultiplier = 1;

        public float zoomedAmount;
        private Vector2 origSize;
        private bool origSizeSet;

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
        private void Update()
        {
            ApplyZoom();
        }

        private void TouchGestures_onPinch(Vector2 pos1, Vector2 pos2, float zoomDelta, float rotationDelta)
        {
            Vector2 pointPivot = CalculatePivot(pos1);
            Zoom(zoomDelta * touchMultiplier, pointPivot);
            SelfRectTransform.eulerAngles += Vector3.forward * rotationDelta * rotMultiplier;
        }

        public void OnScroll(PointerEventData eventData)
        {
            Vector2 pointPivot = CalculatePivot(eventData.position);
            Zoom(eventData.scrollDelta.y * scrollMultiplier, pointPivot);
        }
        private Vector2 CalculatePivot(Vector2 position)
        {
            Vector2 localScrollPoint;
            #region Calculating local position in rect transform
            if (canvas == null)
                canvas = SelfRectTransform.GetComponentInParent<Canvas>();

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                localScrollPoint = position.GetPositionRelativeTo(SelfRectTransform); //Works for Overlay mode
            else
                RectTransformUtility.ScreenPointToLocalPointInRectangle(SelfRectTransform, position, canvas.worldCamera, out localScrollPoint); //Works for Camera mode
            #endregion

            localScrollPoint = localScrollPoint.RemovePivotOffset(SelfRectTransform);
            Vector2 pointPivot = new Vector2(localScrollPoint.x / SelfRectTransform.rect.size.x, localScrollPoint.y / SelfRectTransform.rect.size.y);
            return pointPivot;
        }

        public void SetSize(Vector2 size)
        {
            origSize = size;
            SetZoomAmount(0);
        }
        public void SetZoomAmount(float amount)
        {
            zoomedAmount = 0;
            ApplyZoom();
        }
        public void Zoom(float amount)
        {
            zoomedAmount += amount;
            ApplyZoom();
        }
        public void Zoom(float amount, Vector2 pivot)
        {
            zoomedAmount += amount;
            ApplyZoom(pivot);
        }

        private void CheckOrigSize()
        {
            if (!origSizeSet)
            {
                origSize = SelfRectTransform.sizeDelta;
                origSizeSet = true;
            }
        }
        private void ApplyZoom()
        {
            CheckOrigSize();
            SelfRectTransform.sizeDelta = origSize + Vector2.one * zoomedAmount;
        }
        private void ApplyZoom(Vector2 pivot)
        {
            CheckOrigSize();
            Vector2 prevPivot = SelfRectTransform.pivot;
            SelfRectTransform.ShiftPivot(pivot);
            SelfRectTransform.sizeDelta = origSize + Vector2.one * zoomedAmount;
            SelfRectTransform.ShiftPivot(prevPivot);
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