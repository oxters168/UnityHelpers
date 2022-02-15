namespace UnityHelpers
{
    /// <summary>
    /// A rect type similar to Unity's, but capable of using any type and with additional functionality.
    /// There are extension methods for Rect and RectInt to convert them to GRect.
    /// </summary>
    public struct GRect<T>
    {
        internal Number<T> _x { get { return _xMin; } set { var tmp = _width; _xMin = value; _width = tmp; } }
        /// <summary>
        /// The position along the "x-axis"
        /// </summary>
        public T x { get { return _x; } set { _x = value; } }
        internal Number<T> _y { get { return _yMin; } set { var tmp = _height; _yMin = value; _height = tmp; } }
        /// <summary>
        /// The position along the "y-axis"
        /// </summary>
        /// <value></value>
        public T y { get { return _y; } set { _y = value; } }

        internal Number<T> _xMin;
        /// <summary>
        /// The minimum value along the "x-axis"
        /// </summary>
        public T xMin { get { return _xMin; } set { _xMin = value; } }
        internal Number<T> _xMax;
        /// <summary>
        /// The maximum value along the "x-axis"
        /// </summary>
        public T xMax { get { return _xMax; } set { _xMax = value; } }
        internal Number<T> _yMin;
        /// <summary>
        /// The minimum value along the "y-axis"
        /// </summary>
        public T yMin { get { return _yMin; } set { _yMin = value; } }
        internal Number<T> _yMax;
        /// <summary>
        /// The maximum value along the "y-axis"
        /// </summary>
        public T yMax { get { return _yMax; } set { _yMax = value; } }

        internal Number<T> _width { get { return _xMax - _xMin; } set { _xMax = _xMin + value; } }
        /// <summary>
        /// The width of the rect, calculated by subtracting xMax from xMin
        /// </summary>
        public T width { get { return _width; } set { _width = value; } }
        internal Number<T> _height { get { return _yMax - _yMin; } set { _yMax = _yMin + value; } }
        /// <summary>
        /// The height of the rect, calculated by subtracting yMax from yMin
        /// </summary>
        public T height { get { return _height; } set { _height = value; } }

        /// <summary>
        /// Casts a GRect from one type to another
        /// </summary>
        /// <typeparam name="A">The type to convert to</typeparam>
        /// <returns>A converted GRect</returns>
        public GRect<A> Cast<A>()
        {
            return Map((value) => (A)(dynamic)value);
        }

        /// <summary>
        /// Maps the GRect's values from one type to another
        /// </summary>
        /// <param name="mapper">The method of mapping the values</param>
        /// <typeparam name="A">The output GRect type</typeparam>
        /// <returns>A GRect whose values have been mapped to a new type</returns>
        public GRect<A> Map<A>(System.Func<T, A> mapper)
        {
            return GRect.MinMaxRect(mapper(xMin), mapper(yMin), mapper(xMax), mapper(yMax));
        }

        private GRect<T> Map(System.Func<Number<T>, Number<T>> mapper)
        {
            return Map(a => (T)mapper(a));
        }

        /// <summary>
        /// Combines two rects
        /// </summary>
        /// <param name="other">The other GRect to be combined with</param>
        /// <param name="zipper">The method of combining each corner</param>
        /// <typeparam name="A">The type of the other GRect</typeparam>
        /// <typeparam name="B">The type of the output GRect</typeparam>
        /// <returns>A GRect that is a combination of two others</returns>
        public GRect<B> ZipWith<A, B>(GRect<A> other, System.Func<T, A, B> zipper)
        {
            return GRect.MinMaxRect(zipper(xMin, other.xMin), zipper(yMin, other.yMin), zipper(xMax, other.xMax), zipper(yMax, other.yMax));
        }

        private GRect<T> ZipWith(GRect<T> other, System.Func<Number<T>, Number<T>, Number<T>> zipper)
        {
            return ZipWith(other, (a,b) => (T)zipper(a,b));
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
        // public static GRect<T> operator +(GRect<T> first, GRect<T> second) => first.ZipWith(second, (a, b) => a + b);
        /// <summary>
        /// Subtracts the corners of the GRects and outputs a GRect that has the differences as its corners
        /// </summary>
        /// <param name="first">The first GRect</param>
        /// <param name="second">The second GRect</param>
        /// <returns>The difference GRect</returns>
        // public static GRect<T> operator -(GRect<T> first, GRect<T> second) => first.ZipWith(second, (a, b) => a - b);
        /// <summary>
        /// Multiplies a value to each corner of the GRect and ouputs a GRect with the products as its corners
        /// </summary>
        /// <param name="first">The GRect</param>
        /// <param name="second">The value</param>
        /// <returns>The product GRect</returns>
        public static GRect<T> operator *(GRect<T> first, T second) => first.Map(val => val * second);
        /// <summary>
        /// Multiplies a value to each corner of the GRect and ouputs a GRect with the products as its corners
        /// </summary>
        /// <param name="first">The value</param>
        /// <param name="second">The GRect</param>
        /// <returns>The product GRect</returns>
        public static GRect<T> operator *(T first, GRect<T> second) => second.Map(val => val * first);
        /// <summary>
        /// Divides a value to each corner of the GRect and ouputs a GRect with the quotient as its corners
        /// </summary>
        /// <param name="first">The value</param>
        /// <param name="second">The GRect</param>
        /// <returns>The quotient GRect</returns>
        public static GRect<T> operator /(GRect<T> first, T second) => first.Map(val => val / second);
        /// <summary>
        /// Divides a value to each corner of the GRect and ouputs a GRect with the quotient as its corners
        /// </summary>
        /// <param name="first">The GRect</param>
        /// <param name="second">The value</param>
        /// <returns>The quotient GRect</returns>
        public static GRect<T> operator /(T first, GRect<T> second) => second.Map(val => first / val);


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
    /// This class only exists because C# doesn't let you use static functions in a generic class implicitly. Meaning they will not infer what type you want based on the parameters of the function.
    /// And so the only use of this class is syntactic sugar as some might say. It allows you to call GRect.MinMaxRect(a, b, c, d), for example, without the need of explicitly specifying what the type is.
    /// </summary>
    public static class GRect
    {
        /// <summary>
        /// Creates a GRect using minimum and maximum values
        /// </summary>
        /// <param name="xMin">The left most value</param>
        /// <param name="yMin">The bottom most value</param>
        /// <param name="xMax">The right most value</param>
        /// <param name="yMax">The top most value</param>
        /// <returns>A GRect with the given values</returns>
        public static GRect<T> MinMaxRect<T>(T xMin, T yMin, T xMax, T yMax)
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
        public static GRect<T> WidthHeightRect<T>(T xMin, T yMin, T width, T height)
        {
            GRect<T> widthHeightRect = default;
            widthHeightRect._xMin = xMin;
            widthHeightRect._yMin = yMin;
            widthHeightRect._width = width;
            widthHeightRect._height = height;
            return widthHeightRect;
        }
    }
}