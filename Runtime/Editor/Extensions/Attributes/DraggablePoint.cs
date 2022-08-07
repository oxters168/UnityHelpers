#if UNITY_EDITOR
using UnityEngine;

namespace UnityHelpers.Editor
{
    /// <summary>
    /// Use this attribute to make the point viewable and movable in the editor
    /// </summary>
    public class DraggablePoint : PropertyAttribute
    {
        public bool local;
        public DraggablePoint(bool isLocal)
        {
            local = isLocal;
        }
    }
}
#endif