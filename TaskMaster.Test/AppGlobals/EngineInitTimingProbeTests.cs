using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic verification of the per-engine attribution probe (issue #211,
    /// <see cref="TaskMaster.EngineInitTimingProbe"/>). The probe's timing/emission logic is
    /// exercised through an injected list-capturing sink, with stub factories returning mocked
    /// engines (or null). No live COM, no live timer, no network/filesystem, and no temporary
    /// files are used; only the deterministic structure of the emitted lines is asserted.
    /// </summary>
    [TestClass]
    public class EngineInitTimingProbeTests
    {
        private static IConditionalEngine<MailItemHelper> StubEngine() =>
            new Mock<IConditionalEngine<MailItemHelper>>().Object;

        [TestMethod]
        public async Task TimeEngineAsync_ThreeEnginesInOrder_EmitsOneLinePerEngineInOrderWithFields()
        {
            // Arrange: capture emitted lines and invoke three engines in a fixed order.
            var emitted = new List<string>();
            var probe = new TaskMaster.EngineInitTimingProbe(s => emitted.Add(s));
            var names = new[] { "Spam", "Triage", "Actionable" };

            // Act
            foreach (var name in names)
            {
                await probe.TimeEngineAsync(name, () => Task.FromResult(StubEngine()));
            }

            // Assert: exactly one [engine-init] line per engine, in the same order, each with the
            // expected fields and a non-null engine (costHint=Deserialization).
            emitted.Should().HaveCount(3);
            for (var i = 0; i < names.Length; i++)
            {
                var line = emitted[i];
                line.Should().StartWith("[engine-init] ");
                line.Should().Contain($"engineName={names[i]} ");
                line.Should().Contain("engineNull=False ");
                line.Should().Contain("costHint=Deserialization ");
                line.Should().MatchRegex(@"engineMs=\d+\.\d ");
                line.Should().MatchRegex(@"threadId=\d+ ");
                // Issue #211 Phase 3.1 worker-thread context fields.
                line.Should().Contain("threadPriority=");
                line.Should().Contain("isThreadPoolThread=");
            }
        }

        [TestMethod]
        public async Task TimeEngineAsync_Always_EmitsWorkerThreadContextFieldsAlongsidePriorFields()
        {
            // Arrange: capture the single [engine-init] line for one engine.
            var emitted = new List<string>();
            var probe = new TaskMaster.EngineInitTimingProbe(s => emitted.Add(s));

            // Act
            await probe.TimeEngineAsync("Spam", () => Task.FromResult(StubEngine()));

            // Assert: the worker-thread context fields (issue #211 Phase 3.1) are present in
            // addition to the unchanged prior fields. threadPriority is a ThreadPriority enum name;
            // isThreadPoolThread is a bool rendered as True/False.
            emitted.Should().ContainSingle();
            var line = emitted[0];
            line.Should().StartWith("[engine-init] ");
            line.Should().Contain("engineName=Spam ");
            line.Should().MatchRegex(@"engineMs=\d+\.\d ");
            line.Should().Contain("engineNull=False ");
            line.Should().MatchRegex(@"threadId=\d+ ");
            line.Should().Contain("costHint=Deserialization ");
            line.Should().MatchRegex(@"threadPriority=\w+ ");
            line.Should().MatchRegex(@"isThreadPoolThread=(True|False)");
        }

        [TestMethod]
        public async Task TimeEngineAsync_NullFactoryResult_EmitsEngineNullTrueAndSkipAndReturnsNull()
        {
            // Arrange: a factory that yields a null engine.
            var emitted = new List<string>();
            var probe = new TaskMaster.EngineInitTimingProbe(s => emitted.Add(s));

            // Act
            // This file has no project-level <Nullable> and no whole-file #nullable pragma; this
            // pre-existing `?` type-argument annotation needs an explicit annotations context to
            // avoid CS8632. Scoping narrowly to annotations-only avoids introducing new CS86xx
            // diagnostics elsewhere in this file (no behavior change per AC7).
#nullable enable annotations
            var result = await probe.TimeEngineAsync(
                "Project",
                () => Task.FromResult<IConditionalEngine<MailItemHelper>?>(null)
            );
#nullable restore annotations

            // Assert: null engine produces engineNull=True and costHint=Skip; return is null.
            result.Should().BeNull();
            emitted.Should().ContainSingle();
            var line = emitted[0];
            line.Should().Contain("engineName=Project ");
            line.Should().Contain("engineNull=True ");
            line.Should().Contain("costHint=Skip");
        }

        [TestMethod]
        public void EmitConfigTiming_Always_EmitsOneConfigLineWithFields()
        {
            // Arrange
            var emitted = new List<string>();
            var probe = new TaskMaster.EngineInitTimingProbe(s => emitted.Add(s));

            // Act
            probe.EmitConfigTiming(123.45, 7);

            // Assert: exactly one [engine-init-config] line with the F1 configMs and threadId fields.
            emitted.Should().ContainSingle();
            var line = emitted[0];
            line.Should().StartWith("[engine-init-config] ");
            line.Should().Contain("configMs=123.5"); // F1 rounding of 123.45
            line.Should().Contain("threadId=7");
            line.Should().MatchRegex(@"configMs=\d+\.\d ");
        }

        [TestMethod]
        public async Task TimeEngineAsync_FactoryThrows_PropagatesAndEmitsNoLine()
        {
            // Arrange: a factory that throws, mirroring the pre-instrumentation path where a
            // throwing factory propagated through .SelectAwait (fail-fast; instrumentation must
            // not swallow engine-init failures).
            var emitted = new List<string>();
            var probe = new TaskMaster.EngineInitTimingProbe(s => emitted.Add(s));
            var boom = new InvalidOperationException("engine init failed");

            // Act
            // Same CS8632 annotations-context scoping as above.
#nullable enable annotations
            Func<Task> act = () =>
                probe.TimeEngineAsync(
                    "Context",
                    () => Task.FromException<IConditionalEngine<MailItemHelper>?>(boom)
                );
#nullable restore annotations

            // Assert: the original exception propagates and no [engine-init] line is emitted for
            // the failed call (timing line is only written after a successful await).
            (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage(
                "engine init failed"
            );
            emitted.Should().BeEmpty();
        }

        [TestMethod]
        public async Task TimeEngineAsync_NullArguments_ThrowArgumentNullException()
        {
            // Arrange
            var probe = new TaskMaster.EngineInitTimingProbe(_ => { });

            // Act / Assert: guard clauses fail fast on null engineName or null factory.
            Func<Task> nullName = () =>
                probe.TimeEngineAsync(null!, () => Task.FromResult(StubEngine()));
            Func<Task> nullFactory = () => probe.TimeEngineAsync("Spam", null!);

            await nullName.Should().ThrowAsync<ArgumentNullException>();
            await nullFactory.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_NullSink_ThrowsArgumentNullException()
        {
            // Act / Assert: the sink is a required collaborator.
            Action act = () => new TaskMaster.EngineInitTimingProbe(null!);
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
