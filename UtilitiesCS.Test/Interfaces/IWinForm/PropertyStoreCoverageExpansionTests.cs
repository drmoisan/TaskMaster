using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Interfaces;

namespace UtilitiesCS.Test.Interfaces.IWinForm
{
    /// <summary>
    /// Focused PropertyStore coverage for typed wrapper conversion and reset paths without UI
    /// automation.
    /// </summary>
    [TestClass]
    public class PropertyStoreCoverageExpansionTests
    {
        [TestInitialize]
        public void DisableDebugAssertUi()
        {
            if (Trace.Listeners["Default"] is DefaultTraceListener defaultTraceListener)
            {
                defaultTraceListener.AssertUiEnabled = false;
            }
        }

        [TestMethod]
        public void SetGetAndOverwrite_WhenIntegerAndObjectValuesChange_ReturnLatestValues()
        {
            // Arrange
            var store = new PropertyStore();
            var integerKey = PropertyStore.CreateKey();
            var objectKey = PropertyStore.CreateKey();

            // Act
            store.SetInteger(integerKey, 10);
            store.SetInteger(integerKey, 20);
            store.SetObject(objectKey, "initial");
            store.SetObject(objectKey, "updated");

            // Assert
            store.GetInteger(integerKey, out bool integerFound).Should().Be(20);
            integerFound.Should().BeTrue();
            store.GetObject(objectKey, out bool objectFound).Should().Be("updated");
            objectFound.Should().BeTrue();
        }

        [TestMethod]
        public void MissingKeys_WhenRead_ReturnDefaultValuesAndFoundFalse()
        {
            // Arrange
            var store = new PropertyStore();
            var integerKey = PropertyStore.CreateKey();
            var objectKey = PropertyStore.CreateKey();

            // Act
            var integerValue = store.GetInteger(integerKey, out bool integerFound);
            var objectValue = store.GetObject(objectKey, out bool objectFound);

            // Assert
            integerValue.Should().Be(0);
            integerFound.Should().BeFalse();
            objectValue.Should().BeNull();
            objectFound.Should().BeFalse();
        }

        [TestMethod]
        public void TypedWrapperValues_WhenSet_ReturnTypedValuesAndFoundTrue()
        {
            // Arrange
            var store = new PropertyStore();
            var colorKey = PropertyStore.CreateKey();
            var paddingKey = PropertyStore.CreateKey();
            var rectangleKey = PropertyStore.CreateKey();
            var sizeKey = PropertyStore.CreateKey();

            // Act
            store.SetColor(colorKey, Color.AliceBlue);
            store.SetPadding(paddingKey, new Padding(1, 2, 3, 4));
            store.SetRectangle(rectangleKey, new Rectangle(5, 6, 7, 8));
            store.SetSize(sizeKey, new Size(9, 10));

            // Assert
            store.GetColor(colorKey, out bool colorFound).Should().Be(Color.AliceBlue);
            colorFound.Should().BeTrue();
            store
                .GetPadding(paddingKey, out bool paddingFound)
                .Should()
                .Be(new Padding(1, 2, 3, 4));
            paddingFound.Should().BeTrue();
            store
                .GetRectangle(rectangleKey, out bool rectangleFound)
                .Should()
                .Be(new Rectangle(5, 6, 7, 8));
            rectangleFound.Should().BeTrue();
            store.GetSize(sizeKey, out bool sizeFound).Should().Be(new Size(9, 10));
            sizeFound.Should().BeTrue();
        }

        [TestMethod]
        public void TypedWrapperValues_WhenStoredObjectHasWrongType_ReturnEmptyAndFoundFalse()
        {
            // Arrange
            var store = new PropertyStore();
            var colorKey = PropertyStore.CreateKey();
            var paddingKey = PropertyStore.CreateKey();
            var rectangleKey = PropertyStore.CreateKey();
            var sizeKey = PropertyStore.CreateKey();

            // Act
            store.SetObject(colorKey, "not-a-color");
            store.SetObject(paddingKey, "not-padding");
            store.SetObject(rectangleKey, "not-rectangle");
            store.SetObject(sizeKey, "not-size");

            // Assert
            store.GetColor(colorKey, out bool colorFound).Should().Be(Color.Empty);
            colorFound.Should().BeFalse();
            store.GetPadding(paddingKey, out bool paddingFound).Should().Be(Padding.Empty);
            paddingFound.Should().BeFalse();
            store.GetRectangle(rectangleKey, out bool rectangleFound).Should().Be(Rectangle.Empty);
            rectangleFound.Should().BeFalse();
            store.GetSize(sizeKey, out bool sizeFound).Should().Be(Size.Empty);
            sizeFound.Should().BeFalse();
        }

        [TestMethod]
        public void RemoveOperations_WhenValuesExist_ResetContainsAndReadDefaults()
        {
            // Arrange
            var store = new PropertyStore();
            var integerKey = CreateAlignedKey();
            var retainedIntegerKey = PropertyStore.CreateKey();
            var objectKey = PropertyStore.CreateKey();
            store.SetInteger(integerKey, 42);
            store.SetInteger(retainedIntegerKey, 84);
            store.SetObject(objectKey, "value");

            // Act
            store.RemoveInteger(integerKey);
            store.RemoveObject(objectKey);

            // Assert
            store.ContainsInteger(integerKey).Should().BeFalse();
            store.GetInteger(integerKey, out bool integerFound).Should().Be(0);
            integerFound.Should().BeFalse();
            store.GetInteger(retainedIntegerKey, out bool retainedIntegerFound).Should().Be(84);
            retainedIntegerFound.Should().BeTrue();
            store.ContainsObject(objectKey).Should().BeFalse();
            store.GetObject(objectKey, out bool objectFound).Should().BeNull();
            objectFound.Should().BeFalse();
        }

        private static int CreateAlignedKey()
        {
            int key;
            do
            {
                key = PropertyStore.CreateKey();
            } while ((key & 3) != 0);

            return key;
        }
    }
}
