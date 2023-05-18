using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;


namespace UtilitiesVB
{
    public static class ExplorerActions
    {
        public static object GetCurrentItem(Application OlApp)
        {
            if (OlApp == null) { throw new ArgumentNullException(nameof(OlApp)); }
            else if (OlApp.ActiveWindow() is Explorer) { return Readable(OlApp.ActiveExplorer().Selection[0]); }
            else if (OlApp.ActiveWindow() is Inspector) { return Readable(OlApp.ActiveInspector().CurrentItem); }
            else { return null; }
        }

        internal static object Readable(object item)
        {
            if ((item is MailItem) && ((MailItem)item).IsMailUnReadable())
            { return null; }
            return item;
        }

    }
}