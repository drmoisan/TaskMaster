
namespace CleanProjectToTest
{
    internal class CTF_Incidence2
    {
        private string _emailConversationID;
        private int _folderCount;
        private string[] _emailFolder;
        private int[] _emailConversationCount;

        public CTF_Incidence2()
        {
            _emailFolder = new string[My.MySettingsProperty.Settings.MaxFolders_ConvID + 1];
            _emailConversationCount = new int[My.MySettingsProperty.Settings.MaxFolders_ConvID + 1];
        }

        public string Email_Conversation_ID
        {
            get
            {
                return _emailConversationID;
            }
            set
            {
                _emailConversationID = value;
            }
        }

        public int Folder_Count
        {
            get
            {
                return _folderCount;
            }
            set
            {
                _folderCount = value;
            }
        }

        public string[] Email_Folder
        {
            get
            {
                return _emailFolder;
            }
            set
            {
                _emailFolder = value;
            }
        }

        public int[] Email_Conversation_Count
        {
            get
            {
                return _emailConversationCount;
            }
            set
            {
                _emailConversationCount = value;
            }
        }
    }
}