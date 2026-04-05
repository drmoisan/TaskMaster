using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectListViewDemo;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class ShellUtilitiesStatic_Tests
    {
        [TestMethod]
        public void Execute_NonexistentPath_ReturnsErrorCode()
        {
            var result = ShellUtilitiesStatic.Execute(@"C:\nonexistent\path\xyz.abc");

            result.Should().BeLessThanOrEqualTo(31);
        }

        [TestMethod]
        public void Execute_WithOperation_ReturnsResult()
        {
            var result = ShellUtilitiesStatic.Execute(@"C:\nonexistent\path\xyz.abc", "open");

            result.Should().BeLessThanOrEqualTo(31);
        }

        [TestMethod]
        public void GetFileType_WithExistingFile_ReturnsNonEmptyString()
        {
            var type = ShellUtilitiesStatic.GetFileType(GetExistingFilePath());

            type.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public void GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension()
        {
            var directoryIcon = ShellUtilitiesStatic.GetFileIcon(
                GetExistingDirectoryPath(),
                isSmallImage: true,
                useFileType: true
            );
            var fileTypeIcon = ShellUtilitiesStatic.GetFileIcon(
                ".txt",
                isSmallImage: false,
                useFileType: true
            );

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
            var icon = ShellUtilitiesStatic.GetFileIcon(
                GetExistingFilePath(),
                isSmallImage: false,
                useFileType: false
            );
            var imageIndex = ShellUtilitiesStatic.GetSysImageIndex(GetExistingFilePath());

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
