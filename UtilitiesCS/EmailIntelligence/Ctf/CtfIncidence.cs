using System;
using System.Collections.Generic;


namespace UtilitiesCS
{
    [Obsolete("Use CtfMapEntry Instead")]
    public class CtfIncidence 
    {
        public CtfIncidence()
        {
            _emailFolders = new();
            _emailCounts = new();
        }

        public CtfIncidence(int MaxFoldersPerConv)
        {
            _maxFoldersPerConv = MaxFoldersPerConv;
            _emailFolders = new();
            _emailCounts = new();
        }

        public CtfIncidence(string emailConversationID,
                            int folderCount,
                            List<string> emailFolder,
                            List<int> emailConversationCount)
        {
            EmailConversationID = emailConversationID;
            FolderCount = folderCount;
            EmailFolders = emailFolder;
            EmailCounts = emailConversationCount;
        }

        private string _emailConversationID;
        private int _folderCount;
        private List<string> _emailFolders;
        private List<int> _emailCounts;
        private int _maxFoldersPerConv = 3;

        public int MaxFoldersPerConv { get => _maxFoldersPerConv; set => _maxFoldersPerConv = value; }

        public string EmailConversationID { get => _emailConversationID; set => _emailConversationID = value; }

        public int FolderCount { get => _folderCount; set => _folderCount = value;}
        
        public List<string> EmailFolders { get => _emailFolders; set => _emailFolders = value;}
        
        public List<int> EmailCounts { get => _emailCounts; set => _emailCounts = value; }
        
    }
}