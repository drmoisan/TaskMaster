#nullable enable
using System;
using Microsoft.Win32;

namespace UtilitiesCS
{
    /// <summary>
    /// Detects the Windows system application theme (Dark or Light) by reading the registry.
    /// </summary>
    public static class SystemThemeDetector
    {
        private const string RegistryKeyPath =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private const string RegistryValueName = "AppsUseLightTheme";

        /// <summary>
        /// Returns true if the Windows system is configured for Dark Mode, false for Light Mode.
        /// When the registry key is missing or unreadable, returns false (defaults to Light Mode).
        /// </summary>
        public static bool IsSystemDarkMode()
        {
            TryGetIsSystemDarkMode(out bool isDarkMode);
            return isDarkMode;
        }

        /// <summary>
        /// Attempts to read the Windows registry to detect the active application theme.
        /// </summary>
        /// <param name="isDarkMode">
        /// Set to true when AppsUseLightTheme is 0 (Dark Mode is active);
        /// set to false when the value is 1 or the key is absent.
        /// </param>
        /// <returns>
        /// true when the registry key and value were read successfully;
        /// false when the key is missing, the value is absent, or an exception occurs.
        /// </returns>
        public static bool TryGetIsSystemDarkMode(out bool isDarkMode)
        {
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key == null)
                    {
                        isDarkMode = false;
                        return false;
                    }

                    object? value = key.GetValue(RegistryValueName);
                    if (value is int intValue)
                    {
                        // AppsUseLightTheme = 0 means Dark Mode is active
                        isDarkMode = intValue == 0;
                        return true;
                    }

                    isDarkMode = false;
                    return false;
                }
            }
            catch (Exception)
            {
                // Broad catch is justified: this is a TryGet defensive helper.
                // Registry reads can fail with SecurityException, UnauthorizedAccessException,
                // or IOException on locked-down environments; we signal failure via return value.
                isDarkMode = false;
                return false;
            }
        }
    }
}
