using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SVGControl;

namespace SVGControl.Test
{
    [TestClass]
    public class GetRelativePath_Test
    {
        [TestMethod]
        public void MakeRelativePath_Test()
        {
            string anchorPath = "C:\\L1\\L2\\L3\\";
            string pathToMakeRelative = "C:\\L1\\L2\\L3\\L4\\L5\\test.txt";
            string testRelative = RelativePath.MakeRelativePath(
                pathToMakeRelative: pathToMakeRelative, anchorPath: anchorPath);
            
            string targetRelative = "L4\\L5\\test.txt";
            Assert.AreEqual(targetRelative, testRelative);
        }

        [TestMethod]
        public void MakeRelativePath_Test2()
        {
            string anchorPath = "C:\\L1\\L2\\L3\\";
            string pathToMakeRelative = "C:\\L1\\L4\\L5\\test.txt";
            string testRelative = RelativePath.MakeRelativePath(
                pathToMakeRelative: pathToMakeRelative, anchorPath: anchorPath);

            string targetRelative = "..\\..\\L4\\L5\\test.txt";
            Assert.AreEqual(targetRelative, testRelative);
        }

        [TestMethod]
        public void GetRelativeURI_Test()
        {
            string anchorPath = "C:\\L1 L01\\L2\\L3\\";
            string pathToMakeRelative = "C:\\L1 L01\\L2\\L3\\L4\\L5\\test.txt";
            string testRelative = RelativePath.GetRelativeURI(
                pathToMakeRelative: pathToMakeRelative, anchorPath: anchorPath);

            string targetRelative = "./L4/L5/test.txt";
            Assert.AreEqual(targetRelative, testRelative);
        }

        [TestMethod]
        public void AbsoluteFromURI_Test1()
        {
            string anchorPath = "C:\\L1\\L2\\L3";
            string uriToMakeAbsolute = "./L4/L5/test.txt";
            
            string testAbsolute = RelativePath.AbsoluteFromURI(
                uriToMakeAbsolute: uriToMakeAbsolute, anchorPath: anchorPath);

            string targetAbsolute = "C:\\L1\\L2\\L3\\L4\\L5\\test.txt";

            Assert.AreEqual(targetAbsolute, testAbsolute);
        }

        [TestMethod]
        public void AbsoluteFromURI_Test2()
        {
            string anchorPath = "C:\\L1\\L2\\L3\\";
            string uriToMakeAbsolute = "./L4/L5/test.txt";

            string testAbsolute = RelativePath.AbsoluteFromURI(
                uriToMakeAbsolute: uriToMakeAbsolute, anchorPath: anchorPath);

            string targetAbsolute = "C:\\L1\\L2\\L3\\L4\\L5\\test.txt";

            Assert.AreEqual(targetAbsolute, testAbsolute);
        }

        [TestMethod]
        public void AbsoluteFromURI_Test3()
        {
            string anchorPath = "C:\\L1\\L2\\L3\\";
            string uriToMakeAbsolute = "../../L4/L5/test.txt";

            string testAbsolute = RelativePath.AbsoluteFromURI(
                uriToMakeAbsolute: uriToMakeAbsolute, anchorPath: anchorPath);

            string targetAbsolute = "C:\\L1\\L4\\L5\\test.txt";

            Assert.AreEqual(targetAbsolute, testAbsolute);
        }

        [TestMethod]
        public void NormalizeFolderpath_Test1()
        {
            string rawPath = "C:\\L1 with space\\L2 with space\\L3 with space\\";
            string testPath = RelativePath.NormalizeFolderpath(rawPath);
            string targetPath = "C:\\L1 with space\\L2 with space\\L3 with space\\";
            Assert.AreEqual(targetPath, testPath);
        }

        [TestMethod]
        public void NormalizeFolderpath_Test2()
        {
            string rawPath = "C:\\L1 with space\\L2 with space\\L3 with space";
            string testPath = RelativePath.NormalizeFolderpath(rawPath);
            string targetPath = "C:\\L1 with space\\L2 with space\\L3 with space\\";
            Assert.AreEqual(targetPath, testPath);
        }

        [TestMethod]
        public void NormalizeFolderpath_Test3()
        {
            string rawPath = "C:\\L1 with space\\L2 with space\\L3 with space.xls";
            string testPath = RelativePath.NormalizeFolderpath(rawPath);
            string targetPath = "C:\\L1 with space\\L2 with space\\";
            Assert.AreEqual(targetPath, testPath);
        }
    }
}
