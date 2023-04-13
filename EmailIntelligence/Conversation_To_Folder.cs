
namespace EmailIntelligence
{
    public class Conversation_To_Folder 
    {
        private string _email_Folder;
        private string _email_Conversation_ID;
        private int _email_Conversation_Count;

        public string Email_Folder
        {
            get
            {
                return _email_Folder;
            }
            set
            {
                _email_Folder = value;
            }
        }

        public string Email_Conversation_ID
        {
            get
            {
                return _email_Conversation_ID;
            }
            set
            {
                _email_Conversation_ID = value;
            }
        }

        public int Email_Conversation_Count
        {
            get
            {
                return _email_Conversation_Count;
            }
            set
            {
                _email_Conversation_Count = value;
            }
        }
    }
}