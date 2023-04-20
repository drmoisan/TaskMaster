namespace QuickFiler.Interfaces
{
	public interface IQfcHomeController
	{
		IQfcExplorerController ExplCtrlr { get; set; }
		IQfcFormController FrmCtrlr { get; set; }
		IQfcKeyboardHandler KbdHndlr { get; set; }
		bool Loaded { get; }
		void Run();
		void Iterate();
	}
}