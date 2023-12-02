using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using ToDoModel;
using Microsoft.Office.Core;
using QuickFiler;
using UtilitiesCS;
using UtilitiesCS.Threading;


[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

namespace TaskMaster
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            logger.Debug($"Application Starting");
            // Ensure that forms are ready for high resolution
            InitializeDPI();

            // Create the global variables
            _globals = new ApplicationGlobals(Application);

            // Grab the sync context for the UI thread
            UIThreadExtensions.InitUiContext(monitorUiThread: false);

            // Set the indent for TreeListView Renderer which does not autoscale.
            // Default pixels per level was 16 + 1 but designed for 100% scaling.
            // This add-in is designed for 200% scaling.
            var tlvIndent = 34; 
            tlvIndent = (int)(tlvIndent * UIThreadExtensions.AutoScaleFactor.Width);
            BrightIdeasSoftware.TreeListView.TreeRenderer.PIXELS_PER_LEVEL = tlvIndent;

            // Initialize the global variables on a low priority thread
            _ = _globals.LoadAsync();

            // Initialize long loading elements on a low priority thread
            EfcViewerQueue.BuildQueue(2);
            ItemViewerQueue.BuildQueueBackground(30);
            
            // Initialize IdleAction Queue so that breakpoint is hit after UI
            IdleActionQueue.AddEntry(()=>Debug.WriteLine("App Idle"));
            //IdleActionQueue.AddEntry(() => _globals.TD.LoadPrefixList());

            // Redirect the console output to the debug window for Deedle df.Print() calls
            DebugTextWriter tw = new DebugTextWriter();
            Console.SetOut(tw);
            
            // Send a reference to the ribbon controller and external utilities for future use
            _ribbonController.SetGlobals(_globals);
            _externalUtilities.SetGlobals(_globals, _ribbonController);

            // Hook the Inbox and ToDo events
            _globals.Events.Hook();
            logger.Debug("ThisAddIn_Startup() complete");

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
            if (_externalUtilities is null)
                _externalUtilities = new AddInUtilities();

            return _externalUtilities;
        }

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
