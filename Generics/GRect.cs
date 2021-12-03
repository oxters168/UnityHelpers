using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// A rect type similar to Unity's, but capable of using any type and with additional functionality.
    /// There are extension methods for Rect and RectInt to convert them to GRect.
    /// </summary>
    public struct GRect<T>
    {
        private Number<T> _xMin;
        /// <summary>
        /// The minimum value along the "x-axis"
        /// </summary>
        public T xMin { get { return _xMin; } set { _xMin = value; } }
        private Number<T> _xMax;
        /// <summary>
        /// The maximum value along the "x-axis"
        /// </summary>
        public T xMax { get { return _xMax; } set { _xMax = value; } }
        private Number<T> _yMin;
        /// <summary>
        /// The minimum value along the "y-axis"
        /// </summary>
        public T yMin { get { return _yMin; } set { _yMin = value; } }
        private Number<T> _yMax;
        /// <summary>
        /// The maximum value along the "y-axis"
        /// </summary>
        public T yMax { get { return _yMax; } set { _yMax = value; } }

        private Number<T> _width { get { return _xMax - _xMin; } set { _xMax = _xMin + value; } }
        /// <summary>
        /// The width of the rect, calculated by subtracting xMax from xMin
        /// </summary>
        public T width { get { return _width; } set { _width = value; } }
        private Number<T> _height { get { return _yMax - _yMin; } set { _yMax = _yMin + value; } }
        /// <summary>
        /// The height of the rect, calculated by subtracting yMax from yMin
        /// </summary>
        public T height { get { return _height; } set { _height = value; } }

        /// <summary>
        /// Creates a GRect using minimum and maximum values
        /// </summary>
        /// <param name="xMin">The left most value</param>
        /// <param name="yMin">The bottom most value</param>
        /// <param name="xMax">The right most value</param>
        /// <param name="yMax">The top most value</param>
        /// <returns>A GRect with the given values</returns>
        public static GRect<T> MinMaxRect(T xMin, T yMin, T xMax, T yMax)
        {
            GRect<T> minMaxRect = default;
            minMaxRect._xMin = xMin;
            minMaxRect._yMin = yMin;
            minMaxRect._xMax = xMax;
            minMaxRect._yMax = yMax;
            return minMaxRect;
        }
        /// <summary>
        /// Creates a GRect using x, y, width, and height
        /// </summary>
        /// <param name="xMin">The left most value</param>
        /// <param name="yMin">The bottom most value</param>
        /// <param name="width">The width of the rect</param>
        /// <param name="height">The height of the rect</param>
        /// <returns>A GRect with the given values</returns>
        public static GRect<T> WidthHeightRect(T xMin, T yMin, T width, T height)
        {
            GRect<T> widthHeightRect = default;
            widthHeightRect._xMin = xMin;
            widthHeightRect._yMin = yMin;
            widthHeightRect._width = width;
            widthHeightRect._height = height;
            return widthHeightRect;
        }

        /// <summary>
        /// Casts a GRect from one type to another
        /// </summary>
        /// <param name="converter">A specific way to convert, if left empty will try to do an explicit cast</param>
        /// <typeparam name="A">The type to convert to</typeparam>
        /// <returns>A converted GRect</returns>
        public GRect<A> CastTo<A>(System.Func<T, A> converter = null)
        {
            GRect<A> converted = default;
            if (converter == null)
                converter = (value) => (A)(dynamic)value;

            converted._xMin = converter(xMin);
            converted._yMin = converter(yMin);
            converted._xMax = converter(xMax);
            converted._yMax = converter(yMax);
            return converted;
        }

        public override string ToString()
        {
            return "(x:" + _xMin + ", y:" + _yMin + ", width:" + _width + ", height:" + _height + ")";
        }

        /// <summary>
        /// Adds the corners of the GRects and outputs a GRect with those summed corners
        /// </summary>
        /// <param name="first">The first GRect</param>
        /// <param name="second">The second GRect</param>
        /// <returns>The sum GRect</returns>
        public static GRect<T> operator +(GRect<T> first, GRect<T> second) => GRect<T>.MinMaxRect(first._xMin + second._xMin, first._yMin + second._yMin, first._xMax + second._xMax, first._yMax + second._yMax);
        /// <summary>
        /// Subtracts the corners of the GRects and outputs a GRect that has the differences as its corners
        /// </summary>
        /// <param name="first">The first GRect</param>
        /// <param name="second">The second GRect</param>
        /// <returns>The difference GRect</returns>
        public static GRect<T> operator -(GRect<T> first, GRect<T> second) => GRect<T>.MinMaxRect(first._xMin - second._xMin, first._yMin - second._yMin, first._xMax - second._xMax, first._yMax - second._yMax);
        /// <summary>
        /// Multiplies a value to each corner of the GRect and ouputs a GRect with the products as its corners
        /// </summary>
        /// <param name="first">The GRect</param>
        /// <param name="second">The value</param>
        /// <returns>The product GRect</returns>
        public static GRect<T> operator *(GRect<T> first, T second) => GRect<T>.MinMaxRect(first._xMin * second, first._yMin * second, first._xMax * second, first._yMax * second);
        /// <summary>
        /// Multiplies a value to each corner of the GRect and ouputs a GRect with the products as its corners
        /// </summary>
        /// <param name="first">The value</param>
        /// <param name="second">The GRect</param>
        /// <returns>The product GRect</returns>
        public static GRect<T> operator *(T first, GRect<T> second) => GRect<T>.MinMaxRect(first * second._xMin, first * second._yMin, first * second._xMax, first * second._yMax);
        /// <summary>
        /// Divides a value to each corner of the GRect and ouputs a GRect with the quotient as its corners
        /// </summary>
        /// <param name="first">The value</param>
        /// <param name="second">The GRect</param>
        /// <returns>The quotient GRect</returns>
        public static GRect<T> operator /(GRect<T> first, T second) => GRect<T>.MinMaxRect(first._xMin / second, first._yMin / second, first._xMax / second, first._yMax / second);
        /// <summary>
        /// Divides a value to each corner of the GRect and ouputs a GRect with the quotient as its corners
        /// </summary>
        /// <param name="first">The GRect</param>
        /// <param name="second">The value</param>
        /// <returns>The quotient GRect</returns>
        public static GRect<T> operator /(T first, GRect<T> second) => GRect<T>.MinMaxRect(first / second._xMin, first / second._yMin, first / second._xMax, first / second._yMax);


        //These implicit operators allow the switching between Rect and Rect<float> and RectInt and Rect<int> without any extra code
        //They would be enabled if only C# let you specify Rect<float> and Rect<int> as the outcomes to Rect and RectInt respectively
        //These work just fine, but can be misleading when passing a Rect as a Rect<Vector2> for example
        //C# doesn't give a compile time error, only an error at runtime, so they are disabled and replaced with extension functions

        // public static implicit operator GRect<T>(Rect rect) => GRect<T>.MinMaxRect((T)(dynamic)rect.xMin, (T)(dynamic)rect.xMax, (T)(dynamic)rect.yMin, (T)(dynamic)rect.yMax);
        // public static implicit operator GRect<T>(RectInt rectInt) => GRect<T>.MinMaxRect((T)(dynamic)rectInt.xMin, (T)(dynamic)rectInt.xMax, (T)(dynamic)rectInt.yMin, (T)(dynamic)rectInt.yMax);
        // public static implicit operator Rect(GRect<T> rect) => new Rect((float)(dynamic)rect._xMin, (float)(dynamic)rect._yMin, (float)(dynamic)rect._width, (float)(dynamic)rect._height);
        // public static implicit operator RectInt(GRect<T> rect) => new RectInt((int)(dynamic)rect._xMin, (int)(dynamic)rect._yMin, (int)(dynamic)(rect._width), (int)(dynamic)(rect._height));
    }

    /// <summary>
    /// A wrapper struct for any type to ease mathematics with generics
    /// </summary>
    internal struct Number<T>
    {
        private T stored;

        public Number(T value)
        {
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
        private Number<T> DynamicMultiply(Number<T> other)
        {
            return new Number<T>((dynamic)stored * other.stored);
        }
        private Number<T> DynamicDivide(Number<T> other)
        {
            return new Number<T>((dynamic)stored / other.stored);
        }

        public static implicit operator T(Number<T> wrapped) => wrapped.stored;
        public static implicit operator Number<T>(T value) => new Number<T>(value);
        public static Number<T> operator +(Number<T> first, Number<T> second) => first.DynamicAdd(second);
        public static Number<T> operator -(Number<T> first, Number<T> second) => first.DynamicSubtract(second);
        public static Number<T> operator *(Number<T> first, Number<T> second) => first.DynamicMultiply(second);
        public static Number<T> operator /(Number<T> first, Number<T> second) => first.DynamicDivide(second);
    }
}