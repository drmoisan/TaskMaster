namespace QuickFiler
{
	public interface IQfcHomeController
	{
		IQfcExplorerController ExplCtrlr { get; set; }
		IQfcFormController FrmCtrlr { get; set; }
		IQfcCollectionController QfcColCtrlr { get; set; }
		IQfcKeyboardHandler KbdHndlr { get; set; }

		void Iterate();
	}
}