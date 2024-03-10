using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookExtensions
{
    public static class OlToDoTable
    {  
        public static Outlook.Table GetToDoTable(this Outlook.Store store) 
        {
            MAPIFolder folder = null;
            try
            {
                folder = store.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
            }
            catch (System.Exception)
            {
                return null;
            }
            Outlook.Table table = folder.GetTable();
            table.Columns.RemoveAll();
            table.Columns.Add(OlTableExtensions.SchemaToDoID);
            table.Columns.Add("Categories");
            // table.EnumerateTable();
            return table;
        }

        
    }
}
