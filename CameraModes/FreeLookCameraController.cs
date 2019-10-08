using UnityEngine;

namespace UnityHelpers
{
    public class FreeLookCameraController : BaseCameraController
    {
        public float moveSensitivity = 2, lookSensitivity = 1;

        protected override void ApplyInput()
        {
            transform.position += transform.forward * push * moveSensitivity + transform.right.Planar(Vector3.up) * strafe * moveSensitivity;

            Quaternion horizontalDelta = Quaternion.AngleAxis(lookHorizontal * lookSensitivity, Vector3.up);
            Quaternion verticalDelta = Quaternion.AngleAxis(lookVertical * lookSensitivity, transform.right);
            transform.rotation = horizontalDelta * (verticalDelta * transform.rotation);
        }
    }
}