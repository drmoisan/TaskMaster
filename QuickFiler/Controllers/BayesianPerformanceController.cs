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
            
            
            await Task.CompletedTask;
        }
    }
}
