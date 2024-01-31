using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.Extensions.Lazy
{
    public static class LazyExtension
    {
        public static Lazy<T> AsLazyValue<T>(this T value) where T: class
        {
            return new Lazy<T>(value.Return);
        }
        
        public static Func<T> AsFunc<T>(this T value) where T: class
        {
            return new Func<T>(value.Return);
        }        

        internal static T Return<T>(this T value)
        {
            return value;
        }
    }
}
