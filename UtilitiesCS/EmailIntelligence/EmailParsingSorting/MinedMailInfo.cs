
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
            FolderInfo = info.FolderInfo;
            ToRecipients = info.ToRecipients;
            CcRecipients = info.CcRecipients;
            Sender = info.Sender;
            ConversationId = info.ConversationID;
            EntryId = info.EntryId;
            StoreId = info.StoreId;
        }

        private string _categories;
        public string Categories { get => _categories; set => _categories = value; }

        private string[] _tokens;
        public string[] Tokens { get => _tokens; set => _tokens = value; }

        private IFolderInfo _folderInfo;
        public IFolderInfo FolderInfo { get => _folderInfo; set => _folderInfo = value; }

        private RecipientInfo[] _toRecipients;
        public RecipientInfo[] ToRecipients { get => _toRecipients; set => _toRecipients = value; }

        private RecipientInfo[] _ccRecipients;
        public RecipientInfo[] CcRecipients { get => _ccRecipients; set => _ccRecipients = value; }

        private RecipientInfo _sender;
        public RecipientInfo Sender { get => _sender; set => _sender = value; }

        private string _conversationId;
        public string ConversationId { get => _conversationId; set => _conversationId = value; }

        private string _entryID;
        public string EntryId { get => _entryID; set => _entryID = value; }

        private string _storeID;
        public string StoreId { get => _storeID; set => _storeID = value; }
    }

}
