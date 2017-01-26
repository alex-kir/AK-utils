using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class AKMonad
{
    public class _Monad2<T>
    {
        readonly bool failed;
        readonly T value;

        public _Monad2(T value, bool failed)
        {
            this.value = value;
            this.failed = failed;
        }

        public override string ToString()
        {
            return failed ? null : value.ToString();
        }

        public T ToResult()
        {
            return value;
        }

        public T ToResultOrThrow()
        {
            if (failed)
                throw new InvalidOperationException();
            return value;
        }

        public static implicit operator T(_Monad2<T> self)
        {
            return self.value;
        }

        public _Monad2<T> Do(Action<T> action)
        {
            if (!failed)
                action(value);
            return this;
        }

        public _Monad2<TResult> Select<TResult>(Func<T, TResult> selector) where TResult : class
        {
            if (failed)
                return new _Monad2<TResult>(default(TResult), failed);
            var next = selector(value);
            return new _Monad2<TResult>(next, next == null);
        }

        public _Monad2<TResult> SelectValue<TResult>(Func<T, TResult> selector) where TResult : struct
        {
            if (failed)
                return new _Monad2<TResult>(default(TResult), failed);
            var next = selector(value);
            return new _Monad2<TResult>(next, failed);
        }

        public _Monad2<T> Where(Func<T, bool> func)
        {
            if (failed)
                return new _Monad2<T>(default(T), failed);
            return new _Monad2<T>(value, func(value));
        }

        public _Monad2<T> Else(Func<T> func)
        {
            if (failed) {
                var next = func();
                return new _Monad2<T>(next, next == null);
            }
            return this;
        }
    }

    public static _Monad2<T> ToMonad<T>(this T self) where T : class
    {
        return new _Monad2<T>(self, self == null);
    }

    public static _Monad2<TResult> ToMonad<T, TResult>(this T self, Func<T, TResult> selector)
        where T : class
        where TResult : class
    {
        return new _Monad2<T>(self, self == null).Select(selector);
    }

    public static _Monad2<T> ToValueMonad<T>(this T self) where T : struct
    {
        return new _Monad2<T>(self, false);
    }

    public static _Monad2<TResult> ToValueMonad<T, TResult>(this T self, Func<T, TResult> selector)
        where T : struct
        where TResult : struct
    {
        return new _Monad2<T>(self, false).SelectValue(selector);
    }
}
