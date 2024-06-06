using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups.Triage
{
    public class Triage
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public Triage(IApplicationGlobals globals, ScDictionary<string, BayesianClassifierGroup> manager, CancellationToken token = default)
        {
            Manager = manager;
            Globals = globals;
            Token = token;
            TokenizeAsync = new EmailTokenizer().TokenizeAsync;
            CallbackAsync = (item, value) => Task.Run(() => ((MailItem)item).SetUdf("Triage", value));
        }

        #endregion Constructors

        #region Properties

        internal ScDictionary<string, BayesianClassifierGroup> Manager { get; }          
        internal CancellationToken Token { get; }

        internal IApplicationGlobals Globals { get; }

        /// <summary>
        /// Async Delegate Function that extracts an array of string tokens from an object
        /// </summary>
        public Func<object, IApplicationGlobals, CancellationToken, Task<string[]>> TokenizeAsync { get => _tokenizeAsync; set => _tokenizeAsync = value; }
        private Func<object, IApplicationGlobals, CancellationToken, Task<string[]>> _tokenizeAsync;

        public Func<object, string, Task> CallbackAsync { get => _callbackAsync; set => _callbackAsync = value; }
        private Func<object, string, Task> _callbackAsync;

        #endregion Properties

        public async Task CreateNewTriageClassifierGroupAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                var group = new BayesianClassifierGroup();
                Manager["Triage"] = group;
                Manager.Serialize();
            }, token);
        }

        public async Task ClassifyAsync(Selection selection, CancellationToken token = default)
        {
            await selection.Cast<object>()
                .Where(x => x is MailItem)
                .ToAsyncEnumerable()
                .ForEachAwaitWithCancellationAsync(async (item, token) =>
                {
                    var predictions = await Manager["Triage"].ClassifyAsync(item, token);
                    var mostLikely = predictions.FirstOrDefault().Class;
                    if (CallbackAsync is not null) { await CallbackAsync(item, mostLikely); }
                }, token);
        }

        public async Task TrainAsync(Selection selection, string triageId, CancellationToken token = default)
        {
            await selection.Cast<object>()
                           .Where(x => x is MailItem)
                           .Cast<MailItem>()
                           .ToAsyncEnumerable()
                           .ForEachAwaitWithCancellationAsync((item, token) => TrainAsync(item, triageId), token);
            
            //foreach (object item in selection)
            //{
            //    if (item is MailItem mailItem)
            //    {
            //        await TrainAsync(mailItem, triageId);
            //    }
            //}
            Manager.Serialize();
        }

        public async Task TrainAsync(object item, string triageId)
        {
            TokenizeAsync.ThrowIfNull($"{nameof(TokenizeAsync)} delegate function cannot be null to Train classifier");
            item.ThrowIfNull($"{nameof(item)} cannot be null to Train classifier");
            var tokens = await TokenizeAsync(item, Globals, Token);
            await TrainAsync(tokens, triageId);
            if (CallbackAsync is not null) { await CallbackAsync(item, triageId); }
        }

        public async Task TrainAsync(string[] tokens, string triageId)
        {
            var classifierName = triageId;
            Manager["Triage"].Classifiers[classifierName].Train(await tokens.GroupAndCountAsync(), 1);
        }

    }
}
