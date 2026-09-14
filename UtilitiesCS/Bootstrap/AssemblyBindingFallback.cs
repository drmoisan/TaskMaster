#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace UtilitiesCS.Bootstrap
{
    /// <summary>
    /// Host-neutral assembly-binding fallback for the add-in process.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The add-in binds <c>FSharp.Core 11.0.0.0</c>, which references
    /// <c>netstandard, Version=2.1.0.0, PublicKeyToken=cc7b13ffcd2ddd51</c>. No
    /// <c>netstandard 2.1.0.0</c> exists for .NET Framework on any machine, so the default
    /// binder cannot satisfy that reference. The only assembly that can satisfy it is the
    /// <c>2.0.0.0</c> facade, returned through a resolve handler that does not compare
    /// versions. This type installs such a handler at a deterministic point in add-in
    /// startup rather than relying on an unrelated component having installed one first.
    /// </para>
    /// <para>
    /// This type is deliberately host-neutral: it references no user-interface type, no
    /// Outlook automation type, and no third-party logging framework. Logging through the
    /// repository's usual logging framework from inside an
    /// <see cref="AppDomain.AssemblyResolve"/> handler can itself re-enter assembly
    /// loading, so diagnostics here go through <see cref="Trace"/> only.
    /// </para>
    /// </remarks>
    public static class AssemblyBindingFallback
    {
        /// <summary>Simple name of the facade assembly this fallback exists to supply.</summary>
        internal const string NetstandardSimpleName = "netstandard";

        /// <summary>The only <c>netstandard</c> version that exists for .NET Framework.</summary>
        internal const string NetstandardFacadeVersion = "2.0.0.0";

        /// <summary>File name probed for in the runtime directory by ladder rung 3.</summary>
        internal const string NetstandardFacadeFileName = "netstandard.dll";

        /// <summary>Trace category used for every diagnostic this type emits.</summary>
        internal const string TraceCategory = "AssemblyBindingFallback";

        /// <summary>
        /// Zero until <see cref="Install"/> has subscribed the handler, one afterwards.
        /// Exchanged atomically so that concurrent callers attach exactly one handler.
        /// </summary>
        private static int _installed;

        /// <summary>
        /// Simple name currently being resolved on this thread, or <c>null</c>. A nested
        /// request for the same simple name would mean the handler has re-entered itself,
        /// which returns <c>null</c> immediately rather than recursing.
        /// </summary>
        [ThreadStatic]
        private static string? _resolvingSimpleName;

        /// <summary>
        /// Subscribes the binding fallback to the current application domain, at most once
        /// per domain however many times this method is called.
        /// </summary>
        /// <remarks>
        /// This method never throws. It is called from a static constructor in the add-in,
        /// where an escaping exception would surface as a
        /// <see cref="TypeInitializationException"/> and disable the whole add-in.
        /// </remarks>
        public static void Install()
        {
            try
            {
                if (Interlocked.Exchange(ref _installed, 1) != 0)
                {
                    return;
                }

                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            }
            catch (Exception ex)
            {
                // Boundary catch: this method is reachable from a type initializer, where an
                // escaping exception would take the add-in down. Absorb and trace instead.
                Trace.WriteLine("Install failed: " + ex, TraceCategory);
            }
        }

        /// <summary>
        /// Resolution seam. Returns the assembly that satisfies <paramref name="requested"/>,
        /// or <c>null</c> when no ladder rung applies.
        /// </summary>
        /// <param name="requested">The identity the CLR binder failed to resolve.</param>
        /// <returns>
        /// A loaded assembly, or <c>null</c>. The declared return type is non-nullable
        /// because it mirrors the un-annotated <see cref="ResolveEventHandler"/> contract,
        /// for which <c>null</c> is the documented "not resolved" result.
        /// </returns>
        internal static Assembly Resolve(AssemblyName requested)
        {
            if (requested is null)
            {
                return null!;
            }

            string? simpleName = requested.Name;
            if (string.IsNullOrEmpty(simpleName))
            {
                return null!;
            }

            if (string.Equals(_resolvingSimpleName, simpleName, StringComparison.OrdinalIgnoreCase))
            {
                return null!;
            }

            _resolvingSimpleName = simpleName;
            try
            {
                return CreateProductionLadder().Resolve(requested)!;
            }
            finally
            {
                _resolvingSimpleName = null;
            }
        }

        /// <summary>
        /// Builds a ladder wired to the production sources: the current domain's loaded
        /// assembly set, <see cref="Assembly.Load(string)"/>,
        /// <see cref="Assembly.LoadFrom(string)"/>, <see cref="File.Exists(string)"/> and
        /// <see cref="RuntimeEnvironment.GetRuntimeDirectory"/>.
        /// </summary>
        internal static AssemblyBindingLadder CreateProductionLadder()
        {
            return new AssemblyBindingLadder(
                () => AppDomain.CurrentDomain.GetAssemblies(),
                Assembly.Load,
                Assembly.LoadFrom,
                File.Exists,
                RuntimeEnvironment.GetRuntimeDirectory
            );
        }

        /// <summary>
        /// Handler attached to <see cref="AppDomain.AssemblyResolve"/>. Never throws and
        /// never propagates an exception to the CLR binder.
        /// </summary>
        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                if (args is null || string.IsNullOrEmpty(args.Name))
                {
                    return null!;
                }

                Assembly resolved = Resolve(new AssemblyName(args.Name));
                if (resolved is null)
                {
                    Trace.WriteLine("Unresolved: " + args.Name, TraceCategory);
                    return null!;
                }

                return resolved;
            }
            catch (Exception ex)
            {
                // Boundary catch: an exception escaping here reaches the CLR binder, which
                // turns a recoverable miss into a hard load failure. Absorb and trace.
                Trace.WriteLine("Resolve failed: " + ex, TraceCategory);
                return null!;
            }
        }

        /// <summary>
        /// The ordered resolution ladder. Every external source is an injected delegate so
        /// the ordering logic is unit-testable without touching the GAC or the filesystem.
        /// </summary>
        internal sealed class AssemblyBindingLadder
        {
            private readonly Func<Assembly[]> _getLoadedAssemblies;
            private readonly Func<string, Assembly> _loadByDisplayName;
            private readonly Func<string, Assembly> _loadFromPath;
            private readonly Func<string, bool> _fileExists;
            private readonly Func<string> _getRuntimeDirectory;

            /// <summary>
            /// Creates a ladder over the five supplied sources.
            /// </summary>
            /// <param name="getLoadedAssemblies">Returns the domain's loaded assembly set.</param>
            /// <param name="loadByDisplayName">Loads an assembly by full display name.</param>
            /// <param name="loadFromPath">Loads an assembly from an absolute file path.</param>
            /// <param name="fileExists">Reports whether a file path exists.</param>
            /// <param name="getRuntimeDirectory">Returns the CLR runtime directory.</param>
            /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
            internal AssemblyBindingLadder(
                Func<Assembly[]> getLoadedAssemblies,
                Func<string, Assembly> loadByDisplayName,
                Func<string, Assembly> loadFromPath,
                Func<string, bool> fileExists,
                Func<string> getRuntimeDirectory
            )
            {
                _getLoadedAssemblies =
                    getLoadedAssemblies
                    ?? throw new ArgumentNullException(nameof(getLoadedAssemblies));
                _loadByDisplayName =
                    loadByDisplayName ?? throw new ArgumentNullException(nameof(loadByDisplayName));
                _loadFromPath =
                    loadFromPath ?? throw new ArgumentNullException(nameof(loadFromPath));
                _fileExists = fileExists ?? throw new ArgumentNullException(nameof(fileExists));
                _getRuntimeDirectory =
                    getRuntimeDirectory
                    ?? throw new ArgumentNullException(nameof(getRuntimeDirectory));
            }

            /// <summary>
            /// Walks the four rungs in order and returns the first non-null result, or
            /// <c>null</c> when no rung applies.
            /// </summary>
            /// <param name="requested">The identity the CLR binder failed to resolve.</param>
            internal Assembly? Resolve(AssemblyName requested)
            {
                if (requested is null)
                {
                    return null;
                }

                return FromAlreadyLoaded(requested)
                    ?? FromFullDisplayName(requested)
                    ?? FromRuntimeDirectory(requested)
                    ?? FromProbeDirectory(requested);
            }

            /// <summary>
            /// Rung 1: an already-loaded assembly whose simple name matches
            /// case-insensitively and whose public key token is equal. Version is
            /// deliberately not compared.
            /// </summary>
            /// <remarks>Behaviour-empty seam; the ladder is implemented in Phase 3.</remarks>
            private Assembly? FromAlreadyLoaded(AssemblyName requested)
            {
                _ = requested;
                _ = _getLoadedAssemblies;
                return null;
            }

            /// <summary>
            /// Rung 2: <see cref="Assembly.Load(string)"/> of the full display name, using
            /// <c>Version=2.0.0.0</c> for the <c>netstandard</c> identity specifically.
            /// </summary>
            /// <remarks>Behaviour-empty seam; the ladder is implemented in Phase 3.</remarks>
            private Assembly? FromFullDisplayName(AssemblyName requested)
            {
                _ = requested;
                _ = _loadByDisplayName;
                return null;
            }

            /// <summary>
            /// Rung 3: the runtime-directory facade, loaded from an absolute path, for the
            /// <c>netstandard</c> identity only.
            /// </summary>
            /// <remarks>Behaviour-empty seam; the ladder is implemented in Phase 3.</remarks>
            private Assembly? FromRuntimeDirectory(AssemblyName requested)
            {
                _ = requested;
                _ = _getRuntimeDirectory;
                return null;
            }

            /// <summary>
            /// Rung 4: a directory probe for the simple name plus <c>.dll</c> next to the
            /// executing assembly.
            /// </summary>
            /// <remarks>Behaviour-empty seam; the ladder is implemented in Phase 3.</remarks>
            private Assembly? FromProbeDirectory(AssemblyName requested)
            {
                _ = requested;
                _ = _fileExists;
                _ = _loadFromPath;
                return null;
            }
        }
    }
}
