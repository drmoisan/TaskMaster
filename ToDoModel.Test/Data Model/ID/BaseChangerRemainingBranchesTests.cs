using System;
using System.Numerics;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoModel;

namespace ToDoModel.Test
{
    /// <summary>
    /// Covers the <see cref="BaseChanger"/> branches not already exercised by
    /// <c>BaseChangerTests</c>: the <see cref="BaseChanger.ToBase10(char,int)"/> overload, the
    /// <c>ValidateParams</c> error guards (base &lt; 1, base &gt; MaxBase, negative number), the
    /// invalid-character path, the <see cref="BaseChanger.MaxBase"/> accessor, and the
    /// <see cref="BaseChanger.ToBase(BigInteger,int,int)"/> zero / single-digit / odd-length
    /// padding edges. All methods are pure arithmetic with no external dependencies.
    /// </summary>
    [TestClass]
    public class BaseChangerRemainingBranchesTests
    {
        // ---- MaxBase ----

        [TestMethod]
        public void MaxBase_EqualsConverterStringLength_36()
        {
            // Arrange / Act
            var maxBase = BaseChanger.MaxBase;

            // Assert
            maxBase.Should().Be(36, "the converter string defines base-36 (0-9, A-Z)");
        }

        // ---- ToBase10(char, int) ----

        [TestMethod]
        public void ToBase10Char_Digit_ReturnsNumericValue()
        {
            // Arrange / Act
            var result = '7'.ToBase10(36);

            // Assert
            result.Should().Be(7);
        }

        [TestMethod]
        public void ToBase10Char_UpperLetter_ReturnsIndexValue()
        {
            // Arrange / Act: 'A' is index 10 in "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".
            var result = 'A'.ToBase10(36);

            // Assert
            result.Should().Be(10);
        }

        [TestMethod]
        public void ToBase10Char_MaxDigit_ReturnsThirtyFive()
        {
            // Arrange / Act
            var result = 'Z'.ToBase10(36);

            // Assert
            result.Should().Be(35, "'Z' is the maximum supported base-36 digit");
        }

        [TestMethod]
        public void ToBase10Char_CharacterNotInConverter_ThrowsArgumentOutOfRangeException()
        {
            // Arrange / Act: lowercase 'a' is not part of the upper-case converter string.
            Action act = () => 'a'.ToBase10(36);

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>(
                    "a character outside the converter alphabet is rejected"
                );
        }

        // ---- ValidateParams guards (exercised through public entry points) ----

        [TestMethod]
        public void ToBase10Char_BaseBelowOne_ThrowsArgumentOutOfRangeException()
        {
            // Arrange / Act
            Action act = () => '1'.ToBase10(0);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>("a base less than 1 is invalid");
        }

        [TestMethod]
        public void ToBase10Char_BaseAboveMaxBase_ThrowsArgumentOutOfRangeException()
        {
            // Arrange / Act
            Action act = () => '1'.ToBase10(BaseChanger.MaxBase + 1);

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>(
                    "a base above MaxBase exceeds the converter alphabet"
                );
        }

        [TestMethod]
        public void ToBase_NegativeNumber_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            BigInteger negative = -1;

            // Act
            Action act = () => negative.ToBase(36);

            // Assert
            act.Should()
                .Throw<ArgumentOutOfRangeException>("a negative number cannot be converted");
        }

        // ---- ToBase edges ----

        [TestMethod]
        public void ToBase_Zero_ReturnsEvenPaddedZero()
        {
            // Arrange
            BigInteger zero = 0;

            // Act: single digit "0" is padded to even length "00" (prependCount == 1).
            var result = zero.ToBase(36);

            // Assert
            result.Should().Be("00", "a single base-36 digit is padded to two digits by default");
        }

        [TestMethod]
        public void ToBase_SingleDigit_PadsToTwoDigits()
        {
            // Arrange
            BigInteger five = 5;

            // Act
            var result = five.ToBase(36);

            // Assert
            result.Should().Be("05");
        }

        [TestMethod]
        public void ToBase_ThreeDigitResult_PrependsSingleZero()
        {
            // Arrange: 1296 => "100" (3 digits) => odd length => prepend "0" => "0100".
            BigInteger value = 1296;

            // Act
            var result = value.ToBase(36);

            // Assert
            result.Should().Be("0100", "odd-length output is padded with a single leading zero");
        }

        [TestMethod]
        public void ToBase_MinDigitsOne_DoesNotPadSingleDigit()
        {
            // Arrange: with intMinDigits == 1, prependCount == 0 and no padding is added.
            BigInteger five = 5;

            // Act
            var result = five.ToBase(36, intMinDigits: 1);

            // Assert
            result.Should().Be("5", "minimum-digit 1 disables even-length padding");
        }
    }
}
