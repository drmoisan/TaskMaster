using QuickFiler.Viewers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.Threading;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;

namespace QuickFiler.Controllers
{
    public class BayesianPerformanceController
    {
        public BayesianPerformanceController(IApplicationGlobals globals) { _globals = globals; }

        private IApplicationGlobals _globals;
        internal IApplicationGlobals Globals {get => _globals; set => _globals = value; }
                
        internal BayesianSerializationHelper Serialization { get; set; }
        
        public async Task InvestigatePerformance()
        {
            Serialization ??= new BayesianSerializationHelper(Globals);
            var ppkg = (new ProgressPackage()).InitializeAsync(cancelSource: _globals.AF.CancelSource, cancel: _globals.AF.CancelToken , progressTrackerPane: _globals.AF.ProgressTracker);
            var errors = await Serialization.DeserializeAsync<ClassificationErrors[]>("ClassificationErrors[]");
            
            
        }

        public async Task InitializeViewer(ClassificationErrors[] errors)
        {
            Viewer ??= new BayesianPerformanceViewer(this);
            Viewer.FpCount.Text = errors[0].FP.ToString("N0");
            Viewer.FnCount.Text = errors[0].FN.ToString("N0");
            
            //if (!Viewer.Visible) { Viewer.Show(); }
        }

        public void AssignFormValues(ClassificationErrors error)
        {
            Viewer.FpCount.Text = error.FP.ToString("N0");
            Viewer.FnCount.Text = error.FN.ToString("N0");
        }

        protected BayesianPerformanceViewer _viewer;
        internal virtual BayesianPerformanceViewer Viewer { get => _viewer; private set => _viewer = value; }
        
    }
}
