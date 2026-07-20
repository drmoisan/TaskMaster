using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
#nullable enable

    public static class ExceptionExtensions
    {
        public static int GetLineNumber(this System.Exception ex)
        {
            // Get stack trace for the exception with source file information
            var st = new StackTrace(ex, true);

            // Get the top stack frame
            var frame = st.GetFrame(0);

            // Get the line number from the stack frame.
            // StackTrace.GetFrame is annotated as returning a nullable StackFrame; the
            // null-forgiving operator preserves the original behavior (an NRE if the
            // exception carried no frames) rather than introducing a new guard/return path.
            return frame!.GetFileLineNumber();
        }
    }
}
