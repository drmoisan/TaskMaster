using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Microsoft.VisualStudio.Services.Common;
using System.IO;
using System;
using Microsoft.ReportingServices.Diagnostics.Internal;
using CleanProjectToTest;
using System.Collections.Generic;

namespace ToDoModel.Test
{
    [TestClass]
    public class PeopleDictTest
    {
        
        [TestMethod]
        public void Test_WriteDictJSON()
        {
            String filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "testkey.json");
            Dictionary<String, String> dict = new Dictionary<String, String>();
            dict.Add("adalberto.aguilar@pepsico.com", "Tag PPL Adalberto Aguilar");
            dict.Add("ana.henao@pepsico.com", "Tag PPL Ana Henao");
            dict.Add("celso.borges@pepsico.com", "Tag PPL Celso Borges");
            TestSerDic.WriteDictJSON(dict, filepath);
            Assert.IsTrue(File.Exists(filepath));
        }

        [TestMethod]
        public void Test_ReadDictJSON()
        {
            String filepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "testkey.json");
            Dictionary<String, String> dict = TestSerDic.GetDictJSON(filepath);
            Assert.IsNotNull(dict);
        }
    }
}
