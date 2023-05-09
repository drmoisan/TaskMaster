using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;

namespace ToDoModel
{

    public static class CaptureEmailAddressesModule
    {
        public static List<string> CaptureEmailAddresses(MailItem OlMail, string emailRootFolder, Dictionary<string, string> dictRemap)
        {
            int i;
            int j;
            string[] strAddresses;
            bool blContains;
            var emailAddressList = new List<string>();

            string[] strEmail = CaptureEmailDetailsModule.CaptureEmailDetails(OlMail, emailRootFolder, dictRemap);

            if (strEmail is Array == true)
            {
                for (i = 4; i <= 6; i++)
                {
                    if (!string.IsNullOrEmpty(strEmail[i]))
                    {
                        strAddresses = Strings.Split(strEmail[i], "; ");
                        var loopTo = Information.UBound(strAddresses);
                        for (j = 0; j <= loopTo; j++)
                        {
                            blContains = false;

                            foreach (var strTmp in emailAddressList)
                            {

                                if ((Strings.LCase(Strings.Trim(strTmp)) ?? "") == (Strings.LCase(Strings.Trim(strAddresses[j])) ?? ""))
                                {
                                    blContains = true;
                                }
                            }

                            if (blContains == false)
                            {
                                if (Strings.StrComp(strAddresses[j], "dan.moisan@planetpartnership.com", Constants.vbTextCompare) != 0)
                                {
                                    emailAddressList.Add(Strings.LCase(Strings.Trim(strAddresses[j])));
                                }
                            }

                        }
                    }
                }
            }
            return emailAddressList;
        }

    }
}