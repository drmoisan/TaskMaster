using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for the attachment-saving partial of <see cref="SortEmail"/> (issue #959).
    /// This phase-one file carries the L3 regression test against the four static enum answer
    /// fields; a later phase of the same item replaces it when the fields become prompt
    /// sessions.
    /// </summary>
    [TestClass]
    public class SortEmail_AttachmentSaving_Tests
    {
        /// <summary>
        /// L3 (phase one). Scenario: a sticky answer is held in one of the four static answer
        /// fields and Cleanup_Files runs. Expected: the field reads Empty afterwards. The
        /// reflective static write is order-independent: the only concurrent writer in a test
        /// run writes the value this assertion expects (UT5 call-out in the feature spec).
        /// </summary>
        [DataTestMethod]
        [DataRow(
            "_responseSaveFile",
            DisplayName = "Cleanup_Files_ResetsEveryPromptAnswerField [_responseSaveFile]"
        )]
        [DataRow(
            "_attachmentsOverwrite",
            DisplayName = "Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsOverwrite]"
        )]
        [DataRow(
            "_attachmentsAltName",
            DisplayName = "Cleanup_Files_ResetsEveryPromptAnswerField [_attachmentsAltName]"
        )]
        [DataRow(
            "_picturesOverwrite",
            DisplayName = "Cleanup_Files_ResetsEveryPromptAnswerField [_picturesOverwrite]"
        )]
        public void Cleanup_Files_ResetsEveryPromptAnswerField(string fieldName)
        {
            // Arrange
            var field = typeof(SortEmail).GetField(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Static
            );
            field.Should().NotBeNull();
            field.SetValue(null, YesNoToAllResponse.YesToAll);

            // Act
            SortEmail.Cleanup_Files();

            // Assert
            field.GetValue(null).Should().Be(YesNoToAllResponse.Empty);
        }
    }
}
