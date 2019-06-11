using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorHelpers
{
    public static Vector2 xy(this Vector3 vector) {
        return new Vector2(vector.x, vector.y);
    }
    public static Vector2 xz(this Vector3 vector) {
        return new Vector2(vector.x, vector.z);
    }
    public static Vector2 yz(this Vector3 vector) {
        return new Vector2(vector.y, vector.z);
    }

}
