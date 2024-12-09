using System;
using ToDoModel;
using System.Threading.Tasks;
using System.Threading;
using UtilitiesCS;
using System.Diagnostics;
using QuickFiler.Controllers;

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
		Stopwatch StopWatch { get; }
        //IQfcDatamodel DataModel { get; }
        IQfcExplorerController ExplorerController { get; set; }
		IFilerFormController FormController { get; }
		IQfcKeyboardHandler KeyboardHandler { get; set; }
        FilerQueue FilerQueue { get; }
        //QfcFormViewer FormViewer { get; }

        #endregion

        #region Major Actions

        //void Iterate();
        void QuickFileMetrics_WRITE(string filename);

        #endregion
    }
}