using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using UtilitiesVB;
using Microsoft.Office.Interop.Outlook;
using ToDoModel;
using System.Security.Policy;

namespace TaskMaster.Test
{
    [TestClass]
    public class ToDoObj_Test
    {
        private Application _olApp;

        [TestInitialize]
        public void Initialize() 
        { 
            _olApp = new Application();
        }

        [TestMethod]
        public void Constructor_Test()
        {
            String Filepath = Path.Combine(Environment.GetFolderPath(
                                       Environment.SpecialFolder.LocalApplicationData),
                                       "TaskMaster", 
                                       "UsedIDList.bin");
            ToDoObj<IListOfIDs> Obj = new ToDoObj<IListOfIDs>(Filepath, ListOfIDs.LoadFromFile);
            Assert.IsNotNull(Obj);
        }
        
        [TestMethod]
        public void Load_Test() 
        {
            String Filepath = Path.Combine(Environment.GetFolderPath(
                                       Environment.SpecialFolder.LocalApplicationData),
                                       "TaskMaster",
                                       "UsedIDList.bin");
            ToDoObj<IListOfIDs> Obj = new ToDoObj<IListOfIDs>(Filepath, ListOfIDs.LoadFromFile);
            Obj.LoadFromFile(Filepath, _olApp);
            Assert.IsNotNull(Obj.Item);
        }

        [TestMethod]
        public void Test_ItemType()
        {
            String Filepath = Path.Combine(Environment.GetFolderPath(
                                       Environment.SpecialFolder.LocalApplicationData),
                                       "TaskMaster",
                                       "UsedIDList.bin");
            ToDoObj<IListOfIDs> Obj = new ToDoObj<IListOfIDs>(Filepath, ListOfIDs.LoadFromFile);
            Obj.LoadFromFile(Filepath, _olApp);
            Type mytype = Obj.Item.GetType();
            Assert.IsInstanceOfType(Obj.Item, typeof(ListOfIDs));
        }

    }
}
