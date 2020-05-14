using UnityEngine;

namespace UnityHelpers
{
    [System.Serializable]
    public class ValueWrapper
    {
        [System.Flags]
        public enum CommonType { axis = 0x1, toggle = 0x2, direction = 0x4, point = 0x8, orientation = 0x10, }

        public string name;
        public CommonType storedTypes;

        [SerializeField]
        private float axis;
        [SerializeField]
        private bool toggle;
        [SerializeField]
        private Vector3 direction;
        [SerializeField]
        private Vector3 point;
        [SerializeField]
        private Quaternion orientation;

        public bool Contains(CommonType queryType)
        {
            return (storedTypes & queryType) != 0;
        }

        public void SetValue(CommonType type, object value)
        {
            if ((type & CommonType.axis) != 0)
                SetAxis((float)value);
            else if ((type & CommonType.toggle) != 0)
                SetToggle((bool)value);
            else if ((type & CommonType.direction) != 0)
                SetDirection((Vector3)value);
            else if ((type & CommonType.point) != 0)
                SetPoint((Vector3)value);
            else if ((type & CommonType.orientation) != 0)
                SetOrientation((Quaternion)value);
        }
        public object GetValue(CommonType type)
        {
            object value;

            if ((type & CommonType.axis) != 0)
                value = GetAxis();
            else if ((type & CommonType.toggle) != 0)
                value = GetToggle();
            else if ((type & CommonType.direction) != 0)
                value = GetDirection();
            else if ((type & CommonType.point) != 0)
                value = GetPoint();
            else if ((type & CommonType.orientation) != 0)
                value = GetOrientation();
            else
                throw new System.ArgumentException("Bad type");
            
            return value;
        }
        public void Remove(CommonType type)
        {
            if ((type & CommonType.axis) != 0)
                RemoveAxis();
            else if ((type & CommonType.toggle) != 0)
                RemoveToggle();
            else if ((type & CommonType.direction) != 0)
                RemoveDirection();
            else if ((type & CommonType.point) != 0)
                RemovePoint();
            else if ((type & CommonType.orientation) != 0)
                RemoveOrientation();
        }

        public void SetAxis(float value)
        {
            storedTypes |= CommonType.axis;
            axis = value;
        }
        public float GetAxis()
        {
            if (!Contains(CommonType.axis))
                throw new System.InvalidOperationException("Value type not set");

            return axis;
        }
        public void RemoveAxis()
        {
            storedTypes &= ~CommonType.axis;
            axis = default;
        }

        public void SetToggle(bool value)
        {
            storedTypes |= CommonType.toggle;
            toggle = value;
        }
        public bool GetToggle()
        {
            if (!Contains(CommonType.toggle))
                throw new System.InvalidOperationException("Value type not set");

            return toggle;
        }
        public void RemoveToggle()
        {
            storedTypes &= ~CommonType.toggle;
            toggle = default;
        }

        public void SetDirection(Vector3 value)
        {
            storedTypes |= CommonType.direction;
            direction = value;
        }
        public Vector3 GetDirection()
        {
            if (!Contains(CommonType.direction))
                throw new System.InvalidOperationException("Value type not set");

            return direction;
        }
        public void RemoveDirection()
        {
            storedTypes &= ~CommonType.direction;
            direction = default;
        }

        public void SetPoint(Vector3 value)
        {
            storedTypes |= CommonType.point;
            point = value;
        }
        public Vector3 GetPoint()
        {
            if (!Contains(CommonType.point))
                throw new System.InvalidOperationException("Value type not set");

            return point;
        }
        public void RemovePoint()
        {
            storedTypes &= ~CommonType.point;
            point = default;
        }

        public void SetOrientation(Quaternion value)
        {
            storedTypes |= CommonType.orientation;
            orientation = value;
        }
        public Quaternion GetOrientation()
        {
            if (!Contains(CommonType.orientation))
                throw new System.InvalidOperationException("Value type not set");

            return orientation;
        }
        public void RemoveOrientation()
        {
            storedTypes &= ~CommonType.orientation;
            orientation = default;
        }
    }
}