namespace UnityHelpers
{
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