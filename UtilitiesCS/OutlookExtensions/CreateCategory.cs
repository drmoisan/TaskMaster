using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;


namespace UtilitiesCS
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
                    strTemp = newCatName.Length > prefix.Value.Length ? 
                        newCatName.Substring(0,prefix.Value.Length) != prefix.Value ? 
                            prefix.Value + newCatName : 
                            newCatName : 
                        prefix.Value + newCatName;
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
                        MessageBox.Show($"Color category {strTemp} already exists. Cannot add a duplicate.");
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
                MessageBox.Show($"Error: Parameter {nameof(newCatName)} must have a value to create a category.");
            }

            return objCategory;

        }
    }
}