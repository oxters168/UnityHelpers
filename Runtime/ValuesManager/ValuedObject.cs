using UnityEngine;

namespace UnityHelpers
{
    public abstract class ValuedObject : MonoBehaviour, IValueManager
    {
        public ValuesVault values;

        public void SetAxis(string name, float value)
        {
            values.GetValue(name).SetAxis(value);
        }
        public float GetAxis(string name)
        {
            return values.GetValue(name).GetAxis();
        }
        public void SetToggle(string name, bool value)
        {
            values.GetValue(name).SetToggle(value);
        }
        public bool GetToggle(string name)
        {
            return values.GetValue(name).GetToggle();
        }
        public void SetDirection(string name, Vector3 value)
        {
            values.GetValue(name).SetDirection(value);
        }
        public Vector3 GetDirection(string name)
        {
            return values.GetValue(name).GetDirection();
        }
        public void SetPoint(string name, Vector3 value)
        {
            values.GetValue(name).SetPoint(value);
        }
        public Vector3 GetPoint(string name)
        {
            return values.GetValue(name).GetPoint();
        }
        public void SetOrientation(string name, Quaternion value)
        {
            values.GetValue(name).SetOrientation(value);
        }
        public Quaternion GetOrientation(string name)
        {
            return values.GetValue(name).GetOrientation();
        }
    }
}