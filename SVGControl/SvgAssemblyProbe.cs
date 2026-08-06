#nullable enable
using System;
using System.Collections.Generic;
using System.IO;

namespace SVGControl
{
    /// <summary>
    /// Pure path-string and public-key-token helpers used by the SVGControl assembly-resolve
    /// fallback. Separated from SvgRenderer because these concern assembly probing and identity
    /// matching rather than SVG rendering, and because they carry no renderer state, which makes
    /// them directly unit-testable.
    /// </summary>
    internal static class SvgAssemblyProbe
    {
        // Converts a file:// code base to a directory, returning null for null, empty, whitespace-only,
        // or unparsable input. Never raises, so it is safe inside an AssemblyResolve handler.
        internal static string? TryGetDirectoryFromCodeBase(string? codeBase)
        {
            if (
                codeBase == null
                || !Uri.TryCreate(codeBase.Trim(), UriKind.Absolute, out Uri parsed)
                || !parsed.IsFile
                || parsed.LocalPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0
            )
            {
                return null;
            }
            return Path.GetDirectoryName(parsed.LocalPath);
        }

        // Ordered, case-insensitively de-duplicated directories to probe for a failed bind: the
        // assembly's own directory, then its code-base directory, then the AppDomain base directory.
        // Unusable entries are dropped, first occurrence wins, and the method never raises.
        internal static IReadOnlyList<string> GetProbeDirectories(
            string? assemblyLocation,
            string? assemblyCodeBase,
            string? baseDirectory
        )
        {
            // An assembly loaded from a byte array reports an empty Location, so that candidate is
            // skipped rather than being resolved against the current directory.
            string? location = assemblyLocation?.Trim();
            string?[] candidates =
            {
                location != null
                && location.Length > 0
                && location.IndexOfAny(Path.GetInvalidPathChars()) < 0
                    ? Path.GetDirectoryName(location)
                    : null,
                TryGetDirectoryFromCodeBase(assemblyCodeBase),
                baseDirectory != null && baseDirectory.IndexOfAny(Path.GetInvalidPathChars()) < 0
                    ? baseDirectory
                    : null,
            };
            // Candidates are null-checked explicitly rather than with IsNullOrWhiteSpace: net481 has
            // no NotNullWhen post-conditions, so that call would not narrow state and Add emits CS8604.
            var ordered = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string? candidate in candidates)
            {
                if (candidate != null && candidate.Trim().Length > 0 && seen.Add(candidate))
                {
                    ordered.Add(candidate);
                }
            }
            return ordered;
        }

        // Compares two public key tokens for equality, treating null and zero-length as equivalent
        // (both denote an unsigned assembly). Never raises, so it is safe inside an AssemblyResolve
        // handler.
        internal static bool PublicKeyTokensEqual(byte[]? a, byte[]? b)
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
