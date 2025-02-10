
using System;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    [Serializable]
    public class MinedMailInfo : ICloneable
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
            Subject = info.Subject;
            Actionable = info.Actionable;
        }

        private string _categories;
        public string Categories { get => _categories; set => _categories = value; }

        private string[] _tokens;
        public string[] Tokens { get => _tokens; set => _tokens = value; }

        private IFolderInfo _folderInfo;
        public IFolderInfo FolderInfo { get => _folderInfo; set => _folderInfo = value; }

        private IRecipientInfo[] _toRecipients;
        public IRecipientInfo[] ToRecipients { get => _toRecipients; set => _toRecipients = value; }

        private IRecipientInfo[] _ccRecipients;
        public IRecipientInfo[] CcRecipients { get => _ccRecipients; set => _ccRecipients = value; }

        private IRecipientInfo _sender;
        public IRecipientInfo Sender { get => _sender; set => _sender = value; }

        private string _conversationId;
        public string ConversationId { get => _conversationId; set => _conversationId = value; }

        private string _entryID;
        public string EntryId { get => _entryID; set => _entryID = value; }

        private string _storeID;
        public string StoreId { get => _storeID; set => _storeID = value; }

        private string _subject;
        public string Subject { get => _subject; set => _subject = value; }

        public string Actionable { get; set; }

        internal string GroupingKey { get; set; }

        #region IClonable

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public MinedMailInfo DeepCopy()
        {
            var deepCopy = new MinedMailInfo
            {
                Categories = this.Categories,
                Tokens = (string[])this.Tokens?.Clone(),
                FolderInfo = this.FolderInfo, // Assuming IFolderInfo is immutable or has its own deep copy method
                ToRecipients = (IRecipientInfo[])this.ToRecipients?.Clone(),
                CcRecipients = (IRecipientInfo[])this.CcRecipients?.Clone(),
                Sender = this.Sender, // Assuming IRecipientInfo is immutable or has its own deep copy method
                ConversationId = this.ConversationId,
                EntryId = this.EntryId,
                StoreId = this.StoreId,
                Subject = this.Subject,
                Actionable = this.Actionable,
                GroupingKey = this.GroupingKey
            };

            return deepCopy;

            #endregion IClonable
        }
    }
}
