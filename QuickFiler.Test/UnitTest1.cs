using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Forms;
using UtilitiesCS;
using Moq;

namespace QuickFiler.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            QfcFormViewer qf = new QfcFormViewer();
            QfcItemViewer iv = new QfcItemViewer();
            Mock<IQfcFormController> formController = new Mock<IQfcFormController>();
            int itemHeight = iv.Height;
            formController.Setup(x => x.ButtonCancel_Click()).Callback(qf.Hide);
            formController.Setup(x => x.ButtonOK_Click()).Callback(qf.Hide);
            qf.SetController(formController.Object);
            qf.Show();
            qf.Refresh();
            
            RowStyle rowStyle = new RowStyle(SizeType.Absolute, itemHeight);
            TableLayoutHelper.InsertSpecificRow(panel: qf.L1v0L2L3v_TableLayout, rowIndex: 0, newStyle: rowStyle);
            qf.L1v0L2L3v_TableLayout.Height += itemHeight;

            qf.Refresh();

            
            qf.L1v0L2L3v_TableLayout.Controls.Add(iv, 0, 0);
            iv.AutoSize = true;
            iv.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            iv.BorderStyle = BorderStyle.FixedSingle;
            iv.Dock = DockStyle.Fill;

            //iv.Show(); 
            //qf.Refresh();
            qf.Hide();
            qf.ShowDialog();
        }
    }
}
