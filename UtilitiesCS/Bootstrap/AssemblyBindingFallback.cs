#nullable enable

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
        /// A loaded assembly, or <c>null</c> when no ladder rung applies. The declared return
        /// type is nullable because returning <c>null</c> is the ordinary result on this seam,
        /// not an exceptional one: every guard clause and every exhausted ladder declines by
        /// returning <c>null</c>. Callers must handle a <c>null</c> result.
        /// </returns>
        internal static Assembly? Resolve(AssemblyName requested)
        {
            if (requested is null)
            {
                return null;
            }

            string? simpleName = requested.Name;
            if (string.IsNullOrEmpty(simpleName))
            {
                return null;
            }

            if (string.Equals(_resolvingSimpleName, simpleName, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            _resolvingSimpleName = simpleName;
            try
            {
                return CreateProductionLadder().Resolve(requested);
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

                Assembly? resolved = Resolve(new AssemblyName(args.Name));
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
            /// deliberately not compared, because supplying a same-token assembly of a
            /// different version is the whole purpose of this fallback.
            /// </summary>
            /// <param name="requested">The identity the CLR binder failed to resolve.</param>
            private Assembly? FromAlreadyLoaded(AssemblyName requested)
            {
                byte[]? requestedToken = requested.GetPublicKeyToken();
                if (requestedToken is null || requestedToken.Length == 0)
                {
                    // A request carrying no strong-name token cannot be matched against a
                    // strongly named loaded assembly without weakening the comparison to the
                    // simple name alone, which would return arbitrary assemblies.
                    return null;
                }

                try
                {
                    foreach (Assembly candidate in _getLoadedAssemblies())
                    {
                        AssemblyName candidateName = candidate.GetName();
                        bool sameSimpleName = string.Equals(
                            candidateName.Name,
                            requested.Name,
                            StringComparison.OrdinalIgnoreCase
                        );
                        if (
                            sameSimpleName
                            && TokensAreEqual(candidateName.GetPublicKeyToken(), requestedToken)
                        )
                        {
                            return candidate;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Rung-local catch: a failure of one source must not stop the ladder.
                    Trace.WriteLine("Rung 1 (already loaded) failed: " + ex, TraceCategory);
                }

                return null;
            }

            /// <summary>
            /// Rung 2: <see cref="Assembly.Load(string)"/> of the full display name, using
            /// <c>Version=2.0.0.0</c> for the <c>netstandard</c> identity specifically.
            /// </summary>
            /// <param name="requested">The identity the CLR binder failed to resolve.</param>
            private Assembly? FromFullDisplayName(AssemblyName requested)
            {
                try
                {
                    return _loadByDisplayName(BuildDisplayName(requested));
                }
                catch (Exception ex)
                {
                    // Rung-local catch: a miss here is ordinary and the ladder continues.
                    Trace.WriteLine("Rung 2 (display name) failed: " + ex, TraceCategory);
                    return null;
                }
            }

            /// <summary>
            /// Rung 3: the runtime-directory facade, loaded from an absolute path, for the
            /// <c>netstandard</c> identity only. This rung bypasses assembly-cache lookup
            /// entirely, which is why it is robust to whatever the reported
            /// <c>2.0.0.0</c> lookup failure turns out to mean.
            /// </summary>
            /// <param name="requested">The identity the CLR binder failed to resolve.</param>
            private Assembly? FromRuntimeDirectory(AssemblyName requested)
            {
                if (!IsNetstandard(requested))
                {
                    return null;
                }

                try
                {
                    string path = Path.Combine(
                        _getRuntimeDirectory(),
                        NetstandardFacadeFileName
                    );
                    if (!_fileExists(path))
                    {
                        return null;
                    }

                    return _loadFromPath(path);
                }
                catch (Exception ex)
                {
                    // Rung-local catch: a failure of one source must not stop the ladder.
                    Trace.WriteLine("Rung 3 (runtime directory) failed: " + ex, TraceCategory);
                    return null;
                }
            }

            /// <summary>
            /// Rung 4: a directory probe for the simple name plus <c>.dll</c> next to the
            /// executing assembly.
            /// </summary>
            /// <param name="requested">The identity the CLR binder failed to resolve.</param>
            private Assembly? FromProbeDirectory(AssemblyName requested)
            {
                string? simpleName = requested.Name;
                if (string.IsNullOrEmpty(simpleName))
                {
                    return null;
                }

                try
                {
                    string? directory = Path.GetDirectoryName(
                        typeof(AssemblyBindingFallback).Assembly.Location
                    );
                    if (string.IsNullOrEmpty(directory))
                    {
                        return null;
                    }

                    string path = Path.Combine(directory, simpleName + ".dll");
                    if (!_fileExists(path))
                    {
                        return null;
                    }

                    return _loadFromPath(path);
                }
                catch (Exception ex)
                {
                    // Rung-local catch: a failure of one source must not stop the ladder.
                    Trace.WriteLine("Rung 4 (probe directory) failed: " + ex, TraceCategory);
                    return null;
                }
            }

            /// <summary>
            /// Reports whether <paramref name="requested"/> names the facade assembly this
            /// fallback exists to supply.
            /// </summary>
            private static bool IsNetstandard(AssemblyName requested)
            {
                return string.Equals(
                    requested.Name,
                    NetstandardSimpleName,
                    StringComparison.OrdinalIgnoreCase
                );
            }

            /// <summary>
            /// Composes the full display name rung 2 asks for: the requested simple name,
            /// culture and public key token, with the version pinned to the facade version
            /// for the <c>netstandard</c> identity and taken from the request otherwise.
            /// </summary>
            private static string BuildDisplayName(AssemblyName requested)
            {
                string simpleName = requested.Name ?? string.Empty;
                string version = IsNetstandard(requested)
                    ? NetstandardFacadeVersion
                    : (requested.Version?.ToString() ?? "0.0.0.0");

                return simpleName
                    + ", Version="
                    + version
                    + ", Culture=neutral, PublicKeyToken="
                    + FormatToken(requested.GetPublicKeyToken());
            }

            /// <summary>
            /// Renders a public key token as the lower-case hexadecimal form an assembly
            /// display name uses, or the literal <c>null</c> when there is no token.
            /// </summary>
            private static string FormatToken(byte[]? token)
            {
                if (token is null || token.Length == 0)
                {
                    return "null";
                }

                var builder = new StringBuilder(token.Length * 2);
                foreach (byte value in token)
                {
                    builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }

            /// <summary>
            /// Compares two public key tokens for byte equality, treating an absent or
            /// zero-length token as never equal.
            /// </summary>
            private static bool TokensAreEqual(byte[]? candidate, byte[]? requested)
            {
                if (candidate is null || requested is null)
                {
                    return false;
                }

                if (candidate.Length == 0 || candidate.Length != requested.Length)
                {
                    return false;
                }

                for (int i = 0; i < candidate.Length; i++)
                {
                    if (candidate[i] != requested[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
