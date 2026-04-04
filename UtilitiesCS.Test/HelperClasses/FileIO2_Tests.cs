using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class FileIO2_Tests
    {
        [TestMethod]
        public void DeleteTextFile_WhenTargetIsMissing_ShouldNotThrow()
        {
            Action act = () => FileIO2.DELETE_TextFile("missing.csv", GetMissingFolder());

            act.Should().NotThrow();
        }

        [TestMethod]
        public void WriteTextFile_WhenDevicePathIsUsed_ShouldThrowNotSupportedException()
        {
            Action act = () => FileIO2.WriteTextFile("NUL", new[] { "alpha", "beta" }, "");

            act.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public async Task WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing()
        {
            var (fileName, folderPath) = GetFixtureLocation();
            var filePath = Path.Combine(folderPath, fileName);

            using (new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                Func<Task> act = () =>
                    FileIO2.WriteTextFileAsync(
                        fileName,
                        new[] { "delta" },
                        folderPath,
                        CancellationToken.None
                    );

                await act.Should().NotThrowAsync();
            }
        }

        [TestMethod]
        public void CsvReaders_WithFixtureAndMissingFiles_ShouldRespectHeaderOptions()
        {
            var (fileName, folderPath) = GetFixtureLocation();

            FileIO2.CSV_ReadTxtF(fileName, folderPath).Should().Equal("Alpha,1", "Beta,2");
            FileIO2
                .CSV_ReadTxtF(fileName, folderPath, skipHeaders: false)
                .Should()
                .Equal("Name,Value", "Alpha,1", "Beta,2");
            FileIO2.CSV_ReadTxtF("missing.csv", folderPath).Should().BeNull();
            FileIO2
                .CsvRead(fileName, folderPath, skipHeaders: true)
                .Should()
                .Equal("Alpha,1", "Beta,2");
            FileIO2.CsvRead(fileName, folderPath).Should().Equal("Name,Value", "Alpha,1", "Beta,2");
            FileIO2.CsvRead("missing.csv", folderPath).Should().BeNull();
        }

        [TestMethod]
        public void SplitArrayTo2D_ShouldSupportZeroAndOneBasedLayouts()
        {
            var source = new[] { "A,B", "C,D,E" };

            var oneBased = FileIO2.SplitArrayTo2D(source);
            var zeroBased = FileIO2.SplitArrayTo2D(source, zerobased: true);

            oneBased[1, 1].Should().Be("A");
            oneBased[2, 3].Should().Be("E");
            zeroBased[0, 0].Should().Be("A");
            zeroBased[1, 2].Should().Be("E");
        }

        [TestMethod]
        public void CsvReadTo2D_AndCsvReadToJagged_ShouldProjectFixtureRows()
        {
            var (fileName, folderPath) = GetFixtureLocation();

            var matrix = FileIO2.CsvReadTo2D(fileName, folderPath, skipHeaders: true);
            var jagged = FileIO2.CsvReadToJagged(fileName, folderPath, skipHeaders: true);

            matrix[1, 1].Should().Be("Alpha");
            matrix[2, 2].Should().Be("2");
            jagged[0].Should().Equal("Alpha", "1");
            jagged[1].Should().Equal("Beta", "2");
        }

        private static string GetMissingFolder()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "missing-fileio2-folder-for-tests"
            );
        }

        private static (string FileName, string FolderPath) GetFixtureLocation()
        {
            var fullPath = Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\TestData\FileIO2\sample.csv"
                )
            );

            return (Path.GetFileName(fullPath), Path.GetDirectoryName(fullPath));
        }
    }
}
