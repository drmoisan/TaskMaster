using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuickFiler
{
    // Forwarding implementations for the narrowed IItemViewer button command events and menu intent
    // members (Seam B, Cluster 2b). Each event forwards add/remove to the underlying Designer control
    // event; each check-state property round-trips the underlying control's Checked value. The whole
    // ItemViewer type is [ExcludeFromCodeCoverage] via its primary partial in ItemViewer.cs.
    public partial class ItemViewer
    {
        public event EventHandler DeleteItemClicked
        {
            add => BtnDelItem.Click += value;
            remove => BtnDelItem.Click -= value;
        }

        public event EventHandler FlagTaskClicked
        {
            add => BtnFlagTask.Click += value;
            remove => BtnFlagTask.Click -= value;
        }

        public event EventHandler PopOutClicked
        {
            add => BtnPopOut.Click += value;
            remove => BtnPopOut.Click -= value;
        }

        public event EventHandler ReplyClicked
        {
            add => BtnReply.Click += value;
            remove => BtnReply.Click -= value;
        }

        public event EventHandler ReplyAllClicked
        {
            add => BtnReplyAll.Click += value;
            remove => BtnReplyAll.Click -= value;
        }

        public event EventHandler ForwardClicked
        {
            add => BtnForward.Click += value;
            remove => BtnForward.Click -= value;
        }

        public event EventHandler ConversationModeChanged
        {
            add => ConversationMenuItem.CheckedChanged += value;
            remove => ConversationMenuItem.CheckedChanged -= value;
        }

        public bool ConversationModeChecked
        {
            get => ConversationMenuItem.Checked;
            set => ConversationMenuItem.Checked = value;
        }

        public event EventHandler EmailCopyChanged
        {
            add => SaveEmailMenuItem.CheckedChanged += value;
            remove => SaveEmailMenuItem.CheckedChanged -= value;
        }

        public bool EmailCopyChecked
        {
            get => SaveEmailMenuItem.Checked;
            set => SaveEmailMenuItem.Checked = value;
        }

        public event EventHandler AttachmentsChanged
        {
            add => SaveAttachmentsMenuItem.CheckedChanged += value;
            remove => SaveAttachmentsMenuItem.CheckedChanged -= value;
        }

        public bool AttachmentsChecked
        {
            get => SaveAttachmentsMenuItem.Checked;
            set => SaveAttachmentsMenuItem.Checked = value;
        }

        public event EventHandler PicturesChanged
        {
            add => SavePicturesMenuItem.CheckedChanged += value;
            remove => SavePicturesMenuItem.CheckedChanged -= value;
        }

        public bool PicturesChecked
        {
            get => SavePicturesMenuItem.Checked;
            set => SavePicturesMenuItem.Checked = value;
        }

        public DialogResult FlagTaskDialogResult
        {
            get => BtnFlagTask.DialogResult;
            set => BtnFlagTask.DialogResult = value;
        }

        public Color FlagTaskBackColor
        {
            get => BtnFlagTask.BackColor;
            set => BtnFlagTask.BackColor = value;
        }
    }
}
