using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public static class AutoFile2
    {
        public static IList<string> AutoFindPeople(object objItem,
                                                   IScoDictionaryNew<string, string> ppl_dict,
                                                   string emailRootFolder,
                                                   IScoDictionary<string, string> dictRemap,
                                                   string userAddress,
                                                   bool blNotifyMissing = true,
                                                   bool blExcludeFlagged = true)
        {
            MailItem OlMail;
            List<string> emailAddressList;
            IList<string> peopleList = new List<string>();
            string strMissing = "";
            string strTmp;

            if (objItem is MailItem)
            {
                OlMail = (MailItem)objItem;
                if (MailResolution.IsMailUnReadable(OlMail) == false)
                {
                    emailAddressList = CaptureEmailAddressesModule2.GetEmailAddresses(OlMail, emailRootFolder, dictRemap, userAddress);
                    for (int i = emailAddressList.Count - 1; i >= 0; i -= 1)
                    {
                        strTmp = emailAddressList[i];
                        if (ppl_dict.ContainsKey(strTmp))
                        {

                            if (blExcludeFlagged)
                            {
                                if (!Category_IsAlreadySelected(objItem, ppl_dict[strTmp]))
                                {
                                    peopleList.Add(ppl_dict[strTmp]);
                                }
                            }
                            else
                            {
                                peopleList.Add(ppl_dict[strTmp]);
                            }
                        }
                        else
                        {
                            strMissing = strMissing + "; " + strTmp;
                        }
                    }
                    if (strMissing.Length > 0 & blNotifyMissing)
                    {
                        strMissing = strMissing.Substring(2);
                        MessageBox.Show("Recipients not in list of people: " + strMissing);
                    }
                }
            }

            return peopleList;
        }

        private static bool Category_IsAlreadySelected(dynamic objItem, string strCat)
        {

            int i;
            bool blSelected;

            blSelected = false;
            string[] varCats = (objItem.Categories as string).Split(',', trim: true);
            var loopTo = varCats.Length;
            for (i = 0; i < loopTo; i++)
            {
                if ((strCat ?? "") == (varCats[i] ?? ""))
                {
                    blSelected = true;
                }
            }
            return blSelected;
        }
    }
}
