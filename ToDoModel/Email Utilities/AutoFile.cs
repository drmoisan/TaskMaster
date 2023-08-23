using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Outlook;
using Tags;
using UtilitiesCS;
using UtilitiesCS;
using System.Windows.Forms;

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
                                                   Dictionary<string, string> ppl_dict,
                                                   string emailRootFolder,
                                                   Dictionary<string, string> dictRemap,
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
                    emailAddressList = CaptureEmailAddressesModule.CaptureEmailAddresses(OlMail, emailRootFolder, dictRemap, userAddress);
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

        public static IList<string> dictPPL_AddMissingEntries(MailItem OlMail, Dictionary<string, string> ppl_dict, List<IPrefix> prefixes, string prefixKey, string emailRootFolder, string stagingPath, Dictionary<string, string> dictRemap, string filename_dictppl, DictPPL_Save dictPPLSave, string currentUserEmail)
        {

            var addressList = new List<string>();
            string strTmp3;
            bool blNew = false;
            // Dim catTmp As Outlook.Category
            var colReturnCatNames = new List<string>();
            Regex objRegex;
            TagViewer _viewer;
            SortedDictionary<string, bool> dictNAMES;

            dictNAMES = ppl_dict.GroupBy(x => x.Value).ToDictionary(y => y.Key, z => false).ToSortedDictionary();


            if (MailResolution.IsMailUnReadable(OlMail) == false)
            {
                addressList = CaptureEmailAddressesModule.CaptureEmailAddresses(OlMail, emailRootFolder, dictRemap, currentUserEmail);
            }

            // Discard any email addresses from the email that
            // are already in the people dictionary
            addressList = addressList.Where(x => !ppl_dict.ContainsKey(x)).Select(x => x).ToList();


            foreach (string address in addressList)
            {

                var vbR = MessageBox.Show("Add entry for " + address, "Dialog",MessageBoxButtons.YesNo);
                if (vbR == DialogResult.Yes)
                {
                    objRegex = new Regex(@"([a-zA-z\d]+)\.([a-zA-z\d]+)@([a-zA-z\d]+)\.com", RegexOptions.Multiline);

                    string newPplTag = objRegex.Replace(address, ("$1 $2")); //Proper case
                    var selections = new List<string>() { newPplTag };

                    // Check if it is a new address for existing contact
                    _viewer = new TagViewer();

                    var _controller = new TagController(viewer_instance: _viewer,
                                                        dictOptions: dictNAMES,
                                                        autoAssigner: null,
                                                        prefixes: prefixes,
                                                        selections: selections,
                                                        prefixKey: prefixKey,
                                                        objItemObject: OlMail,
                                                        userEmailAddress: currentUserEmail)
                    {
                        ButtonNewActive = false,
                        ButtonAutoAssignActive = false
                    };
                    _controller.SetSearchText(newPplTag);

                    var unused = _viewer.ShowDialog();
                    strTmp3 = _controller.SelectionString();

                    if (!string.IsNullOrEmpty(strTmp3))
                    {
                        ppl_dict.Add(address, strTmp3);
                        blNew = true;
                        colReturnCatNames.Add(strTmp3);
                        // Commented out because it seems completely redundant
                        // Else
                        // newPplTag = InputBox("Enter name for " & address, DefaultResponse:=newPplTag)
                        // catTmp = CreateCategory(My.Settings.Prefix_People, newPplTag, Globals.ThisAddIn._OlNS)

                        // If Not catTmp Is Nothing Then
                        // ppl_dict.Add(address, My.Settings.Prefix_People & newPplTag)
                        // blNew = True
                        // colReturnCatNames.Add(My.Settings.Prefix_People & newPplTag)
                        // End If
                    }
                }
            }
            if (blNew)
            {
                dictPPLSave();
                // WriteDictPPL(Path.Combine(stagingPath, filename_dictppl), ppl_dict)
            }


            return colReturnCatNames;

        }
    }
}