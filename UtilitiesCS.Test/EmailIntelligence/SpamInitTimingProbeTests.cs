using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Deterministic tests for the pure <see cref="SpamInitTimingProbe"/> line-formatting/emission
    /// helper introduced for the issue #211 Phase 3.5 SpamBayes-init attribution probe. The probe
    /// has no clock reads, no Stopwatch, no COM, and no I/O, so these tests use a list-capturing
    /// sink and exercise the formatter directly. No live COM, no live timer, no network/filesystem,
    /// no temporary files.
    /// </summary>
    [TestClass]
    public class SpamInitTimingProbeTests
    {
        private static SpamInitTimingProbe CreateProbe(out List<string> captured)
        {
            var lines = new List<string>();
            captured = lines;
            return new SpamInitTimingProbe(s => lines.Add(s));
        }

        // --- FormatStep: exact structured output (P2-T3 a) ---

        [TestMethod]
        public void FormatStep_SubStepValidatePathsSet_ProducesExactStructuredLine()
        {
            // Arrange
            var probe = CreateProbe(out _);

            // Act
            var line = probe.FormatStep("ValidatePathsSet", 12.0);

            // Assert
            line.Should().Be("[spam-init] step=ValidatePathsSet ms=12.0");
        }

        [TestMethod]
        public void FormatStep_SubStepValidateSpamClassifier_ProducesExactStructuredLine()
        {
            // Arrange
            var probe = CreateProbe(out _);

            // Act
            var line = probe.FormatStep("ValidateSpamClassifier", 3.5);

            // Assert
            line.Should().Be("[spam-init] step=ValidateSpamClassifier ms=3.5");
        }

        [TestMethod]
        public void FormatStep_SubStepInitAsyncModelLoad_ProducesExactStructuredLine()
        {
            // Arrange
            var probe = CreateProbe(out _);

            // Act
            var line = probe.FormatStep("InitAsync(modelLoad)", 67500.0);

            // Assert
            line.Should().Be("[spam-init] step=InitAsync(modelLoad) ms=67500.0");
        }

        [TestMethod]
        public void FormatStep_FolderJunkCertain_ProducesExactStructuredLine()
        {
            // Arrange
            var probe = CreateProbe(out _);

            // Act
            var line = probe.FormatStep("ValidatePathsSet.JunkCertain", 0.2);

            // Assert
            line.Should().Be("[spam-init] step=ValidatePathsSet.JunkCertain ms=0.2");
        }

        [TestMethod]
        public void FormatStep_FolderJunkPotential_ProducesExactStructuredLine()
        {
            // Arrange
            var probe = CreateProbe(out _);

            // Act
            var line = probe.FormatStep("ValidatePathsSet.JunkPotential", 1.4);

            // Assert
            line.Should().Be("[spam-init] step=ValidatePathsSet.JunkPotential ms=1.4");
        }

        [TestMethod]
        public void FormatStep_FolderInbox_ProducesExactStructuredLine()
        {
            // Arrange
            var probe = CreateProbe(out _);

            // Act
            var line = probe.FormatStep("ValidatePathsSet.Inbox", 113000.0);

            // Assert
            line.Should().Be("[spam-init] step=ValidatePathsSet.Inbox ms=113000.0");
        }

        // --- FormatStep: F1 / InvariantCulture formatting (P2-T3 b) ---

        [TestMethod]
        public void FormatStep_FractionalMilliseconds_RoundsToOneDecimalPlaceInvariant()
        {
            // Arrange
            var probe = CreateProbe(out _);

            // Act
            var line = probe.FormatStep("InitAsync(modelLoad)", 115449.45);

            // Assert
            // F1 rounds to one decimal place; InvariantCulture uses '.' as the decimal separator.
            line.Should().Be("[spam-init] step=InitAsync(modelLoad) ms=115449.5");
        }

        [TestMethod]
        public void FormatStep_WholeNumberMilliseconds_AppendsOneDecimalZero()
        {
            // Arrange
            var probe = CreateProbe(out _);

            // Act
            var line = probe.FormatStep("ValidatePathsSet", 0.0);

            // Assert
            line.Should().Be("[spam-init] step=ValidatePathsSet ms=0.0");
        }

        // --- EmitStep: routes the formatted line to the sink exactly once (P2-T3 c) ---

        [TestMethod]
        public void EmitStep_RoutesFormattedLineToSinkExactlyOnce()
        {
            // Arrange
            var probe = CreateProbe(out var captured);

            // Act
            probe.EmitStep("ValidatePathsSet.Inbox", 113000.0);

            // Assert
            captured.Should().ContainSingle();
            captured[0].Should().Be("[spam-init] step=ValidatePathsSet.Inbox ms=113000.0");
        }

        [TestMethod]
        public void EmitStep_MultipleCalls_EmitOneLinePerCallInOrder()
        {
            // Arrange
            var probe = CreateProbe(out var captured);

            // Act
            probe.EmitStep("ValidatePathsSet", 1.0);
            probe.EmitStep("ValidateSpamClassifier", 2.0);
            probe.EmitStep("InitAsync(modelLoad)", 3.0);

            // Assert
            captured
                .Should()
                .Equal(
                    "[spam-init] step=ValidatePathsSet ms=1.0",
                    "[spam-init] step=ValidateSpamClassifier ms=2.0",
                    "[spam-init] step=InitAsync(modelLoad) ms=3.0"
                );
        }

        // --- Constructor null-guard (P2-T3 d) ---

        [TestMethod]
        public void Constructor_NullEmit_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            Action act = () => _ = new SpamInitTimingProbe(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("emit");
        }

        // --- step null-guard on FormatStep and EmitStep (P2-T3 e) ---

        [TestMethod]
        public void FormatStep_NullStep_ThrowsArgumentNullException()
        {
            // Arrange
            var probe = CreateProbe(out _);

            // Act
            Action act = () => probe.FormatStep(null, 1.0);

            // Assert
            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("step");
        }

        [TestMethod]
        public void EmitStep_NullStep_ThrowsArgumentNullExceptionAndEmitsNothing()
        {
            // Arrange
            var probe = CreateProbe(out var captured);

            // Act
            Action act = () => probe.EmitStep(null, 1.0);

            // Assert
            act.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("step");
            captured.Should().BeEmpty();
        }
    }
}
