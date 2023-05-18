using System.Diagnostics;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;

namespace UtilitiesVB
{

    public static class CreateCategoryModule
    {
        public static Category CreateCategory(NameSpace OlNS, IPrefix prefix, string newCatName)
        {

            Category objCategory = null;
            // Dim OlColor As OlCategoryColor
            string strTemp;

            if (!string.IsNullOrEmpty(newCatName))
            {
                if (!string.IsNullOrEmpty(prefix.Value))
                {
                    strTemp = Strings.Len(newCatName) > Strings.Len(prefix) ? (Strings.Left(newCatName, Strings.Len(prefix.Value)) ?? "") != (prefix.Value ?? "") ? prefix.Value + newCatName : newCatName : prefix.Value + newCatName;
                }
                else
                {
                    strTemp = newCatName;
                }


                bool exists = false;
                foreach (Category currentObjCategory in OlNS.Categories)
                {
                    objCategory = currentObjCategory;
                    if ((objCategory.Name ?? "") == (strTemp ?? ""))
                    {
                        exists = true;
                        var unused1 = Interaction.MsgBox("Color category " + strTemp + " already exists. Cannot add a duplicate.");
                        return objCategory;
                    }
                }

                if (!exists)
                {
                    try
                    {
                        objCategory = OlNS.Categories.Add(strTemp, prefix.Color, OlCategoryShortcutKey.olCategoryShortcutKeyNone);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace.ToString());
                    }
                }
            }
            else
            {
                var unused = Interaction.MsgBox("Error: Parameter " + nameof(newCatName) + " must have a value to create a category.");
            }

            return objCategory;

        }
    }
}