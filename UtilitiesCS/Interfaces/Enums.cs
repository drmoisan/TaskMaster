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

        public enum TriState
        {
            Undetermined = -1,
            False = 0,
            True = 1
        }

        [Flags]
        public enum FlagsToSet
        {
            None = 0,
            Context = 1,
            People = 2,
            Projects = 4,
            Program = 8,
            Topics = 16,
            Priority = 32,
            Taskname = 64,
            Worktime = 128,
            Today = 256,
            Bullpin = 512,
            Kbf = 1024,
            DueDate = 2048,
            Reminder = 4096,
            All = 8191
        }

        public enum NotFoundEnum
        {
            Skip = 0,
            Create = 1,
            Ask = 2,
            Throw = 3
        }

    }
}