using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrentObservableCollections.ConcurrentObservableDictionary;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace TaskMaster.AppGlobals
{
    public class ManagerLazyClass : ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>
    {
        
    }
}
