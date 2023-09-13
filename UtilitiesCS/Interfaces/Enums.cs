namespace UtilitiesCS
{
    public static class Enums
    {
        public enum ToggleState { Off = 0, On = 1 }

        /// <summary>
        /// Enumeration that controls whether keypair matches are found by Subject or by Folder
        /// <list type="number">
        ///     <item>
        ///         <term>Subject</term>
        ///         <description>Find matches using a standardized email subject</description>
        ///     </item>
        ///     <item>
        ///         <term>Folder</term>
        ///         <description>Find matches using a folder path</description>
        ///     </item>
        /// </list>
        /// </summary>
        public enum FindBy
        {
            Subject = 1,
            Folder = 2
        }

    }
}