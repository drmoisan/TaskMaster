using CleanProjectToTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ToDoModel.Test
{
    [TestClass]
    public class CTFIncidence_Test
    {
        [TestMethod]
        public void CTFIncidenceTextFileREAD_Test()
        {
            FolderPathsTest folderPathsTest = new FolderPathsTest();
            Module1.CTF_Incidence_Text_File_READ1(folderPathsTest);
        }
    }
}
