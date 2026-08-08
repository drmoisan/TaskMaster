using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using Office = Microsoft.Office.Core;

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
    [ExcludeFromCodeCoverage]
    public partial class RibbonViewer : Office.IRibbonExtensibility
    {
        public RibbonViewer(RibbonController Controller)
        {
            _controller = Controller;
            _loadFolderFilterAsync = () => _controller.Try.TryLoadFolderFilterAsync();
            _reportFolderFilterInitializationFailure = ReportFolderFilterInitializationFailure;
        }

        internal RibbonViewer(
            Func<Task> loadFolderFilterAsync,
            Action<System.Exception> reportFolderFilterInitializationFailure
        )
        {
            _controller = null;
            _loadFolderFilterAsync =
                loadFolderFilterAsync
                ?? throw new ArgumentNullException(nameof(loadFolderFilterAsync));
            _reportFolderFilterInitializationFailure =
                reportFolderFilterInitializationFailure
                ?? throw new ArgumentNullException(nameof(reportFolderFilterInitializationFailure));
        }

        private Office.IRibbonUI _ribbon;
        private RibbonController _controller;
        private readonly Func<Task> _loadFolderFilterAsync;
        private readonly Action<System.Exception> _reportFolderFilterInitializationFailure;
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );
        internal RibbonController Controller => _controller;

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

        public async void BtnHideHeadersNoChildren_Click(Office.IRibbonControl control)
        {
            await _controller.HideHeadersNoChildrenAsync();
        }

        public async void BtnShowHeadersNoChildren_Click(Office.IRibbonControl control)
        {
            await _controller.ShowHeadersNoChildrenAsync();
        }

        public void BtnRefreshIDList_Click(Office.IRibbonControl control)
        {
            _controller.RefreshIDList();
        }

        public async void BtnSplitToDoID_Click(Office.IRibbonControl control)
        {
            await _controller.SplitToDoIdAsync();
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

        public void BtnPopulateUdf_Click(Office.IRibbonControl control) =>
            _controller.PopulateUdf();

        public async void QuickFiler_Click(Office.IRibbonControl control)
        {
            await _controller.LoadQuickFilerAsync();
        }

        public async void QuickFilerHighConfidence_Click(Office.IRibbonControl control)
        {
            await _controller.LoadQuickFilerHighConfidenceAsync();
        }

        public async void SortEmail_Click(Office.IRibbonControl control) =>
            await _controller.SortEmailAsync();

        public async void UndoSort_Click(Office.IRibbonControl control) =>
            await _controller.UndoSortAsync();

        public async void FindFolder_Click(Office.IRibbonControl control) =>
            await _controller.FindFolderAsync();

        public bool ToggleDarkMode_GetPressed(Office.IRibbonControl control) =>
            _controller.IsDarkModeActive();

        public void ToggleDarkMode_Click(Office.IRibbonControl control, bool pressed) =>
            _controller.ToggleDarkMode();

        #endregion

        #region SettingsMenu

        public bool MoveEntireConversation_GetPressed(Office.IRibbonControl control) =>
            _controller.IsMoveEntireConversationActive();

        public void MoveEntireConversation_Click(Office.IRibbonControl control, bool pressed) =>
            _controller.ToggleMoveEntireConversation();

        public bool SaveAttachments_GetPressed(Office.IRibbonControl control) =>
            _controller.IsSaveAttachmentsActive();

        public void SaveAttachments_Click(Office.IRibbonControl control, bool pressed) =>
            _controller.ToggleSaveAttachments();

        public bool SaveEmailCopy_GetPressed(Office.IRibbonControl control) =>
            _controller.IsSaveEmailCopyActive();

        public void SaveEmailCopy_Click(Office.IRibbonControl control, bool pressed) =>
            _controller.ToggleSaveEmailCopy();

        public bool SavePictures_GetPressed(Office.IRibbonControl control) =>
            _controller.IsSavePicturesActive();

        public void SavePictures_Click(Office.IRibbonControl control, bool pressed) =>
            _controller.ToggleSavePictures();

        public void FolderSettings_Click(Office.IRibbonControl control) =>
            _controller.FolderStoresSettings();

        public void DisabledStoresSettings_Click(Office.IRibbonControl control) =>
            _controller.DisabledStoresSettings();

        public string HighConfidenceThreshold_GetText(Office.IRibbonControl control) =>
            _controller.GetHighConfidenceThresholdText();

        public void HighConfidenceThreshold_OnChange(Office.IRibbonControl control, string text) =>
            _controller.SetHighConfidenceThresholdText(text);

        #endregion SettingsMenu

        #region BayesianPerformance

        public async void TestClassifier_Click(Office.IRibbonControl control) =>
            await _controller.Try.TryTestClassifierAsync();

        public async void TestClassifierVerbose_Click(Office.IRibbonControl control) =>
            await _controller.Try.TryTestClassifierVerboseAsync();

        public async void GetConfusionDrivers_Click(Office.IRibbonControl control) =>
            await _controller.GetConfusionDriversAsync();

        public async void ChartMetrics_Click(Office.IRibbonControl control) =>
            await _controller.TryChartMetricsAsync();

        public async void InvestigateErrors_Click(Office.IRibbonControl control) =>
            await _controller.InvestigateErrorsAsync();

        #endregion BayesianPerformance

        #region Folder Classifier

        public async void ScrapeAndMine_Click(Office.IRibbonControl control) =>
            await _controller.ScrapeAndMineAsync();

        public async void BuildFolderClassifier_Click(Office.IRibbonControl control) =>
            await _controller.BuildFolderClassifierAsync();

        public async void BuildCategoryClassifier_Click(Office.IRibbonControl control) =>
            await _controller.BuildCategoryClassifierAsync();

        public async void BuildActionableClassifier_Click(Office.IRibbonControl control) =>
            await _controller.BuildActionableClassifierAsync();

        #endregion Folder Classifier

        public async void CompareFolders_Click(Office.IRibbonControl control) =>
            await _controller.CompareFoldersAsync();

        #region TryMethods
        public void NewTaskHeader_Click(Office.IRibbonControl control) =>
            _controller.Try.TryNewTaskHeader();

        public void DeepCompareEmails_Click(Office.IRibbonControl control) =>
            _controller.TryDeepCompareEmails();

        public void GetConversationDataframe_Click(Office.IRibbonControl control) =>
            _controller.Try.TryGetConversationDataframe();

        public void GetConversationOutlookTable_Click(Office.IRibbonControl control) =>
            _controller.Try.TryGetConversationOutlookTable();

        public void GetMailItemInfo_Click(Office.IRibbonControl control) =>
            _controller.Try.TryGetMailItemInfo();

        public void GetQfcDataModel_Click(Office.IRibbonControl control) =>
            _controller.Try.TryGetQfcDataModel();

        public void GetTableInView_Click(Office.IRibbonControl control) =>
            _controller.Try.TryGetTableInView();

        public void RebuildProjInfo_Click(Office.IRibbonControl control) =>
            _controller.Try.TryRebuildProjInfo();

        public void RecipientGetInfo_Click(Office.IRibbonControl control) =>
            _controller.Try.TryRecipientGetInfo();

        public void SubstituteIdRoot_Click(Office.IRibbonControl control) =>
            _controller.Try.TrySubstituteIdRoot();

        public void GetImage_Click(Office.IRibbonControl control) => _controller.Try.TryGetImage();

        public void LoadFolderFilter_Click(Office.IRibbonControl control) =>
            RunFolderFilterCallback();

        internal async void RunFolderFilterCallback()
        {
            try
            {
                await _loadFolderFilterAsync();
            }
            catch (System.Exception exception)
            {
                try
                {
                    _reportFolderFilterInitializationFailure(exception);
                }
                catch (System.Exception reporterException)
                {
                    logger.Error(
                        "Unable to report the folder-filter initialization failure.",
                        reporterException
                    );
                }
            }
        }

        private static void ReportFolderFilterInitializationFailure(System.Exception exception)
        {
            logger.Error("Unable to initialize the folder-filter viewer.", exception);
            MessageBox.Show($"Unable to initialize the folder-filter viewer: {exception.Message}");
        }

        public void LoadFolderRemap_Click(Office.IRibbonControl control) =>
            _controller.Try.TryLoadFolderRemap();

        public async void RebuildSubjectMap_Click(Office.IRibbonControl control) =>
            await _controller.Try.RebuildSubjectMapAsync();

        public void ShowSubjectMapMetrics_Click(Office.IRibbonControl control) =>
            _controller.Try.ShowSubjectMapMetrics();

        public async void TokenizeEmail_Click(Office.IRibbonControl control) =>
            await _controller.Try.TryTokenizeEmail();

        public async void MineEmails_Click(Office.IRibbonControl control) =>
            await _controller.Try.TryMineEmails();

        public async void BuildClassifier_Click(Office.IRibbonControl control) =>
            await _controller.Try.TryBuildClassifier();

        public void PrintManagerState_Click(Office.IRibbonControl control) =>
            _controller.Try.TryPrintManagerState();

        public void SerializeMailInfo_Click(Office.IRibbonControl control) =>
            _controller.Try.TrySerializeMailInfo();

        public void TryGetInboxes_Click(Office.IRibbonControl control) =>
            _controller.Try.TryGetInboxes();
        #endregion

        public void DeleteTriageSpamFields_Click(Office.IRibbonControl control) =>
            _controller.TryDeleteTriageSpamFields();

        public async void Intelligence_Click(Office.IRibbonControl control) =>
            await _controller.IntelligenceAsync();

        public void GetFolderInfo_Click(Office.IRibbonControl control) =>
            _controller.GetFolderInfo();

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0, loopTo = resourceNames.Length - 1; i <= loopTo; i++)
            {
                if (
                    string.Compare(
                        resourceName,
                        resourceNames[i],
                        StringComparison.OrdinalIgnoreCase
                    ) == 0
                )
                {
                    using (
                        var resourceReader = new System.IO.StreamReader(
                            asm.GetManifestResourceStream(resourceNames[i])
                        )
                    )
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
