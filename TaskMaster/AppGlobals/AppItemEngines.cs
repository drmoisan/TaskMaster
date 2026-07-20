using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using SDILReader;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.ClassifierGroups;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories;
using UtilitiesCS.Extensions;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.ReusableTypeClasses.NewSmartSerializable.Config;
using UtilitiesCS.Threading;

namespace TaskMaster
{
    [ExcludeFromCodeCoverage]
    public class AppItemEngines : IAppItemEngines
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region ctor

        public AppItemEngines(IApplicationGlobals globals)
        {
            Globals = globals;
        }

        public async Task InitAsync()
        {
            // Diagnosis-only per-engine attribution probe (issue #211). Behavior-preserving:
            // the probe only wraps the existing awaits with a Stopwatch and emits structured
            // log lines through the existing log4net logger. Phase order, the engine set, the
            // config.Value.Engine filter, the EngineInitializer lookup, the null filters, and the
            // ToConcurrentDictionaryAsync semantics are unchanged.
            var probe = new EngineInitTimingProbe(s => logger.Debug(s));

            var configStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var configs = await Globals.AF.Manager.Configuration;
            configStopwatch.Stop();
            probe.EmitConfigTiming(
                configStopwatch.Elapsed.TotalMilliseconds,
                System.Threading.Thread.CurrentThread.ManagedThreadId
            );

            // SelectAwait (System.Linq.Async) is obsolete (CS0618) per the framework's migration
            // guidance ("Use Select... the SelectAwait functionality now exists as overloads of
            // Select"), but migrating to the new overload signature is a call-shape change to
            // production code, not an annotation-only edit. Suppressing narrowly preserves the
            // exact pre-existing behavior (no behavior change per AC7).
#pragma warning disable CS0618
            InboxEngines = await configs
                .Where(config => config.Value.Engine)
                .Select(config =>
                    (
                        config.Key,
                        EngineFunc: EngineInitializer.TryGetValue(config.Key, out var engineAsync)
                            ? engineAsync
                            : null
                    )
                )
                .Where(tup => tup.EngineFunc is not null)
                .ToAsyncEnumerable()
                .SelectAwait(async tup =>
                {
                    var engine = await probe.TimeEngineAsync(
                        tup.Key,
                        () => tup.EngineFunc(Globals)
                    );
                    return (tup.Key, Engine: engine);
                })
                .Where(tup => tup.Engine is not null)
                .ToConcurrentDictionaryAsync(tup => tup.Key, tup => tup.Engine);
#pragma warning restore CS0618
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

        public ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>> InboxEngines
        {
            get;
            protected set;
        } = [];

        private Dictionary<
            string,
            Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>
        > _engineInitializer;
        internal Dictionary<
            string,
            Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>
        > EngineInitializer
        {
            get
            {
                _engineInitializer ??= GetEngineInitializer();
                return _engineInitializer;
            }
        }

        internal Dictionary<
            string,
            Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>
        > GetEngineInitializer()
        {
            Dictionary<
                string,
                Func<IApplicationGlobals, Task<IConditionalEngine<MailItemHelper>>>
            > ei = new()
            {
                {
                    "Spam",
                    async globals =>
                    {
                        var sb = await SpamBayes.CreateEngineAsync(globals);
                        return sb;
                    }
                },
                {
                    "Triage",
                    async globals =>
                    {
                        var triage = await Triage.CreateEngineAsync(globals);
                        return triage;
                    }
                },
                {
                    "Project",
                    async globals =>
                    {
                        var project = await CategoryClassifierGroup.CreateEngineAsync(
                            globals,
                            "Project"
                        );
                        project.CategorySetter = ProjectCategorySetterAsync;
                        return project;
                    }
                },
                {
                    "Context",
                    async globals =>
                    {
                        var context = await CategoryClassifierGroup.CreateEngineAsync(
                            globals,
                            "Context"
                        );
                        context.CategorySetter = ContextCategorySetterAsync;
                        return context;
                    }
                },
                {
                    "Actionable",
                    async globals =>
                    {
                        var actionable = await ActionableClassifierGroup.CreateEngineAsync(
                            globals,
                            "Actionable"
                        );
                        return actionable;
                    }
                },
            };
            return ei;
        }

        #region CategoryClassifierActions

        internal async Task ProjectCategorySetterAsync(
            IEnumerable<string> categories,
            MailItemHelper helper
        )
        {
            await Task.Run(() =>
            {
                var todo = new ToDoItem(new OutlookItemFlaggable(helper.Item));
                todo.Projects.AsListNoPrefix = [.. categories];
                todo.FlagAsTask = true;
            });
        }

        internal async Task ContextCategorySetterAsync(
            IEnumerable<string> categories,
            MailItemHelper helper
        )
        {
            await Task.Run(() =>
            {
                var todo = new ToDoItem(new OutlookItemFlaggable(helper.Item));
                todo.Context.AsListNoPrefix = [.. categories];
            });
        }

        #endregion CategoryClassifierActions

        #region Activation and Configuration

        public async Task ShowDiskDialog(string engineName, bool local)
        {
            if (InboxEngines.TryGetValue(engineName, out var engine))
            {
                if (local)
                {
                    engine.Config.ActivateLocalDisk();
                }
                else
                {
                    engine.Config.ActivateNetDisk();
                }
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
            if (InboxEngines.TryGetValue(engineName, out var engine) && engine is not null)
            {
                ConfigController.Show(Globals, engine.Config);
            }
        }

        #endregion Activation and Configuration
    }
}
