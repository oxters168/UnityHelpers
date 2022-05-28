using UnityEngine;

namespace UnityHelpers
{
    public static class RectTransformHelpers
    {
        public static Vector2 CalculatePivotPercentFromCenter(this RectTransform rectTransform)
        {
            return rectTransform.pivot - Vector2.one * 0.5f;
        }
        public static Vector2 RemovePivotOffset(this Vector2 localPosition, RectTransform relativeTo)
        {
            return localPosition + relativeTo.pivot * relativeTo.rect.size;
        }
        public static void ShiftPivot(this RectTransform rectTransform, Vector2 newPivot)
        {
            Vector2 positionOffset = (newPivot - rectTransform.pivot) * rectTransform.rect.size;
            rectTransform.localPosition += (Vector3)positionOffset;
            rectTransform.pivot = newPivot;
        }
        public static Vector2 GetRealLocalPosition(this RectTransform rectTransform)
        {
            Vector2 centeredPivot = rectTransform.CalculatePivotPercentFromCenter();
            Vector2 selfOffset = new Vector2(centeredPivot.x * rectTransform.rect.width, centeredPivot.y * rectTransform.rect.height);

            RectTransform parentTransform = rectTransform.parent.GetComponent<RectTransform>();
            Vector2 parentOffset = Vector2.zero;
            if (parentTransform)
            {
                centeredPivot = parentTransform.CalculatePivotPercentFromCenter();
                parentOffset = new Vector2(centeredPivot.x * parentTransform.rect.width, centeredPivot.y * parentTransform.rect.height);
            }

            return (Vector2)rectTransform.localPosition - selfOffset + parentOffset;
        }
        public static Vector2 GetRealLocalMin(this RectTransform rectTransform)
        {
            return rectTransform.GetRealLocalPosition() - rectTransform.rect.size / 2;
        }
        public static Vector2 GetRealLocalMax(this RectTransform rectTransform)
        {
            return rectTransform.GetRealLocalPosition() + rectTransform.rect.size / 2;
        }
        public static Vector2 GetPositionRelativeTo(this Vector2 position, RectTransform to, RectTransform from = null)
        {
            //Need to check this, I don't think it works properly
            Vector3 adjustedPosition = position;
            if (from)
                adjustedPosition = from.TransformPoint(adjustedPosition);

            if (to)
                adjustedPosition = to.InverseTransformPoint(adjustedPosition);

            return adjustedPosition;
        }
    }
}