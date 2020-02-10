using UnityEngine;

namespace UnityHelpers
{
    public class HealthController : MonoBehaviour
    {
        [Range(0, 1)]
        public float startPercent = 1;
        public float fullValue = 100;

        [Space(10), Tooltip("The current health value, do not modify directly")]
        public float value;

        public HealthEvent onValueChanged, onHurt, onHealed, onDead;

        private void Start()
        {
            SetPercent(startPercent);
        }

        public void HealPercent(float percent)
        {
            Add(percent);
        }
        public void HealValue(float value)
        {
            value = Mathf.Clamp(Mathf.Abs(value), 0, float.MaxValue);
            Add(value / fullValue);
        }
        public void HurtPercent(float percent)
        {
            Remove(percent);
        }
        public void HurtValue(float value)
        {
            value = Mathf.Clamp(Mathf.Abs(value), 0, float.MaxValue);
            Remove(value / fullValue);
        }

        public void SetPercent(float percent)
        {
            float delta = percent - value;
            if (delta < 0)
                Remove(delta);
            else if (delta > 0)
                Add(delta);
        }
        public void SetValue(float value)
        {
            float percent = value / fullValue;
            SetPercent(percent);
        }

        private void Add(float percent)
        {
            percent = Mathf.Clamp01(Mathf.Abs(percent));
            value = Mathf.Clamp01(value + percent);

            onValueChanged?.Invoke(value);
            onHealed?.Invoke(percent);
        }
        private void Remove(float percent)
        {
            percent = Mathf.Clamp01(Mathf.Abs(percent));
            value = Mathf.Clamp01(value - percent);

            onValueChanged?.Invoke(value);
            onHurt?.Invoke(-percent);
            if (value <= 0)
                onDead?.Invoke(-percent);
        }

        [System.Serializable]
        public class HealthEvent : UnityEngine.Events.UnityEvent<float>
        { }
    }
}