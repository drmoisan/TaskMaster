#nullable enable
using System;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS.OutlookObjects.Fields;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.OutlookExtensions
{
    public static class OlToDoTable
    {
        public static Outlook.Table? GetToDoTable(this Outlook.Store store)
        {
            MAPIFolder? folder = null;
            try
            {
                folder = store.GetDefaultFolder(OlDefaultFolders.olFolderToDo);
            }
            catch (System.Exception)
            {
                return null;
            }

            EnsureToDoIdExists(folder);

            Outlook.Table table = folder.GetTable();
            table.Columns.RemoveAll();
            table.Columns.Add(MAPIFields.Schemas.ToDoID);
            table.Columns.Add("Categories");
            // table.EnumerateTable();
            return table;
        }

        private static void EnsureToDoIdExists(MAPIFolder folder)
        {
            EnsureFolderField(folder);
            EnsureItemValues(folder);
        }

        private static void EnsureFolderField(MAPIFolder folder)
        {
            const string fieldName = "ToDoID";

            try
            {
                var userDefinedProperties = folder.UserDefinedProperties;
                UserDefinedProperty? field = null;

                try
                {
                    field = userDefinedProperties[fieldName];
                }
                catch
                {
                    field = null;
                }

                if (field == null)
                {
                    userDefinedProperties.Add(
                        fieldName,
                        OlUserPropertyType.olText,
                        Type.Missing,
                        Type.Missing
                    );
                }
            }
            catch
            {
                // Some providers do not allow adding folder-level fields.
            }
        }

        private static void EnsureItemValues(MAPIFolder folder)
        {
            Items? items = null;
            try
            {
                items = folder.Items;
                int itemCount = items.Count;
                for (int i = 1; i <= itemCount; i++)
                {
                    object? itemObj = null;
                    try
                    {
                        itemObj = items[i];
                        if (itemObj == null)
                        {
                            continue;
                        }

                        dynamic item = itemObj;
                        PropertyAccessor accessor = item.PropertyAccessor;
                        string? entryId = item.EntryID as string;

                        if (string.IsNullOrWhiteSpace(entryId))
                        {
                            continue;
                        }

                        string? value = null;
                        try
                        {
                            value = accessor.GetProperty(MAPIFields.Schemas.ToDoID) as string;
                        }
                        catch
                        {
                            value = null;
                        }

                        if (string.IsNullOrWhiteSpace(value))
                        {
                            accessor.SetProperty(MAPIFields.Schemas.ToDoID, entryId);
                            item.Save();
                        }
                    }
                    catch
                    {
                        // Skip unreadable/unwritable items.
                    }
                }
            }
            catch
            {
                // Ignore provider/folder limitations.
            }
        }
    }
}
