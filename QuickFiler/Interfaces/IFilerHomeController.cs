using System;
using ToDoModel;
using System.Threading.Tasks;
using System.Threading;
using UtilitiesCS;

namespace QuickFiler.Interfaces
{
	public interface IFilerHomeController
	{
        #region Constructors, Initializers, and Destructors
        
        void Run();
        Task RunAsync(ProgressTracker progress);
        void Cleanup();
        
        #endregion

        #region Public Properties

        SynchronizationContext UiSyncContext { get; }
        CancellationTokenSource TokenSource { get; }
        CancellationToken Token { get; }
        bool Loaded { get; }
		StopWatch StopWatch { get; }
        //IQfcDatamodel DataModel { get; }
        IQfcExplorerController ExplorerCtlr { get; set; }
		IFilerFormController FormCtrlr { get; }
		IQfcKeyboardHandler KeyboardHndlr { get; set; }
        //QfcFormViewer FormViewer { get; }

        #endregion

        #region Major Actions
        
		//void Iterate();
        void QuickFileMetrics_WRITE(string filename);

        #endregion
    }
}