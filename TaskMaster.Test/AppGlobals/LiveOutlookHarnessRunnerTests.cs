#nullable enable
using System;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.AppGlobals
{
    /// <summary>
    /// Deterministic regression tests for <see cref="LiveOutlookHarnessRunner.Run{T}"/>. These run
    /// in the standard suite (NO <c>[TestCategory("LiveOutlook")]</c>), touch no live Outlook, and
    /// use no temporary files.
    /// </summary>
    /// <remarks>
    /// No Moq mock is used: the <see cref="Func{T}"/> (construct) and <see cref="Action{T}"/>
    /// (exercise) delegate parameters ARE the injected seam, so each scenario is expressed directly
    /// as a delegate that throws or succeeds. This is the minimal seam per repo DI guidance.
    /// </remarks>
    [TestClass]
    public class LiveOutlookHarnessRunnerTests
    {
        private const int RpcESysCallFailed = unchecked((int)0x80010100); // RPC_E_SYS_CALL_FAILED
        private const int EFail = unchecked((int)0x80004005); // E_FAIL (arbitrary non-availability HRESULT)

        [TestMethod]
        public void Run_WhenConstructionThrowsRpcSysCallFailedComException_ReportsSkipNotCapture()
        {
            // Arrange: construction throws the specific defect HRESULT 0x80010100.
            Func<object> construct = () =>
                throw new COMException("RPC to Outlook failed on construction", RpcESysCallFailed);
            Action<object> exercise = _ => { };

            // Act
            var outcome = LiveOutlookHarnessRunner.Run(construct, exercise);

            // Assert: a construction COMException is a skip regardless of HRESULT.
            outcome
                .SkipReason.Should()
                .NotBeNull("a construction-phase COMException must report a skip");
            outcome
                .SkipReason.Should()
                .Contain("80010100", "the skip reason must surface the offending HRESULT");
            outcome.Captured.Should().BeNull("a skip must not also capture a failure");
        }

        [TestMethod]
        public void Run_WhenConstructionThrowsArbitraryHResultComException_ReportsSkipRegardlessOfHResult()
        {
            // Arrange: construction throws a DIFFERENT HRESULT that a narrow whitelist would miss.
            Func<object> construct = () =>
                throw new COMException("construction failed with E_FAIL", EFail);
            Action<object> exercise = _ => { };

            // Act
            var outcome = LiveOutlookHarnessRunner.Run(construct, exercise);

            // Assert: skip decision is scoped to the construction phase, not to an HRESULT set.
            outcome
                .SkipReason.Should()
                .NotBeNull("any construction-phase COMException must report a skip");
            outcome
                .SkipReason.Should()
                .Contain("80004005", "the skip reason must surface the offending HRESULT");
            outcome.Captured.Should().BeNull("a skip must not also capture a failure");
        }

        [TestMethod]
        public void Run_WhenConstructionThrowsNonComException_CapturesFailureAndDoesNotSkip()
        {
            // Arrange: construction throws a NON-COM exception. This is a real failure, not an
            // "Outlook unavailable" condition, so it must be captured as a failure and never
            // converted to a skip (only construction-phase COMExceptions are skips).
            var thrown = new InvalidOperationException("construction failed for a non-COM reason");
            Func<object> construct = () => throw thrown;
            Action<object> exercise = _ => { };

            // Act
            var outcome = LiveOutlookHarnessRunner.Run(construct, exercise);

            // Assert
            outcome
                .Captured.Should()
                .BeSameAs(thrown, "a non-COM construction failure must be captured, not skipped");
            outcome.SkipReason.Should().BeNull("a non-COM construction failure must not be a skip");
        }

        [TestMethod]
        public void Run_WhenExerciseThrowsInvalidOperationException_CapturesFailure()
        {
            // Arrange: construction succeeds; the exercise phase throws a non-COM exception.
            var thrown = new InvalidOperationException("exercise phase fault");
            Func<object> construct = () => new object();
            Action<object> exercise = _ => throw thrown;

            // Act
            var outcome = LiveOutlookHarnessRunner.Run(construct, exercise);

            // Assert: exercise-phase exceptions are captured failures, never skips.
            outcome
                .Captured.Should()
                .BeSameAs(thrown, "an exercise-phase exception must be captured as the failure");
            outcome.SkipReason.Should().BeNull("an exercise-phase failure must not be a skip");
        }

        [TestMethod]
        public void Run_WhenExerciseThrowsComException_CapturesFailureAndDoesNotSkip()
        {
            // Arrange: construction succeeds; the exercise phase throws a COMException. This must
            // still be a captured failure — strict failure semantics are retained for the phase in
            // which code-under-test runs, even for COMExceptions.
            var thrown = new COMException("exercise-phase COM fault", EFail);
            Func<object> construct = () => new object();
            Action<object> exercise = _ => throw thrown;

            // Act
            var outcome = LiveOutlookHarnessRunner.Run(construct, exercise);

            // Assert
            outcome
                .Captured.Should()
                .BeSameAs(
                    thrown,
                    "an exercise-phase COMException must be captured, not swallowed as a skip"
                );
            outcome.SkipReason.Should().BeNull("an exercise-phase COMException must not be a skip");
        }

        [TestMethod]
        public void Run_WhenBothPhasesSucceed_ReportsNeitherSkipNorCaptureAndRunsExercise()
        {
            // Arrange: construction and exercise both succeed; a side-effect flag proves the
            // exercise delegate actually ran.
            var exercised = false;
            Func<object> construct = () => new object();
            Action<object> exercise = _ => exercised = true;

            // Act
            var outcome = LiveOutlookHarnessRunner.Run(construct, exercise);

            // Assert
            outcome.Captured.Should().BeNull("a successful run must capture nothing");
            outcome.SkipReason.Should().BeNull("a successful run must not skip");
            exercised.Should().BeTrue("the exercise delegate must have been invoked");
        }

        [TestMethod]
        public void Run_WhenConstructDelegateIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Action<object> exercise = _ => { };

            // Act
            Action act = () => LiveOutlookHarnessRunner.Run<object>(null!, exercise);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("construct");
        }

        [TestMethod]
        public void Run_WhenExerciseDelegateIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Func<object> construct = () => new object();

            // Act
            Action act = () => LiveOutlookHarnessRunner.Run<object>(construct, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("exercise");
        }
    }
}
