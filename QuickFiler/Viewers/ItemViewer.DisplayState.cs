using System;
using System.Drawing;

namespace QuickFiler
{
    // Forwarding implementations for the narrowed IItemViewer display-state intent members
    // (Seam B, Cluster 2a). Each member forwards to the existing Designer-backed control so the
    // underlying WinForms controls remain private to the view while the controller consumes intent.
    // The whole ItemViewer type is [ExcludeFromCodeCoverage] via its primary partial in
    // ItemViewer.cs; the attribute is not (and cannot be) repeated here (CS0579, non-AllowMultiple).
    public partial class ItemViewer
    {
        public string SenderText
        {
            get => LblSender.Text;
            set => LblSender.Text = value;
        }

        public string SubjectText
        {
            get => LblSubject.Text;
            set => LblSubject.Text = value;
        }

        public string BodyText
        {
            get => TxtboxBody.Text;
            set => TxtboxBody.Text = value;
        }

        public string TriageText
        {
            get => LblTriage.Text;
            set => LblTriage.Text = value;
        }

        public string SentOnText
        {
            get => LblSentOn.Text;
            set => LblSentOn.Text = value;
        }

        public string ActionableText
        {
            get => LblActionable.Text;
            set => LblActionable.Text = value;
        }

        public string ItemNumberText
        {
            get => LblItemNumber.Text;
            set => LblItemNumber.Text = value;
        }

        public string FolderText
        {
            get => LblFolder.Text;
            set => LblFolder.Text = value;
        }

        public string ConversationCountText
        {
            get => LblConvCt.Text;
            set => LblConvCt.Text = value;
        }

        public Color ConversationCountBackColor
        {
            get => LblConvCt.BackColor;
            set => LblConvCt.BackColor = value;
        }

        public event EventHandler BodyDoubleClick
        {
            add => TxtboxBody.DoubleClick += value;
            remove => TxtboxBody.DoubleClick -= value;
        }

        public bool FocusSubject() => LblSubject.Focus();
    }
}
