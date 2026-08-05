#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SVGControl
{
    /// <summary>
    /// Installs and implements the SVGControl AppDomain assembly-resolve fallback. Separated from
    /// SvgRenderer because it concerns assembly binding rather than SVG rendering and carries no
    /// renderer state.
    /// </summary>
    internal static class SvgAssemblyResolver
    {
        // Why this fallback exists. The deployed packages are Svg 3.4.8 and ExCSS 4.3.2, and only
        // packages/ExCSS.4.3.2/ is present on disk, so a request for any other ExCSS version can be
        // satisfied only by a binding redirect or by this handler. Hosts that apply the project
        // binding redirects resolve it themselves: production is a VSTO add-in inside OUTLOOK.EXE
        // whose per-add-in AppDomain applies TaskMaster.dll.config, and the vstest testhost does
        // apply them too, so the ExCSS bind succeeds there. The host that does NOT apply them is
        // devenv.exe, which loads SVGControl.dll for the WinForms designer with no ExCSS entry in
        // its own configuration; there the bind fails and this handler is the only recovery. See
        // docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/research/
        // 2026-08-04T15-05-svg-renderer-null-document-research.md for the host matrix and the
        // measured conclusions. The handler satisfies any version request for an assembly with a
        // matching simple name and public key token, sourced from the already-loaded set, then the
        // probing path, then the directories next to SVGControl.dll.
        private static int _resolverInstalled;

        [ThreadStatic]
        private static HashSet<string>? _resolving;

        // Subscribes the fallback exactly once per AppDomain. Called from the SvgRenderer static
        // constructor, so touching SvgRenderer still installs the handler.
        internal static void Install()
        {
            if (Interlocked.Exchange(ref _resolverInstalled, 1) == 0)
            {
                AppDomain.CurrentDomain.AssemblyResolve += ResolveByNameAndKey;
            }
        }

        private static System.Reflection.Assembly? ResolveByNameAndKey(
            object sender,
            ResolveEventArgs args
        )
        {
            var requested = new System.Reflection.AssemblyName(args.Name);
            byte[] requestedKey = requested.GetPublicKeyToken();
            foreach (var loaded in System.AppDomain.CurrentDomain.GetAssemblies())
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
                byte[] loadedKey = loadedName.GetPublicKeyToken();
                if (SvgAssemblyProbe.PublicKeyTokensEqual(loadedKey, requestedKey))
                {
                    return loaded;
                }
            }

            // No loaded match — fall back to loading by simple name from the probing path.
            // This recovers cases where a versioned reference (e.g., ExCSS 4.2.3) is being
            // requested but only a newer same-key version is deployed alongside the test DLL.
            // Re-entrance guard prevents infinite recursion when Assembly.Load itself fails
            // and re-raises AssemblyResolve on this thread.
            _resolving ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!_resolving.Add(requested.Name))
            {
                return null;
            }
            try
            {
                // Strategy 2 — load by simple name from the probing path.
                try
                {
                    var name = new System.Reflection.AssemblyName(requested.Name);
                    var byName = System.Reflection.Assembly.Load(name);
                    byte[]? byNameKey = byName?.GetName().GetPublicKeyToken();
                    if (
                        byName != null
                        && SvgAssemblyProbe.PublicKeyTokensEqual(byNameKey, requestedKey)
                    )
                    {
                        return byName;
                    }
                }
                // Trace, not log4net: log4net inside an AssemblyResolve handler can itself trigger a
                // re-entrant assembly load, so this diagnostic must not depend on it being loadable.
                catch (Exception ex)
                {
                    Trace.TraceWarning(
                        $"SvgRenderer load '{requested.Name}': {SvgRenderer.DescribeFailure(ex)}"
                    );
                }

                // Strategy 3 — probe candidate directories for a same-key file on disk. Ordered after
                // strategies 1 and 2 so an already-loaded match always wins over a fresh LoadFrom.
                var self = typeof(SvgRenderer).Assembly;
                IReadOnlyList<string> probeDirectories = SvgAssemblyProbe.GetProbeDirectories(
                    self.Location,
                    self.CodeBase,
                    AppDomain.CurrentDomain.BaseDirectory
                );
                foreach (string directory in probeDirectories)
                {
                    string path = Path.Combine(directory, requested.Name + ".dll");
                    if (!File.Exists(path))
                    {
                        continue;
                    }
                    // Trace here for the same re-entrancy reason given above.
                    try
                    {
                        var loaded = System.Reflection.Assembly.LoadFrom(path);
                        byte[] loadedFileKey = loaded.GetName().GetPublicKeyToken();
                        if (SvgAssemblyProbe.PublicKeyTokensEqual(loadedFileKey, requestedKey))
                        {
                            return loaded;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceWarning(
                            $"SvgRenderer load '{path}': {SvgRenderer.DescribeFailure(ex)}"
                        );
                    }
                }
            }
            // Containment boundary: nothing may escape an AssemblyResolve handler, or a recoverable
            // bind failure becomes a hard failure at whatever triggered the bind. Trace, not log4net,
            // for the re-entrancy reason given above.
            catch (Exception ex)
            {
                Trace.TraceWarning(
                    $"SvgRenderer resolve '{requested.Name}': {SvgRenderer.DescribeFailure(ex)}"
                );
            }
            finally
            {
                _resolving.Remove(requested.Name);
            }

            return null;
        }
    }
}
