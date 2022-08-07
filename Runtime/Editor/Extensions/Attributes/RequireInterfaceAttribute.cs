#if UNITY_EDITOR
using UnityEngine;

namespace UnityHelpers.Editor
{
    /// <summary>
    /// Attribute that require implementation of the provided interface.
    /// Source: https://www.patrykgalach.com/2020/01/27/assigning-interface-in-unity-inspector/?cn-reloaded=1
    /// </summary>
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        /// <summary>
        /// Interface type
        /// </summary>
        public System.Type requiredType { get; private set; }

        /// <summary>
        /// Requiring implementation of the given interface.
        /// </summary>
        /// <param name="type">Interface type.</param>
        public RequireInterfaceAttribute(System.Type type)
        {
            this.requiredType = type;
        }
    }
}
#endif