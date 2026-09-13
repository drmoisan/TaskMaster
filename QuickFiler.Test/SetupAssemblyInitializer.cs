using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickFiler.Test
{
    /// <summary>
    /// Sets up windows forms for hd resolution
    /// </summary>
    [TestClass]
    public class SetupAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // Issue #877: install this assembly's own AssemblyResolve fallback before any test
            // class runs, so QuickFiler.Test no longer depends on an unrelated class touching
            // SVGControl.SvgRenderer first. See TestSupport/TestAssemblyResolver.cs.
            global::TaskMaster.TestSupport.TestAssemblyResolver.Install();

            // Set up windows forms for hd resolution
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        }
    }
}
