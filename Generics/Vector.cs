//Written by @bmabsout

using UnityEngine;
using System;

namespace UnityHelpers
{
    using N1 = S<N0>;
    using N2 = S<S<N0>>;
    using N3 = S<S<S<N0>>>;
    
    public class Vector : MonoBehaviour
    {
        public void Start()
        {
            var thing3 = N.z.s.s.s;
            var thing2 = N.z.s.s;
            var thing1 = N.z.s;
            var v1 = ZV.v(0.1f).v(0.2f);
            var v2 = ZV.v(2f).v(1f);
            var v3 = ZV.v(3f).v(1f);
            var m = ZV.v(v1).v(v2).v(v3);
        }
    }
    public abstract class N
    {
        public static N0 z = new N0();
        public abstract int ToInt();
    }

    // public class N
    // {
    //     public static N0 z = new N0();
    //     // public S<Z> one { get { return new S<Z>(); }}

    //     public static G Add<G>(G first, N0 second) where G : Peano
    //     {
    //         return first;
    //     }
    //     public static G Add<G>(N0 first, G second) where G : Peano
    //     {
    //         return second;
    //     }
    //     public static S<G> Add<G, A, B>(A first, S<B> second) where G : Peano where A : Peano where B : Peano
    //     {
    //         return Add(first, second.rest).s;
    //     }
        
    //     // public class Plus<A, B> where A : Peano where B : Peano
    //     // {

    //     // }
    // }

    public class S<G> : N
        where G : N
    {
        public S<S<G>> s { get { return new S<S<G>>(this); } }

        public G rest;

        public S(G value)
        {
            rest = value;
        }

        public override int ToInt()
        {
            return rest.ToInt() + 1;
        }
    }
    public class N0 : N
    {
        public S<N0> s { get { return new S<N0>(this); } }

        public override int ToInt()
        {
            return 0;
        }
    }
    // public class N1 : S<N0>
    // {
    //     public N1(N0 zero) : base(zero)
    //     {
            
    //     }
    // }
    // public class N2 : S<S<N0>>
    // {
    //     public N2(N1 one) : base (one)
    //     {

    //     }
    // }
    // public class N3 : S<S<S<N0>>>
    // {
    //     public N3(N2 two) : base(two)
    //     {

    //     }
    // }



    public abstract class Vec<T, A> where T : N
    {
        public abstract Vec<T, B> Map<B>(Func<A, B> f);

        public Vec<S<T>, A> v(A a)
        {
            return new Cons<T, A>(a, this);
        }

        public abstract Vec<T, C> Zip<B, C>(Vec<T, B> second, Func<A, B, C> f);

        private static Func<A, A, A> Dynamic(Func<dynamic, dynamic, A> f)
        {
            return (a, b) => f((dynamic)a, (dynamic)b);
        }

        public static Vec<T, A> operator +(Vec<T, A> v1, Vec<T, A> v2)
        {
            return v1.Zip(v2, Dynamic((a,b) => a + b));
        }

        public static Vec<T, A> operator -(Vec<T, A> v1, Vec<T, A> v2)
        {
            return v1.Zip(v2, Dynamic((a, b) => a - b));
        }
    }

    public class Nothing<A> : Vec<N0, A>
    {
        public override Vec<N0, B> Map<B>(Func<A, B> f)
        {
            return new Nothing<B>();
        }

        public override string ToString()
        {
            return "[]";
        }

        public override Vec<N0, C> Zip<B, C>(Vec<N0, B> second, Func<A, B, C> f)
        {
            return new Nothing<C>();
        }  
    }

    public class Cons<T, A> : Vec<S<T>, A> where T : N
    {
    public A held;
    public Vec<T, A> tail;
    
        public Cons(A hold, Vec<T, A> tail)
        {
            this.held = hold;
            this.tail = tail;
        }

        public override Vec<S<T>, B> Map<B>(Func<A, B> f)
        {
            return new Cons<T, B>(f(held), tail.Map(f));
        }

        public override string ToString()
        {
            return held.ToString() + ":" + tail.ToString();
        }

        public override Vec<S<T>, C> Zip<B, C>(Vec<S<T>, B> second, Func<A, B, C> f)
        {
            Cons<T, B> realDeal = (Cons<T, B>)second;
            return tail.Zip(realDeal.tail, f).v(f(held, (realDeal.held)));
        }
    }

    public static class ZV
    {
        public static Vec<S<N0>, A> v<A>(A a)
        {
            return new Cons<N0, A>(a, new Nothing<A>());
        }
    }
}