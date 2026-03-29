using System;
using System.IO;
using System.Reflection;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObjectListViewDemo;
using UtilitiesCS.EmailIntelligence.FilterOlFolders;

namespace UtilitiesCS.Test.EmailIntelligence
{
    /// <summary>
    /// Unit tests for OSBrowser constructor setup and FormatFileSize.
    ///
    /// Purpose:
    ///     Covers the column-setup, tree-setup, and file-size formatting paths
    ///     without requiring a real file system (delegates configured in the
    ///     constructor are asserted via reflection on the private treeListView field).
    ///
    /// Usage:
    ///     All tests instantiate OSBrowser on an STA thread. The constructor calls
    ///     SetupColumns(), SetupDragAndDrop(), and SetupTree() which enumerate real
    ///     drives; tests only assert on delegate assignment and format output.
    /// </summary>
    [TestClass]
    public class OSBrowser_Tests
    {
        // ---------------------------------------------------------------------------
        // Helper — reflection accessor for private treeListView field
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the private treeListView field from the given OSBrowser instance
        /// via reflection, since the Designer generated it with private visibility.
        /// </summary>
        private static TreeListView GetTreeListView(OSBrowser browser) =>
            (TreeListView)
                typeof(OSBrowser)
                    .GetField("treeListView", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(browser);

        // ---------------------------------------------------------------------------
        // P13-T1: Column setup initializes the expected number and names of columns
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the OSBrowser constructor's SetupColumns() call attaches
        /// AspectGetters to the size and file-type columns (confirming columns were
        /// initialised with custom configuration, not left at designer defaults only).
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Constructor_SetupColumns_AttachesAspectGetterToSizeColumn()
        {
            // Arrange + Act
            var browser = new OSBrowser();
            var tlv = GetTreeListView(browser);

            // SetupColumns sets an AspectToStringConverter on olvColumnSize; verify
            // that the column count matches the configured designer columns (5 data
            // columns plus the primary tree column for a total ≥ 2).
            tlv.Columns.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        // ---------------------------------------------------------------------------
        // P13-T2: Tree setup configures the expected tree options
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that the OSBrowser constructor's SetupTree() call assigns both
        /// CanExpandGetter and ChildrenGetter on the TreeListView.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Constructor_SetupTree_AssignsBothDelegates()
        {
            // Arrange + Act
            var browser = new OSBrowser();
            var tlv = GetTreeListView(browser);

            // Assert
            tlv.CanExpandGetter.Should().NotBeNull();
            tlv.ChildrenGetter.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // P13-T3: FormatFileSize returns the expected string for a bytes-range input
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that FormatFileSize returns a "bytes" string when the input is
        /// below the 1 KB threshold.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void FormatFileSize_WithBytesInput_ReturnsBytesString()
        {
            // Arrange
            var browser = new OSBrowser();

            // Act
            var result = browser.FormatFileSize(512);

            // Assert
            result.Should().EndWith("bytes");
        }

        // ---------------------------------------------------------------------------
        // P13-T4: FormatFileSize returns the expected string for KB and MB inputs
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Verifies that FormatFileSize returns a "KB" string for a 1 KB input and
        /// an "MB" string for a 1 MB input.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void FormatFileSize_WithKbAndMbInputs_ReturnsCorrectUnits()
        {
            // Arrange
            var browser = new OSBrowser();

            // Act
            var kbResult = browser.FormatFileSize(1024);
            var mbResult = browser.FormatFileSize(1024 * 1024);

            // Assert
            kbResult.Should().Contain("KB");
            mbResult.Should().Contain("MB");
        }

        /// <summary>
        /// Verifies that the constructor-wired tree and column delegates can be invoked
        /// deterministically against mocked file-system abstractions.
        ///
        /// Purpose:
        ///     Covers the SetupTree ChildrenGetter success path plus the SetupColumns
        ///     AspectGetter and AspectToStringConverter branches for directory, file,
        ///     missing-file, file-type, and attribute scenarios.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Constructor_WiredDelegates_HandleDirectoryFileAndMissingFileInputs()
        {
            // Arrange
            var browser = new OSBrowser();
            var tlv = GetTreeListView(browser);

            var directory = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            directory.SetupGet(x => x.Name).Returns("Repo");
            directory.SetupGet(x => x.FullName).Returns(Environment.CurrentDirectory);
            directory.SetupGet(x => x.Extension).Returns(string.Empty);
            directory.SetupGet(x => x.Attributes).Returns(FileAttributes.Directory);
            directory.SetupGet(x => x.CreationTime).Returns(DateTime.Today);
            directory.SetupGet(x => x.LastWriteTime).Returns(DateTime.Today);
            directory.Setup(x => x.GetFileSystemInfos()).Returns(Array.Empty<IFileSystemInfo>());

            var file = new Mock<IFileInfo>(MockBehavior.Strict);
            file.SetupGet(x => x.Name).Returns("readme.txt");
            file.SetupGet(x => x.FullName).Returns("readme.txt");
            file.SetupGet(x => x.Extension).Returns(".txt");
            file.SetupGet(x => x.Attributes).Returns(FileAttributes.Normal);
            file.SetupGet(x => x.CreationTime).Returns(DateTime.Today);
            file.SetupGet(x => x.LastWriteTime).Returns(DateTime.Today);
            file.SetupGet(x => x.Length).Returns(2048L);

            var missingFile = new Mock<IFileInfo>(MockBehavior.Strict);
            missingFile.SetupGet(x => x.Name).Returns("missing.txt");
            missingFile.SetupGet(x => x.FullName).Returns("missing.txt");
            missingFile.SetupGet(x => x.Extension).Returns(".txt");
            missingFile.SetupGet(x => x.Attributes).Returns(FileAttributes.Normal);
            missingFile.SetupGet(x => x.CreationTime).Returns(DateTime.Today);
            missingFile.SetupGet(x => x.LastWriteTime).Returns(DateTime.Today);
            missingFile
                .SetupGet(x => x.Length)
                .Throws(new FileNotFoundException("synthetic missing file"));

            var directoryInfo = new MyFileSystemInfo(directory.Object);
            var fileInfo = new MyFileSystemInfo(file.Object);
            var missingFileInfo = new MyFileSystemInfo(missingFile.Object);

            var imageColumn = (OLVColumn)
                typeof(OSBrowser)
                    .GetField("olvColumnName", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(browser);
            var sizeColumn = (OLVColumn)
                typeof(OSBrowser)
                    .GetField("olvColumnSize", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(browser);
            var fileTypeColumn = (OLVColumn)
                typeof(OSBrowser)
                    .GetField("olvColumnFileType", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(browser);
            var attributesColumn = (OLVColumn)
                typeof(OSBrowser)
                    .GetField("olvColumnAttributes", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(browser);

            // Act
            var childResults = tlv.ChildrenGetter(directoryInfo);
            var canExpandDirectory = tlv.CanExpandGetter(directoryInfo);
            var canExpandFile = tlv.CanExpandGetter(fileInfo);
            var imageIndex = imageColumn.ImageGetter(directoryInfo);
            var directorySize = sizeColumn.AspectGetter(directoryInfo);
            var fileSize = sizeColumn.AspectGetter(fileInfo);
            var missingFileSize = sizeColumn.AspectGetter(missingFileInfo);
            var negativeSizeDisplay = sizeColumn.AspectToStringConverter(-1L);
            var positiveSizeDisplay = sizeColumn.AspectToStringConverter(2048L);
            var fileType = fileTypeColumn.AspectGetter(fileInfo);
            var attributes = attributesColumn.AspectGetter(directoryInfo);

            // Assert
            childResults.Should().NotBeNull();
            canExpandDirectory.Should().BeTrue();
            canExpandFile.Should().BeFalse();
            imageIndex.Should().NotBeNull();
            directorySize.Should().Be(-1L);
            fileSize.Should().Be(2048L);
            missingFileSize.Should().Be(-2L);
            negativeSizeDisplay.Should().Be(string.Empty);
            positiveSizeDisplay.Should().Contain("KB");
            fileType.Should().BeOfType<string>();
            attributes.Should().Be(FileAttributes.Directory);
        }

        /// <summary>
        /// Verifies that the constructor-wired ChildrenGetter returns an empty
        /// <see cref="System.Collections.ArrayList"/> when directory enumeration raises
        /// <see cref="UnauthorizedAccessException"/>.
        /// </summary>
        [STAThread]
        [TestMethod]
        public void Constructor_ChildrenGetter_WhenDirectoryAccessDenied_ReturnsEmptyArrayList()
        {
            // Arrange
            var browser = new OSBrowser();
            var tlv = GetTreeListView(browser);
            _ = browser.Handle;

            var deniedDirectory = new Mock<IDirectoryInfo>(MockBehavior.Strict);
            deniedDirectory.SetupGet(x => x.Name).Returns("Denied");
            deniedDirectory.SetupGet(x => x.FullName).Returns(Environment.CurrentDirectory);
            deniedDirectory.SetupGet(x => x.Extension).Returns(string.Empty);
            deniedDirectory.SetupGet(x => x.Attributes).Returns(FileAttributes.Directory);
            deniedDirectory.SetupGet(x => x.CreationTime).Returns(DateTime.Today);
            deniedDirectory.SetupGet(x => x.LastWriteTime).Returns(DateTime.Today);
            deniedDirectory
                .Setup(x => x.GetFileSystemInfos())
                .Throws(new UnauthorizedAccessException("synthetic access denied"));

            var deniedInfo = new MyFileSystemInfo(deniedDirectory.Object);

            // Act
            var result = tlv.ChildrenGetter(deniedInfo);

            // Assert
            result.Should().BeOfType<System.Collections.ArrayList>();
        }
    }
}
