using UnityEngine;

namespace UnityHelpers
{
    public class FreeLookController : MonoBehaviour
    {
        public float strafe { get; private set; }
        public float push { get; private set; }
        public float lookVertical { get; private set; }
        public float lookHorizontal { get; private set; }

        public float moveSensitivity = 2, lookSensitivity = 1;

        private void Update()
        {
            transform.position += transform.forward * push * moveSensitivity + transform.right.Planar(Vector3.up) * strafe * moveSensitivity;
            transform.forward = Quaternion.AngleAxis(lookVertical * lookSensitivity, transform.right) * (Quaternion.AngleAxis(lookHorizontal * lookSensitivity, Vector3.up) * transform.forward);
        }

        public void SetStrafe(float amount)
        {
            strafe = Mathf.Clamp(amount, -1, 1);
        }
        public void SetPush(float amount)
        {
            push = Mathf.Clamp(amount, -1, 1);
        }
        public void SetLookVertical(float amount)
        {
            lookVertical = Mathf.Clamp(amount, -1, 1);
        }
        public void SetLookHorizontal(float amount)
        {
            lookHorizontal = Mathf.Clamp(amount, -1, 1);
        }
    }
}