﻿using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// Use this attribute to make the point viewable and movable in the editor
    /// </summary>
    public class DraggablePoint : PropertyAttribute
    {
        internal bool local;
        public DraggablePoint(bool isLocal)
        {
            local = isLocal;
        }
    }
}