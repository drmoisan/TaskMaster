using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;

namespace QuickFiler.Controllers
{
    public class BayesianPerformanceController
    {
        public BayesianPerformanceController(IApplicationGlobals globals) { _globals = globals; }

        private IApplicationGlobals _globals;
        internal IApplicationGlobals Globals => _globals;

        internal BayesianSerializationHelper Serialization { get; set; }
        

        public async Task InvestigatePerformance()
        {
            Serialization ??= new BayesianSerializationHelper(Globals);

            var errors = await Serialization.DeserializeAsync<ClassificationErrors[]>("ClassificationErrors");
            //var errors = Serialization.Deserialize<ClassificationErrors[]>("ClassificationErrors");
            var errors2 = errors.Select(e => new ClassificationErrors2()
            {
                Class = e.Class,
                FalsePositives = e.FalsePositives.Count(),
                FalseNegatives = e.FalseNegatives.Count(),
                VerboseOutcomes = e.FalsePositives
                    .Select(x => new KeyValuePair<VerboseTestOutcome, string> (x, "FalsePositive"))
                    .Concat(e.FalseNegatives.Select(x => new KeyValuePair<VerboseTestOutcome, string> (x, "FalseNegative")))
                    .ToDictionary()
            }).ToArray();

            Serialization.SerializeAndSave(errors2, typeof(ClassificationErrors[]).Name);
            var errors3 = Serialization.Deserialize<ClassificationErrors2[]>("ClassificationErrors[]");
            await Task.CompletedTask;
        }
    }
}
