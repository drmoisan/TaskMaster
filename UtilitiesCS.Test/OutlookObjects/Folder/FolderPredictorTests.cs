using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class FolderPredictorTests
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
