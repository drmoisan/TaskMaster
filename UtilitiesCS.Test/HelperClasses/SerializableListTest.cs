using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class SerializableList_Test
    {
        //[TestMethod]
        //public void Serialize_Test()
        //{
        //    string _oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial");
        //    string _flow = Path.Combine(_oneDrive, "Email attachments from Flow");
        //    string filepath = Path.Combine(_flow, "TestSerializedList.json");
        //    SerializableList<string> testList = new SerializableList<string>();
        //    testList.Add("Entry1");
        //    testList.Add(@"Folder1\Entry2");
        //    testList.Add(@"Folder1\Folder2\Entry3");
        //    testList.Add(@"Folder1\Folder2\Entry4");
        //    testList.Add(@"Folder1\Entry5");
        //    testList.Filepath = filepath;
        //    testList.Serialize();
        //    Assert.IsTrue(File.Exists(filepath));
        //}
        //[TestMethod]
        //public void Deserialize_Test()
        //{
        //    SerializableList<string> targetList = new SerializableList<string>();
        //    targetList.Add("Entry1");
        //    targetList.Add(@"Folder1\Entry2");
        //    targetList.Add(@"Folder1\Folder2\Entry3");
        //    targetList.Add(@"Folder1\Folder2\Entry4");
        //    targetList.Add(@"Folder1\Entry5");

        //    string _oneDrive = Environment.GetEnvironmentVariable("OneDriveCommercial");
        //    string _flow = Path.Combine(_oneDrive, "Email attachments from Flow");
        //    string filepath = Path.Combine(_flow, "TestSerializedList.json");
        //    SerializableList<string> testList = new SerializableList<string>();
        //    testList.Filepath = filepath;
        //    testList.Deserialize();
        //    Assert.IsTrue(targetList.SequenceEqual(testList));
        //}
    }
}
