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
        public RibbonViewer(RibbonController Controller)
        {
            _controller = Controller;
        }
        
        private Office.IRibbonUI _ribbon;
        private RibbonController _controller;

        public void SetController(RibbonController Controller)
        {
            _controller = Controller;
        }

        public string GetCustomUI(string ribbonID)
        {
            switch (ribbonID)
            {
                case "Microsoft.Outlook.Explorer":
                    return GetResourceText("TaskMaster.Ribbon.RibbonExplorer.xml");
                //case "Microsoft.Outlook.Mail.Compose":
                //    return GetResourceText("TaskMaster.Ribbon.RibbonMailCompose.xml");
                //case "Microsoft.Outlook.Appointment":
                //    return GetResourceText("TaskMaster.Ribbon.RibbonAppointment.xml");
                default:
                    return null;
            }
            //return GetResourceText("TaskMaster.Ribbon.RibbonViewer.xml");
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
            _controller.ReviseProjectData();
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

        public void BtnPopulateUdf_Click(Office.IRibbonControl control) => _controller.PopulateUdf();
        
        //public void QuickFilerOld_Click(Office.IRibbonControl control)
        //{
        //    _controller.LoadQuickFilerOld();
        //}

        //public void QuickFiler_Click(Office.IRibbonControl control)
        //{
        //    _controller.LoadQuickFiler();
        //}

        public async void QuickFiler_Click(Office.IRibbonControl control)
        {
            await _controller.LoadQuickFilerAsync();
        }

        public void Runtest(Office.IRibbonControl control)
        {
            _controller.RunTry();
        }

        public async void SortEmail_Click(Office.IRibbonControl control) => await _controller.SortEmailAsync();

        public void UndoSort_Click(Office.IRibbonControl control)
        {
            _controller.UndoSort();
        }

        public bool ToggleDarkMode_GetPressed(Office.IRibbonControl control) => _controller.IsDarkModeActive();
        public void ToggleDarkMode_Click(Office.IRibbonControl control, bool pressed) => _controller.ToggleDarkMode();

        #endregion

        #region SettingsMenu

        public bool MoveEntireConversation_GetPressed(Office.IRibbonControl control) => _controller.IsMoveEntireConversationActive();
        public void MoveEntireConversation_Click(Office.IRibbonControl control, bool pressed) => _controller.ToggleMoveEntireConversation();
        
        public bool SaveAttachments_GetPressed(Office.IRibbonControl control) => _controller.IsSaveAttachmentsActive();
        public void SaveAttachments_Click(Office.IRibbonControl control, bool pressed) => _controller.ToggleSaveAttachments();
        
        public bool SaveEmailCopy_GetPressed(Office.IRibbonControl control) => _controller.IsSaveEmailCopyActive();
        public void SaveEmailCopy_Click(Office.IRibbonControl control, bool pressed) => _controller.ToggleSaveEmailCopy();
        
        public bool SavePictures_GetPressed(Office.IRibbonControl control) => _controller.IsSavePicturesActive();
        public void SavePictures_Click(Office.IRibbonControl control, bool pressed) => _controller.ToggleSavePictures();


        #endregion SettingsMenu

        #region BayesianPerformance

        public async void TestClassifier_Click(Office.IRibbonControl control) => await _controller.TryTestClassifier();
        public async void TestClassifierVerbose_Click(Office.IRibbonControl control) => await _controller.TryTestClassifierVerbose();
        public async void GetConfusionDrivers_Click(Office.IRibbonControl control) => await _controller.GetConfusionDrivers();
        public async void ChartMetrics_Click(Office.IRibbonControl control) => await _controller.TryChartMetrics();
        public async void InvestigateErrors_Click(Office.IRibbonControl control) => await _controller.InvestigateErrorsAsync();

        #endregion BayesianPerformance

        #region TryMethods  
        public void DeepCompareEmails_Click(Office.IRibbonControl control) => _controller.TryDeepCompareEmails();
        public void GetConversationDataframe_Click(Office.IRibbonControl control) => _controller.TryGetConversationDataframe();
        public void GetConversationOutlookTable_Click(Office.IRibbonControl control) => _controller.TryGetConversationOutlookTable();
        public void GetMailItemInfo_Click(Office.IRibbonControl control) => _controller.TryGetMailItemInfo();
        public void GetQfcDataModel_Click(Office.IRibbonControl control) => _controller.TryGetQfcDataModel();
        public void GetTableInView_Click(Office.IRibbonControl control) => _controller.TryGetTableInView();
        public void RebuildProjInfo_Click(Office.IRibbonControl control) => _controller.TryRebuildProjInfo();
        public void RecipientGetInfo_Click(Office.IRibbonControl control) => _controller.TryRecipientGetInfo();
        public void SubstituteIdRoot_Click(Office.IRibbonControl control) => _controller.TrySubstituteIdRoot();
        public void GetImage_Click(Office.IRibbonControl control) => _controller.TryGetImage();
        public void LoadFolderFilter_Click(Office.IRibbonControl control) => _controller.TryLoadFolderFilter();
        public void LoadFolderRemap_Click(Office.IRibbonControl control) => _controller.TryLoadFolderRemap();
        public async void RebuildSubjectMap_Click(Office.IRibbonControl control) => await _controller.RebuildSubjectMapAsync();
        public void ShowSubjectMapMetrics_Click(Office.IRibbonControl control) => _controller.ShowSubjectMapMetrics();
        public async void TokenizeEmail_Click(Office.IRibbonControl control) => await _controller.TryTokenizeEmail();
        public async void MineEmails_Click(Office.IRibbonControl control) => await _controller.TryMineEmails();
        public async void BuildClassifier_Click(Office.IRibbonControl control) => await _controller.TryBuildClassifier();        
        public void PrintManagerState_Click(Office.IRibbonControl control) => _controller.TryPrintManagerState();
        public void SaveManagerLocally_Click(Office.IRibbonControl control) => _controller.TrySaveManagerLocally();
        public void SaveManagerNetwork_Click(Office.IRibbonControl control) => _controller.TrySaveManagerNetwork();
        public void SerializeMailInfo_Click(Office.IRibbonControl control) => _controller.TrySerializeMailInfo();
        

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