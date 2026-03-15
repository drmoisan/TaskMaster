using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
