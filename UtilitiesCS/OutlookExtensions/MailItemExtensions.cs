using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.OutlookExtensions
{
    public static class MailItemExtensions
    {
        public static byte[] ToMIME(this Microsoft.Office.Interop.Outlook.MailItem mailItem)
        {
            byte[] mimeContent = mailItem.PropertyAccessor.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x10130102") as byte[];
            return mimeContent;
        }
    }
}
