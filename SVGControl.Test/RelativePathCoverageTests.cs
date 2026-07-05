using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SVGControl;

namespace SVGControl.Test
{
    [TestClass]
    public class RelativePathCoverageTests
    {
        [TestMethod]
        public void MakeRelativePath_WhenPathsShareRoot_ReturnsDecodedRelativeFilePath()
        {
            string anchorPath = "C:\\Root Folder\\Parent\\";
            string targetPath = "C:\\Root Folder\\Parent\\Child Folder\\Report File.svg";

            string relativePath = targetPath.MakeRelativePath(anchorPath);

            relativePath.Should().Be("Child Folder\\Report File.svg");
        }

        [TestMethod]
        public void MakeRelativePath_WhenSchemesDiffer_ReturnsOriginalTarget()
        {
            string anchorPath = "file:///C:/Root/Parent/";
            string targetPath = "https://example.test/assets/icon.svg";

            string relativePath = targetPath.MakeRelativePath(anchorPath);

            relativePath.Should().Be(targetPath);
        }

        [TestMethod]
        public void GetRelativeUri_WhenTargetIsDescendant_AddsCurrentDirectoryPrefix()
        {
            string anchorPath = "C:\\Root Folder\\Parent\\";
            string targetPath = "C:\\Root Folder\\Parent\\Child Folder\\Report File.svg";

            string relativeUri = targetPath.GetRelativeURI(anchorPath);

            relativeUri.Should().Be("./Child Folder/Report File.svg");
        }

        [TestMethod]
        public void GetRelativeUri_WhenTargetRequiresTraversal_PreservesTraversalSegments()
        {
            string anchorPath = "C:\\Root\\Parent\\Nested\\";
            string targetPath = "C:\\Root\\Sibling\\Icon.svg";

            string relativeUri = targetPath.GetRelativeURI(anchorPath);

            relativeUri.Should().Be("../../Sibling/Icon.svg");
        }

        [TestMethod]
        public void AbsoluteFromUri_WhenUriIsRelativeTraversal_NormalizesSegments()
        {
            string anchorPath = "C:\\Root\\Parent\\Nested\\";
            string relativeUri = "../../Sibling/Icon.svg";

            string absolutePath = relativeUri.AbsoluteFromURI(anchorPath);

            absolutePath.Should().Be("C:\\Root\\Sibling\\Icon.svg");
        }

        [TestMethod]
        public void AbsoluteFromUri_WhenUriIsAbsoluteUri_ReturnsOriginalValue()
        {
            string absoluteUri = "https://example.test/assets/icon.svg";

            string result = absoluteUri.AbsoluteFromURI("C:\\Root\\Parent\\");

            result.Should().Be(absoluteUri);
        }

        [TestMethod]
        public void GetFullPath_WhenRelativePathIsDriveRooted_UsesBaseDrive()
        {
            string absolutePath = RelativePath.GetFullPath(
                "\\Sibling\\Icon.svg",
                "C:\\Root\\Parent\\"
            );

            absolutePath.Should().Be("C:\\Sibling\\Icon.svg");
        }

        [TestMethod]
        public void GetFullPath_WhenBasePathIsNotFullyQualified_ThrowsArgumentException()
        {
            Action act = () => RelativePath.GetFullPath("Child\\Icon.svg", "Root\\Parent\\");

            act.Should()
                .Throw<ArgumentException>()
                .WithParameterName("basePath")
                .WithMessage("*Arg_BasePathNotFullyQualified*");
        }

        [TestMethod]
        public void PublicPathMethods_WhenRequiredInputsAreEmpty_ThrowArgumentNullException()
        {
            Action emptyAnchor = () => "C:\\Root\\Icon.svg".MakeRelativePath(string.Empty);
            Action emptyTarget = () => string.Empty.GetRelativeURI("C:\\Root\\");
            Action emptyAbsoluteInput = () => string.Empty.AbsoluteFromPath("C:\\Root\\");

            emptyAnchor.Should().Throw<ArgumentNullException>().WithParameterName("anchorPath");
            emptyTarget
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("pathToMakeRelative");
            emptyAbsoluteInput
                .Should()
                .Throw<ArgumentNullException>()
                .WithParameterName("pathToMakeAbsolute");
        }

