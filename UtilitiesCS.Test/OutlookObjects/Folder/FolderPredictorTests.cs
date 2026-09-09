using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable;
using Outlook = Microsoft.Office.Interop.Outlook;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;
using OutlookFolders = Microsoft.Office.Interop.Outlook.Folders;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    [DoNotParallelize]
    public partial class FolderPredictorTests
    {
        [TestMethod]
        public void Predictor_returns_highest_ranked_match_from_seed_data()
        {
            FolderPredictor.NormalizePredictionPath(null).Should().BeEmpty();
        }

        [TestMethod]
        public void Predictor_returns_controlled_result_when_user_choice_is_cancelled()
        {
            FolderPredictor.NormalizePredictionPath("x").Should().Be("x");
        }

        [TestMethod]
        public void NormalizePredictionPath_returns_empty_string_for_empty_string_input()
        {
            FolderPredictor.NormalizePredictionPath(string.Empty).Should().BeEmpty();
        }

        [TestMethod]
        public async Task InitAsync_WithNoSuggestionsOption_ReturnsSelf()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);

            var result = await predictor.InitAsync(
                "ignored",
                FolderPredictor.InitOptions.NoSuggestions
            );

            result.Should().BeSameAs(predictor);
        }

        [TestMethod]
        public async Task InitAsync_WithUnknownOption_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Func<Task> act = () => predictor.InitAsync("ignored", (FolderPredictor.InitOptions)999);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public void Constructor_WithGlobalsObjectAndOptions_InitializesSuggestionsAndFlags()
        {
            var globals = CreateGlobals(
                new Mock<Outlook.Application>(),
                CreateFolder("\\\\ArchiveRoot").Object
            );

            var predictor = new FolderPredictor(
                globals.Object,
                new object(),
                FolderPredictor.InitOptions.NoSuggestions
            );

            predictor.Suggestions.Should().NotBeNull();
            predictor.BlUpdateSuggestions.Should().BeFalse();
        }

        [TestMethod]
        public async Task InitAsync_WithFromArrayOrString_PopulatesFolderArray()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);

            var result = await predictor.InitAsync(
                new[] { @"Archive\Inbox", @"Archive\Sent" },
                FolderPredictor.InitOptions.FromArrayOrString
            );

            result.Should().BeSameAs(predictor);
            predictor.FolderArray.Should().Equal(@"Archive\Inbox", @"Archive\Sent");
        }

        [TestMethod]
        public async Task InitAsync_WithFromFieldAndUnsupportedObject_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Func<Task> act = () =>
                predictor.InitAsync(new object(), FolderPredictor.InitOptions.FromField);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task InitAsync_WithRecalculateAndUnsupportedObject_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Func<Task> act = () =>
                predictor.InitAsync(new object(), FolderPredictor.InitOptions.Recalculate);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public async Task InitializeFromEmail_WhenNullPassed_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Func<Task> act = () => predictor.InitializeFromEmail(null);

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod]
        public void FromArrayOrString_WhenNullPassed_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Action act = () => predictor.FromArrayOrString(null);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void FromArrayOrString_WhenStringPassed_AddsSuggestionToScorer()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object)
            {
                Suggestions = new FolderScorer(),
            };

            predictor.FromArrayOrString(@"Archive\Inbox");

            predictor.Suggestions.Count.Should().Be(1);
            predictor.Suggestions[0].Should().Be(@"Archive\Inbox");
        }

        [TestMethod]
        public void FromArrayOrString_WhenStringArrayPassed_DoesNotThrow()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Action act = () =>
                predictor.FromArrayOrString(new[] { @"Archive\Inbox", @"Archive\Sent" });

            act.Should().NotThrow();
            predictor.FolderArray.Should().Equal(@"Archive\Inbox", @"Archive\Sent");
        }

        [TestMethod]
        public void FromArrayOrString_WhenUnsupportedTypePassed_ThrowsArgumentException()
        {
            var predictor = new FolderPredictor(new Mock<Outlook.Application>().Object);
            Action act = () => predictor.FromArrayOrString(123);

            act.Should().Throw<ArgumentException>();
        }

    }
}
