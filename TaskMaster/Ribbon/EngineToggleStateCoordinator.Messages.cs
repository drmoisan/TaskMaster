using System;
using System.Globalization;

namespace TaskMaster
{
    internal sealed partial class EngineToggleStateCoordinator
    {
        /// <summary>
        /// Rendered in place of an engine key when the caller supplied null or empty, so a message
        /// is never ambiguous about which key was seen.
        /// </summary>
        private const string NullEngineNameToken = "(null)";

        /// <summary>
        /// Renders an engine key for inclusion in a message, so a null key is never ambiguous.
        /// </summary>
        private static string RenderEngineName(string engineName)
        {
            return string.IsNullOrEmpty(engineName) ? NullEngineNameToken : engineName;
        }

        /// <summary>
        /// The message emitted when a toggle click is refused because the engines are unavailable.
        /// </summary>
        private static string BuildUnavailableMessage(string engineName)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "The engine '{0}' is not available yet, so its enable/disable setting cannot be "
                    + "changed. Please try again once initialization completes.",
                RenderEngineName(engineName)
            );
        }

        /// <summary>
        /// The message logged when the toggle path faults.
        /// </summary>
        private static string BuildToggleFailedMessage(string engineName)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "Toggling the enable/disable setting for engine '{0}' failed.",
                RenderEngineName(engineName)
            );
        }

        /// <summary>
        /// The message logged when the state prime faults.
        /// </summary>
        private static string BuildPrimeFailedMessage(string engineName)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "Reading the activation state for engine '{0}' failed; its toggle continues to "
                    + "report unchecked. Further failures of this kind for this engine are not "
                    + "logged again.",
                RenderEngineName(engineName)
            );
        }

        /// <summary>
        /// The message carried by the <see cref="ArgumentException"/> for an unmapped engine key.
        /// </summary>
        private static string BuildUnmappedKeyMessage(string engineName)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "The engine key '{0}' has no toggle checkbox in EngineToggleCatalog.",
                RenderEngineName(engineName)
            );
        }
    }
}
