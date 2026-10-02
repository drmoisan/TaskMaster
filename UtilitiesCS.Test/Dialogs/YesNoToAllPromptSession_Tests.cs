using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.Dialogs
{
    /// <summary>
    /// Unit tests for <see cref="YesNoToAllPromptSession"/>: delegate validation, prompting only
    /// while no answer is held, releasing single-use answers, keeping "ToAll" answers and
    /// resetting. Every prompt is a test delegate; no dialog is shown and no state is shared
    /// between tests.
    /// </summary>
    [TestClass]
    public class YesNoToAllPromptSession_Tests
    {
        /// <summary>
        /// S1. Scenario: the constructor receives a null prompt delegate. Expected:
        /// ArgumentNullException naming the showDialog parameter.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenShowDialogIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Func<string, YesNoToAllResponse> showDialog = null;

            // Act
            Action act = () => _ = new YesNoToAllPromptSession(showDialog);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("showDialog");
        }

        /// <summary>
        /// S2. Scenario: Ask is called on a new session, which holds no answer. Expected: the
        /// prompt receives the message once, its answer is returned and the session holds it.
        /// </summary>
        [TestMethod]
        public void Ask_WhenNoAnswerIsHeld_InvokesPromptAndStoresAnswer()
        {
            // Arrange
            var messages = new List<string>();
            var session = new YesNoToAllPromptSession(message =>
            {
                messages.Add(message);
                return YesNoToAllResponse.Yes;
            });
            var initial = session.Response;

            // Act
            var answer = session.Ask("first");

            // Assert
            initial.Should().Be(YesNoToAllResponse.Empty);
            answer.Should().Be(YesNoToAllResponse.Yes);
            session.Response.Should().Be(YesNoToAllResponse.Yes);
            messages.Should().Equal("first");
        }

        /// <summary>
        /// S3. Scenario: Ask is called while the session holds an answer. Expected: the held
        /// answer is returned and the prompt is not invoked again.
        /// </summary>
        [TestMethod]
        public void Ask_WhenAnswerIsHeld_ReturnsItWithoutInvokingPrompt()
        {
            // Arrange
            var calls = 0;
            var session = new YesNoToAllPromptSession(_ =>
            {
                calls++;
                return YesNoToAllResponse.NoToAll;
            });
            _ = session.Ask("first");

            // Act
            var answer = session.Ask("second");

            // Assert
            answer.Should().Be(YesNoToAllResponse.NoToAll);
            calls.Should().Be(1);
        }

        /// <summary>
        /// S4. Scenario: ReleaseSingleAnswer is called on sessions holding Yes and No. Expected:
        /// both sessions hold no answer afterwards.
        /// </summary>
        [TestMethod]
        public void ReleaseSingleAnswer_WhenAnswerIsYesOrNo_ClearsIt()
        {
            // Arrange
            var yesSession = new YesNoToAllPromptSession(_ => YesNoToAllResponse.Yes);
            var noSession = new YesNoToAllPromptSession(_ => YesNoToAllResponse.No);
            _ = yesSession.Ask("question");
            _ = noSession.Ask("question");

            // Act
            yesSession.ReleaseSingleAnswer();
            noSession.ReleaseSingleAnswer();

            // Assert
            yesSession.Response.Should().Be(YesNoToAllResponse.Empty);
            noSession.Response.Should().Be(YesNoToAllResponse.Empty);
        }

        /// <summary>
        /// S5. Scenario: ReleaseSingleAnswer is called on sessions holding YesToAll and NoToAll.
        /// Expected: both sessions keep their answer.
        /// </summary>
        [TestMethod]
        public void ReleaseSingleAnswer_WhenAnswerIsYesToAllOrNoToAll_KeepsIt()
        {
            // Arrange
            var yesSession = new YesNoToAllPromptSession(_ => YesNoToAllResponse.YesToAll);
            var noSession = new YesNoToAllPromptSession(_ => YesNoToAllResponse.NoToAll);
            _ = yesSession.Ask("question");
            _ = noSession.Ask("question");

            // Act
            yesSession.ReleaseSingleAnswer();
            noSession.ReleaseSingleAnswer();

            // Assert
            yesSession.Response.Should().Be(YesNoToAllResponse.YesToAll);
            noSession.Response.Should().Be(YesNoToAllResponse.NoToAll);
        }

        /// <summary>
        /// S6. Scenario: Reset is called while the session holds a "ToAll" answer. Expected: the
        /// session holds no answer and the next Ask invokes the prompt again.
        /// </summary>
        [TestMethod]
        public void Reset_WhenToAllAnswerIsHeld_ClearsItSoThePromptIsShownAgain()
        {
            // Arrange
            var calls = 0;
            var session = new YesNoToAllPromptSession(_ =>
            {
                calls++;
                return YesNoToAllResponse.YesToAll;
            });
            _ = session.Ask("first");

            // Act
            session.Reset();
            var afterReset = session.Response;
            _ = session.Ask("second");

            // Assert
            afterReset.Should().Be(YesNoToAllResponse.Empty);
            calls.Should().Be(2);
        }

        /// <summary>
        /// S7. Scenario: the prompt returns Empty (the Cancel button). Expected: the session
        /// holds no answer and the next Ask invokes the prompt again.
        /// </summary>
        [TestMethod]
        public void Ask_WhenPromptReturnsEmpty_HoldsNoAnswerAndAsksAgain()
        {
            // Arrange
            var messages = new List<string>();
            var session = new YesNoToAllPromptSession(message =>
            {
                messages.Add(message);
                return YesNoToAllResponse.Empty;
            });

            // Act
            var first = session.Ask("first");
            var second = session.Ask("second");

            // Assert
            first.Should().Be(YesNoToAllResponse.Empty);
            second.Should().Be(YesNoToAllResponse.Empty);
            session.Response.Should().Be(YesNoToAllResponse.Empty);
            messages.Should().Equal("first", "second");
        }
    }
}
