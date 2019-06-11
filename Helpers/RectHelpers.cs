using UnityEngine;

public static class RectHelpers
{
    /// <summary>
    /// Scales the rect while maintaining it's center position
    /// </summary>
    /// <param name="rect">Original rect</param>
    /// <param name="scaleBy">Scale to be applied</param>
    /// <returns>Scaled rect</returns>
    public static Rect ScaleFromCenter(this Rect rect, Vector2 scaleBy)
    {
        Vector2 newSize = Vector2.Scale(rect.size, scaleBy);
        return rect.ResizeFromCenter(newSize);
    }
    /// <summary>
    /// Resizes the rect while maintaining it's center position
    /// </summary>
    /// <param name="rect">Original rect</param>
    /// <param name="newSize">New size</param>
    /// <returns>Resized rect</returns>
    public static Rect ResizeFromCenter(this Rect rect, Vector2 newSize)
    {
        rect.position += (rect.size - newSize) / 2f;
        rect.size = newSize;
        return rect;
    }
    /// <summary>
    /// Resizes the rect while maintaining it's center position
    /// </summary>
    /// <param name="rect">Original rect</param>
    /// <param name="width">New width</param>
    /// <param name="height">New height</param>
    /// <returns>Resized rect</returns>
    public static Rect ResizeFromCenter(this Rect rect, float width, float height)
    {
        return ResizeFromCenter(rect, new Vector2(width, height));
    }
    /// <summary>
    /// Draws lines in editor of rect in horizontal plane
    /// </summary>
    /// <param name="rect">Rect to be drawn</param>
    /// <param name="duration">How long to keep rect drawn in seconds</param>
    /// <param name="height">Height to draw rect at</param>
    public static void Draw(this Rect rect, float duration = 0.2f, float height = 0)
    {
        Vector3 center = new Vector3(rect.center.x, height, rect.center.y);
        Vector3 halfHeight = Vector3.forward * rect.height / 2f;
        Vector3 halfWidth = Vector3.right * rect.width / 2f;
        Debug.DrawLine(center + halfHeight - halfWidth, center + halfHeight + halfWidth, Color.red, duration); //Top line
        Debug.DrawLine(center + halfWidth - halfHeight, center + halfWidth + halfHeight, Color.green, duration); //Right line
        Debug.DrawLine(center - halfHeight - halfWidth, center - halfHeight + halfWidth, Color.red, duration); //Bottom line
        Debug.DrawLine(center - halfWidth - halfHeight, center - halfWidth + halfHeight, Color.green, duration); //Left line
    }
    /// <summary>
    /// Grows the rect to contain the point
    /// </summary>
    /// <param name="rect">Original rect</param>
    /// <param name="point">Point to be contained</param>
    /// <returns>Grown rect</returns>
    public static Rect Grow(this Rect rect, Vector2 point)
    {
        rect.xMin = Mathf.Min(point.x, rect.xMin);
        rect.yMin = Mathf.Min(point.y, rect.yMin);
        rect.xMax = Mathf.Max(point.x, rect.xMax);
        rect.yMax = Mathf.Max(point.y, rect.yMax);

        return rect;
    }
    /// <summary>
    /// Grows the rect to fit the other rect
    /// </summary>
    /// <param name="rect">Original rect</param>
    /// <param name="growBy">Other rect</param>
    /// <returns>Summed rect</returns>
    public static Rect Grow(this Rect rect, Rect growBy)
    {
        return rect.Grow(growBy.min).Grow(growBy.max);
    }
}
