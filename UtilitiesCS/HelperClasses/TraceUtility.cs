using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

namespace UtilitiesCS
{
    public class TraceUtility
    {
        [Conditional("TRACE")]
        public static void LogMethodCall(params object[] callingMethodParamValues)
        {
            var sf = new StackTrace();
            //var assembliesAndMethods = Enumerable.Range(0, sf.FrameCount).Select(i => (sf.GetFrame(i).GetMethod().DeclaringType.Assembly.GetName().Name, sf.GetFrame(i).GetMethod().Name)).ToArray();
            
            int frameLevel = 0;
            MethodBase method, methodCalledBy;
            try
            {
                method = GetFirstMethodOfMine(sf, ref frameLevel);
                if (method is null)
                {
                    frameLevel = 1;
                    method = new StackFrame(skipFrames: frameLevel).GetMethod();
                }
            }
            catch
            {
                frameLevel = 1;
                method = null;
            }

            var methodParameters = method?.GetParameters();
            string methodName = "";
            if (method is not null) 
            {
                methodName = $"{GetClassName(method)}.{method.Name}";
            }

            try
            {
                methodCalledBy = GetFirstMethodOfMine(sf, ref frameLevel);
                if (methodCalledBy is null)
                {
                    frameLevel = 2;
                    methodCalledBy = new StackFrame(skipFrames: frameLevel).GetMethod();
                }
            }
            catch (Exception)
            {
                frameLevel = 2;
                methodCalledBy = null;
            }
            
            var methodCaller = $"{GetClassName(methodCalledBy)}.{methodCalledBy.Name}()" ?? "";

            // Exclude out parameters
            var methodParamsExcludingOut = methodParameters?.Where(p => !p.IsOut).ToArray();
            if (methodParamsExcludingOut.Length == callingMethodParamValues.Length)
            {
                List<string> parameterList = new List<string>();
                foreach (var parameter in methodParamsExcludingOut)
                {
                    parameterList.Add($"{parameter.Name}={callingMethodParamValues[parameter.Position]}");
                }

                logger.Info($"TRACE\t{methodCaller} -> {methodName}({string.Join(", ", parameterList)})");
            }
            else
            {
                logger.Info($"TRACE\t{methodCaller} -> {method.Name}(/* Please update to pass in all parameters */)");
            }
        }

        public static string[] GetMyStackSummary(StackTrace sf)
        {
            var stack = GetMyStack(sf);
            var stackSummary = stack.Select(m => $"{GetClassName(m)}.{m.Name}").ToArray();
            return stackSummary;
        }

        private static string GetClassName(MethodBase m)
        {
            if (m.IsStatic) { return m.Module.Name; }
            else { return m.DeclaringType.Name; }
        }
        
        public static List<MethodBase> GetMyStack(StackTrace sf)
        {
            List<MethodBase> stack = new List<MethodBase>();
            
            for (int i = 0; i < sf.FrameCount; i++)
            {
                var method = sf.GetFrame(i).GetMethod();
                string assemblyName;
                if (method.IsStatic)
                {
                    assemblyName = method.Module.Assembly.GetName().Name;
                }
                else
                {
                    assemblyName = method.DeclaringType.Assembly.GetName().Name;
                }
                if (ProjectNames.Contains(assemblyName))
                {
                    if (method.Name != "MoveNext")
                    {
                        stack.Add(method);
                    }
                }
            }
            
            return stack;
        }

        private static MethodBase GetFirstMethodOfMine(StackTrace sf, ref int i)
        {
            MethodBase methodCalledBy = null;
            bool repeat = true;
            do
            {
                try
                {
                    if(++i >= sf.FrameCount) 
                    {
                        methodCalledBy = null;
                        repeat = false;
                    }
                    else
                    {
                        var m = sf.GetFrame(i).GetMethod();
                        string assemblyName;
                        
                        if (m.IsStatic) { assemblyName = m.Module.Assembly.GetName().Name; }
                        else { assemblyName = m.DeclaringType.Assembly.GetName().Name; }
                        
                        if (ProjectNames.Contains(assemblyName))
                        {
                            methodCalledBy = sf.GetFrame(i).GetMethod();
                            if (methodCalledBy.Name == "MoveNext") { methodCalledBy = null; }
                            else { repeat = false; }
                        }
                    }
                    
                }
                catch (Exception)
                {
                    methodCalledBy = null;
                    repeat = false;
                }
            } while (repeat);
            return methodCalledBy;
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static List<string> _projectNames;
        internal static List<string> ProjectNames
        {
            get
            {
                if (_projectNames is null)
                {
                    _projectNames = new List<string> 
                    { 
                        "Tags", "ToDoModel", "ToDoModel.Test", "TaskVisualization", 
                        "TaskMaster.Test", "UtilitiesCS", "UtilitiesCS.Test", "QuickFiler", 
                        "QuickFiler.Test", "TaskVisualization.Test", "SVGControl", 
                        "SVGControl.Test", "TaskTree", "TaskMaster", "UtilitiesSwordfish.NET.General", 
                        "UtilitiesSwordfish.NET.Test", "Tags.Test" 
                    };
                }
                return _projectNames;
            }
        }
        
        
    }
}
