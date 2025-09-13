using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskVisualization
{
    public partial class EditFilterViewer : Form
    {
        public EditFilterViewer()
        {
            InitializeComponent();
        }

        public List<Label> GetTips() => new List<Label>
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
    }
}
