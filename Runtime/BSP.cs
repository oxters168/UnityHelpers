using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    /// <summary>
    /// Written by @bmabsout
    /// </summary>
    public class BSP
    {
        Rect rect;
        (BSP, BSP)? subrects;

        public BSP(Rect rect)
        {
            this.rect = rect;
            subrects = null;
        }

        public BSP(Rect rect, (BSP, BSP) subrects)
        {
            this.rect = rect;
            this.subrects = subrects;
        }

        public static (Rect, Rect) Split(Rect rect)
        {
            if(rect.height >= rect.width)
            {
                Rect bottom = Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMax, rect.yMin + rect.height/2f);
                Rect top = Rect.MinMaxRect(rect.xMin, rect.yMin+rect.height/2f, rect.xMax, rect.yMax);
                return (top, bottom);
            }
            else
            {
                Rect left = Rect.MinMaxRect(rect.xMin, rect.yMin, rect.xMin + rect.width/2f, rect.yMax);
                Rect right = Rect.MinMaxRect(rect.xMin + rect.width/2f, rect.yMin, rect.xMax, rect.yMax);
                return (left, right);
            }
        }

        public uint Size()
        {
            return 1 + (subrects == null ? 0 : subrects.Value.Item1.Size() + subrects.Value.Item2.Size());
        }

        public IEnumerable<Rect> EnumerateRects()
        {
            if (subrects == null)
                yield return rect;
            else
            {
                var (subrect1, subrect2) = subrects.Value;
                foreach (var drat in subrect1.EnumerateRects())
                    yield return drat;
                foreach (var drat in subrect2.EnumerateRects())
                    yield return drat;
            }
        }

        public static BSP Partition(Rect partitionMe, uint num_splits)
        {
            if(num_splits <= 1)
            {
                return new BSP(partitionMe);
            }
            else
            {
                var (splitten1, splitten2) = BSP.Split(partitionMe);
                
                uint num_splits1 = num_splits/2;
                BSP subrect1 = BSP.Partition(splitten1, num_splits1);

                uint num_splits2 = num_splits/2 + num_splits % 2;
                BSP subrect2 = BSP.Partition(splitten2, num_splits2);
                
                return new BSP(partitionMe, (subrect1, subrect2));
            }
        }
    }
}