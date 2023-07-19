using Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{

    public static class MailResolution_ToRemove
    {
        public static bool IsMailUnReadable(this MailItem item)
        {
            return item.MessageClass == "IPM.Note.SMIME" | item.MessageClass == "IPM.Note.Secure" | item.MessageClass == "IPM.Note.Secure.Sign" | item.MessageClass == "IPM.Outlook.Recall";
        }

        public static MailItem TryResolveMailItemDep(object objItem)
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