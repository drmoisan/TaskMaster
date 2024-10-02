using Microsoft.Office.Interop.Outlook;
using SDILReader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.Extensions;

namespace TaskMaster
{
    public class AppItemEngines : IAppItemEngines
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ctor

        public AppItemEngines(IApplicationGlobals globals)
        {
            Globals = globals;
        }

        internal IApplicationGlobals Globals { get; set; }

        public async Task InitAsync()
        {
            //InboxEngines = await 
            //    (await Globals.AF.Manager.Configuration)
            //    .Where(config => config.Value.Engine)
            //    .Select(config => new KeyValuePair<string, Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>>(
            //        config.Key, engineInitiator.TryGetValue(config.Key, out var engineAsync) ? engineAsync : null))
            //    .Where(kvp => kvp.Value is not null)
            //    .ToAsyncEnumerable()
            //    .SelectAwait(async kvp => new KeyValuePair<string, IConditionalEngine<MailItemHelper>>(
            //        kvp.Key, await kvp.Value(Globals)))
            //    .ToConcurrentDictionaryAsync(kvp => kvp.Key, kvp => kvp.Value);

            InboxEngines = await
                (await Globals.AF.Manager.Configuration)
                .Where(config => config.Value.Engine)
                .Select(config => (
                    config.Key,
                    EngineFunc: engineInitiator.TryGetValue(config.Key, out var engineAsync) ? engineAsync : null))
                .Where(tup => tup.EngineFunc is not null)
                .ToAsyncEnumerable()
                .SelectAwait(async tup => (tup.Key, Engine: await tup.EngineFunc(Globals)))
                .ToConcurrentDictionaryAsync(tup => tup.Key, tup => tup.Engine);
        }

        #endregion ctor

        public ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>> InboxEngines { get; protected set; } = [];

        internal Dictionary<string, Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>> engineInitiator { get; } = new()
            {
                { "Spam", async globals => await SpamBayes.CreateEngineAsync(globals) },
                { "Triage", async globals => await Triage.CreateEngineAsync(globals) }
            };
    }
}
