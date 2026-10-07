using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for the seeded moved-mails header of <see cref="SortEmail"/> (issue #959,
    /// defect L4 and the header shape). Both tests pass recording delegates for the file-existence
    /// check and the text write, so the rooted literal paths are never touched on disk.
    /// </summary>
    [TestClass]
    public class SortEmail_UndoAndMoveLog_Tests
    {
        private const string LogFolder = @"C:\Sortemail959Sandbox\logs";
        private const string LogFileName = "MovedMails.txt";
        private const string ExpectedHeader =
            "Triage\tFolderName\tSent_On\tFrom\tTo\tCC\tSubject\tBody\tfromDomain\tConversation_ID\tEntryID\tAttachments\tFlaggedAsTask";

        /// <summary>
        /// L4-T1. Scenario: the log file does not exist. Expected: the existence check receives
        /// the folder-then-file-name combination and exactly one write of a single tab-separated
        /// header line with thirteen columns goes to that file name and folder.
        /// </summary>
        [TestMethod]
        public void WriteCSV_WhenFileDoesNotExist_WritesSingleTabSeparatedHeader()
        {
            // Arrange
            var queried = new List<string>();
            var writes = new List<(string FileName, string[] Lines, string Folder)>();
            Func<string, bool> fileExists = path =>
            {
                queried.Add(path);
                return false;
            };
            Action<string, string[], string> writeTextFile = (fileName, lines, folder) =>
                writes.Add((fileName, lines, folder));

            // Act
            SortEmail.WriteCSV_StartNewFileIfDoesNotExist(
                LogFileName,
                LogFolder,
                fileExists,
                writeTextFile
            );

            // Assert
            queried.Should().Equal(Path.Combine(LogFolder, LogFileName));
            writes.Should().ContainSingle();
            writes[0].FileName.Should().Be(LogFileName);
            writes[0].Folder.Should().Be(LogFolder);
            writes[0].Lines.Should().ContainSingle();
            writes[0].Lines[0].Should().Be(ExpectedHeader);
            writes[0].Lines[0].Split('\t').Should().HaveCount(13);
        }

        /// <summary>
        /// L4-T2. Scenario: the log file already exists. Expected: nothing is written.
        /// </summary>
        [TestMethod]
        public void WriteCSV_WhenFileExists_DoesNotWrite()
        {
            // Arrange
            var writes = 0;
            Func<string, bool> fileExists = _ => true;
            Action<string, string[], string> writeTextFile = (_, _, _) => writes++;

            // Act
            SortEmail.WriteCSV_StartNewFileIfDoesNotExist(
                LogFileName,
                LogFolder,
                fileExists,
                writeTextFile
            );

            // Assert
            writes.Should().Be(0);
        }
    }
}
