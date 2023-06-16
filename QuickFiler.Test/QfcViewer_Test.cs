using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Forms;
using UtilitiesCS;
using Moq;
using System.Collections;
using System.Collections.Generic;
using QuickFiler.Controllers;
using QuickFiler.Interfaces;

namespace QuickFiler.Test
{
    [TestClass]
    public class QfcViewer_Test
    {
        private QfcFormViewer qf;
        private QfcItemViewer iv;
        private Mock<IQfcFormController> formController;

        

        [TestInitialize] public void Init() 
        {
            qf = new QfcFormViewer();
            qf.ShowDialog();
            qf.Refresh();
            iv = new QfcItemViewer();
            formController = new Mock<IQfcFormController>();
        }
        
        [TestMethod]
        public void TestMethod1()
        {
            //QfcFormViewer qf = new QfcFormViewer();
            //QfcItemViewer iv = new QfcItemViewer();
            //Mock<IQfcFormController> formController = new Mock<IQfcFormController>();
            int itemHeight = iv.Height;
            formController.Setup(x => x.ButtonCancel_Click()).Callback(qf.Hide);
            formController.Setup(x => x.ButtonOK_Click()).Callback(qf.Hide);
            qf.SetController(formController.Object);
            qf.Show();
            qf.Refresh();
            
            RowStyle rowStyle = new RowStyle(SizeType.Absolute, itemHeight);
            TableLayoutHelper.InsertSpecificRow(panel: qf.L1v0L2L3v_TableLayout, rowIndex: 0, templateStyle: rowStyle);
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

        [TestMethod]
        public void TestToggleTips() 
        {
            //qf.Show();
            qf.Refresh();

            IList<Label> tipsLabels = qf.QfcItemViewerTemplate.TipsLabels;
            IList<QfcTipsDetails> listTipsDetails = new List<QfcTipsDetails>();
            foreach (Label tipsLabel in tipsLabels) 
            {
                listTipsDetails.Add(new QfcTipsDetails(tipsLabel));
            }
            qf.Refresh();
            qf.Refresh();

            foreach (QfcTipsDetails tipsDetails in listTipsDetails)
            {
                tipsDetails.Toggle(IQfcTipsDetails.ToggleState.Off);
            }
            qf.Refresh();
            qf.Refresh();

            foreach (QfcTipsDetails tipsDetails in listTipsDetails)
            {
                tipsDetails.Toggle();
            }
            qf.Refresh();
            qf.Refresh();
            //qf.qfcItemViewer1
        }
    }
}
