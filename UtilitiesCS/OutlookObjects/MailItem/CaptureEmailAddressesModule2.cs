using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS;


namespace UtilitiesCS
{
    public static class CaptureEmailAddressesModule2
    {
        

        //public static List<string> GetEmailAddresses(this MailItem OlMail, string emailRootFolder, IScoDictionary<string, string> dictRemap, string currentUserEmail)
        //{
        //    int i;
        //    int j;
        //    string[] strAddresses;
        //    bool blContains;
        //    var emailAddressList = new List<string>();

        //    //string[] strEmail = CaptureEmailDetailsModule.CaptureEmailDetails(OlMail, emailRootFolder, dictRemap);
        //    string[] strEmail = OlMail.Details(emailRootFolder, dictRemap);

        //    if (strEmail is Array == true)
        //    {
        //        for (i = 4; i <= 6; i++)
        //        {
        //            if (!string.IsNullOrEmpty(strEmail[i]))
        //            {
        //                strAddresses = strEmail[i].Split(';', trim: true);
        //                var loopTo = strAddresses.Length;
        //                for (j = 0; j < loopTo; j++)
        //                {
        //                    blContains = false;

        //                    foreach (var strTmp in emailAddressList)
        //                    {

        //                        if ((strTmp.Trim().ToLower() ?? "") == (strAddresses[j].Trim().ToLower() ?? ""))
        //                        {
        //                            blContains = true;
        //                        }
        //                    }

        //                    if (blContains == false)
        //                    {
        //                        if (strAddresses[j].ToLower() != currentUserEmail.ToLower())
        //                        {
        //                            emailAddressList.Add(strAddresses[j].Trim().ToLower());
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return emailAddressList;
        //}


    }
}
