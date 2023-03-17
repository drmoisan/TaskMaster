using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UtilitiesCS;

namespace UtilitiesCS.Test
{
    [TestClass]
    public class UnitTest1
    {
        private delegate DialogResult TestDelegate();
        //private TestDelegate testDelegate1 = buttonOK;
        //private TestDelegate testDelegate2 = buttonCancel;
        

        [TestMethod]
        public void TestMethod1()
        {
            
            //string title, string message, Dictionary< string,Delegate > map
            string title = "TestDialog";
            string message = "This is a test to see if this is working properly";
            Dictionary<string, Delegate> map = new Dictionary<string, Delegate>();
            map.Add("OK", new TestDelegate(buttonOk));
            map.Add("CANCEL", new TestDelegate(buttonCancel));


            //MyBoxTemplate _box = new MyBoxTemplate();
            MyBoxTemplate _box = new MyBoxTemplate(title, message, map);
            DialogResult result = _box.ShowDialog();
            Assert.IsTrue(result == DialogResult.OK);

        }

        private DialogResult buttonOk()
        {
            return DialogResult.OK;
        }

        //public static DialogResult buttonOK()
        //{
        //    return DialogResult.OK;
        //}

        public DialogResult buttonCancel()
        {
            return DialogResult.Cancel;
        }
        

    }
    //public static class MyDelegates
    //{
    //    public static DialogResult buttonOK()
    //    {
    //        return DialogResult.OK;
    //    }
        
    //    public static DialogResult buttonCancel() 
    //    { 
    //        return DialogResult.Cancel;
    //    }
    //}
    
}
