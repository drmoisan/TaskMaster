using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests covering how the QuickFiler datamodel propagates a failure raised while
    /// the email frame is being built.
    /// </summary>
    [TestClass]
    public class QfcDatamodelRethrowTests
    {
        /// <summary>
        /// The frame the sentinel originates in. A <c>throw e;</c> rethrow resets the stack to the
        /// rethrow site and loses it; a <c>throw;</c> rethrow preserves it.
        /// </summary>
        private const string OriginatingFrame = nameof(ThrowSentinelAtTableAcquisition);

        /// <summary>Throws the sentinel from a named frame the assertion can look for.</summary>
        private static object ThrowSentinelAtTableAcquisition(System.Exception sentinel) =>
            throw sentinel;

        /// <summary>Yields <paramref name="exception"/> and every exception nested inside it.</summary>
        private static IEnumerable<System.Exception> Unwrap(System.Exception exception)
        {
            if (exception is null)
            {
                yield break;
            }

            yield return exception;
            var nested = exception is AggregateException aggregate
                ? aggregate.InnerExceptions.AsEnumerable()
                : new[] { exception.InnerException };
            foreach (var inner in nested.SelectMany(Unwrap))
            {
                yield return inner;
            }
        }

        /// <summary>Sets a private instance field by name.</summary>
        private static void SetPrivateField(object target, string fieldName, object value) =>
            target
                .GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(target, value);

        /// <summary>
        /// Globals whose MAPI namespace reports offline, so the offline toggle short-circuits
        /// without touching the explorer's command bars.
        /// </summary>
        private static IApplicationGlobals BuildOfflineGlobals()
        {
            var session = new Mock<NameSpace>(MockBehavior.Loose);
            session.SetupGet(n => n.Offline).Returns(true);

            var ol = new Mock<IOlObjects>(MockBehavior.Loose);
            ol.SetupGet(o => o.NamespaceMAPI).Returns(session.Object);

            var globals = new Mock<IApplicationGlobals>(MockBehavior.Loose);
            globals.SetupGet(g => g.Ol).Returns(ol.Object);
            return globals.Object;
        }

        /// <summary>
        /// A progress tracker that records nothing and spawns itself, so no progress viewer is
        /// created and no UI thread is touched.
        /// </summary>
        private static ProgressTracker BuildInertProgress(CancellationTokenSource tokenSource)
        {
            var progress = new Mock<ProgressTracker>(tokenSource);
            progress.SetupAllProperties();
            progress.Setup(p => p.Report(It.IsAny<double>()));
            progress.Setup(p => p.Report(It.IsAny<double>(), It.IsAny<string>()));
            progress.Setup(p => p.Report(It.IsAny<ValueTuple<int, string>>()));
            progress.Setup(p => p.Increment(It.IsAny<double>())).Returns(() => progress.Object);
            progress
                .Setup(p => p.Increment(It.IsAny<double>(), It.IsAny<string>()))
                .Returns(() => progress.Object);
            progress.Setup(p => p.SpawnChild()).Returns(() => progress.Object);
            progress.Setup(p => p.SpawnChild(It.IsAny<int>())).Returns(() => progress.Object);
            progress.Setup(p => p.SpawnChild(It.IsAny<double>())).Returns(() => progress.Object);
            return progress.Object;
        }

        /// <summary>
        /// AC4: a failure raised while the email frame is being built must reach the caller with the
        /// originating frame still in the stack, so the launch failure is diagnosable.
        /// </summary>
        /// <remarks>
        /// The assertion is shape-agnostic. Wrapping happens at the dataframe-transform
        /// <c>TimeoutAfter</c> call, which is downstream of table acquisition, so a sentinel thrown
        /// at acquisition reaches the boundary unwrapped while one thrown during the transform
        /// reaches it wrapped in an <c>AggregateException</c>. The test therefore walks the thrown
        /// exception and every exception nested inside it and requires the originating frame in at
        /// least one of their stacks; it assumes neither unwrapping nor wrapping.
        /// </remarks>
        [TestMethod]
        public async Task GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack()
        {
            // Arrange
            var sentinel = new InvalidOperationException("qfc-798 table acquisition sentinel");
            var datamodel = (QfcDatamodel)
                FormatterServices.GetUninitializedObject(typeof(QfcDatamodel));
            SetPrivateField(datamodel, "_globals", BuildOfflineGlobals());

            var cancellation = new CancellationTokenSource();
            datamodel.TokenSource = cancellation;
            datamodel.Token = cancellation.Token;

            var explorer = new Mock<Explorer>(MockBehavior.Loose);
            explorer
                .SetupGet(e => e.CurrentView)
                .Returns(() => ThrowSentinelAtTableAcquisition(sentinel));

            var method = typeof(QfcDatamodel).GetMethod(
                "GetEmailsInViewDfAsync",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Act
            System.Exception observed = null;
            try
            {
                await (Task)
                    method.Invoke(
                        datamodel,
                        new object[] { explorer.Object, BuildInertProgress(cancellation) }
                    );
            }
            catch (System.Exception thrown)
            {
                observed = thrown;
            }

            // Assert
            observed.Should().NotBeNull("the inner failure must reach the caller");
            var stacks = Unwrap(observed).Select(e => e.StackTrace ?? string.Empty).ToList();
            stacks
                .Should()
                .Contain(
                    trace => trace.IndexOf(OriginatingFrame, StringComparison.Ordinal) >= 0,
                    "the rethrow must preserve the frame the failure originated in"
                );
        }
    }
}
