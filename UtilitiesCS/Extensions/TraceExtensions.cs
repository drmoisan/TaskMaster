using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.Extensions
{
    public static class TraceExtensions
    {
        public static MethodBase GetCallerByName(this StackTrace sf, string methodName)
        {
            if (methodName.IsNullOrEmpty() || methodName == "MoveNext")
                return null;
            
            MethodBase caller = null;
            bool repeat = true;
            int i = 0;
            do
            {
                try
                {
                    if (++i >= sf.FrameCount)
                    {
                        caller = null;
                        repeat = false;
                    }
                    else
                    {
                        var m = sf.GetFrame(i).GetMethod();
                        if (m.Name == methodName)
                        {
                            caller = m;
                            repeat = false;
                        }
                    }

                }
                catch (System.Exception)
                {
                    caller = null;
                    repeat = false;
                }
            } while (repeat);
            return caller;
        }

        public static string GetParameterName(this MethodBase method, int index)
        {
            return method.GetParameters()[index].Name;
        }

        public static string[] GetParameterNames(this MethodBase method)
        {
            return method.GetParameters().Select(x => x.Name).ToArray();
        }
    }
}
