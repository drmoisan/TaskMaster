using QuickFiler.Controllers;
using System.Windows.Forms;

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

        private BayesianPerformanceController _controller;
        public virtual BayesianPerformanceController Controller { get => _controller; internal set => _controller = value; }
    }
}
