using UnityEngine;

public class OrbitCameraController : BaseCameraController
{
    public Transform target;
    public Vector3 offset;
    public float distance = 10;

    public float moveSensitivity = 2, lookSensitivity = 1;
    [Range(0.01f, 1)]
    public float shiftMinimum = 0.5f;

    public event ShiftHandler shiftRight, shiftLeft;
    public delegate void ShiftHandler();

    protected override void ApplyInput()
    {
        Debug.Assert(target != null, "Orbit Camera: Target not set!");
        if (target != null)
            transform.position = target.position + offset;
        else
            transform.position = Vector3.zero;

        Quaternion horizontalDelta = Quaternion.AngleAxis(lookHorizontal * lookSensitivity, Vector3.up);
        Quaternion verticalDelta = Quaternion.AngleAxis(lookVertical * lookSensitivity, transform.right);
        transform.rotation = horizontalDelta * (verticalDelta * transform.rotation);

        distance -= push * moveSensitivity;
        distance = Mathf.Clamp(distance, 0, float.MaxValue);
        transform.position -= transform.forward * distance;

        if (strafe >= shiftMinimum)
            shiftRight?.Invoke();
        else if (strafe <= -shiftMinimum)
            shiftLeft?.Invoke();
    }
}
