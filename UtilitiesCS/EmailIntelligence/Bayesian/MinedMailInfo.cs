
using System;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    [Serializable]
    public class MinedMailInfo
    {
        public MinedMailInfo() { }
        public MinedMailInfo(IItemInfo info)
        {
            Categories = info.Categories;
            Tokens = info.Tokens;
            FolderPath = info.FolderName;
            ToRecipients = info.ToRecipients;
            CcRecipients = info.CcRecipients;
            Sender = info.Sender;
            ConversationId = info.ConversationID;
        }
        public MinedMailInfo(string folderPath, string[] tokens)
        {
            Tokens = tokens;
            FolderPath = folderPath;
        }

        private string _categories;
        public string Categories { get => _categories; set => _categories = value; }

        private string[] _tokens;
        public string[] Tokens { get => _tokens; set => _tokens = value; }

        public string _folderPath;
        public string FolderPath { get => _folderPath; set => _folderPath = value; }

        private RecipientInfo[] _toRecipients;
        public RecipientInfo[] ToRecipients { get => _toRecipients; set => _toRecipients = value; }

        private RecipientInfo[] _ccRecipients;
        public RecipientInfo[] CcRecipients { get => _ccRecipients; set => _ccRecipients = value; }

        private RecipientInfo _sender;
        public RecipientInfo Sender { get => _sender; set => _sender = value; }

        private string _conversationId;
        public string ConversationId { get => _conversationId; set => _conversationId = value; }
    }

}
