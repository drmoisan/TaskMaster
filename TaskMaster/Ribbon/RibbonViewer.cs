using System;
using Office = Microsoft.Office.Core;
using Microsoft.VisualBasic;
using System.Windows.Forms;

namespace TaskMaster
{
    // TODO:  Follow these steps to enable the Ribbon (XML) item:

    // 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

    // Protected Overrides Function CreateRibbonExtensibilityObject() As Microsoft.Office.Core.IRibbonExtensibility
    // Return New Ribbon()
    // End Function

    // 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
    // actions, such as clicking a button. Note: if you have exported this Ribbon from the
    // Ribbon designer, move your code from the event handlers to the callback methods and
    // modify the code to work with the Ribbon extensibility (RibbonX) programming model.

    // 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.

    // For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.

    [System.Runtime.InteropServices.ComVisible(true)]
    public class RibbonViewer : Office.IRibbonExtensibility
    {

        private Office.IRibbonUI _ribbon;
        private RibbonController _controller;

        public RibbonViewer(RibbonController Controller)
        {
            _controller = Controller;
        }

        public void SetController(RibbonController Controller)
        {
            _controller = Controller;
        }

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("TaskMaster.Ribbon.RibbonViewer.xml");
        }

        #region Ribbon Callbacks
        // Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226
        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            _ribbon = ribbonUI;
            _controller.SetViewer(this);
        }

        public void BtnLoadTree_Click(Office.IRibbonControl control)
        {
            _controller.LoadTaskTree();
        }

        public void FlagAsTask_Click(Office.IRibbonControl control)
        {
            _controller.FlagAsTask();
        }

        public void BtnHideHeadersNoChildren_Click(Office.IRibbonControl control)
        {
            _controller.HideHeadersNoChildren();
        }

        public void BtnRefreshIDList_Click(Office.IRibbonControl control)
        {
            _controller.RefreshIDList();
        }

        public void BtnSplitToDoID_Click(Office.IRibbonControl control)
        {
            _controller.SplitToDoID();
        }

        public void BtnReviseProjectInfo_Click(Office.IRibbonControl control)
        {
            _controller.ReviseProjectInfo();
        }

        public void BtnCompressIDs_Click(Office.IRibbonControl control)
        {
            _controller.CompressIDs();
        }

        public void BtnHookToggle_Click(Office.IRibbonControl control)
        {
            _controller.ToggleEventsHook(_ribbon);
        }

        public string GetHookButtonText(Office.IRibbonControl control)
        {
            return _controller.GetHookButtonText(control);
        }

        public void BtnMigrateIDs_Click(Office.IRibbonControl control)
        {
            _controller.BtnMigrateIDs_Click();
            // MessageBox.Show("Not Implemented");
        }

        public void QuickFilerOld_Click(Office.IRibbonControl control)
        {
            _controller.LoadQuickFilerOld();
        }

        public void QuickFiler_Click(Office.IRibbonControl control)
        {
            _controller.LoadQuickFiler();
        }

        public void Runtest(Office.IRibbonControl control)
        {
            _controller.Runtest();
        }

        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0, loopTo = resourceNames.Length - 1; i <= loopTo; i++)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (var resourceReader = new System.IO.StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader is not null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion

    }
}