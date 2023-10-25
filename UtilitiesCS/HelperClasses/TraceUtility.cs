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
            var assembliesAndMethods = Enumerable.Range(0, sf.FrameCount).Select(i => (sf.GetFrame(i).GetMethod().DeclaringType.Assembly.GetName().Name, sf.GetFrame(i).GetMethod().Name)).ToArray();
            
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
                methodName = $"{method.DeclaringType.Name}.{method.Name}";
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

            //var methodCaller = $"{methodCalledBy.DeclaringType.Name}.{methodCalledBy.Name}()" ?? "";
            var methodCaller = $"{methodCalledBy.DeclaringType.Name}.{methodCalledBy.Name}()" ?? "";

            if (methodParameters.Length == callingMethodParamValues.Length)
            {
                List<string> parameterList = new List<string>();
                foreach (var parameter in methodParameters)
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

        private static MethodBase GetFirstMethodOfMine(StackTrace sf, ref int i)
        {
            MethodBase methodCalledBy = null;
            bool repeat = true;
            do
            {
                try
                {
                    var assemblyName = sf.GetFrame(++i).GetMethod().DeclaringType.Assembly.GetName().Name;
                    if (ProjectNames.Contains(assemblyName))
                    {
                        methodCalledBy = sf.GetFrame(i).GetMethod();
                        if (methodCalledBy.Name == "MoveNext")
                        {
                            methodCalledBy = null;
                        }
                        else
                        {
                            repeat = false;
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
        
        //internal static List<string> ProjectPaths 
        //{
        //    get 
        //    { 
        //        if (_projectPaths is null)
        //        {
        //            var solutionPath = GetSolutionPath();
        //            var content = File.ReadAllText(solutionPath);
        //            Regex projReg = new Regex(
        //                "Project\\(\"\\{[\\w-]*\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs|vcx|vb)proj)\"", 
        //                RegexOptions.Compiled);
        //            var matches = projReg.Matches(content).Cast<Match>();
        //            var projects = matches.Select(x => x.Groups[2].Value).ToList();
        //            for (int i = 0; i < projects.Count; ++i)
        //            {
        //                if (!Path.IsPathRooted(projects[i]))
        //                    projects[i] = Path.Combine(Path.GetDirectoryName(solutionPath),
        //                        projects[i]);
        //                projects[i] = Path.GetFullPath(projects[i]);
        //            }
        //            _projectPaths = projects;
        //        }
        //        return _projectPaths;
        //    }
        //}

        //Doesn't work
        //internal static string GetSolutionPath()
        //{
        //    var currentDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    while (currentDirPath != null)
        //    {
        //        var fileInCurrentDir = Directory.GetFiles(currentDirPath).Select(f => f.Split(@"\").Last()).ToArray();
        //        var solutionFileName = fileInCurrentDir.SingleOrDefault(f => f.EndsWith(".sln", StringComparison.InvariantCultureIgnoreCase));
        //        if (solutionFileName != null)
        //            return Path.Combine(currentDirPath, solutionFileName);

        //        currentDirPath = Directory.GetParent(currentDirPath)?.FullName;
        //    }

        //    throw new FileNotFoundException("Cannot find solution file path");
        //}
    }
}
