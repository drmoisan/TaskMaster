using QuickFiler.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS.Interfaces.IWinForm;

namespace QuickFiler
{
    public interface IQfcFormViewer : IForm
    {
        List<Control> Buttons { get; }
        List<Control> Panels { get; }
        TaskScheduler UiScheduler { get; }
        SynchronizationContext UiSyncContext { get; }
        System.ComponentModel.BackgroundWorker Worker { get; }

        void SetController(IFilerFormController controller);
        void SetKeyboardHandler(IQfcKeyboardHandler keyboardHandler);

        TableLayoutPanel L1v0L2L3v_TableLayout { get; set; }
        ItemViewer QfcItemViewerTemplate { get; }
        ItemViewerExpanded QfcItemViewerExpandedTemplate { get; }
        TableLayoutPanel L1v_TableLayout { get; }
        System.Windows.Forms.NumericUpDown L1v1L2h5_SpnEmailPerLoad { get; }
        System.Windows.Forms.Button L1v1L2h2_ButtonOK { get; }
        System.Windows.Forms.Button L1v1L2h3_ButtonCancel { get; }
        System.Windows.Forms.Button L1v1L2h4_ButtonUndo { get; }
        System.Windows.Forms.Button L1v1L2h5_BtnSkip { get; }
        Panel L1v0L2_PanelMain { get; }

    }
}