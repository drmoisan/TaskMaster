using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Outlook;
using Tags;
using UtilitiesCS;
using System.Globalization;
using System.Windows.Forms;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;

namespace ToDoModel
{

    public static class AutoFile
    {
        private const int NumberOfFields = 13;

        public static string[] CaptureEmailRecipients(MailItem OlMail)
        {
            string[] strAry;
            string StrSMTPAddress;
            Recipients OlRecipients;
            string StrRecipientName;
            PropertyAccessor OlPA;

            int i;


            strAry = new string[14];

            const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

            OlRecipients = OlMail.Recipients;

            foreach (Recipient OlRecipient in OlRecipients)
            {
                OlPA = OlRecipient.PropertyAccessor;
                try
                {
                    StrRecipientName = OlRecipient.Name;
                }
                catch
                {
                    StrRecipientName = "";
                }

                try
                {
                    StrSMTPAddress = (string)(OlPA.GetProperty(PR_SMTP_ADDRESS));
                }
                catch
                {
                    try
                    {
                        StrSMTPAddress = OlRecipient.Address;
                    }
                    catch
                    {
                        StrSMTPAddress = StrRecipientName;
                    }
                }



                if (OlRecipient.Type == (int)OlMailRecipientType.olTo)
                {
                    strAry[1] = strAry[1] + "; " + StrRecipientName;
                    strAry[2] = strAry[2] + "; " + StrSMTPAddress;
                }
                else if (OlRecipient.Type == (int)OlMailRecipientType.olCC)
                {
                    strAry[3] = strAry[3] + "; " + StrRecipientName;
                    strAry[4] = strAry[4] + "; " + StrSMTPAddress;
                }
                
            }

            for (i = 1; i <= 4; i++)
            {
                if (strAry[i].Length > 2)
                    strAry[i] = strAry[i].Substring(2);
            }

            if (OlMail.Sender.Type == "EX")
            {

                OlPA = OlMail.Sender.PropertyAccessor;

                // On Error Resume Next
                try
                {
                    strAry[5] = OlMail.Sender.Name;
                }
                catch
                {
                    strAry[5] = "";
                }

                try
                {
                    strAry[6] = (string)(OlPA.GetProperty(PR_SMTP_ADDRESS));
                }
                catch
                {
                    strAry[6] = strAry[5];
                }
            }

            else
            {
                strAry[5] = OlMail.SenderEmailAddress;
                strAry[6] = OlMail.SenderEmailAddress;
            }

            return strAry;

        }

        public static bool AreConversationsGrouped(Explorer ActiveExplorer)
        {
            bool AreConversationsGroupedRet = default;
            bool blTemp;
            if (ActiveExplorer.CommandBars.GetPressedMso("ShowInConversations"))
            {
                blTemp = true;
            }
            else
            {
                blTemp = false;
            }

            AreConversationsGroupedRet = blTemp;
            return AreConversationsGroupedRet;
        }


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
                    emailAddressList = CaptureEmailAddressesModule.GetEmailAddresses(OlMail, emailRootFolder, dictRemap, userAddress);
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
            string[] varCats = (objItem.Categories as string).Split(',',trim: true);
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

        public delegate void DictPPL_Save();

    }
}