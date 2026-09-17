using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Regression tests for issue #792 AC-U3 on <see cref="EfcDataModel"/>: a folder handler
    /// carried from a pop-out source item is adopted when it is a concrete predictor and no
    /// explicit list is supplied; otherwise the existing initialization path runs. In both cases
    /// the carry is released afterwards. Data models are allocated without a constructor, so no
    /// Outlook COM context is required; the one test that runs the existing path supplies a
    /// mocked globals so the predictor constructor can read <c>Ol.App</c>.
    /// </summary>
    [TestClass]
    public sealed class EfcDataModelIssue792CarryTests
    {
        private static EfcDataModel CreateUninitializedModel()
        {
            return (EfcDataModel)FormatterServices.GetUninitializedObject(typeof(EfcDataModel));
        }

        private static FolderPredictor CreateUninitializedPredictor()
        {
            return (FolderPredictor)
                FormatterServices.GetUninitializedObject(typeof(FolderPredictor));
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.Should().NotBeNull($"{fieldName} must remain available for this headless seam");
            field.SetValue(target, value);
        }

        /// <summary>
        /// A null list with a concrete predictor carry is adopted as-is. Before the fix the stub
        /// always declines, so the first assertion fails.
        /// </summary>
        [TestMethod]
        public void TryAdoptCarriedFolderHandler_WithNullListAndConcretePredictor_Adopts()
        {
            // Arrange
            var carried = CreateUninitializedPredictor();

            // Act
            bool adopts = EfcDataModel.TryAdoptCarriedFolderHandler(
                null,
                carried,
                out FolderPredictor adopted
            );

            // Assert
            adopts.Should().BeTrue("a concrete predictor with no explicit list must be adopted");
            adopted.Should().BeSameAs(carried, "the adopted instance is the carried instance");
        }

        /// <summary>Control: with nothing carried there is nothing to adopt.</summary>
        [TestMethod]
        public void TryAdoptCarriedFolderHandler_WithNullCarry_DoesNotAdopt()
        {
            // Act
            bool adopts = EfcDataModel.TryAdoptCarriedFolderHandler(
                null,
                null,
                out FolderPredictor adopted
            );

            // Assert
            adopts.Should().BeFalse("a null carry cannot be adopted");
            adopted.Should().BeNull("nothing was adopted");
        }

        /// <summary>
        /// Control: an interface-only handler is not a predictor and cannot be adopted.
        /// </summary>
        [TestMethod]
        public void TryAdoptCarriedFolderHandler_WithNonPredictorHandler_DoesNotAdopt()
        {
            // Arrange
            var carried = new Mock<IFolderSearchHandler>().Object;

            // Act
            bool adopts = EfcDataModel.TryAdoptCarriedFolderHandler(
                null,
                carried,
                out FolderPredictor adopted
            );

            // Assert
            adopts.Should().BeFalse("only a concrete predictor can be adopted");
            adopted.Should().BeNull("nothing was adopted");
        }

        /// <summary>Control: an explicit list always wins over a carried predictor.</summary>
        [TestMethod]
        public void TryAdoptCarriedFolderHandler_WithExplicitListAndPredictor_DoesNotAdopt()
        {
            // Arrange
            var carried = CreateUninitializedPredictor();
            var folderList = new[] { "Inbox" };

            // Act
            bool adopts = EfcDataModel.TryAdoptCarriedFolderHandler(
                folderList,
                carried,
                out FolderPredictor adopted
            );

            // Assert
            adopts.Should().BeFalse("an explicit list must run the existing list path");
            adopted.Should().BeNull("nothing was adopted");
        }

        /// <summary>
        /// A carried predictor becomes the model's folder helper without constructing a new one,
        /// and the carry is released. Before the fix the existing path runs against a null
        /// globals, so this fails either on the assertion or with the construction exception.
        /// </summary>
        [TestMethod]
        public async Task InitFolderHandlerAsync_WithCarriedPredictor_AdoptsItAndReleasesTheCarry()
        {
            // Arrange
            var model = CreateUninitializedModel();
            var carried = CreateUninitializedPredictor();
            model.CarriedFolderHandler = carried;

            // Act
            await model.InitFolderHandlerAsync();

            // Assert
            model.FolderHelper.Should().BeSameAs(carried, "the carried predictor must be adopted");
            model
                .CarriedFolderHandler.Should()
                .BeNull("the carry must be released once it has been consumed");
            model.CarriedMailHelper.Should().BeNull("no mail helper was carried");
        }

        /// <summary>
        /// A non-predictor carry runs the existing null-list path (a fresh predictor over the
        /// globals) and is released afterwards. Before the fix the moved body never consults the
        /// adoption decision and never releases the carry, so the release assertion fails.
        /// </summary>
        [TestMethod]
        public async Task InitFolderHandlerAsync_WithNonPredictorCarry_RunsTheExistingPathAndReleasesTheCarry()
        {
            // Arrange
            var model = CreateUninitializedModel();
            var ol = new Mock<IOlObjects>();
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.Ol).Returns(ol.Object);
            SetPrivateField(model, "_globals", globals.Object);
            var carried = new Mock<IFolderSearchHandler>().Object;
            model.CarriedFolderHandler = carried;

            // Act
            await model.InitFolderHandlerAsync();

            // Assert
            model.FolderHelper.Should().NotBeNull("the existing path must construct a predictor");
            model
                .FolderHelper.Should()
                .NotBeSameAs(carried, "a non-predictor carry must not become the folder helper");
            model
                .CarriedFolderHandler.Should()
                .BeNull("the carry must be released after the existing path has run");
        }
    }
}
