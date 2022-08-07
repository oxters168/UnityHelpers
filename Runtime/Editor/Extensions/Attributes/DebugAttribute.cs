#if UNITY_EDITOR
using UnityEngine;

namespace UnityHelpers.Editor
{
    /// <summary>
    /// Gives you some space in the inspector to be able to output text to.
    /// Works best at runtime since serialized values cannot be changed easily.
    /// This means that values you change in the script and then save will not
    /// immediately be picked up.
    /// </summary>
    public class DebugAttribute : PropertyAttribute
    {
        public bool collapsable;
        public bool highlightable;
        public UnityEditor.MessageType msgType = UnityEditor.MessageType.None;

        public DebugAttribute() : this(false, false) {}
        public DebugAttribute(UnityEditor.MessageType _msgType) : this(_msgType, false) {}
        public DebugAttribute(bool _collapsable) : this(_collapsable, false) {}
        public DebugAttribute(UnityEditor.MessageType _msgType, bool _collapsable) : this(_collapsable, false)
        {
            msgType = _msgType;
        }
        public DebugAttribute(bool _collapsable, bool _highlightable)
        {
            collapsable = _collapsable;
            highlightable = _highlightable;
        }
    }
}
#endif