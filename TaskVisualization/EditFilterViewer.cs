using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;

namespace TaskVisualization
{
    [ExcludeFromCodeCoverage]
    public partial class EditFilterViewer : Form, IEditFilterViewer
    {
        public EditFilterViewer()
        {
            InitializeComponent();
        }

        public List<Label> GetTips() =>
            new List<Label>
            {
                this.XlCancel,
                this.XlContext,
                this.XlFilterName,
                this.XlFolders,
                this.XlOk,
                this.XlPeople,
                this.XlProject,
                this.XlTopic,
            };

        #region IEditFilterViewer text surfaces

        public string ContextSelectionText
        {
            get => ContextSelection.Text;
            set => ContextSelection.Text = value;
        }

        public string PeopleSelectionText
        {
            get => PeopleSelection.Text;
            set => PeopleSelection.Text = value;
        }

        public string ProjectSelectionText
        {
            get => ProjectSelection.Text;
            set => ProjectSelection.Text = value;
        }

        public string TopicSelectionText
        {
            get => TopicSelection.Text;
            set => TopicSelection.Text = value;
        }

        public string FilterNameText
        {
            get => FilterName.Text;
            set => FilterName.Text = value;
        }

        #endregion IEditFilterViewer text surfaces

        #region IEditFilterViewer click events

        public event EventHandler ContextSelectionClick
        {
            add => ContextSelection.Click += value;
            remove => ContextSelection.Click -= value;
        }

        public event EventHandler PeopleSelectionClick
        {
            add => PeopleSelection.Click += value;
            remove => PeopleSelection.Click -= value;
        }

        public event EventHandler ProjectSelectionClick
        {
            add => ProjectSelection.Click += value;
            remove => ProjectSelection.Click -= value;
        }

        public event EventHandler TopicSelectionClick
        {
            add => TopicSelection.Click += value;
            remove => TopicSelection.Click -= value;
        }

        public event EventHandler FoldersSelectedClick
        {
            add => FoldersSelected.Click += value;
            remove => FoldersSelected.Click -= value;
        }

        public event EventHandler OkClick
        {
            add => BtnOk.Click += value;
            remove => BtnOk.Click -= value;
        }

        public event EventHandler CancelClick
        {
            add => BtnCancel.Click += value;
            remove => BtnCancel.Click -= value;
        }

        #endregion IEditFilterViewer click events

        /// <summary>
        /// Toggles every tip label to the Off state. Reproduces the initial
        /// "all tips off" view state that the controller formerly set via
        /// <see cref="GetTips"/> during initialization.
        /// </summary>
        public void ResetTips()
        {
            GetTips()
                .Select(label => new QfcTipsDetails(label))
                .ToList()
                .ForEach(tip => tip.Toggle(Enums.ToggleState.Off));
        }
    }
}
