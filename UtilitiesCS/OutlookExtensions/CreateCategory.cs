using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;


namespace UtilitiesCS
{

    public static class CreateCategoryModule
    {
        public static Category CreateCategory(this NameSpace olNS, IPrefix prefix, string newCatName)
        {
            Category objCategory = null;
            
            string strTemp;

            if (!string.IsNullOrEmpty(newCatName))
            {
                if (!string.IsNullOrEmpty(prefix.Value))
                {
                    if (newCatName.Length > prefix.Value.Length)
                    {
                        if (newCatName.Substring(0, prefix.Value.Length) != prefix.Value)
                        {
                            strTemp = prefix.Value + newCatName;
                        }
                        else
                        {
                            strTemp = newCatName;
                        }
                    }
                    else
                    {
                        strTemp = prefix.Value + newCatName;
                    }
                    //strTemp = newCatName.Length > prefix.Value.Length ? 
                    //    newCatName.Substring(0,prefix.Value.Length) != prefix.Value ? 
                    //        prefix.Value + newCatName : 
                    //        newCatName : 
                    //    prefix.Value + newCatName;
                }
                else
                {
                    strTemp = newCatName;
                }

                bool exists = false;
                foreach (Category currentObjCategory in olNS.Categories)
                {
                    objCategory = currentObjCategory;
                    if (objCategory.Name == strTemp)
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
                        objCategory = olNS.Categories.Add(strTemp, prefix.Color, OlCategoryShortcutKey.olCategoryShortcutKeyNone);
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