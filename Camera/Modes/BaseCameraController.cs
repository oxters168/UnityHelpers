using UnityEngine;

namespace UnityHelpers
{
    public abstract class BaseCameraController : MonoBehaviour
    {
        public float strafe { get; protected set; }
        public float push { get; protected set; }
        public float lookVertical { get; protected set; }
        public float lookHorizontal { get; protected set; }

        [Range(0, 1)]
        public float strafeDeadzone = 0;
        [Range(0, 1)]
        public float pushDeadzone = 0;
        [Range(0, 1)]
        public float lookVerticalDeadzone = 0;
        [Range(0, 1)]
        public float lookHorizontalDeadzone = 0;

        private void Update()
        {
            ApplyInput();
        }

        protected abstract void ApplyInput();

        public void SetStrafe(float amount)
        {
            strafe = ApplyDeadzone(Mathf.Clamp(amount, -1, 1), strafeDeadzone);
        }
        public void SetPush(float amount)
        {
            push = ApplyDeadzone(Mathf.Clamp(amount, -1, 1), pushDeadzone);
        }
        public void SetLookVertical(float amount)
        {
            lookVertical = ApplyDeadzone(Mathf.Clamp(amount, -1, 1), lookVerticalDeadzone);
        }
        public void SetLookHorizontal(float amount)
        {
            lookHorizontal = ApplyDeadzone(Mathf.Clamp(amount, -1, 1), lookHorizontalDeadzone);
        }

        private static float ApplyDeadzone(float original, float deadzone)
        {
            return Mathf.Sign(original) * Mathf.Clamp01(Mathf.Abs(original) - deadzone) / (1 - deadzone);
        }
    }
}