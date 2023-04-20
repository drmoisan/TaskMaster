using Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static class MailResolution
    {
        public static bool IsMailUnReadable(MailItem item)
        {
            return item.MessageClass == "IPM.Note.SMIME" | item.MessageClass == "IPM.Note.Secure" | item.MessageClass == "IPM.Note.Secure.Sign" | item.MessageClass == "IPM.Outlook.Recall";
        }

        public static MailItem TryResolveMailItem(object objItem)
        {
            MailItem OlMail = null;
            if (objItem is MailItem)
            {
                OlMail = (MailItem)objItem;
                if (IsMailUnReadable(OlMail) == true)
                {
                    OlMail = null;
                }
            }
            return OlMail;
        }

    }
}