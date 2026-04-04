using System;
using System.ComponentModel;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class FilePathHelper_Tests
    {
        [TestMethod]
        public void DefaultConstructor_ShouldInitializeEmptyProperties()
        {
            var fph = new FilePathHelper();

            fph.FilePath.Should().Be("");
            fph.FolderPath.Should().Be("");
            fph.FileName.Should().Be("");
        }

        [TestMethod]
        public void Constructor_WithFileNameAndFolderPath_ShouldSetFilePath()
        {
            var fph = new FilePathHelper("test.json", @"C:\data");

            fph.FileName.Should().Be("test.json");
            fph.FolderPath.Should().Be(@"C:\data");
            fph.FilePath.Should().Be(@"C:\data\test.json");
        }

        [TestMethod]
        public void FromSeed_ShouldBuildFileNameFromParts()
        {
            var fph = FilePathHelper.FromSeed("report", ".json", "_backup", @"C:\data");

            fph.FileStemSeed.Should().Be("report");
            fph.FileExtension.Should().Be(".json");
            fph.FileStemSuffix.Should().Be("_backup");
            fph.FolderPath.Should().Be(@"C:\data");
        }

        [TestMethod]
        public void Exists_WhenFilePathIsEmpty_ShouldReturnFalse()
        {
            var fph = new FilePathHelper();

            fph.Exists().Should().BeFalse();
        }

        [TestMethod]
        public void Exists_WhenFileDoesNotExist_ShouldReturnFalse()
        {
            var fph = new FilePathHelper("nonexistent.json", @"C:\does_not_exist_xyz");

            fph.Exists().Should().BeFalse();
        }

        [TestMethod]
        public void GetLastWriteTimeUtc_WhenFileDoesNotExist_ShouldReturnDefault()
        {
            var fph = new FilePathHelper("nonexistent.json", @"C:\does_not_exist_xyz");

            fph.GetLastWriteTimeUtc().Should().Be(default(DateTime));
        }

        [TestMethod]
        public void StemInitialized_WhenSeedIsNull_ShouldReturnFalse()
        {
            var fph = new FilePathHelper();

            fph.StemInitialized().Should().BeFalse();
        }

        [TestMethod]
        public void CalcMaxSeedLength_WhenNotInitialized_ShouldReturnMaxPath()
        {
            var fph = new FilePathHelper();

            fph.CalcMaxSeedLength().Should().Be(FilePathHelper.MAX_PATH);
        }

        [TestMethod]
        public void CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths()
        {
            var fph = FilePathHelper.FromSeed("data", ".json", "_bk", @"C:\output");

            var result = fph.CalcMaxSeedLength();

            result
                .Should()
                .Be(FilePathHelper.MAX_PATH - @"C:\output".Length - ".json".Length - "_bk".Length);
        }

        [TestMethod]
        public void ExtractStemAndExtension_ShouldSplitFileName()
        {
            var fph = new FilePathHelper();

            var (stem, ext) = fph.ExtractStemAndExtension("report.json");

            stem.Should().Be("report");
            ext.Should().Be(".json");
        }

        [TestMethod]
        public void ExtractStemAndExtension_WhenNoExtension_ShouldReturnEmptyExtension()
        {
            var fph = new FilePathHelper();

            var (stem, ext) = fph.ExtractStemAndExtension("readme");

            stem.Should().Be("readme");
            ext.Should().Be("");
        }

        [TestMethod]
        public void ExtractStemAndExtension_WhenFileNameIsOnlyExtension_ShouldTreatExtensionAsStem()
        {
            var fph = new FilePathHelper();

            var (stem, ext) = fph.ExtractStemAndExtension(".gitignore");

            stem.Should().Be(".gitignore");
            ext.Should().Be("");
        }

        [TestMethod]
        public void TryParseFileStem_WhenEmpty_ShouldReturnFalse()
        {
            var fph = new FilePathHelper();

            var result = fph.TryParseFileStem("", out string seed, out string suffix);

            result.Should().BeFalse();
        }

        [TestMethod]
        public void TryParseFileStem_WhenSeedAndSuffixEmpty_ShouldSetSeedToFileStem()
        {
            var fph = new FilePathHelper();

            var result = fph.TryParseFileStem("mydata", out string seed, out string suffix);

            result.Should().BeTrue();
            seed.Should().Be("mydata");
        }

        [TestMethod]
        public void TryParseFileName_WhenEmpty_ShouldReturnFalse()
        {
            var fph = new FilePathHelper();

            var result = fph.TryParseFileName("");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void TryParseFileName_WhenValid_ShouldSetStemAndExtension()
        {
            var fph = FilePathHelper.FromSeed("data", ".json", "_bk", @"C:\output");

            var result = fph.TryParseFileName("data_bk.json");

            result.Should().BeTrue();
            fph.FileStemSeed.Should().Be("data");
            fph.FileStemSuffix.Should().Be("_bk");
            fph.FileExtension.Should().Be(".json");
        }

        [TestMethod]
        public void Clone_ShouldReturnShallowCopy()
        {
            var fph = new FilePathHelper("test.json", @"C:\data");

            var clone = (FilePathHelper)fph.Clone();

            clone.FileName.Should().Be("test.json");
            clone.FolderPath.Should().Be(@"C:\data");
            clone.FilePath.Should().Be(@"C:\data\test.json");
            clone.Should().NotBeSameAs(fph);
        }

        [TestMethod]
        public void DeepCopy_ShouldCreateIndependentCopy()
        {
            var fph = FilePathHelper.FromSeed("report", ".json", "_bk", @"C:\data");

            var copy = fph.DeepCopy();

            copy.FileStemSeed.Should().Be("report");
            copy.FileExtension.Should().Be(".json");
            copy.FileStemSuffix.Should().Be("_bk");
            copy.Should().NotBeSameAs(fph);
        }

        [TestMethod]
        public void CopyFrom_ShouldOverwriteAllFields()
        {
            var source = new FilePathHelper("src.json", @"C:\src");
            var target = new FilePathHelper();

            target.CopyFrom(source);

            target.FileName.Should().Be("src.json");
            target.FolderPath.Should().Be(@"C:\src");
        }

        [TestMethod]
        public void PropertyChanged_FileName_ShouldRecomputeFilePath()
        {
            var fph = new FilePathHelper("old.json", @"C:\data");
            string changedProp = null;
            fph.PropertyChanged += (s, e) => changedProp = e.PropertyName;

            fph.FileName = "new.json";

            fph.FilePath.Should().Be(@"C:\data\new.json");
        }

        [TestMethod]
        public void PropertyChanged_FolderPath_ShouldRecomputeFilePath()
        {
            var fph = new FilePathHelper("test.json", @"C:\old");

            fph.FolderPath = @"C:\new";

            fph.FilePath.Should().Be(@"C:\new\test.json");
        }

        [TestMethod]
        public void PropertyChanged_FilePath_ShouldSplitIntoFolderAndFile()
        {
            var fph = new FilePathHelper();

            fph.FilePath = @"C:\folder\myfile.json";

            fph.FolderPath.Should().Be(@"C:\folder");
            fph.FileName.Should().Be("myfile.json");
        }

        [TestMethod]
        public void PropertyChanged_FilePath_WhenEmpty_ShouldClearFolderAndFile()
        {
            var fph = new FilePathHelper("test.json", @"C:\data");

            fph.FilePath = "";

            fph.FolderPath.Should().Be("");
            fph.FileName.Should().Be("");
        }

        [TestMethod]
        public void AdjustForMaxPath_Static_ShouldTruncateSeedWhenPathExceedsMax()
        {
            string folder = @"C:\data";
            string longSeed = new string('a', 300);
            string ext = ".json";

            var result = FilePathHelper.AdjustForMaxPath(folder, longSeed, ext);

            result.Length.Should().BeLessThanOrEqualTo(FilePathHelper.MAX_PATH);
        }

        [TestMethod]
        public void AdjustForMaxPath_Instance_WhenNotInitialized_ShouldReturnFalse()
        {
            var fph = new FilePathHelper();

            fph.AdjustForMaxPath().Should().BeFalse();
        }

        [TestMethod]
        public void AdjustForMaxPath_Instance_WhenPathExceedsLimit_ShouldTruncateSeed()
        {
            var longSeed = new string('a', 300);
            var fph = FilePathHelper.FromSeed(longSeed, ".json", "_tail", @"C:\data");

            var adjusted = fph.AdjustForMaxPath();
            var adjustedPath = Path.Combine(
                fph.FolderPath,
                $"{fph.FileStemSeed}{fph.FileStemSuffix}{fph.FileExtension}"
            );

            adjusted.Should().BeTrue();
            fph.FileStemSeed.Length.Should().BeLessThan(longSeed.Length);
            adjustedPath.Length.Should().BeLessThanOrEqualTo(FilePathHelper.MAX_PATH);
        }

        [TestMethod]
        public void CopyChanged_ShouldReturnListOfChangedProperties()
        {
            var original = new FilePathHelper("old.json", @"C:\old");
            var updated = new FilePathHelper("new.json", @"C:\new");

            var changed = original.CopyChanged(updated);

            changed.Should().Contain("FolderPath");
            changed.Should().Contain("FileName");
        }

        // P89-T2: TryParseFileStem boundary combinations

        [TestMethod]
        public void TryParseFileStem_WhenSeedPresentAndSuffixEmpty_ShouldReturnTrueAndPreserveSeed()
        {
            // Arrange: fph knows the seed but has no suffix; fileStem starts with seed + extra chars.
            var fph = FilePathHelper.FromSeed("report", ".json", "", @"C:\data");

            // Act: parse a stem that begins with the known seed followed by extra chars.
            var result = fph.TryParseFileStem("report_v2", out string seed, out string suffix);

            // Assert: parsing succeeds and the seed output includes the original seed value.
            result.Should().BeTrue();
            seed.Should().StartWith("report");
        }

        [TestMethod]
        public void TryParseFileStem_WhenSuffixPresentInStem_ShouldStripSuffixAndReturnSeed()
        {
            // Arrange: fph knows both seed and suffix; the fileStem is seed+suffix concatenated.
            var fph = FilePathHelper.FromSeed("data", ".json", "_bk", @"C:\output");

            // Act: parse the exact concatenation of known seed and suffix.
            var result = fph.TryParseFileStem("data_bk", out string seed, out string suffix);

            // Assert: parsing succeeds, the seed strips the suffix portion, suffix is preserved.
            result.Should().BeTrue();
            seed.Should().Be("data");
            suffix.Should().Be("_bk");
        }

        // P89-T3: AdjustForMaxPath preserves extension when truncating seed

        [TestMethod]
        public void AdjustForMaxPath_Static_ShouldPreserveExtensionWhenTruncatingSeed()
        {
            // Arrange: construct a path that exceeds MAX_PATH; ext must survive truncation.
            string folder = @"C:\data";
            string longSeed = new string('x', 300);
            string ext = ".json";

            // Act: truncate to fit within MAX_PATH.
            var result = FilePathHelper.AdjustForMaxPath(folder, longSeed, ext);

            // Assert: result fits within the limit AND the extension is preserved intact.
            result.Length.Should().BeLessThanOrEqualTo(FilePathHelper.MAX_PATH);
            result.Should().EndWith(ext);
        }

        [TestMethod]
        public void PropertyChanged_FileStemParts_ShouldRecomputeFileNameAndStem()
        {
            var fph = FilePathHelper.FromSeed("report", ".json", "_bk", @"C:\data");

            fph.FileStemSeed = "summary";
            fph.FileStem.Should().Be("summary_bk");
            fph.FileName.Should().Be("summary_bk.json");

            fph.FileStemSuffix = "_archive";
            fph.FileStem.Should().Be("summary_archive");
            fph.FileName.Should().Be("summary_archive.json");

            fph.FileExtension = ".txt";
            fph.FileStem.Should().Be("summary_archive");
            fph.FileName.Should().Be("summary_archive.txt");
        }
    }
}
