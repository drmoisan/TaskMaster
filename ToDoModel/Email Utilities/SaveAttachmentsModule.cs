using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;


using UtilitiesCS;
using UtilitiesCS;

namespace ToDoModel
{

    public static class SaveAttachmentsModule
    {
        public static string strFolderNotToCreate;
        private static YesNoToAllResponse ResponseSaveFile = default;
        private static YesNoToAllResponse ResponseOverwriteFile = default;

        // Public Enum YesNoToAllResponse
        // Empty = 0
        // Yes = 1
        // No = 2
        // YesToAll = 4
        // NoToAll = 8
        // End Enum

        public static long SaveAttachmentsFromSelection(IApplicationGlobals AppGlobals, string SavePath, bool DELFILE = false, bool Verify_Action = false, IList<MailItem> selItems = null, bool save_images = false, bool SaveACopy = false, bool SaveMSG = false, bool blVerifySaveDuplicate = true)
        {

            var _globals = AppGlobals;
            object objFolder;       // The selected folder object from Browse for Folder dialog box.
            object objSubFolders;
            // Dim selItems            As Selection    ' A collection of Outlook item objects in a folder.
            object objNewFolder;       // A specific member of a Collection object either by position or by key.
                                       // A document or link to a document contained in an Outlook item.
            string strAtmtPath;       // The full saving path of the attachment.
            string strAtmtPath2;       // The full saving path of the attachment. (With error for FLOW of +1 day)
            string strAtmtFullName;       // The full name of an attachment.
            var strAtmtName = new string[2];       // strAtmtName(0): to save the name; strAtmtName(1): to save the file extension. They are separated by dot of an attachment file name.
            string strAtmtNameTemp;       // To save a temporary attachment file name.
            int intDotPosition;      // The dot position in an attachment name.
            Attachments Atmts;  // A set of Attachment objects that represent the attachments in an Outlook item.
            long lCountEachItem;         // The number of attachments in each Outlook item.
            long lCountAllItems;         // The number of attachments in all Outlook items.
            string strFolderPath;       // The selected folder path.
            bool blnIsEnd;      // End all code execution.
            bool blnIsSave;      // Consider if it is need to save.
            DateTime emailDate;         // Holds date of email
            DateTime emailDate2;         // Holds date of email + 1 day for error in FLOW
            string DteString;       // Holds string portion of time stamp of email
            string DteString2;       // Holds string portion of time stamp of email plus one day (for error in FLOW)
            MailItem objMailItem;
            bool AlreadyExists;      // Checks to see whether file already exists
            int atmtct;      // Error to count number of traces through attachment loop
            bool FileExtExists;      // Boolean value to check if file extension exists
            MailItem MSG;
            bool blnFolderExists;
            const int MAX_PATH = 260;
            YesNoToAllResponse response;

            // Dim Response            As Variant      ' Response to user input

            blnIsEnd = false;
            blnIsSave = false;
            blnFolderExists = true;
            lCountAllItems = 0L;
            atmtct = 0;
            FileExtExists = false;

            // On Error Resume Next


            if (selItems is null)
            {
                selItems = (IList<MailItem>)_globals.Ol.App.ActiveExplorer().Selection;
            }



            strFolderPath = SavePath + @"\"; // 'New ADDED BY DAN TO BYPASS FOLDER SELECTION

            // Check to see if destination directory exists on the file system
            // If it doesn't, ask the user what to do with it
            if (!Directory.Exists(strFolderPath))
            {
                if ((strFolderNotToCreate ?? "") == (strFolderPath ?? ""))
                {
                    blnFolderExists = false;
                }
                else
                {
                    using (var fnfViewer = new FolderNotFoundViewer())
                    {
                        fnfViewer.FolderName = strFolderPath;
                        fnfViewer.ShowDialog();
                        switch (fnfViewer.FolderAction ?? "")
                        {
                            case "Create":
                                {
                                    objNewFolder = Directory.CreateDirectory(strFolderPath);
                                    break;
                                }
                            case "Find":
                                {
                                    break;
                                }
                            case "NoToAll":
                                {
                                    strFolderNotToCreate = strFolderPath;
                                    blnFolderExists = false;
                                    break;
                                }

                            default:
                                {
                                    blnFolderExists = false;
                                    break;
                                }
                        }
                    }
                }
            }

            // /* Go through each item in the selection. */

            if (blnFolderExists)
            {
                foreach (var objItem in selItems)
                {
                    if (objItem is MailItem)
                    {
                        MSG = (MailItem)objItem;
                        lCountEachItem = MSG.Attachments.Count;
                        emailDate = MSG.SentOn;
                        // Add a day to catch error from flow
                        emailDate2 = emailDate.AddDays(1);
                        DteString = emailDate.ToString("yyMMdd");
                        DteString2 = emailDate2.ToString("yyMMdd"); // Add a day to catch error from flow

                        if (SaveMSG == true)
                        {
                            if (!string.IsNullOrEmpty(MSG.Subject))
                            {
                                strAtmtFullName = MSG.Subject;
                                ReplaceCharsForFileName(ref strAtmtFullName, "-");
                                strAtmtPath = strFolderPath + DteString + " " + strAtmtFullName;
                                MSG.SaveAs(strAtmtPath, 3);
                            }
                           
                        }

                        // /* If the current item contains attachments. */
                        if (lCountEachItem > 0L)
                        {
                            Atmts = (Attachments)MSG.Attachments;
                            foreach (Attachment Atmt in Atmts)
                            {
                                atmtct = atmtct + 1;

                                AlreadyExists = false;
                                // Get the full name of the current attachment.
                                if (Atmt.Type != OlAttachmentType.olOLE)
                                {
                                    strAtmtFullName = Atmt.FileName;
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
                                    intDotPosition = strAtmtFullName.IndexOf(".");

                                    // Get the name.
                                    strAtmtName[0] = strAtmtFullName.Substring(0, intDotPosition - 1);

                                    // Get the file extension.
                                    strAtmtName[1] = strAtmtFullName.Substring(strAtmtFullName.Length - intDotPosition);
                                }

                                else
                                {
                                    FileExtExists = false;
                                    strAtmtName[0] = strAtmtFullName;
                                    strAtmtName[1] = "NONE";
                                }


                                // Get the full saving path of the current attachment.
                                strAtmtPath = strFolderPath + DteString + " " + strAtmtFullName;
                                strAtmtPath2 = strFolderPath + DteString2 + " " + strAtmtFullName;

                                // /* If the length of the saving path is not larger than 260 characters.*/
                                if (strAtmtPath.Length <= MAX_PATH)
                                {
                                    // True: This attachment can be saved.
                                    if (save_images == true | strAtmtName[1].ToUpper() != "PNG" & strAtmtName[1].ToUpper() != "JPG" & strAtmtName[1].ToUpper() != "GIF")
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

                                                strAtmtNameTemp = strAtmtName[0] + DateTime.Now.ToString("_MMddhhmmss");
                                                strAtmtPath = strFolderPath + DteString + strAtmtNameTemp;
                                                if (FileExtExists)
                                                    strAtmtPath = strAtmtPath + "." + strAtmtName[1];

                                                // /* If the length of the saving path is over 260 characters.*/
                                                if (strAtmtPath.Length > MAX_PATH)
                                                {
                                                    lCountEachItem = lCountEachItem - 1L;
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

                                                objMailItem = (MailItem)objItem;

                                                if ((int)ResponseOverwriteFile + (int)ResponseSaveFile == 0)
                                                {
                                                    objMailItem.Display();
                                                }


                                                if (AlreadyExists == true)
                                                {
                                                    // Response = MsgBox("File Already Exists. Save file: " & strAtmtPath, vbCritical + vbYesNo)
                                                    if (ResponseOverwriteFile == YesNoToAllResponse.Empty)
                                                    {
                                                        response = YesNoToAll.ShowDialog("File Already Exists. Save file: " + strAtmtPath);
                                                        if (response == YesNoToAllResponse.NoToAll | response == YesNoToAllResponse.YesToAll)
                                                            ResponseOverwriteFile = response;
                                                    }
                                                    else
                                                    {
                                                        response = ResponseOverwriteFile;
                                                    }
                                                }
                                                // Response = MsgBox("Save file: " & strAtmtPath, vbYesNo + vbExclamation)
                                                else if (ResponseSaveFile == YesNoToAllResponse.Empty)
                                                {
                                                    response = YesNoToAll.ShowDialog("Save file: " + strAtmtPath);
                                                    if (response == YesNoToAllResponse.NoToAll | response == YesNoToAllResponse.YesToAll)
                                                        ResponseSaveFile = response;
                                                }
                                                else
                                                {
                                                    response = ResponseSaveFile;

                                                }

                                                if (response == YesNoToAllResponse.Yes | response == YesNoToAllResponse.YesToAll)
                                                {
                                                    strAtmtName[0] = InputBox.ShowDialog($"Email Subject: {MSG.Subject} \n Rename file: {strAtmtPath}",
                                                                                        "Input Dialog", DefaultResponse: strAtmtName[0]);
                                                    if (string.IsNullOrEmpty(strAtmtName[0]))
                                                    {
                                                        if (MessageBox.Show($"Revert to file name: {strAtmtPath}", "", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                                                            response = YesNoToAllResponse.No;
                                                    }
                                                    else
                                                    {
                                                        strAtmtPath = strFolderPath + DteString + " " + strAtmtName[0];
                                                        if (FileExtExists)
                                                            strAtmtPath = strAtmtPath + "." + strAtmtName[1];
                                                    }
                                                }

                                                objMailItem.Close(OlInspectorClose.olDiscard);
                                            }
                                            else
                                            {
                                                response = YesNoToAllResponse.Yes;
                                            }
                                            if (response == YesNoToAllResponse.Yes | response == YesNoToAllResponse.YesToAll)
                                                Atmt.SaveAsFile(strAtmtPath);
                                        }
                                    }
                                }
                                else
                                {
                                    lCountEachItem = lCountEachItem - 1L;
                                }
                            }
                        }

                        // Count the number of attachments in all Outlook items.
                        lCountAllItems = lCountAllItems + lCountEachItem;
                    }
                    else
                    {
                    }
                }
            }
            else if ((strFolderNotToCreate ?? "") == (strFolderPath ?? ""))
            {
            }
            else
            {
                MessageBox.Show("Canceled save due to non-existant folder");
            }

            return default;
            // 'End If


            // /* For run-time error:
            // The Explorer has been closed and cannot be used for further operations.
            // Review your code and restart Outlook. */

        }

