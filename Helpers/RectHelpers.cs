using UnityEngine;

public static class RectHelpers
{
    public static Rect ScaleFromCenter(this Rect rect, Vector2 scaleBy) {
        Vector2 newSize = Vector2.Scale(rect.size, scaleBy);
        return rect.ResizeFromCenter(newSize);
    }
    public static Rect ResizeFromCenter(this Rect rect, Vector2 newSize) {
        rect.position += (rect.size - newSize) / 2f;
        rect.size = newSize;
        return rect;
    }
    public static void Draw(this Rect rect, float duration = 0.2f, float height = 0) {
        Vector3 center = new Vector3(rect.center.x, height, rect.center.y);
        Vector3 halfHeight = Vector3.forward * rect.height / 2f;
        Vector3 halfWidth = Vector3.right * rect.width / 2f;
        Debug.DrawLine(center + halfHeight - halfWidth, center + halfHeight + halfWidth, Color.red, duration); //Top line
        Debug.DrawLine(center + halfWidth - halfHeight, center + halfWidth + halfHeight, Color.green, duration); //Right line
        Debug.DrawLine(center - halfHeight - halfWidth, center - halfHeight + halfWidth, Color.red, duration); //Bottom line
        Debug.DrawLine(center - halfWidth - halfHeight, center - halfWidth + halfHeight, Color.green, duration); //Left line
    }
    public static Rect Grow(this Rect rect, Vector2 point) {
        float xMin = Mathf.Min(point.x, rect.xMin);
        float yMin = Mathf.Min(point.y, rect.yMin);
        float xMax = Mathf.Max(point.x, rect.xMax);
        float yMax = Mathf.Max(point.y, rect.yMax);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    public static Rect Grow(this Rect rect, Rect growBy) {
        return rect.Grow(growBy.min).Grow(growBy.max);
    }
}
