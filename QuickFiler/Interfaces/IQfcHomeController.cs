using System;
using ToDoModel;

namespace QuickFiler.Interfaces
{
	public interface IQfcHomeController
	{
        IQfcExplorerController ExplCtrlr { get; set; }
		IQfcFormController FrmCtrlr { get; }
		IQfcKeyboardHandler KbdHndlr { get; set; }
        IQfcDatamodel DataModel { get; }
        bool Loaded { get; }
		void Run();
		void Iterate();
		void ExecuteMoves();
		cStopWatch StopWatch { get; }
	}
}