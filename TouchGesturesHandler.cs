using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityHelpers
{
    public class TouchGesturesHandler : MonoBehaviour
    {
        private Canvas canvas;

        private RectTransform _selfRectTransform;
        public RectTransform SelfRectTransform { get { if (!_selfRectTransform) _selfRectTransform = GetComponent<RectTransform>(); return _selfRectTransform; } }

        public event DragHandler onDrag, onEndDrag, onDown, onUp;
        public delegate void DragHandler(Vector2 position, Vector2 delta);

        public event PinchHandler onPinchStart, onPinch, onPinchEnd;
        public delegate void PinchHandler(Vector2 pos1, Vector2 pos2, float deltaZoom, float deltaRotation);

        public float tapDuration = 0.2f;
        private float startTouchTime;

        public bool isDragging { get; private set; }
        public bool isPinching { get; private set; }
        private bool isFirstTouchInside;
        private int highestTouchCount;

        private int currentNumberOfTouches;

        private Vector2[] prevTouches = new Vector2[10];

        void Update()
        {
            if (Input.touches.Length > 0 && currentNumberOfTouches <= 0)
            {
                Vector2 localTouch = GetTouchAsLocal(0);

                isFirstTouchInside = SelfRectTransform.rect.Contains(localTouch);

                if (isFirstTouchInside)
                    Down(Input.touches[0].position);

                startTouchTime = Time.time;
            }
            else if (Input.touches.Length > 0)
            {
                //Have to do this since when the number of fingers changes it causes
                //the previous position to change dramatically depending on what finger
                //was removed.
                if (currentNumberOfTouches != Input.touches.Length)
                    for (int i = 0; i < Input.touches.Length; i++)
                        prevTouches[i] = Input.touches[i].position;

                if (isFirstTouchInside)
                {
                    Drag(Input.touches[0].position);
                }

                if (Input.touches.Length > 1)
                {
                    if (!isPinching && isFirstTouchInside)
                    {
                        Vector2 position1 = Input.touches[0].position;
                        Vector2 position2 = Input.touches[1].position;

                        StartPinch(position1, position2);
                    }
                    else if (isPinching)
                    {
                        Vector2 position1 = Input.touches[0].position;
                        Vector2 position2 = Input.touches[1].position;
                        Pinching(position1, position2);
                    }
                }
                else
                    EndPinch();
            }
            else
            {
                EndPinch();
                EndDrag();

                if (highestTouchCount == 1 && Time.time - startTouchTime <= tapDuration && (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.GetComponent<IPointerClickHandler>() == null))
                    onUp?.Invoke(prevTouches[0], Vector2.zero);

                startTouchTime = -1;
                highestTouchCount = 0;
                isFirstTouchInside = false;
            }

            currentNumberOfTouches = Input.touches.Length;
            highestTouchCount = Mathf.Max(currentNumberOfTouches, highestTouchCount);
        }

        private void Down(Vector2 position)
        {
            Vector2 delta = position - prevTouches[0];
            prevTouches[0] = position;
            onDown?.Invoke(position, delta);
        }
        private void Drag(Vector2 position)
        {
            isDragging = true;
            Vector2 delta = position - prevTouches[0];
            prevTouches[0] = position;
            onDrag?.Invoke(position, delta);
        }
        private void EndDrag()
        {
            isDragging = false;
            onEndDrag?.Invoke(prevTouches[0], Vector2.zero);
        }

        private void StartPinch(Vector2 pos1, Vector2 pos2)
        {
            isPinching = true;
            prevTouches[0] = pos1;
            prevTouches[1] = pos2;

            onPinchStart?.Invoke(pos1, pos2, 0, 0);
        }
        private void Pinching(Vector2 pos1, Vector2 pos2)
        {
            Vector2 dir1 = (pos2 - pos1).normalized;
            Vector2 dir2 = (prevTouches[1] - prevTouches[0]).normalized;
            float rot = Vector2.SignedAngle(dir2, dir1);

            float oldDistance = Vector2.Distance(prevTouches[0], prevTouches[1]);
            float newDistance = Vector2.Distance(pos1, pos2);
            prevTouches[0] = pos1;
            prevTouches[1] = pos2;

            float delta = newDistance - oldDistance;
            onPinch?.Invoke(pos1, pos2, delta, rot);
        }
        private void EndPinch()
        {
            isPinching = false;
            onPinchEnd?.Invoke(prevTouches[0], prevTouches[1], 0, 0);
        }

        private Vector2 GetTouchAsLocal(int touchIndex)
        {
            if (canvas == null)
                canvas = SelfRectTransform.GetComponentInParent<Canvas>();

            Vector2 touchPos = Input.touches[touchIndex].position;
            Vector2 localTouch;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                localTouch = touchPos.GetPositionRelativeTo(SelfRectTransform);
            else
                RectTransformUtility.ScreenPointToLocalPointInRectangle(SelfRectTransform, touchPos, canvas.worldCamera, out localTouch);
            return localTouch;
        }
    }
}