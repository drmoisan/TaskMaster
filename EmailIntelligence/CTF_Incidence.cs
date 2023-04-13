
namespace EmailIntelligence
{
    public class CTF_Incidence 
    {
        private string _emailConversationID;
        private int _folderCount;
        private string[] _emailFolder;
        private int[] _emailConversationCount;
        private int _maxFoldersPerConv;

        public CTF_Incidence()
        {
            _maxFoldersPerConv = Properties.Settings.Default.MaxFoldersTrackedPerConversation;
            _emailFolder = new string[_maxFoldersPerConv + 1];
            _emailConversationCount = new int[_maxFoldersPerConv + 1];
        }

        public CTF_Incidence(int MaxFoldersPerConv)
        {
            _maxFoldersPerConv = MaxFoldersPerConv;
            _emailFolder = new string[_maxFoldersPerConv + 1];
            _emailConversationCount = new int[_maxFoldersPerConv + 1];
        }

        public int MaxFoldersPerConv { get => _maxFoldersPerConv; set => _maxFoldersPerConv = value; }

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