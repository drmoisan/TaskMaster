#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Deedle;
using Microsoft.Office.Interop.Outlook;
using SDILReader;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public static partial class SortEmail
    {
        private const int MAX_PATH = 256;

        [ExcludeFromCodeCoverage]
        internal static void SaveAttachmentsOld(
            MailItem mailItem,
            string fsLocation,
            string DteString,
            string DteString2,
            bool save_images,
            bool DELFILE,
            bool Verify_Action
        )
        {
            #region tocollapse
            int atmtct = 0;
            bool AlreadyExists;
            string strAtmtFullName;
            bool FileExtExists;
            string[] strAtmtName = new string[2];
            string strAtmtPath;
            string strAtmtPath2;
            bool blnIsSave;
            YesNoToAllResponse response;
            #endregion
            var lCountEachItem = mailItem.Attachments.Count;
            if (lCountEachItem > 0)
            {
                foreach (Attachment attachment in mailItem.Attachments)
                {
                    #region Hide
                    atmtct = atmtct + 1;

                    AlreadyExists = false;

                    // Get the full name of the current attachment.
                    if (attachment.Type != OlAttachmentType.olOLE)
                    {
                        strAtmtFullName = attachment.FileName;
                    }
                    else
                    {
                        strAtmtFullName = "NOTHING";
                    }

                    // Is there a dot in the file extension?
                    if (strAtmtFullName.Contains("."))
                    {
                        FileExtExists = true;

                        // Find the dot postion in atmtFullName.
                        int intDotPosition = strAtmtFullName.IndexOf(".");

                        // Get the name.
                        strAtmtName[0] = strAtmtFullName.Substring(0, intDotPosition - 1);

                        // Get the file extension.
                        strAtmtName[1] = strAtmtFullName.Substring(
                            strAtmtFullName.Length - intDotPosition
                        );
                    }
                    else
                    {
                        FileExtExists = false;
                        strAtmtName[0] = strAtmtFullName;
                        strAtmtName[1] = "NONE";
                    }

                    // Get the full saving path of the current attachment.
                    strAtmtPath = fsLocation + DteString + " " + strAtmtFullName;
                    strAtmtPath2 = fsLocation + DteString2 + " " + strAtmtFullName;

                    // /* If the length of the saving path is not larger than 260 characters.*/
                    if (strAtmtPath.Length >= MAX_PATH)
                    {
                        strAtmtPath = strAtmtPath.Substring(0, MAX_PATH - 7);
                    }
                    #endregion

                    // True: This attachment can be saved.
                    if (
                        save_images == true
                        | strAtmtName[1].ToUpper() != "PNG"
                            & strAtmtName[1].ToUpper() != "JPG"
                            & strAtmtName[1].ToUpper() != "GIF"
                    )
                    {
                        // True: Not a picture
                        if (DELFILE == true)
                        {
                            if (File.Exists(strAtmtPath) == true)
                            {
                                File.Delete(strAtmtPath);
                            }
                            else if (File.Exists(strAtmtPath2) == true)
                            {
                                File.Delete(strAtmtPath2);
                            }
                            blnIsSave = false;
                        }
                        else
                        {
                            blnIsSave = true;

                            // /* Loop until getting the file name which does not exist in the folder. */
                            while (File.Exists(strAtmtPath))
                            {
                                AlreadyExists = true;

                                var strAtmtNameTemp =
                                    strAtmtName[0] + DateTime.Now.ToString("_MMddhhmmss");
                                strAtmtPath = fsLocation + DteString + strAtmtNameTemp;
                                if (FileExtExists)
                                    strAtmtPath = strAtmtPath + "." + strAtmtName[1];

                                // /* If the length of the saving path is over 260 characters.*/
                                if (strAtmtPath.Length > MAX_PATH)
                                {
                                    lCountEachItem = lCountEachItem - 1;
                                    // False: This attachment cannot be saved.
                                    blnIsSave = false;
                                    break;
                                }
                            }
                        }

                        // /* Save the current attachment if it is a valid file name. */
                        if (blnIsSave)
                        {
                            if (Verify_Action == true)
                            {
                                if ((int)_attachmentsOverwrite + (int)_responseSaveFile == 0)
                                {
                                    mailItem.Display();
                                }

                                if (AlreadyExists == true)
                                {
                                    // Response = MsgBox("File Already Exists. Save file: " & strAtmtPath, vbCritical + vbYesNo)
                                    if (_attachmentsOverwrite == YesNoToAllResponse.Empty)
                                    {
                                        response = YesNoToAll.ShowDialog(
                                            "File Already Exists. Save file: " + strAtmtPath
                                        );
                                        if (
                                            response == YesNoToAllResponse.NoToAll
                                            | response == YesNoToAllResponse.YesToAll
                                        )
                                            _attachmentsOverwrite = response;
                                    }
                                    else
                                    {
                                        response = _attachmentsOverwrite;
                                    }
                                }
                                // Response = MsgBox("Save file: " & strAtmtPath, vbYesNo + vbExclamation)
                                else if (_responseSaveFile == YesNoToAllResponse.Empty)
                                {
                                    response = YesNoToAll.ShowDialog("Save file: " + strAtmtPath);
                                    if (
                                        response == YesNoToAllResponse.NoToAll
                                        | response == YesNoToAllResponse.YesToAll
                                    )
                                        _responseSaveFile = response;
                                }
                                else
                                {
                                    response = _responseSaveFile;
                                }

                                if (
                                    response == YesNoToAllResponse.Yes
                                    | response == YesNoToAllResponse.YesToAll
                                )
                                {
                                    strAtmtName[0] = InputBox.ShowDialog(
                                        $"Email Subject: {mailItem.Subject} \n Rename file: {strAtmtPath}",
                                        "Input Dialog",
                                        DefaultResponse: strAtmtName[0]
                                    )!;
                                    if (string.IsNullOrEmpty(strAtmtName[0]))
                                    {
                                        if (
                                            MessageBox.Show(
                                                $"Revert to file name: {strAtmtPath}",
                                                "",
                                                MessageBoxButtons.OKCancel
                                            ) == DialogResult.Cancel
                                        )
                                            response = YesNoToAllResponse.No;
                                    }
                                    else
                                    {
                                        strAtmtPath = fsLocation + DteString + " " + strAtmtName[0];
                                        if (FileExtExists)
                                            strAtmtPath = strAtmtPath + "." + strAtmtName[1];
                                    }
                                }

                                mailItem.Close(OlInspectorClose.olDiscard);
                            }
                            else
                            {
                                response = YesNoToAllResponse.Yes;
                            }
                            if (
                                response == YesNoToAllResponse.Yes
                                | response == YesNoToAllResponse.YesToAll
                            )
                                attachment.SaveAsFile(strAtmtPath);
                        }
                    }
                }
            }
        }
    }
}
