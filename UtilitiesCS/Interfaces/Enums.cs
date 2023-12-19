namespace UtilitiesCS
{
    public static class Enums
    {
        [System.Flags]
        public enum ToggleState { Off = 0, On = 1, Force = 2 }

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

        public enum Corpus
        {
            Negative = 0,
            Positive = 1
        }
        
        public enum WorkerState
        {
            Idle = 0,
            Working = 1,
            Paused = 2,
            Stopped = 3,
            Completed = 4
        }

        public enum LoadState 
        { 
            NotLoaded = 0, 
            Loading = 1, 
            Loaded = 2 
        }
    }
}