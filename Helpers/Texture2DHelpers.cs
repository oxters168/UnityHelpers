using UnityEngine;

public static class Texture2DHelpers
{
    /// <summary>
    /// Turns a Texture2D object to a Sprite object.
    /// </summary>
    /// <param name="texture">The original image</param>
    /// <returns>The texture as a sprite</returns>
    public static Sprite ToSprite(this Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
