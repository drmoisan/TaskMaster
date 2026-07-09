using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace TaskMaster.Logging
{
    /// <summary>
    /// Filesystem seam for <see cref="LogDirectoryInitializer"/>. Abstracts the two directory
    /// operations the initializer needs so the pure resolve/ensure logic can be unit-tested with a
    /// Moq stub, without touching the real filesystem, a live log4net appender, or Outlook/COM.
    /// </summary>
    public interface ILogDirectoryFileSystem
    {
        /// <summary>Returns <c>true</c> when the directory at <paramref name="path"/> exists.</summary>
        bool DirectoryExists(string path);

        /// <summary>Creates the directory at <paramref name="path"/> (and any missing parents).</summary>
        void CreateDirectory(string path);
    }

    /// <summary>
    /// Production <see cref="ILogDirectoryFileSystem"/> backed by <see cref="System.IO.Directory"/>.
    /// This is the thin, host-bound I/O wrapper; it is excluded from coverage because it cannot be
    /// exercised without touching the real filesystem (temporary-file use in tests is prohibited by
    /// repository policy). All decision logic lives in the coverable <see cref="LogDirectoryInitializer"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class LogDirectoryFileSystem : ILogDirectoryFileSystem
    {
        /// <inheritdoc />
        public bool DirectoryExists(string path) => Directory.Exists(path);

        /// <inheritdoc />
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Pure, host-neutral log-directory resolution and creation. Extracted from the add-in startup
    /// path (issue #208) so the directory that log4net's file appenders target is guaranteed to
    /// exist before the assembly-level <c>XmlConfigurator</c> attribute configures the appenders and
    /// opens their files. Isolating this logic behind an <see cref="ILogDirectoryFileSystem"/> seam
    /// keeps it testable without a live appender or a live Outlook process.
    /// </summary>
    public sealed class LogDirectoryInitializer
    {
        private readonly ILogDirectoryFileSystem _fileSystem;

        /// <summary>
        /// Creates an initializer that uses <paramref name="fileSystem"/> for directory operations.
        /// </summary>
        /// <param name="fileSystem">The filesystem seam. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileSystem"/> is null.</exception>
        public LogDirectoryInitializer(ILogDirectoryFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        /// Resolves the absolute directory that must exist for a configured log path. Mirrors how
        /// log4net's <c>FileAppender</c> resolves a relative <c>file</c> value: relative paths are
        /// combined with <paramref name="baseDirectory"/> and normalized with
        /// <see cref="Path.GetFullPath(string)"/>; rooted paths are normalized as-is. The directory
        /// portion of the resulting path is returned (log4net appends the rolling filename to the
        /// configured value, so a trailing separator denotes a directory).
        /// </summary>
        /// <param name="baseDirectory">
        /// The base directory relative paths are resolved against (production passes the runtime
        /// working directory). Ignored when <paramref name="logPath"/> is rooted.
        /// </param>
        /// <param name="logPath">The configured log path (for example <c>logs\</c>). Required.</param>
        /// <returns>The absolute directory path to ensure.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="logPath"/> is null/blank.</exception>
        public static string ResolveLogDirectory(string baseDirectory, string logPath)
        {
            if (string.IsNullOrWhiteSpace(logPath))
            {
                throw new ArgumentException("Log path must be a non-empty value.", nameof(logPath));
            }

            var trimmed = logPath.Trim();
            var combined = Path.IsPathRooted(trimmed)
                ? trimmed
                : Path.Combine(baseDirectory ?? string.Empty, trimmed);

            var full = Path.GetFullPath(combined);

            // If the configured value carried a trailing separator (a directory prefix), GetFullPath
            // preserves it; strip it so the returned value is the directory itself. If the value had
            // no trailing separator, treat the resolved path as the directory to ensure.
            var trimmedFull = full.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            );

            return trimmedFull.Length == 0 ? full : trimmedFull;
        }

        /// <summary>
        /// Ensures <paramref name="logDirectory"/> exists, creating it if missing. Fails fast on an
        /// invalid path and propagates any I/O failure (for example an unwritable path) to the
        /// caller so the failure is explicit rather than silently swallowed.
        /// </summary>
        /// <param name="logDirectory">The absolute directory to ensure. Required.</param>
        /// <returns><c>true</c> if the directory was created; <c>false</c> if it already existed.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="logDirectory"/> is null/blank.</exception>
        public bool EnsureLogDirectory(string logDirectory)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                throw new ArgumentException(
                    "Log directory must be a non-empty value.",
                    nameof(logDirectory)
                );
            }

            if (_fileSystem.DirectoryExists(logDirectory))
            {
                return false;
            }

            _fileSystem.CreateDirectory(logDirectory);
            return true;
        }

        /// <summary>
        /// Convenience combination of <see cref="ResolveLogDirectory(string, string)"/> and
        /// <see cref="EnsureLogDirectory(string)"/>: resolves the absolute directory for the
        /// configured log path and ensures it exists.
        /// </summary>
        /// <param name="baseDirectory">The base directory relative paths resolve against.</param>
        /// <param name="logPath">The configured log path (for example <c>logs\</c>). Required.</param>
        /// <returns><c>true</c> if the directory was created; <c>false</c> if it already existed.</returns>
        public bool EnsureLogDirectoryForPath(string baseDirectory, string logPath)
        {
            var directory = ResolveLogDirectory(baseDirectory, logPath);
            return EnsureLogDirectory(directory);
        }
    }
}
