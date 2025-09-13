using System;
using Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Core;
using UtilitiesCS;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UtilitiesCS.Threading;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
[assembly: InternalsVisibleTo("TaskMaster.Test")]
namespace TaskMaster
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            logger.Debug("ThisAddIn_Startup() fired");

            // Ensure that forms are ready for high resolution
            InitializeDPI();

            // Grab the sync context for the UI thread
            UiThread.Init(monitorUiThread: false);

            Application.Startup += Application_Startup;
        }

        private void Application_Startup()
        {
            logger.Debug("Application_Startup() fired");
            //IdleAsyncQueue.AddEntry(false, async () => await Task.Run(() => 
            //{ 
            SetUpBrightIdeasSettings();
            SetUpDeedle();
            //}));

            _globals = new ApplicationGlobals(Application, true);
            _ribbonController.SetGlobals(_globals);
            _externalUtilities.SetGlobals(_globals, _ribbonController);

            IdleAsyncQueue.AddEntry(true, async () =>
            {                
                await _globals.LoadAsync(false);
                logger.Debug("Finished loading globals");
            });
            
            //IdleAsyncQueue.AddEntry(false, async () => await Task.Run(() => _ribbonController.SetGlobals(_globals)));
            //IdleAsyncQueue.AddEntry(false, async () => await Task.Run(() => _externalUtilities.SetGlobals(_globals, _ribbonController)));
            IdleAsyncQueue.AddEntry(false, async () => await Task.Run(() => logger.Debug("IdleAsyncQueue Complete")));
            logger.Debug("Application_Startup() complete");
        }

        private void SetUpDeedle()
        {
            // Redirect the console output to the debug window for Deedle df.Print() calls
            DebugTextWriter tw = new();
            Console.SetOut(tw);
        }

        /// <summary>
        /// Set the indent for TreeListView Renderer which does not autoscale.
        /// Default pixels per level was 16 + 1 but designed for 100% scaling.
        /// This add-in is designed for 200% scaling.
        /// </summary>
        private void SetUpBrightIdeasSettings()
        {            
            var tlvIndent = 34;
            tlvIndent = (int)(tlvIndent * UiThread.AutoScaleFactor.Width);
            BrightIdeasSoftware.TreeListView.TreeRenderer.PIXELS_PER_LEVEL = tlvIndent;
        }

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ApplicationGlobals _globals;
        private AddInUtilities _externalUtilities;
        private RibbonController _ribbonController;
        
        /// <summary>
        /// Overrides the default behavior of the COM add-in to create an XML ribbon
        /// <seealso cref="RibbonViewer"/> which is controlled by 
        /// <seealso cref="RibbonController"/>.
        /// </summary>
        /// <returns><seealso cref="IRibbonExtensibility"/> object</returns>
        protected override IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            _ribbonController = new RibbonController();
            return new RibbonViewer(_ribbonController);
        }

        /// <summary>
        /// Sets the DPI awareness for the application to enable high resolution with text scaling
        /// </summary>
        [STAThread]
        public static void InitializeDPI()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
        }

        /// <summary>
        /// Overrides the default behavior of the COM add-in to expose specific methods 
        /// to other office applications so that they can be called from VBA.
        /// </summary>
        /// <returns>Instance of the <seealso cref="AddInUtilities"/> class</returns>
        protected override object RequestComAddInAutomationService()
        {
            _externalUtilities ??= new AddInUtilities();

            return _externalUtilities;
        }

        //private async Task FinishLoadingGlobalsAsync()
        //{
        //    await loadGlobals;
        //    logger.Debug("Finished loading globals");

        //}

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Note: Outlook no longer raises this event. If you have code that 
            //    must run when Outlook shuts down, see https://go.microsoft.com/fwlink/?LinkId=506785
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
