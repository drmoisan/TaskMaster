using System;
using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Unit tests for the issue #503 per-engine-key readiness signal. The states exercised here
    /// correspond to the spec state model S0 through S5: pre-init and init-in-flight (empty
    /// dictionary), init complete with the engine present, init complete with the engine filtered
    /// out by configuration, engine restarted, and a failed initialization.
    /// </summary>
    /// <remarks>
    /// No test reaches the gate through <c>RibbonController.SB</c>, <c>Triage</c>, or
    /// <c>TriageAsync</c>: those getters install a real <c>WindowsFormsSynchronizationContext</c>
    /// on the calling thread as a side effect. The gate is exercised directly through its injected
    /// <c>Func&lt;IAppItemEngines&gt;</c> accessor.
    /// </remarks>
    [TestClass]
    public class EngineReadinessGateTests
    {
        private const string SpamEngineName = "Spam";

        /// <summary>
        /// Wraps the supplied dictionary in a mocked <see cref="IAppItemEngines"/> accessor.
        /// </summary>
        private static Func<IAppItemEngines> AccessorFor(
            ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>> inboxEngines
        )
        {
            var engines = new Mock<IAppItemEngines>();
            engines.SetupGet(x => x.InboxEngines).Returns(inboxEngines);
            return () => engines.Object;
        }

        [TestMethod]
        public void IsEngineReady_WhenAccessorReturnsNull_ReturnsFalse()
        {
            // Arrange: models the window before RibbonController.SetGlobals has run, where
            // `() => Globals?.Engines` yields null.
            var gate = new EngineReadinessGate(() => null);

            // Act
            var ready = gate.IsEngineReady(SpamEngineName);

            // Assert
            ready.Should().BeFalse("a null engines container must read as not ready, never throw");
        }

        [TestMethod]
        public void IsEngineReady_WhenInboxEnginesIsNull_ReturnsFalse()
        {
            // Arrange
            var gate = new EngineReadinessGate(AccessorFor(null));

            // Act
            var ready = gate.IsEngineReady(SpamEngineName);

            // Assert
            ready.Should().BeFalse("a null InboxEngines must read as not ready, never throw");
        }

        [TestMethod]
        public void IsEngineReady_WhenInboxEnginesIsEmpty_ReturnsFalse()
        {
            // Arrange: the #503 repro window — the field-initializer dictionary before
            // InitAsync() assigns the populated one.
            var gate = new EngineReadinessGate(
                AccessorFor(new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>())
            );

            // Act
            var ready = gate.IsEngineReady(SpamEngineName);

            // Assert
            ready.Should().BeFalse("the engine is absent for the whole initialization window");
        }

        [TestMethod]
        public void IsEngineReady_WhenKeyPresentWithNonNullEngine_ReturnsTrue()
        {
            // Arrange
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            inboxEngines[SpamEngineName] = new Mock<IConditionalEngine<MailItemHelper>>().Object;
            var gate = new EngineReadinessGate(AccessorFor(inboxEngines));

            // Act
            var ready = gate.IsEngineReady(SpamEngineName);

            // Assert
            ready.Should().BeTrue("a present, non-null engine is the only ready state");
        }

        [TestMethod]
        public void IsEngineReady_WhenKeyPresentWithNullValue_ReturnsFalse()
        {
            // Arrange: a key whose value is null would still satisfy TryGetValue, so the null
            // value must be rejected explicitly.
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            inboxEngines[SpamEngineName] = null;
            var gate = new EngineReadinessGate(AccessorFor(inboxEngines));

            // Act
            var ready = gate.IsEngineReady(SpamEngineName);

            // Assert
            ready.Should().BeFalse("a present key with a null engine is not a ready state");
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void IsEngineReady_WithNullOrWhitespaceName_ReturnsFalse(string engineName)
        {
            // Arrange: engines are fully loaded, so only the name can make the probe fail.
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            inboxEngines[SpamEngineName] = new Mock<IConditionalEngine<MailItemHelper>>().Object;
            var gate = new EngineReadinessGate(AccessorFor(inboxEngines));

            // Act
            var ready = gate.IsEngineReady(engineName);

            // Assert
            ready
                .Should()
                .BeFalse("a null or whitespace engine name can never identify an engine");
        }

        [TestMethod]
        public void IsEngineReady_IsOrdinalCaseSensitive()
        {
            // Arrange: ConcurrentDictionary's default comparer is ordinal and case-sensitive.
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            inboxEngines[SpamEngineName] = new Mock<IConditionalEngine<MailItemHelper>>().Object;
            var gate = new EngineReadinessGate(AccessorFor(inboxEngines));

            // Act
            var exactCase = gate.IsEngineReady("Spam");
            var lowerCase = gate.IsEngineReady("spam");

            // Assert
            exactCase.Should().BeTrue("\"Spam\" is the registered key");
            lowerCase
                .Should()
                .BeFalse("\"spam\" is not \"Spam\" under the ordinal, case-sensitive default");
        }

        [TestMethod]
        public void IsEngineReady_AfterDictionaryPopulated_ReturnsTrue()
        {
            // Arrange: the same dictionary instance is mutated between the two calls, modelling
            // the S1 -> S2 transition and RestartEngineAsync with no timing dependency at all.
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            var gate = new EngineReadinessGate(AccessorFor(inboxEngines));

            // Act
            var beforePopulation = gate.IsEngineReady(SpamEngineName);
            inboxEngines[SpamEngineName] = new Mock<IConditionalEngine<MailItemHelper>>().Object;
            var afterPopulation = gate.IsEngineReady(SpamEngineName);

            // Assert
            beforePopulation.Should().BeFalse("the engine had not been registered yet");
            afterPopulation
                .Should()
                .BeTrue("readiness is recomputed on every query and is never cached in the gate");
        }

        [TestMethod]
        public void TryGetEngine_WhenReady_OutputsSameInstance()
        {
            // Arrange
            var expected = new Mock<IConditionalEngine<MailItemHelper>>().Object;
            var inboxEngines =
                new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>();
            inboxEngines[SpamEngineName] = expected;
            var gate = new EngineReadinessGate(AccessorFor(inboxEngines));

            // Act
            var resolved = gate.TryGetEngine(SpamEngineName, out var engine);

            // Assert
            resolved.Should().BeTrue();
            engine
                .Should()
                .BeSameAs(expected, "the gate must hand back the registered instance, not a copy");
        }

        [TestMethod]
        public void TryGetEngine_WhenNotReady_OutputsNull()
        {
            // Arrange
            var gate = new EngineReadinessGate(
                AccessorFor(new ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>>())
            );

            // Act
            var resolved = gate.TryGetEngine(SpamEngineName, out var engine);

            // Assert
            resolved.Should().BeFalse();
            engine.Should().BeNull("a failed lookup must not leave a stale out value");
        }

        [TestMethod]
        public void Constructor_WithNullAccessor_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () => new EngineReadinessGate(null);

            // Assert: constructor-time invariant, per repository policy.
            act.Should().Throw<ArgumentNullException>().WithParameterName("enginesAccessor");
        }
    }
}
