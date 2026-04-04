using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectListViewDemo;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class ShellUtilities_Tests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_CreatesInstance()
        {
            var su = new ShellUtilities();
            su.Should().NotBeNull();
        }

        #endregion

        #region GetFileType

        [TestMethod]
        public void GetFileType_ExeExtension_ReturnsNonEmptyString()
        {
            var su = new ShellUtilities();
            var type = su.GetFileType(GetExistingFilePath());

            type.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension()
        {
            var su = new ShellUtilities();
            var directoryIcon = su.GetFileIcon(
                GetExistingDirectoryPath(),
                isSmallImage: true,
                useFileType: true
            );
            var fileTypeIcon = su.GetFileIcon(".txt", isSmallImage: false, useFileType: true);

            try
            {
                directoryIcon.Should().NotBeNull();
                fileTypeIcon.Should().NotBeNull();
            }
            finally
            {
                directoryIcon?.Dispose();
                fileTypeIcon?.Dispose();
            }
        }

        [TestMethod]
        public void GetFileIcon_AndGetSysImageIndex_WithExistingFile_ShouldReturnShellMetadata()
        {
            var su = new ShellUtilities();
            var icon = su.GetFileIcon(
                GetExistingFilePath(),
                isSmallImage: false,
                useFileType: false
            );
            var imageIndex = su.GetSysImageIndex(GetExistingFilePath());

            try
            {
                icon.Should().NotBeNull();
                imageIndex.Should().BeGreaterThanOrEqualTo(0);
            }
            finally
            {
                icon?.Dispose();
            }
        }

        #endregion

        #region Execute

        [TestMethod]
        public void Execute_NonexistentPath_ReturnsErrorCode()
        {
            var su = new ShellUtilities();
            // ShellExecute returns values < 31 for errors
            var result = su.Execute("C:\\nonexistent\\path\\xyz.abc");
            result.Should().BeLessThanOrEqualTo(31);
        }

        [TestMethod]
        public void Execute_WithOperation_ReturnsResult()
        {
            var su = new ShellUtilities();
            var result = su.Execute("C:\\nonexistent\\path\\xyz.abc", "open");
            result.Should().BeLessThanOrEqualTo(31);
        }

        #endregion

        private static string GetExistingDirectoryPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private static string GetExistingFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UtilitiesCS.Test.dll");
        }
    }
}
