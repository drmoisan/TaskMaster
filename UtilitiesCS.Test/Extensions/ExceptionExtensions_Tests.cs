using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class ExceptionExtensions_Tests
    {
        [TestMethod]
        public void GetLineNumber_WhenExceptionIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Exception exception = null;

            // Act
            Action action = () => exception.GetLineNumber();

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetLineNumber_ReturnsSourceLineForThrownException()
        {
            // Arrange
            var exception = CaptureSimpleException();

            // Act
            var lineNumber = exception.GetLineNumber();

            // Assert
            lineNumber.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void GetLineNumber_CanReadInnerExceptionLineNumbers()
        {
            // Arrange
            var exception = CaptureWrappedException();

            // Act
            var outerLine = exception.GetLineNumber();
            var innerLine = exception.InnerException.GetLineNumber();

            // Assert
            outerLine.Should().BeGreaterThan(0);
            innerLine.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void GetLineNumber_CanReadAggregateInnerExceptionLineNumbers()
        {
            // Arrange
            var aggregateException = CaptureAggregateException();

            // Act
            var innerLine = aggregateException.InnerException.GetLineNumber();

            // Assert
            innerLine.Should().BeGreaterThan(0);
        }

        private static Exception CaptureSimpleException()
        {
            try
            {
                ThrowSimpleException();
                throw new InvalidOperationException("Expected an exception to be thrown.");
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static Exception CaptureWrappedException()
        {
            try
            {
                ThrowWrappedException();
                throw new InvalidOperationException("Expected an exception to be thrown.");
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static AggregateException CaptureAggregateException()
        {
            try
            {
                ThrowAggregateException();
                throw new InvalidOperationException("Expected an exception to be thrown.");
            }
            catch (AggregateException exception)
            {
                return exception;
            }
        }

        private static void ThrowSimpleException()
        {
            throw new InvalidOperationException("boom");
        }

        private static void ThrowWrappedException()
        {
            try
            {
                ThrowSimpleException();
            }
            catch (Exception exception)
            {
                throw new ApplicationException("outer", exception);
            }
        }

        private static void ThrowAggregateException()
        {
            try
            {
                ThrowSimpleException();
            }
            catch (Exception exception)
            {
                throw new AggregateException(exception);
            }
        }
    }
}
