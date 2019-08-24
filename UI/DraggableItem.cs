using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityHelpers
{
    [RequireComponent(typeof(TouchGesturesHandler))]
    public class DraggableItem : MonoBehaviour, IPointerClickHandler, IDragHandler, IPointerDownHandler, IEndDragHandler
    {
        private RectTransform _selfRectTransform;
        public RectTransform SelfRectTransform { get { if (!_selfRectTransform) _selfRectTransform = GetComponent<RectTransform>(); return _selfRectTransform; } }
        private RectTransform _canvasRectTransform;
        public RectTransform CanvasRectTransform { get { if (!_canvasRectTransform) _canvasRectTransform = SelfRectTransform.root.GetComponentInChildren<RectTransform>(); return _canvasRectTransform; } }
        private TouchGesturesHandler _touchGestures;
        public TouchGesturesHandler TouchGestures { get { if (!_touchGestures) _touchGestures = GetComponent<TouchGesturesHandler>(); return _touchGestures; } }

        public RectTransform container;

        public bool isDragging { get; private set; }
        public event PointerHandler onDrag, onEndDrag, onClick, onDown;
        public delegate void PointerHandler();

        //private Vector2 origPosition;
        //private Vector2 downPointerPosition;

        private bool isTouching;

        private void Update()
        {
            SelfRectTransform.localPosition = ClampToContainer(SelfRectTransform.localPosition.x, SelfRectTransform.localPosition.y);
        }
        private void OnEnable()
        {
            //TouchGestures.onDown += TouchGestures_onDown;
            TouchGestures.onDrag += TouchGestures_onDrag;
            TouchGestures.onEndDrag += TouchGestures_onEndDrag;
        }
        private void OnDisable()
        {
            //TouchGestures.onDown -= TouchGestures_onDown;
            TouchGestures.onDrag -= TouchGestures_onDrag;
            TouchGestures.onEndDrag -= TouchGestures_onEndDrag;
        }

        private void TouchGestures_onEndDrag(Vector2 position, Vector2 delta)
        {
            EndDrag();
        }
        private void TouchGestures_onDrag(Vector2 position, Vector2 delta)
        {
            Drag(delta);
        }
        private void TouchGestures_onDown(Vector2 position, Vector2 delta)
        {
            //Down(position);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //if (Input.touches.Length <= 0)
            //    Down(eventData.position);
        }
        public void OnDrag(PointerEventData eventData)
        {
            //SelfRectTransform.localPosition = ClampToContainer(nextButtonX, nextButtonY);
            if (Input.touches.Length <= 0)
                Drag(eventData.delta);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (Input.touches.Length <= 0)
                EndDrag();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke();
        }

        /*private void Down(Vector2 position)
        {
            origPosition = SelfRectTransform.localPosition;
            downPointerPosition = position;

            onDown?.Invoke();
        }*/
        private void Drag(Vector2 delta)
        {
            Vector2 nextPos = (Vector2)SelfRectTransform.localPosition + delta;
            //float nextPosY = origPosition.y + (delta.y - downPointerPosition.y);
            SelfRectTransform.localPosition = ClampToContainer(nextPos.x, nextPos.y);

            isDragging = true;
            onDrag?.Invoke();
        }
        private void EndDrag()
        {
            isDragging = false;
            onEndDrag?.Invoke();
        }

        private Vector2 ClampToContainer(float x, float y)
        {
            if (container)
            {
                Vector2 halfContainerSize = container.rect.size / 2;
                Vector2 halfSelfSize = SelfRectTransform.rect.size / 2;

                RectTransform parentTransform = SelfRectTransform.parent.GetComponent<RectTransform>();
                if (parentTransform != container)
                {
                    Vector2 relativePosition = RectTransformHelpers.GetPositionRelativeTo(new Vector2(x, y), container, parentTransform);
                    x = relativePosition.x;
                    y = relativePosition.y;
                }

                x = Mathf.Clamp(x, -(halfContainerSize.x - halfSelfSize.x), halfContainerSize.x - halfSelfSize.x);
                y = Mathf.Clamp(y, -(halfContainerSize.y - halfSelfSize.y), halfContainerSize.y - halfSelfSize.y);

                if (parentTransform != container)
                {
                    Vector2 relativePosition = RectTransformHelpers.GetPositionRelativeTo(new Vector2(x, y), parentTransform, container);
                    x = relativePosition.x;
                    y = relativePosition.y;
                }
            }
            return new Vector2(x, y);
        }
    }
}