        [TestMethod]
        public void GetFullPath_WhenPathIsAlreadyFullyQualified_ReturnsNormalizedPath()
        {
            string absolutePath = RelativePath.GetFullPath(
                "C:\\Root\\Parent\\..\\Sibling\\Icon.svg",
                "D:\\Base\\Folder\\"
            );

            absolutePath.Should().Be("C:\\Root\\Sibling\\Icon.svg");
        }

        [DataTestMethod]
        [DataRow("C:Child\\Icon.svg", "C:\\Root\\Parent\\", "C:\\Root\\Parent\\Child\\Icon.svg")]
        [DataRow("D:Child\\Icon.svg", "C:\\Root\\Parent\\", "D:\\Child\\Icon.svg")]
        [DataRow(
            "Child\\..\\Sibling\\.\\Icon.svg",
            "C:\\Root\\Parent\\",
            "C:\\Root\\Parent\\Sibling\\Icon.svg"
        )]
        public void GetFullPath_WithRelativeForms_NormalizesExpectedPath(
            string path,
            string basePath,
            string expected
        )
        {
            string absolutePath = RelativePath.GetFullPath(path, basePath);

            absolutePath.Should().Be(expected);
        }

        [TestMethod]
        public void GetFullPath_WhenInputsContainNullCharacter_ThrowsArgumentException()
        {
            Action pathAct = () => RelativePath.GetFullPath("Child\0Icon.svg", "C:\\Root\\");
            Action baseAct = () => RelativePath.GetFullPath("Child\\Icon.svg", "C:\\Root\0\\");

            pathAct.Should().Throw<ArgumentException>().WithMessage("*Argument_InvalidPathChars*");
            baseAct.Should().Throw<ArgumentException>().WithMessage("*Argument_InvalidPathChars*");
        }

        [DataTestMethod]
        [DataRow("C:\\Root\\Child\\..\\Sibling\\.\\Icon.svg", 3, "C:\\Root\\Sibling\\Icon.svg")]
        [DataRow("C:\\Root\\\\Child//Icon.svg", 3, "C:\\Root\\Child\\\\Icon.svg")]
        [DataRow("C:\\Root\\Child\\Icon.svg", 3, "C:\\Root\\Child\\Icon.svg")]
        public void RemoveRelativeSegments_NormalizesTraversalAndSeparators(
            string path,
            int rootLength,
            string expected
        )
        {
            string normalized = RelativePath.RemoveRelativeSegments(path, rootLength);

            normalized.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("C:\\Root\\Child", 3)]
        [DataRow("C:Root\\Child", 2)]
        [DataRow("\\\\Server\\Share\\Folder", 14)]
        [DataRow("\\\\?\\UNC\\Server\\Share\\Folder", 20)]
        [DataRow("\\\\?\\C:\\Root", 7)]
        public void GetRootLength_DetectsDosUncAndDeviceRoots(string path, int expected)
        {
            RelativePath.GetRootLength(path).Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(RelativePath.ERROR_FILE_NOT_FOUND, "missing.svg", typeof(FileNotFoundException))]
        [DataRow(
            RelativePath.ERROR_PATH_NOT_FOUND,
            "C:\\Missing",
            typeof(DirectoryNotFoundException)
        )]
        [DataRow(
            RelativePath.ERROR_ACCESS_DENIED,
            "C:\\Denied",
            typeof(UnauthorizedAccessException)
        )]
        [DataRow(RelativePath.ERROR_OPERATION_ABORTED, "", typeof(OperationCanceledException))]
        [DataRow(RelativePath.ERROR_FILENAME_EXCED_RANGE, "", typeof(PathTooLongException))]
        public void GetExceptionForWin32Error_ReturnsSpecificExceptionTypes(
            int errorCode,
            string path,
            Type expectedType
        )
        {
            Exception exception = RelativePath.GetExceptionForWin32Error(errorCode, path);

            exception.Should().BeOfType(expectedType);
        }

        [TestMethod]
        public void ErrorCodeHelpers_ConvertBetweenWin32AndHResultForms()
        {
            int hr = RelativePath.MakeHRFromErrorCode(RelativePath.ERROR_ACCESS_DENIED);

            hr.Should().Be(unchecked((int)0x80070005));
            RelativePath.MakeHRFromErrorCode(hr).Should().Be(hr);
            RelativePath
                .TryMakeWin32ErrorCodeFromHR(hr)
                .Should()
                .Be(RelativePath.ERROR_ACCESS_DENIED);
            RelativePath.TryMakeWin32ErrorCodeFromHR(1234).Should().Be(1234);
        }
    }
}
