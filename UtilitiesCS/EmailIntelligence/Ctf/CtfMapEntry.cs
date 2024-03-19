
namespace UtilitiesCS
{
    public class CtfMapEntry 
    {
        public CtfMapEntry() { }
        public CtfMapEntry(string emailFolder, string conversationID, int emailCount)
        {
            _emailFolder = emailFolder;
            _conversationID = conversationID;
            _emailCount = emailCount;
        }

        private string _emailFolder;
        private string _conversationID;
        private int _emailCount;

        public string EmailFolder { get => _emailFolder; set => _emailFolder = value;}
        
        public string ConversationID { get => _conversationID; set => _conversationID = value;}
        
        public int EmailCount { get => _emailCount; set => _emailCount = value; }
        
    }
}