        // #####################
        // Convert general path.
        // #####################
        public static string CGPath(string path)
        {
            string CGPathRet = default;
            if (path[path.Length-1] != @"\"[0])
                path = path + @"\";
            CGPathRet = path;
            return CGPathRet;
        }

        // ######################################
        // Run this macro for saving attachments.
        // ######################################
        // Private Sub ExecuteSavingDirect(SavePath As String)
        // Dim lNum As Long

        // lNum = SaveAttachmentsFromSelection(SavePath)

        // If lNum > 0 Then
        // MsgBox(CStr(lNum) & " attachment(s) was(were) saved successfully.", vbInformation, "Message from Attachment Saver")
        // Else
        // MsgBox("No attachment(s) in the selected Outlook items.", vbInformation, "Message from Attachment Saver")
        // End If
        // End Sub

        public static void ReplaceCharsForFileName(ref string sName, string sChr)
        {
            string patternMatch = "[/\\\\:\\?<>|&%* {}\\[\\]!]*";
            var rg = new Regex(patternMatch);
            rg.Replace(sName, sChr);
            //sName = sName.Replace("/", sChr);
            //sName = sName.Replace(  @"\", sChr);
            //sName = sName.Replace(":", sChr);
            //sName = sName.Replace("?", sChr);
            //sName = sName.Replace('"'.ToString(), sChr);
            //sName = sName.Replace("<", sChr);
            //sName = sName.Replace(">", sChr);
            //sName = sName.Replace("|", sChr);
            //sName = sName.Replace("&", sChr);
            //sName = sName.Replace("%", sChr);
            //sName = sName.Replace("*", sChr);
            //sName = sName.Replace(" ", sChr);
            //sName = sName.Replace("{", sChr);
            //sName = sName.Replace("[", sChr);
            //sName = sName.Replace("]", sChr);
            //sName = sName.Replace("}", sChr);
            //sName = sName.Replace("!", sChr);
        }

    }
}