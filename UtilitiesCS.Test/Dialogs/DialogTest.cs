using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace Z.Disabled.UtilitiesCS.Test
{
    [TestClass]
    public class DialogTest
    {
        private delegate DialogResult TestDelegate();

        [TestMethod]
        public void ButtonDelegates_ShouldReturnExpectedDialogResults()
        {
            Dictionary<string, Delegate> map = new Dictionary<string, Delegate>();
            map.Add("OK", new TestDelegate(buttonOk));
            map.Add("CANCEL", new TestDelegate(buttonCancel));

            map["OK"].DynamicInvoke().Should().Be(DialogResult.OK);
            map["CANCEL"].DynamicInvoke().Should().Be(DialogResult.Cancel);
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
