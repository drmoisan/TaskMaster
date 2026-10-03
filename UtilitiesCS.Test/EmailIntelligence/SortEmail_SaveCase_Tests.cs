using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for the synchronous SaveCase switch of <see cref="SortEmail"/> (issue #959,
    /// defect L1). Each test passes a Moq attachment and two rooted literal paths; the mocked
    /// SaveAsFile records the call, so no file is created and no dialog is shown.
    /// </summary>
    [TestClass]
    public class SortEmail_SaveCase_Tests
    {
        // Rooted literal paths used only as in-memory values; SaveAsFile is a mock.
        private const string RequestedPath = @"C:\Sortemail959Sandbox\attachments\report.pdf";
        private const string AlternatePath = @"C:\Sortemail959Sandbox\attachments\report_alt.pdf";

        /// <summary>
        /// L1-A. Scenario: the overwrite answer is No or NoToAll. Expected: one save to the
        /// alternate path and none to the requested path.
        /// </summary>
        [DataTestMethod]
        [DataRow(
            YesNoToAllResponse.No,
            DisplayName = "SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [No]"
        )]
        [DataRow(
            YesNoToAllResponse.NoToAll,
            DisplayName = "SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath [NoToAll]"
        )]
        public void SaveCase_WhenAnswerIsNoOrNoToAll_SavesToAlternatePath(YesNoToAllResponse answer)
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);

            // Act
            SortEmail.SaveCase(answer, attachment.Object, RequestedPath, AlternatePath);

            // Assert
            attachment.Verify(x => x.SaveAsFile(AlternatePath), Times.Once);
            attachment.Verify(x => x.SaveAsFile(RequestedPath), Times.Never);
        }

        /// <summary>
        /// L1-B. Scenario: the overwrite answer is Yes or YesToAll. Expected: one save to the
        /// requested path and none to the alternate path.
        /// </summary>
        [DataTestMethod]
        [DataRow(
            YesNoToAllResponse.Yes,
            DisplayName = "SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [Yes]"
        )]
        [DataRow(
            YesNoToAllResponse.YesToAll,
            DisplayName = "SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath [YesToAll]"
        )]
        public void SaveCase_WhenAnswerIsYesOrYesToAll_SavesToRequestedPath(
            YesNoToAllResponse answer
        )
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);

            // Act
            SortEmail.SaveCase(answer, attachment.Object, RequestedPath, AlternatePath);

            // Assert
            attachment.Verify(x => x.SaveAsFile(RequestedPath), Times.Once);
            attachment.Verify(x => x.SaveAsFile(AlternatePath), Times.Never);
        }

        /// <summary>
        /// L1-C (control). Scenario: the answer is Empty (a cancelled prompt). Expected: no save
        /// on either path.
        /// </summary>
        [TestMethod]
        public void SaveCase_WhenAnswerIsEmpty_DoesNotSave()
        {
            // Arrange
            var attachment = new Mock<Attachment>(MockBehavior.Loose);

            // Act
            SortEmail.SaveCase(
                YesNoToAllResponse.Empty,
                attachment.Object,
                RequestedPath,
                AlternatePath
            );

            // Assert
            attachment.Verify(x => x.SaveAsFile(It.IsAny<string>()), Times.Never);
        }
    }
}
