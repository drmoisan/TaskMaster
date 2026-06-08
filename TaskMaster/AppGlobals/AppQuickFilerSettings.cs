using TaskMaster.Properties;
using UtilitiesCS;

namespace TaskMaster
{
    public class AppQuickFilerSettings : IAppQuickFilerSettings
    {
        public bool MoveEntireConversation
        {
            get => Settings.Default.MoveEntireConversations;
            internal set
            {
                Settings.Default.MoveEntireConversations = value;
                Settings.Default.Save();
            }
        }

        public bool SaveAttachments
        {
            get => Settings.Default.SaveAttachments;
            internal set
            {
                Settings.Default.SaveAttachments = value;
                Settings.Default.Save();
            }
        }

        public bool SavePictures
        {
            get => Settings.Default.SavePictures;
            internal set
            {
                Settings.Default.SavePictures = value;
                Settings.Default.Save();
            }
        }

        public bool SaveEmailCopy
        {
            get => Settings.Default.SaveEmailCopy;
            internal set
            {
                Settings.Default.SaveEmailCopy = value;
                Settings.Default.Save();
            }
        }

        public bool HighConfidenceModeEnabled
        {
            get => Settings.Default.HighConfidenceModeEnabled;
            internal set
            {
                Settings.Default.HighConfidenceModeEnabled = value;
                Settings.Default.Save();
            }
        }

        public double HighConfidenceThreshold
        {
            get => Settings.Default.HighConfidenceThreshold;
            internal set
            {
                Settings.Default.HighConfidenceThreshold = value;
                Settings.Default.Save();
            }
        }
    }
}
