using QuickFiler.Controllers;
using System.Collections.Generic;
using System.Windows.Forms;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;

namespace QuickFiler.Viewers
{
    public partial class BayesianPerformanceViewer : Form
    {
        public BayesianPerformanceViewer()
        {
            InitializeComponent();
        }

        public BayesianPerformanceViewer(BayesianPerformanceController controller)
        {
            InitializeComponent();
            Controller = controller;
        }

        public BayesianPerformanceViewer Init()
        {
            this.PredictedClass.GroupKeyGetter = GroupKeyGetter;
            return this;
        }

        private BayesianPerformanceController _controller;
        public virtual BayesianPerformanceController Controller { get => _controller; internal set => _controller = value; }

        internal object GroupKeyGetter(object rowObject)
        {
            try
            {
                var o = (KeyValuePair<VerboseTestOutcome, string>)rowObject;
                return o.Key.Actual;
            }
            catch (System.Exception)
            {
                return "unknown";                
            }            
        }

        private void OlvVerboseDetails_SelectionChanged(object sender, System.EventArgs e)
        {
            Controller?.OlvVerboseDetails_SelectionChanged();
        }

        private void OlvDrivers_SelectionChanged(object sender, System.EventArgs e)
        {
            Controller?.OlvDrivers_SelectionChanged();
        }

      
        private void ClassSelector_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            Controller?.ClassSelector_SelectedIndexChanged();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Controller?.ReSortItem();
        }
    }
}
