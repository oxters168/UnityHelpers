using UnityEngine;

namespace UnityHelpers
{
    public interface ICastable
    {
        bool Cast(out RaycastHit hitInfo);
        RaycastHit[] CastAll();
        Vector3 GetPosition();
        Vector3 GetDirection();
        float GetSize();
        Transform GetParent();
    }
}