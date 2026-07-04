using System;
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
    }
}
