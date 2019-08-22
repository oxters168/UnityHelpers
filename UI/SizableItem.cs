using UnityEngine;
using UnityEngine.EventSystems;

public class SizableItem : MonoBehaviour, IScrollHandler
{
    private RectTransform _selfRectTransform;
    public RectTransform SelfRectTransform { get { if (!_selfRectTransform) _selfRectTransform = GetComponent<RectTransform>(); return _selfRectTransform; } }

    public float scrollMultiplier = 32;
    public float touchMultiplier = 1;

    private Vector2 orig1, orig2;
    private Vector2 current1, current2;
    public bool isPinching { get; private set; }
    private bool isFirstTouchInside;
    private bool isTouching;

    private void Update()
    {
        if (Input.touches.Length > 0 && !isPinching && !isTouching)
        {
            Vector2 touchPos = Input.touches[0].position;
            Vector2 localTouch;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(SelfRectTransform, touchPos, null, out localTouch);
            isFirstTouchInside = SelfRectTransform.rect.Contains(localTouch);
            isTouching = true;
        }
        else if (Input.touches.Length > 1 && !isPinching && isFirstTouchInside)
        {
            Vector2 position1 = Input.touches[0].position;
            Vector2 position2 = Input.touches[1].position;

            Pinched(position1, position2);
        }
        else if (Input.touches.Length > 1 && isPinching)
        {
            Vector2 position1 = Input.touches[0].position;
            Vector2 position2 = Input.touches[1].position;
            Pinching(position1, position2);
        }
        else if (Input.touches.Length < 2)
        {
            PinchEnded();
            if (Input.touches.Length <= 0)
            {
                isFirstTouchInside = false;
                isTouching = false;
            }
        }
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
