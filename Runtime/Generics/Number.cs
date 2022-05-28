using System;

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

        public static implicit operator T(Number<T> wrapped) => wrapped.stored;
        public static implicit operator Number<T>(T value) => new Number<T>(value);
        public static Number<T> operator +(Number<T> first, Number<T> second) => (dynamic)first.stored + second.stored;
        public static Number<T> operator -(Number<T> first, Number<T> second) => (dynamic)first.stored - second.stored;
        public static Number<T> operator *(Number<T> first, Number<T> second) => (dynamic)first.stored * second.stored;
        public static Number<T> operator /(Number<T> first, Number<T> second) => (dynamic)first.stored / second.stored;
    }
}