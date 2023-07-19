using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;

using UtilitiesCS;

namespace ToDoModel
{


    public static class ModuleMailItemsSort
    {
        public static IList MailItemsSort(Items OlItems, SortOptionsEnum options)
        {
            string strFilter;
            string strFilter2;
            // Dim OlRow                   As Outlook.Row
            // Dim OlTable                 As Outlook.Table
            Folder OlFolder;
            // Dim OlItems                 As Outlook.Items
            Items OlItemsTmp;
            Items OlItemsRemainder;
            object objItem;
            MailItem OlMailTmp;
            MailItem OlMailTmp2;
            // Dim OlNameSpace             As Outlook.NameSpace
            IList listEmails;
            var StrTriageOpts = new string[4];
            int i;
            int j;
            bool BlUniqueConv;
            var intFrom = default(int);
            var intTo = default(int);
            var intStep = default(int);
            var blTriage = default(bool);


            // Originally written to maintain filter of the view
            // Need to add in the option to eliminate the filter
            // for two cases: 1) If it is not called from the active view,
            // and 2) in the case that we want to see all emails anyway

            StrTriageOpts[1] = "A";
            StrTriageOpts[2] = "B";
            StrTriageOpts[3] = "C";

            listEmails = new List<MailItem>();
            // OlFolder = _activeExplorer.CurrentFolder
            // objCurView = _activeExplorer.CurrentView
            // strFilter = objCurView.Filter
            // If strFilter <> "" Then
            // strFilter = "@SQL=" & strFilter
            // OlItems = OlFolder.Items.Restrict(strFilter)
            // Else
            // OlItems = OlFolder.Items
            // End If

            if (options.HasFlag(SortOptionsEnum.DateRecentFirst))
            {
                OlItems.Sort("Received", true);
            }
            else if (options.HasFlag(SortOptionsEnum.DateOldestFirst))
            {
                OlItems.Sort("Received", false);
            }

            // Output_Items OlItems

            if (options.HasFlag(SortOptionsEnum.TriageImportantFirst))
            {
                blTriage = true;
                intFrom = 1;
                intTo = 3;
                intStep = 1;
            }
            else if (options.HasFlag(SortOptionsEnum.TriageImportantLast))
            {
                blTriage = true;
                intFrom = 3;
                intTo = 1;
                intStep = -1;
            }

            if (blTriage)
            {
                OlItemsRemainder = OlItems;
                var loopTo = intTo;
                for (i = intFrom; intStep >= 0 ? i <= loopTo : i >= loopTo; i += intStep)
                {
                    strFilter = "[Triage] = " + '"' + StrTriageOpts[i] + '"';
                    strFilter2 = "[Triage] <> " + '"' + StrTriageOpts[i] + '"';
                    OlItemsTmp = OlItems.Restrict(strFilter);
                    OlItemsRemainder = OlItemsRemainder.Restrict(strFilter2);


                    foreach (var currentObjItem in OlItemsTmp)
                    {
                        objItem = currentObjItem;
                        BlUniqueConv = true;
                        if (objItem is MailItem)
                        {
                            OlMailTmp = (MailItem)objItem;
                            if (!MailResolution_ToRemove.IsMailUnReadable(OlMailTmp))
                            {
                                if (options.HasFlag(SortOptionsEnum.ConversationUniqueOnly))
                                {
                                    var loopTo1 = listEmails.Count - 1;
                                    for (j = 0; j <= loopTo1; j++)
                                    {
                                        OlMailTmp2 = (MailItem)listEmails[j];
                                        if ((OlMailTmp.ConversationID ?? "") == (OlMailTmp2.ConversationID ?? ""))
                                        {
                                            BlUniqueConv = false;
                                        }
                                    }
                                } // Options And ConversationUniqueOnly Then

                                if (BlUniqueConv)
                                    listEmails.Add(OlMailTmp);

                            } // If IsMailUnReadable
                        } // If TypeOf ObjItem Is mailItem Then
                    } // For Each ObjItem In OlItemsTmp
                } // For i = 1 To 4

                foreach (var currentObjItem1 in OlItemsRemainder)
                {
                    objItem = currentObjItem1;
                    BlUniqueConv = true;
                    if (objItem is MailItem)
                    {
                        OlMailTmp = (MailItem)objItem;
                        if (!MailResolution_ToRemove.IsMailUnReadable(OlMailTmp))
                        {
                            if (options.HasFlag(SortOptionsEnum.ConversationUniqueOnly))
                            {
                                var loopTo2 = listEmails.Count - 1;
                                for (j = 0; j <= loopTo2; j++)
                                {
                                    OlMailTmp2 = (MailItem)listEmails[j];
                                    if ((OlMailTmp.ConversationID ?? "") == (OlMailTmp2.ConversationID ?? ""))
                                    {
                                        BlUniqueConv = false;
                                    }
                                }
                            } // Options And ConversationUniqueOnly Then

                            if (BlUniqueConv)
                                listEmails.Add(OlMailTmp);

                        } // If IsMailUnReadable
                    } // If TypeOf ObjItem Is mailItem Then
                } // For Each ObjItem In OlItemsRemainder
            }

            else
            {
                foreach (var currentObjItem2 in OlItems)
                {
                    objItem = currentObjItem2;
                    BlUniqueConv = true;
                    if (objItem is MailItem)
                    {
                        OlMailTmp = (MailItem)objItem;
                        if (!MailResolution_ToRemove.IsMailUnReadable(OlMailTmp))
                        {
                            if (options.HasFlag(SortOptionsEnum.ConversationUniqueOnly))
                            {
                                var loopTo3 = listEmails.Count;
                                for (j = 1; j <= loopTo3; j++)
                                {
                                    OlMailTmp2 = (MailItem)listEmails[j];
                                    if ((OlMailTmp.ConversationID ?? "") == (OlMailTmp2.ConversationID ?? ""))
                                    {
                                        BlUniqueConv = false;
                                    }
                                }
                            } // Options And ConversationUniqueOnly Then

                            if (BlUniqueConv)
                                listEmails.Add(OlMailTmp);
                        } // Not IsMailUnReadable(OlMailTmp) Then
                    } // If TypeOf ObjItem Is mailItem Then
                }
            }

            return listEmails;


            OlFolder = null;
            OlItems = null;
            OlItemsTmp = null;
            objItem = null;
            OlMailTmp = null;
            OlMailTmp2 = null;


        }

        [Flags]
        public enum SortOptionsEnum
        {
            TriageIgnore = 1,
            TriageImportantFirst = 2,
            TriageImportantLast = 4,
            DateRecentFirst = 8,
            DateOldestFirst = 16,
            ConversationUniqueOnly = 32
        }


    }
}