using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// An interface to help generalize input between different types
    /// </summary>
    public interface IValueManager
    {
        /// <summary>
        /// Sets an axis' value
        /// </summary>
        /// <param name="name">The name of the axis</param>
        /// <param name="value">The value the axis should be set to</param>
        void SetAxis(string name, float value);
        /// <summary>
        /// Gets the value of the requested axis
        /// </summary>
        /// <param name="name">The name of the axis</param>
        /// <returns>The value of the axis</returns>
        float GetAxis(string name);
        
        /// <summary>
        /// Sets a toggle's value
        /// </summary>
        /// <param name="name">The name of the toggle</param>
        /// <param name="value">The value the toggle should be set to</param>
        void SetToggle(string name, bool value);
        /// <summary>
        /// Gets the value of the requested toggle
        /// </summary>
        /// <param name="name">The name of the toggle</param>
        /// <returns>The value of the toggle</returns>
        bool GetToggle(string name);

        /// <summary>
        /// Sets a direction's value
        /// </summary>
        /// <param name="name">The name of the direction</param>
        /// <param name="value">The value the direction should be set to</param>
        void SetDirection(string name, Vector3 value);
        /// <summary>
        /// Gets the value of the requested direction
        /// </summary>
        /// <param name="name">The name of the direction</param>
        /// <returns>The value of the direction</returns>
        Vector3 GetDirection(string name);

        /// <summary>
        /// Sets a point's value
        /// </summary>
        /// <param name="name">The name of the point</param>
        /// <param name="value">The value the point should be set to</param>
        void SetPoint(string name, Vector3 value);
        /// <summary>
        /// Gets the value of the requested point
        /// </summary>
        /// <param name="name">The name of the point</param>
        /// <returns>The value of the point</returns>
        Vector3 GetPoint(string name);

        /// <summary>
        /// Sets an orientation's value
        /// </summary>
        /// <param name="name">The name of the orientation</param>
        /// <param name="value">The value the orientation should be set to</param>
        void SetOrientation(string name, Quaternion value);
        /// <summary>
        /// Gets the value of the requested orientation
        /// </summary>
        /// <param name="name">The name of the orientation</param>
        /// <returns>The value of the orientation</returns>
        Quaternion GetOrientation(string name);
    }
}