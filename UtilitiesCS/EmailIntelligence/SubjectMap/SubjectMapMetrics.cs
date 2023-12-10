using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace UtilitiesCS.EmailIntelligence.SubjectMap
{
    internal partial class SubjectMapMetrics : Form
    {
        public SubjectMapMetrics()
        {
            InitializeComponent();
        }

        public SubjectMapMetrics(IEnumerable<SubjectMapSco.SummaryMetric> metrics) 
        { 
            InitializeComponent();
            this.DlvMetrics.SetObjects(metrics);
            foreach (OLVColumn col in this.DlvMetrics.Columns)
            {
                col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
        }
    }
}
