using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IPointerClickHandler, IDragHandler, IPointerDownHandler, IEndDragHandler
{
    private RectTransform _selfRectTransform;
    public RectTransform SelfRectTransform { get { if (!_selfRectTransform) _selfRectTransform = GetComponent<RectTransform>(); return _selfRectTransform; } }
    private RectTransform _canvasRectTransform;
    public RectTransform CanvasRectTransform { get { if (!_canvasRectTransform) _canvasRectTransform = SelfRectTransform.root.GetComponentInChildren<RectTransform>(); return _canvasRectTransform; } }

    public RectTransform container;

    public bool isDragging { get; private set; }
    public event PointerHandler onDrag, onEndDrag, onClick, onDown;
    public delegate void PointerHandler();

    private Vector2 origPosition;
    private Vector2 downPointerPosition;

    private bool isTouching;

    private void Update()
    {
        SelfRectTransform.localPosition = ClampToContainer(SelfRectTransform.localPosition.x, SelfRectTransform.localPosition.y);
        if (Input.touches.Length > 0)
        {
            Vector2 touchPos = Input.touches[0].position;
            Vector2 localTouch;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRectTransform, touchPos, null, out localTouch);
            if (!isTouching)
            {
                isTouching = true;
                Down(localTouch);
            }
            else
            {
                Drag(localTouch);
            }
        }
        else if (Input.touches.Length <= 0)
        {
            isTouching = false;
            EndDrag();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.touches.Length <= 0)
            Down(eventData.position);
    }
    public void OnDrag(PointerEventData eventData)
    {
        //SelfRectTransform.localPosition = ClampToContainer(nextButtonX, nextButtonY);
        if (Input.touches.Length <= 0)
            Drag(eventData.position);
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

    private void Down(Vector2 position)
    {
        origPosition = SelfRectTransform.localPosition;
        downPointerPosition = position;

        onDown?.Invoke();
    }
    private void Drag(Vector2 position)
    {
        float nextPosX = origPosition.x + (position.x - downPointerPosition.x);
        float nextPosY = origPosition.y + (position.y - downPointerPosition.y);
        SelfRectTransform.localPosition = ClampToContainer(nextPosX, nextPosY);

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
