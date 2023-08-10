using System;
using ToDoModel;
using System.Threading.Tasks;

namespace QuickFiler.Interfaces
{
	public interface IFilerHomeController
	{
        #region Constructors, Initializers, and Destructors
        
        void Run();
        void Cleanup();
        
        #endregion

        #region Public Properties

        bool Loaded { get; }
		cStopWatch StopWatch { get; }
        //IQfcDatamodel DataModel { get; }
        IQfcExplorerController ExplorerCtlr { get; set; }
		IFilerFormController FormCtrlr { get; }
		IQfcKeyboardHandler KeyboardHndlr { get; set; }
        //QfcFormViewer FormViewer { get; }

        #endregion

        #region Major Actions
        
        Task ExecuteMoves();        
		//void Iterate();
        void QuickFileMetrics_WRITE(string filename);

        #endregion
    }
}