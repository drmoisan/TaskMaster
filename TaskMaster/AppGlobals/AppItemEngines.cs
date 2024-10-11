using Microsoft.Office.Interop.Outlook;
using SDILReader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.Extensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config;
using UtilitiesCS.ReusableTypeClasses.UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Threading;

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

        public async Task InitAsync()
        {
            var configs = await Globals.AF.Manager.Configuration;
            InboxEngines = await configs
                .Where(config => config.Value.Engine)
                .Select(config => (
                    config.Key,
                    EngineFunc: EngineInitializer.TryGetValue(config.Key, out var engineAsync) ? engineAsync : null))
                .Where(tup => tup.EngineFunc is not null)
                .ToAsyncEnumerable()
                .SelectAwait(async tup => 
                { 
                    var engine = await tup.EngineFunc(Globals);
                    return (tup.Key, Engine: engine); 
                })
                .ToConcurrentDictionaryAsync(tup => tup.Key, tup => tup.Engine);
        }

        #endregion ctor

        internal IApplicationGlobals Globals { get; set; }

        public async Task ToggleEngineAsync(string engineName) 
        {
            var configs = await Globals.AF.Manager.Configuration;
            if (configs.TryGetValue(engineName, out var loader))
            {
                loader.Config.ClassifierActivated = !loader.Config.ClassifierActivated;
            }
        }

        public async Task<bool> EngineActiveAsync(string engineName)
        {
            var configs = await Globals.AF.Manager.Configuration;
            if (configs.TryGetValue(engineName, out var loader))
            {
                return loader.Config.ClassifierActivated;
            }
            return false;
        }

        public async Task RestartEngineAsync(string engineName)
        {
            if (EngineInitializer.TryGetValue(engineName, out var engine))
            {
                InboxEngines[engineName] = await engine(Globals);
            }
        }

        public ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>> InboxEngines { get; protected set; } = [];

        private Dictionary<string, Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>> _engineInitializer;
        internal Dictionary<string, Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>> EngineInitializer
        {
            get 
            {
                _engineInitializer ??= GetEngineInitializer();
                return _engineInitializer;
            }
        }        
        internal Dictionary<string, Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>> GetEngineInitializer()
        {
            Dictionary<string, Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>> ei = new()
            {
                { "Spam", async globals =>
                    {
                        var sb = await SpamBayes.CreateEngineAsync(globals);
                        return sb;
                    }
                },
                { "Triage", async globals => 
                    {
                        var triage = await Triage.CreateEngineAsync(globals);
                        return triage;
                    } 
                }
            };
            return ei;
        }

        #region Activation and Configuration

        public async Task ShowDiskDialog(string engineName, bool local)
        {
            if (InboxEngines.TryGetValue(engineName, out var engine))
            {
                if (local) { engine.Config.ActivateLocalDisk(); }
                else { engine.Config.ActivateNetDisk(); }
                await Task.CompletedTask;
                //await ChangeDiskCallback(engine, local);                
            }
        }

        //internal virtual async Task ChangeDiskCallback(IConditionalEngine<MailItemHelper> engine, bool local)
        //{
        //    var response = MessageBox.Show($"SpamBayes is now using {(local ? "local" : "network")} disk. Would you like to save the current classifier?",
        //                    "Save Configuration",
        //                    MessageBoxButtons.YesNo,
        //                    MessageBoxIcon.Question);
        //    if (response == DialogResult.Yes) { engine.Serialize(); }
        //    else
        //    {
        //        response = MessageBox.Show($"Would you like to reload the classifier from {(local ? "local" : "network")}", "Reload Classifier",
        //            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        //        if (response == DialogResult.Yes)
        //        {
        //            var configs = await Globals.AF.Manager.Configuration;
        //            if (configs.TryGetValue(engine.EngineName, out var loader))
        //            {
        //                Globals.AF.Manager.ResetLoadClassifierAsyncLazy(engine.EngineName, loader);
        //            }
        //        }
        //    }
        //}

        public void ShowSaveInfo(string engineName) 
        {
            if (InboxEngines.TryGetValue(engineName, out var engine))
            {
                ConfigController.Show(Globals, engine.Config); 
            }
        }

        #endregion Activation and Configuration
    }
}
