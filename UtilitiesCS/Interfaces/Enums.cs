using System;

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

        public enum SerializationOptions
        {
            AskUserOnError = 1,
            WriteNew = 2,
        }

        public enum Justification
        {
            Left = 0,
            Center = 1,
            Right = 2,
            Justified = 3
        }

        [Flags]
        public enum DictionaryResult
        { 
            KeyExists = 1,          // 001
            KeysChanged = 2,        // 010
            ValueChanged = 4,       // 100
            // Key removed would be    X10
            // Key added would be      011
            // Value updated would be  101
        }
        
    }
}