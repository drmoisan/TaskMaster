using System;
using System.Collections.Generic;
using System.Reflection;

namespace TaskMaster.TestSupport
{
    /// <summary>
    /// Installs a process-wide AppDomain.CurrentDomain.AssemblyResolve fallback that matches a
    /// requested assembly on simple name plus public key token. Linked into QuickFiler.Test and
    /// UtilitiesCS.Test from this single file so the two copies cannot drift.
    /// <para>
    /// Reason 1, binding redirects are not applied. vstest's testhost
    /// does not reliably honour binding redirects from the test assembly's .dll.config, depending
    /// on AppDomain mode, so a reference recorded at one version but deployed at another raises
    /// FileNotFoundException (for example ExCSS 4.2.3 vs 4.3.1, or
    /// System.Threading.Tasks.Extensions 4.2.0.1 vs 4.2.4.0). Production resolves these through
    /// TaskMaster.exe.config and is unaffected.
    /// </para>
    /// <para>
    /// Reason 2, an unsatisfiable netstandard bind. Both test projects redirect FSharp.Core to
    /// 11.0.0.0, and FSharp.Core 11.0.0.0 references
    /// netstandard, Version=2.1.0.0, PublicKeyToken=cc7b13ffcd2ddd51
    /// The requirement enters the closure through that FSharp.Core redirect and NOT through
    /// Deedle, which asks only for netstandard 2.0.0.0. Nothing on this machine satisfies it: the
    /// GAC holds only v4.0_2.0.0.0__cc7b13ffcd2ddd51 and no config file in the repository contains
    /// the string netstandard. The two versions share a public key token, so a handler matching on
    /// simple name plus token resolves the request while the default binder cannot.
    /// </para>
    /// <para>
    /// Do not delete this handler because Reason 1 looks obsolete. Reason 2 stands on its own:
    /// without the fallback, any test that touches Deedle fails at static-initializer time, and
    /// the CLR caches a failed type initializer for the lifetime of the process.
    /// </para>
    /// </summary>
    internal static class TestAssemblyResolver
    {
        /// <summary>
        /// Attaches the fallback to the current AppDomain. Call this from an assembly's
        /// AssemblyInitialize method so that it runs before any test in that assembly.
        /// Attaching twice in one process is harmless: the handler is a pure lookup with no
        /// state beyond a thread-local re-entrance guard.
        /// </summary>
        public static void Install()
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
            // AssemblyResolve on this thread. Written as an explicit null test rather than a
            // null-coalescing assignment because QuickFiler.Test declares no LangVersion
            // element and compiles at the C# 7.3 default, where that operator is CS8370.
            if (_resolving == null)
            {
                _resolving = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
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
                // Swallow - return null so default resolution can run.
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
