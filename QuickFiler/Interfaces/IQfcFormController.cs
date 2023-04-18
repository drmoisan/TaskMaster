namespace QuickFiler
{
    public interface IQfcFormController
    {

        void FormResize(bool Force = false); // might not be necessary
        void ButtonCancel_Click();
        void ButtonOK_Click();
        void ButtonUndo_Click();
        void Cleanup();
        void QFD_Maximize();
        void QFD_Minimize();
        void SpnEmailPerLoad_Change();
        void Viewer_Activate();
        int MaxPixelsForEmail { get; }
    }
}