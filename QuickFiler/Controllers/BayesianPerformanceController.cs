using QuickFiler.Viewers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.Threading;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;
using UtilitiesCS.Extensions;
using Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers
{
    public class BayesianPerformanceController
    {
        public BayesianPerformanceController(IApplicationGlobals globals) { _globals = globals; }

        private IApplicationGlobals _globals;
        internal IApplicationGlobals Globals {get => _globals; set => _globals = value; }
                
        internal BayesianSerializationHelper Serialization { get; set; }

        public ClassificationErrors[] Errors { get => _errors; set => _errors = value; }
        private ClassificationErrors[] _errors;

        internal VerboseTestOutcome ActiveOutcome { get => _activeOutcome; set => _activeOutcome = value; }
        private VerboseTestOutcome _activeOutcome;

        internal ClassificationErrors ActiveError { get => _activeError; set => _activeError = value; }
        private ClassificationErrors _activeError;


        public async Task InvestigatePerformance()
        {
            Serialization ??= new BayesianSerializationHelper(Globals);
            Errors ??= await Serialization.DeserializeAsync<ClassificationErrors[]>("ClassificationErrors[]");
            var ppkg = (new ProgressPackage()).InitializeAsync(cancelSource: _globals.AF.CancelSource, cancel: _globals.AF.CancelToken , progressTrackerPane: _globals.AF.ProgressTracker);
            Viewer = new BayesianPerformanceViewer(this).Init();
            var classes = Errors.Select(x => x.Class).ToArray();
            Viewer.ClassSelector.Items.AddRange(classes);
            Viewer.ClassSelector.SelectedIndex = 0;
            ActiveError = Errors.FirstOrDefault();
            AssignFormValues(ActiveError);
            Viewer.Show();
        }


        public void AssignFormValues(ClassificationErrors error)
        {
            Viewer.FpCount.Text = error.FP.ToString("N0");
            Viewer.FnCount.Text = error.FN.ToString("N0");
            Viewer.TotalCount.Text = error.Errors.ToString("N0");
            Viewer.PrecisionScore.Text = error.Precision.ToString("P2");
            Viewer.RecallScore.Text = error.Recall.ToString("P2");
            Viewer.F1Score.Text = error.F1.ToString("P2");
            Viewer.OlvVerboseDetails.SetObjects(error.VerboseOutcomes);
        }

        internal void OlvVerboseDetails_SelectionChanged()
        {
            var objects = Viewer.OlvVerboseDetails.SelectedObjects;  
            if ((objects is not null) && (objects.Count != 0))
            {
                var outcomePair = (KeyValuePair<VerboseTestOutcome, string>)objects[0];
                var outcome = outcomePair.Key;
                if (!outcome.Drivers.IsNullOrEmpty())
                {
                    Viewer.OlvDrivers.SetObjects(outcome.Drivers);
                    Viewer.OlvDrivers.SelectedIndex = 0;
                    Viewer.OlvDrivers.FocusedItem = Viewer.OlvDrivers.SelectedItems[0];
                }
                else { Viewer.OlvDrivers.Clear(); }
                ActiveOutcome = outcome;
            }
        }

        internal void OlvDrivers_SelectionChanged()
        {
            var objects = Viewer.OlvDrivers.SelectedObjects;
            if ((objects is not null) && (objects.Count != 0))
            {
                var (token, tokenProbability) = ((string Token, double TokenProbability))objects[0];
                var driverPresence = ActiveError.VerboseOutcomes
                    .Where(x => x.Key.Drivers.FindIndex(y => y.Token == token) != -1)
                    .Select(x => (x.Key.Source.Subject, x.Key.Drivers.Find(y => y.Token == token)))
                    .Select(x => (x.Subject, x.Item2.TokenProbability))
                    .ToArray();
                Viewer.OlvDriverPresence.SetObjects(driverPresence);
            }
            else { Viewer.OlvDriverPresence.Clear(); }
        }

        internal void ClassSelector_SelectedIndexChanged()
        {
            var selectedClass = Viewer.ClassSelector.SelectedItem.ToString();
            ActiveError = Errors.FirstOrDefault(x => x.Class == selectedClass);
            if (ActiveError is not null) 
            { 
                AssignFormValues(ActiveError); 
            }
        }

        internal void ReSortItem() 
        {
            var item = (MailItem)Globals.Ol.NamespaceMAPI.GetItemFromID(ActiveOutcome.Source.EntryId, ActiveOutcome.Source.StoreId);
            if (item is not null) 
            {
                var sorter = new EfcHomeController(_globals, () => { }, item);
                sorter.Run();
            }
        }

        protected BayesianPerformanceViewer _viewer;
        internal virtual BayesianPerformanceViewer Viewer { get => _viewer; private set => _viewer = value; }
        
    }
}
