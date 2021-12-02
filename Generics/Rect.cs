using UnityEngine;

namespace UnityHelpers
{
    public struct Rect<T>
    {
        private Number<T> xMin;
        private Number<T> xMax;
        private Number<T> yMin;
        private Number<T> yMax;

        private Number<T> width { get { return xMax - xMin; } }
        private Number<T> height { get { return yMax - yMin; } }

        public Rect(T xMin, T xMax, T yMin, T yMax)
        {
            this.xMin = new Number<T>(xMin);
            this.xMax = new Number<T>(xMax);
            this.yMin = new Number<T>(yMin);
            this.yMax = new Number<T>(yMax);
        }

        public override string ToString()
        {
            return "(x:" + xMin + ", y:" + yMin + ", width:" + width + ", height:" + height + ")";
        }

        public static implicit operator Rect<T>(Rect rect) => new Rect<T>((T)(dynamic)rect.xMin, (T)(dynamic)rect.xMax, (T)(dynamic)rect.yMin, (T)(dynamic)rect.yMax);
        public static implicit operator Rect<T>(RectInt rectInt) => new Rect<T>((T)(dynamic)rectInt.xMin, (T)(dynamic)rectInt.xMax, (T)(dynamic)rectInt.yMin, (T)(dynamic)rectInt.yMax);

        public static implicit operator Rect(Rect<T> rect) => new Rect((float)(dynamic)rect.xMin, (float)(dynamic)rect.yMin, (float)(dynamic)rect.width, (float)(dynamic)rect.height);
        public static implicit operator RectInt(Rect<T> rect) => new RectInt((int)(dynamic)rect.xMin, (int)(dynamic)rect.yMin, (int)(dynamic)(rect.width), (int)(dynamic)(rect.height));
    }

    public struct Number<T>
    {
        private T stored;

        public Number(T value)
        {
            // if (!(value is sbyte) &&
            //     !(value is byte) &&
            //     !(value is short) &&
            //     !(value is ushort) &&
            //     !(value is int) &&
            //     !(value is uint) &&
            //     !(value is long) &&
            //     !(value is ulong) &&
            //     !(value is float) &&
            //     !(value is double) &&
            //     !(value is decimal))
            // {
            //     throw new System.NotImplementedException(value.GetType() + " is not supported in the Number struct.");
            // }

            stored = value;
        }
        public override string ToString()
        {
            return stored.ToString();
        }

        private Number<T> DynamicAdd(Number<T> other)
        {
            return new Number<T>((dynamic)stored + other.stored);
        }
        private Number<T> DynamicSubtract(Number<T> other)
        {
            return new Number<T>((dynamic)stored - other.stored);
        }

        public static implicit operator T(Number<T> wrapped) => wrapped.stored;
        public static explicit operator Number<T>(T value) => new Number<T>(value);
        public static Number<T> operator +(Number<T> first, Number<T> second) => first.DynamicAdd(second);
        public static Number<T> operator -(Number<T> first, Number<T> second) => first.DynamicSubtract(second);
    }
}