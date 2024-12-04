using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public class LazyTry<T>: Lazy<T>
    {
        public LazyTry() : base() { }
        public LazyTry(Func<T> valueFactory) : base(
            () =>
            {
                try

                {
                    return valueFactory();
                }
                catch (global::System.Exception)
                {
                    return default(T);
                }
            }) { }
        public LazyTry(LazyThreadSafetyMode mode): base(mode) { }
        public LazyTry(Func<T> valueFactory, bool isThreadSafe) : base(
            () =>
            {
                try

                {
                    return valueFactory();
                }
                catch (global::System.Exception)
                {
                    return default(T);
                }
            }, 
            isThreadSafe) { }
        public LazyTry(Func<T> valueFactory, LazyThreadSafetyMode mode) : base(
            () =>
            {
                try

                {
                    return valueFactory();
                }
                catch (global::System.Exception)
                {
                    return default(T);
                }
            }, 
            mode) { }

    }
}

