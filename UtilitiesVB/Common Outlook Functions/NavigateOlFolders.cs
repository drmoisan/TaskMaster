using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;

namespace UtilitiesVB
{

    public static class NavigateOlFolders
    {
        public static Folder GetOutlookFolder(string FolderPath, Application OlApp)
        {
            Folder TestFolder;
            string[] FoldersArray;
            int i;

            if (Strings.Left(FolderPath, 2) == @"\\")
            {
                FolderPath = Strings.Right(FolderPath, Strings.Len(FolderPath) - 2);
            }
            // Convert folderpath to array
            FoldersArray = Strings.Split(FolderPath, @"\");
            TestFolder = (Folder)OlApp.Session.Folders[FoldersArray[0]];
            if (TestFolder is not null)
            {
                var loopTo = FoldersArray.Length -1;
                for (i = 0; i <= loopTo; i++)
                {
                    Folders SubFolders;
                    SubFolders = TestFolder.Folders;
                    TestFolder = (Folder)SubFolders[FoldersArray[i]];
                    if (TestFolder is null)
                    {
                        return null;
                    }
                }
            }

            return TestFolder;

        }
        public static string[] OlFolderlist_GetAll(IOlObjects OlObjects)
        {
            string[] OlFolderlist_GetAllRet = default;

            var resultList = new List<string>();
            Folder fldrEmailRoot;

            fldrEmailRoot = GetOutlookFolder(OlObjects.ArchiveRootPath, OlObjects.App);
            var argChildren = fldrEmailRoot.Folders;
            string argRootPath = fldrEmailRoot.FolderPath;
            OlFolder_GetDescendants(ref resultList, ref argChildren, ref argRootPath);
            OlFolderlist_GetAllRet = resultList.ToArray();
            return OlFolderlist_GetAllRet;
        }

        private static void OlFolder_GetDescendants(ref List<string> ResultList, ref Folders Children, ref string RootPath)
        {

            foreach (Folder child in Children)
            {
                string fPath = child.FolderPath;
                fPath = Strings.Right(fPath, Strings.Len(fPath) - Strings.Len(RootPath) - 1);
                ResultList.Add(fPath);
                var argChildren = child.Folders;
                OlFolder_GetDescendants(ref ResultList, ref argChildren, ref RootPath);
            }

        }
    }
}