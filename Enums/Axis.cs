using System;

namespace UnityHelpers
{
    [Flags]
    public enum Axis
    {
        none = 0, x = 0x1, y = 0x2, z = 0x4, w = 0x10
    }
}