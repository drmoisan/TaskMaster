using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Office.Interop.Outlook;
using UtilitiesCS;

namespace ToDoModel
{

    public static class FolderSuggestions
    {
        public static IList LoadEmailDataBase(Explorer activeExplorer, IList listEmailsToLoad = null)
        {
            Folder olFolder;
            View objCurView;
            string strFilter;
            Items olItems;
            // TODO: Move this to Model Component of the MVC

            if (listEmailsToLoad is null)
            {
                olFolder = (Folder)activeExplorer.CurrentFolder;
                objCurView = (View)activeExplorer.CurrentView;
                strFilter = objCurView.Filter;
                if (!string.IsNullOrEmpty(strFilter))
                {
                    strFilter = "@SQL=" + strFilter;
                    olItems = olFolder.Items.Restrict(strFilter);
                }
                else
                {
                    olItems = olFolder.Items;
                }
                return ModuleMailItemsSort.MailItemsSort(olItems, (ModuleMailItemsSort.SortOptionsEnum)((int)ModuleMailItemsSort.SortOptionsEnum.DateRecentFirst + (int)ModuleMailItemsSort.SortOptionsEnum.TriageImportantFirst + (int)ModuleMailItemsSort.SortOptionsEnum.ConversationUniqueOnly));
            }

            else
            {
                return listEmailsToLoad;
            }

        }


        public static void ReloadFolderSuggestionStagingFiles()
        {

            if (NotImplementedDialog.StopAtNotImplemented(MethodBase.GetCurrentMethod().Name))
            {
                throw new NotImplementedException("Folder_Suggestions_Reload not implemented yet");
            }
            else
            {
                Debug.WriteLine("Continuing execution without reloading Folder Suggestions");
            }


            // Dim blOld As Boolean
            // blOld = False
            // CTF_Incidence_Text_File_READ(_globals.FS)
            // Common_Words_Text_File_READ(_globals.FS)
            // SubjectMapReadTextFile(_globals.FS)
            // strFList = Folderlist_GetAll
            // bl_SuggestionFiles_IsLoaded = True
            // Conversation_Weight = 10000
            // Subject_Weight = 1
            // If blOld Then
            // lngConvCtPwr = 3
            // lngSubjectCtPwr = 1
            // Else
            // lngConvCtPwr = 2
            // lngSubjectCtPwr = 3
            // End If


        }

    }
}