#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.OutlookExtensions;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.ClassifierGroups
{
    public class ActionableClassifierGroup : MulticlassEngine<ActionableClassifierGroup>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region ctor

        public ActionableClassifierGroup()
            : base() { }

        public ActionableClassifierGroup(IApplicationGlobals globals)
            : base(globals)
        {
            base.EngineName = "Actionable";
            base.ProbabilityThreshold = 0.2;
            // Set async action
            // Set async condition
        }

        public new async Task<ActionableClassifierGroup?> InitAsync(string groupName)
        {
            var result = await base.InitAsync(groupName);
            if (result is not null)
            {
                result.AsyncAction = (item) =>
                    ((Engine as ActionableClassifierGroup)?.TestAsync(item))!;
                result.AsyncCondition = (item) => Task.Run(() => Condition(item));
            }
            return result;
        }

        public static new async Task<ActionableClassifierGroup?> CreateEngineAsync(
            IApplicationGlobals globals,
            string categoryGroup,
            CancellationToken token = default
        )
        {
            var cg = new ActionableClassifierGroup(globals);
            return await cg.InitAsync(categoryGroup);
        }

        #endregion ctor

        #region Build Category Classifier

        public override async Task<bool> BuildClassifiersAsync(
            BayesianClassifierGroup classifierGroup,
            MinedMailInfo[] collection,
            ProgressPackage ppkg,
            string groupName,
            int minimumCountPerToken = 0
        )
        {
            var groups = collection
                ?.Where(x => x.Actionable is not null)
                .GroupBy(x => x.Actionable);

            // Exit if collection or groupings are null or empty
            if (groups is null || groups.Count() == 0)
            {
                return false;
            }

            var sw = ppkg.StopWatch!;

            bool success = false;
            try
            {
                // The .Where(x => x.Actionable is not null) filter above guarantees a non-null
                // grouping key at runtime, but LINQ's GroupBy key-selector nullability inference
                // does not propagate that guarantee into the closed generic type, so `group`'s
                // key remains annotated `string?` here while BuildClassifierAsync's base-class
                // signature expects `IGrouping<string, MinedMailInfo>`. Suppressing narrowly
                // preserves the exact pre-existing runtime behavior (no behavior change per AC7)
                // rather than widening the shared base-class signature.
#pragma warning disable CS8620
                await AsyncMultiTasker.AsyncMultiTaskChunker(
                    groups,
                    async (group) =>
                    {
                        await BuildClassifierAsync(
                            group,
                            classifierGroup,
                            ppkg.Cancel,
                            minimumCountPerToken
                        );
                    },
                    ppkg.ProgressTrackerPane!,
                    "Building Classifiers",
                    ppkg.Cancel
                );
#pragma warning restore CS8620
                sw.LogDuration("Build Classifiers");
                sw.WriteToLog(clear: false);
                success = true;
            }
            catch (System.Exception e)
            {
                logger.Error(e.Message, e);
            }
            return success;
        }

        #endregion Build Category Classifier

        #region Public Properties

        public async Task<string[]> GetMatchingCategoriesAsync(MailItemHelper helper)
        {
            var results = await ClassifierGroup.ClassifyAsync(helper.Tokens, default);
            var filtered = results
                .Where(x => x.Probability > ProbabilityThreshold)
                .Select(x => x.Class!)
                .Where(x => x != "None")
                .ToArray();
            return filtered;
        }

        public string[] GetMatchingCategories(MailItemHelper helper)
        {
            var results = ClassifierGroup.Classify(helper.Tokens);
            // var results2 = results.ToList();
            var filtered = results
                ?.Where(x => x.Probability > ProbabilityThreshold)
                .Select(x => x.Class!)
                .Where(x => x != "None")
                .ToArray();
            return filtered!;
        }

        public override async Task TestAsync(MailItemHelper helper)
        {
            var results = await GetMatchingCategoriesAsync(helper);
            var value = results.IsNullOrEmpty() ? "None" : results.First();
            var olItem = new OutlookItem(helper.Item);
            olItem
                .Try()
                .SetUdf(
                    "Actionable",
                    value,
                    Microsoft.Office.Interop.Outlook.OlUserPropertyType.olText
                );
        }

        #endregion Public Properties
    }
}
