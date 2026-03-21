using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Item
{
    [TestClass]
    public class OlItemPseudoInterface_Tests
    {
        [TestMethod]
        public void SetCategories_NonOutlookObject_ThrowsArgumentException()
        {
            // Arrange
            var item = new object();

            // Act
            System.Action act = () => OlItemPseudoInterface.SetCategories(item, "cat");

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Unsupported type*");
        }

        [TestMethod]
        public void GetCategories_NonOutlookObject_ThrowsArgumentException()
        {
            // Arrange
            var item = new object();

            // Act
            System.Action act = () => OlItemPseudoInterface.GetCategories(item);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Unsupported type*");
        }

        [TestMethod]
        public void SetCategories_String_ThrowsArgumentException()
        {
            // Arrange
            var item = "not an outlook item";

            // Act
            System.Action act = () => OlItemPseudoInterface.SetCategories(item, "cat");

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void GetCategories_String_ThrowsArgumentException()
        {
            // Arrange
            var item = "not an outlook item";

            // Act
            System.Action act = () => OlItemPseudoInterface.GetCategories(item);

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}
