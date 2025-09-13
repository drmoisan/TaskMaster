using BrightIdeasSoftware;
using Microsoft.Web.WebView2.WinForms;
using QuickFiler.Viewers;
using SVGControl;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using UtilitiesCS.Interfaces.IWinForm;

namespace QuickFiler
{
    public interface IItemViewer: IUserControl
    {
        ButtonSVG BtnDelItem { get; set; }
        ButtonSVG BtnFlagTask { get; set; }
        ButtonSVG BtnForward { get; set; }
        ButtonSVG BtnPopOut { get; set; }
        ButtonSVG BtnReply { get; set; }
        ButtonSVG BtnReplyAll { get; set; }
        ComboBox CboFolders { get; set; }
        IItemControler Controller { get; set; }
        ToolStripMenuItemCb ConversationMenuItem { get; set; }
        IList<Label> ExpandedTipsLabels { get; }
        OLVColumn Infolder { get; set; }
        WebView2 L0v2h2_WebView2 { get; set; }
        TableLayoutPanel L0vh_Tlp { get; set; }
        TableLayoutPanel L1h0L2hv3h_TlpBodyToggle { get; set; }
        Panel L1h1L2v1h3Panel { get; set; }
        Label LblAcBody { get; set; }
        Label LblAcDelete { get; set; }
        Label LblAcFolder { get; set; }
        Label LblAcFwd { get; set; }
        Label LblAcMoveOptions { get; set; }
        Label LblAcOpen { get; set; }
        Label LblAcPopOut { get; set; }
        Label LblAcReply { get; set; }
        Label LblAcReplyAll { get; set; }
        Label LblAcSearch { get; set; }
        Label LblAcTask { get; set; }
        Label LblActionable { get; set; }
        Label LblCaptionPredicted { get; set; }
        Label LblCaptionTriage { get; set; }
        Label LblConvCt { get; set; }
        Label LblFolder { get; set; }
        Label LblItemNumber { get; set; }
        Label LblSearch { get; set; }
        Label LblSender { get; set; }
        Label LblSentOn { get; set; }
        Label LblSubject { get; set; }
        Label LblTriage { get; set; }
        IList<Label> LeftTipsLabels { get; }
        List<Component> MenuItems { get; }
        ToolStripMenuItem MoveOptionsMenu { get; set; }
        MenuStrip MoveOptionsStrip { get; set; }
        ToolStripMenuItemCb SaveAttachmentsMenuItem { get; set; }
        ToolStripMenuItemCb SaveEmailMenuItem { get; set; }
        ToolStripMenuItemCb SavePicturesMenuItem { get; set; }
        OLVColumn Sender { get; set; }
        OLVColumn SentDate { get; set; }
        IList<Label> TipsLabels { get; }
        FastObjectListView TopicThread { get; set; }
        TextBox TxtboxBody { get; set; }
        TextBox TxtboxSearch { get; set; }
        Dispatcher UiDispatcher { get; }
        TaskScheduler UiScheduler { get; }
        SynchronizationContext UiSyncContext { get; }

        void RemoveControlsColsRightOf(Control furthestRight);
    }
}