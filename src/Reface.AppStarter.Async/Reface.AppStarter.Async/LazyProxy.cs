using System;

namespace Reface.AppStarter.Async
{
    public class LazyProxy<T> : Lazy<T>
    {
        static Func<T> realFactory(object context, Func<object, T> factory)
        {
            T value = factory(context);
            return () => value;
        }

        public LazyProxy(object context, Func<object, T> factory)
            : base(realFactory(context, factory))
        {

        }
    }
}
