using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using Office = Microsoft.Office.Core;

namespace TaskMaster
{
    /// <summary>
    /// Issue #503 engine-command wiring for <see cref="RibbonViewer"/>.
    /// </summary>
    /// <remarks>
    /// Thin COM/VSTO glue only. The <c>[ComVisible(true)]</c> and <c>[ExcludeFromCodeCoverage]</c>
    /// attributes declared on the <c>RibbonViewer.cs</c> partial already apply to the whole type,
    /// so neither is repeated here and no new COM-visible type is introduced.
    /// </remarks>
    public partial class RibbonViewer
    {
        /// <summary>
        /// The Office <c>getEnabled</c> callback shared by all eight engine-backed controls.
        /// </summary>
        /// <param name="control">The control Office is querying.</param>
        /// <returns>
        /// <see langword="true"/> only when the control is engine-backed and its engine is loaded;
        /// <see langword="false"/> when the controller or the control is null, so a control this
        /// callback does not own is never disabled by it.
        /// </returns>
        /// <remarks>
        /// The signature is fixed by Office: a <c>public</c> instance method returning
        /// <see cref="bool"/> with a single <c>Office.IRibbonControl</c> parameter. VSTO silently
        /// ignores a signature mismatch — the code compiles and nothing happens — which is why
        /// <c>RibbonExplorerXmlTests</c> pins this signature by reflection.
        /// </remarks>
        /// <remarks>
        /// The null-forgiving operator on <c>control?.Id</c> records that a null id is a supported
        /// input: <c>EngineCommandCatalog.TryGetEngineName</c> returns <see langword="false"/> for
        /// it by contract, so the callback yields <see langword="false"/> rather than throwing.
        /// </remarks>
        public bool EngineCommand_GetEnabled(Office.IRibbonControl control) =>
            _controller?.IsEngineCommandEnabled(control?.Id!) ?? false;

        /// <summary>
        /// Invalidates every engine-backed control so Office re-queries
        /// <see cref="EngineCommand_GetEnabled"/> after engine initialization completes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Office caches each <c>getEnabled</c> response per control until the add-in invalidates
        /// it, so this call is load-bearing: without it the eight buttons stay disabled for the
        /// whole session even after <c>InitAsync()</c> succeeds.
        /// </para>
        /// <para>
        /// <c>IRibbonUI</c> is an Office COM object handed to <c>Ribbon_Load</c> on the STA and
        /// must be called back on the STA. The marshalling is therefore explicit through
        /// <c>UtilitiesCS.UiThread.Dispatcher</c> (declared in <c>UtilitiesCS\Threading\UiThread.cs</c>,
        /// namespace <c>UtilitiesCS</c>) rather than left to the ambient synchronization context:
        /// <c>InitAsync()</c> is launched via <c>Task.Run</c> and only resumes on the STA when a
        /// synchronization context happened to be captured, which is not true on every load path.
        /// </para>
        /// <para>
        /// Returns without throwing when the ribbon has not been loaded yet.
        /// </para>
        /// </remarks>
        internal void InvalidateEngineCommands()
        {
            var ribbon = _ribbon;
            if (ribbon is null)
            {
                return;
            }

            var dispatcher = UiThread.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() =>
                    EngineCommandRefreshPlanner.InvalidateAll(ribbon.InvalidateControl)
                );
                return;
            }

