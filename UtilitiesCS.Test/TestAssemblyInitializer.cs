using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    /// <summary>
    /// Installs the shared process-wide AssemblyResolve fallback before any test in this assembly
    /// runs. The resolver logic, and the full explanation of why it is required, live in
    /// TestSupport/TestAssemblyResolver.cs, which is linked into this project and into
    /// QuickFiler.Test so the two copies cannot drift.
    /// </summary>
    [TestClass]
    public static class TestAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            global::TaskMaster.TestSupport.TestAssemblyResolver.Install();
        }
    }
}
