
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS
{
    public class VerboseLogger<T> where T : class
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public VerboseLogger() 
        {
            var methods = typeof(T).GetMethods();
            var dict = methods.Select(x => new KeyValuePair<string, bool>(x.Name, false)).Distinct().ToDictionary();
            _verboseMethods = new ConcurrentDictionary<string, bool>(dict);
        }

        public ConcurrentDictionary<string, bool> VerboseMethods => _verboseMethods;
        private ConcurrentDictionary<string, bool> _verboseMethods;

        public void SetVerbose(IEnumerable<string> methodNames) => methodNames.ForEach(name => VerboseMethods[name] = true);
        public void SetVerbose(string methodName) => VerboseMethods[methodName] = true;
        public bool IsVerbose([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            
            return VerboseMethods.ContainsKey(memberName) ? VerboseMethods[memberName] : false;
        }
        public void VerboseAction(System.Action action, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (IsVerbose(memberName))
            {
                action();
            }
        }
        public void Log(string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (IsVerbose(memberName))
            {
                //logger.Debug(message);
            }
        }
        public void LogObject(IDictionary<string, long> dict, string name, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            if (IsVerbose(memberName))
            {
                //logger.Debug($"{memberName}.{name} :\n{dict.ToFormattedText()}");
            }
        }
    }
}
