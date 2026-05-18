using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test
{
    /// <summary>
    /// Installs a process-wide AssemblyResolve fallback before any test runs.
    ///
    /// Purpose:
    ///     vstest's testhost does not always honor binding redirects from
    ///     UtilitiesCS.Test.dll.config (depending on AppDomain mode), which causes
    ///     FileNotFoundException for assemblies referenced at one version but deployed
    ///     at another (e.g. ExCSS 4.2.3 vs 4.3.1, System.Threading.Tasks.Extensions
    ///     4.2.0.1 vs 4.2.4.0). Production resolves these via TaskMaster.exe.config and
    ///     is unaffected; this initializer applies an equivalent fallback for the test
    ///     process so any same-name + same-public-key-token request is satisfied by
    ///     whatever version is actually loaded or available alongside the test DLL.
    /// </summary>
    [TestClass]
    public static class TestAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveByNameAndKey;
        }

        [ThreadStatic]
        private static HashSet<string> _resolving;

        private static Assembly ResolveByNameAndKey(object sender, ResolveEventArgs args)
        {
            var requested = new AssemblyName(args.Name);
            byte[] requestedKey = requested.GetPublicKeyToken();

            foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            {
                var loadedName = loaded.GetName();
                if (
                    !string.Equals(
                        loadedName.Name,
                        requested.Name,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }
                if (PublicKeyTokensEqual(loadedName.GetPublicKeyToken(), requestedKey))
                {
                    return loaded;
                }
            }

            // Fall back to a simple-name load from the probing path. Re-entrance guard
            // prevents infinite recursion when Assembly.Load itself fails and re-raises
            // AssemblyResolve on this thread.
            _resolving ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!_resolving.Add(requested.Name))
            {
                return null;
            }
            try
            {
                var byName = Assembly.Load(new AssemblyName(requested.Name));
                if (
                    byName != null
                    && PublicKeyTokensEqual(byName.GetName().GetPublicKeyToken(), requestedKey)
                )
                {
                    return byName;
                }
            }
            catch
            {
                // Swallow — return null so default resolution can run.
            }
            finally
            {
                _resolving.Remove(requested.Name);
            }

            return null;
        }

        private static bool PublicKeyTokensEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null)
            {
                return a == b || (a != null && a.Length == 0) || (b != null && b.Length == 0);
            }
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
