using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using FluentAssertions;
using Moq;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.Bayesian;
using UtilitiesCS.EmailIntelligence.Bayesian.Performance;

namespace QuickFiler.Controllers.Tests
{
    internal static class BayesianPerformanceControllerTestSupport
    {
        internal static void RunWithViewer(
            Action<BayesianPerformanceController, BayesianPerformanceViewer> action
        )
        {
            Exception captured = null;
            var thread = new Thread(() =>
            {
                SynchronizationContext previousContext = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
                BayesianPerformanceViewer viewer = null;
                try
                {
                    var controller = new BayesianPerformanceController(
                        new Mock<IApplicationGlobals>().Object
                    );
                    viewer = new BayesianPerformanceViewer(controller).Init();
                    SetField(controller, "_viewer", viewer);
                    action(controller, viewer);
                }
                catch (Exception ex)
                {
                    captured = ex;
                }
                finally
                {
                    viewer?.Dispose();
                    SynchronizationContext.SetSynchronizationContext(previousContext);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (captured != null)
            {
                ExceptionDispatchInfo.Capture(captured).Throw();
            }
        }

        internal static void SetField(object target, string name, object value)
        {
            FieldInfo field = target
                .GetType()
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            field
                .Should()
                .NotBeNull(because: "field '" + name + "' must exist on " + target.GetType().Name);
            field.SetValue(target, value);
        }

        internal static ClassificationErrors BuildError(
            string @class,
            VerboseTestOutcome outcome,
            string outcomeType = "False Positive"
        )
        {
            return new ClassificationErrors
            {
                Class = @class,
                TP = 12,
                FP = 1234,
                FN = 56,
                TN = 7890,
                Errors = 1290,
                Precision = 0.81234,
                Recall = 0.93456,
                F1 = 0.86918,
                VerboseOutcomes = new System.Collections.Generic.Dictionary<
                    VerboseTestOutcome,
                    string
                >
                {
                    [outcome] = outcomeType,
                },
            };
        }

        internal static VerboseTestOutcome BuildOutcome(
            string subject,
            params (string Token, double TokenProbability)[] drivers
        )
        {
            return new VerboseTestOutcome
            {
                Actual = "ActualClass",
                Predicted = "PredictedClass",
                Probability = 0.75,
                Drivers = drivers,
                Source = new MinedMailInfo
                {
                    EntryId = "entry-" + subject,
                    StoreId = "store-" + subject,
                    Subject = subject,
                    FolderInfo = new Mock<IFolderWrapper>().Object,
                    Sender = new Mock<IRecipientInfo>().Object,
                },
            };
        }
    }
}