            EngineCommandRefreshPlanner.InvalidateAll(ribbon.InvalidateControl);
        }

        #region Spam Manager

        public async void ClearSpam_Click(Office.IRibbonControl control) =>
            await Controller.ClearSpamManagerAsync();

        public async void TrainSpam_Click(Office.IRibbonControl control) =>
            await Controller.RunEngineCommandAsync(
                "TrainSpam",
                () => Controller.SB.TrainAsync(Controller.OlSelection, true)
            );

        public async void TrainHam_Click(Office.IRibbonControl control) =>
            await Controller.RunEngineCommandAsync(
                "TrainHam",
                () => Controller.SB.TrainAsync(Controller.OlSelection, false)
            );

        public async void TestSpam_Click(Office.IRibbonControl control) =>
            await Controller.RunEngineCommandAsync(
                "TestSpam",
                () =>
                    (
                        (SpamBayes)Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine
                    ).TestAsync(Controller.OlSelection)
            );

        public void TestSpamVerbose_Click(Office.IRibbonControl control) =>
            Controller.TestSpamVerbose();

        public void SpamMetrics_Click(Office.IRibbonControl control) => Controller.SpamMetrics();

        public void SpamInvestigateErrors_Click(Office.IRibbonControl control) =>
            Controller.SpamInvestigateErrors();

        #region Spam Config

        public void SpamBayesEnabled_Click(Office.IRibbonControl control, bool pressed) =>
            Controller.Engines.ToggleEngineAsync(SpamBayes.GroupName);

        public async Task<bool> SpamBayesEnabled_GetPressed(Office.IRibbonControl control) =>
            await Controller.Engines.EngineActiveAsync(SpamBayes.GroupName);

        public async void SpamSaveNetwork_Click(Office.IRibbonControl control) =>
            await Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, false);

        public async void SpamSaveLocal_Click(Office.IRibbonControl control) =>
            await Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, true);

        public void GetSaveLocation_Click(Office.IRibbonControl control) =>
            Controller.Engines.ShowSaveInfo(SpamBayes.GroupName);

        public void SpamFolderSettings_Click(Office.IRibbonControl control) =>
            Controller.FolderStoresSettings();

        #endregion Spam Config

        #endregion Spam Manager

        #region Triage

        public async void TriageSelection_Click(Office.IRibbonControl control) =>
            await _controller.TriageSelectionAsync();

        public async void TriageSetA_Click(Office.IRibbonControl control) =>
            await Controller.RunEngineCommandAsync(
                "TriageSetA",
                () => _controller.Triage.OlLogic.TrainSelectionAsync("A")
            );

        public async void TriageSetB_Click(Office.IRibbonControl control) =>
            await Controller.RunEngineCommandAsync(
                "TriageSetB",
                () => _controller.Triage.OlLogic.TrainSelectionAsync("B")
            );

        public async void TriageSetC_Click(Office.IRibbonControl control) =>
            await Controller.RunEngineCommandAsync(
                "TriageSetC",
                () => _controller.Triage.OlLogic.TrainSelectionAsync("C")
            );

        //public async void TriageSetA_Click(Office.IRibbonControl control) => await _controller.TriageSetAAsync();
        //public async void TriageSetB_Click(Office.IRibbonControl control) => await _controller.TriageSetBAsync();
        //public async void TriageSetC_Click(Office.IRibbonControl control) => await _controller.TriageSetCAsync();

        public async void ClearTriage_Click(Office.IRibbonControl control) =>
            await Controller.RunEngineCommandAsync(
                "ClearTriage",
                () => _controller.Triage.OlLogic.UnTrainSelectionAsync()
            );

        public async void ResetTriage_Click(Office.IRibbonControl control) =>
            await _controller.ResetTriageClassifierAync();

        public async void SetPrecision_Click(Office.IRibbonControl control) =>
            await _controller.TriageSetPrecision();

        public async void FilterViewer_Click(Office.IRibbonControl control) =>
            await Controller.RunEngineCommandAsync(
                "FilterTriageGroup",
                () => _controller.Triage.OlLogic.FilterViewAsync()
            );

        #region Triage Config

        public void TriageEnabled_Click(Office.IRibbonControl control, bool pressed) =>
            Controller.Engines.ToggleEngineAsync("Triage");

        public async Task<bool> TriageEnabled_GetPressed(Office.IRibbonControl control) =>
            await Controller.Engines.EngineActiveAsync("Triage");

        public async void TriageSaveNetwork_Click(Office.IRibbonControl control) =>
            await Controller.Engines.ShowDiskDialog("Triage", false);

        public async void TriageSaveLocal_Click(Office.IRibbonControl control) =>
            await Controller.Engines.ShowDiskDialog("Triage", true);

        public void TriageGetSaveLocation_Click(Office.IRibbonControl control) =>
            Controller.Engines.ShowSaveInfo("Triage");

        #endregion Triage Config

        #endregion Triage
    }
}
