using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UtilitiesCS;

namespace Z.Disabled.UtilitiesCS.Test
{
    [TestClass]
    public class DialogTest
    {
        private delegate DialogResult TestDelegate();
                
        [TestMethod]
        public void Form_TestMethod1()
        {
            
            //string title, string message, Dictionary< string,Delegate > map
            string title = "TestDialog";
            string message = "This is a test to see if this is working properly";
            Dictionary<string, Delegate> map = new Dictionary<string, Delegate>();
            map.Add("OK", new TestDelegate(buttonOk));
            map.Add("CANCEL", new TestDelegate(buttonCancel));

            MyBoxViewer _box = new MyBoxViewer(title, message, map);
            
            //Disabled
            //DialogResult result = _box.ShowDialog();
            //Assert.IsTrue(result == DialogResult.OK);

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
