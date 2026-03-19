using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class DebugTextLogger_Tests
    {
        [TestMethod]
        public void Constructor_ShouldEnableAutoFlushAndExposeWritableStream()
        {
            // Arrange
            using var logger = new DebugTextLogger();

            // Assert
            logger.AutoFlush.Should().BeTrue();
            logger.BaseStream.CanWrite.Should().BeTrue();
            logger.BaseStream.CanRead.Should().BeFalse();
            logger.BaseStream.CanSeek.Should().BeFalse();
        }

        [TestMethod]
        public void WriteLine_ShouldAcceptNullEmptyAndTextMessagesWithoutThrowing()
        {
            // Arrange
            using var logger = new DebugTextLogger();

            // Act
            Action act = () =>
            {
                logger.WriteLine((string)null);
                logger.WriteLine(string.Empty);
                logger.WriteLine("hello");
                logger.Flush();
            };

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void BaseStream_ShouldThrowForUnsupportedReadSeekAndLengthOperations()
        {
            // Arrange
            using var logger = new DebugTextLogger();
            var stream = logger.BaseStream;

            // Act
            Action read = () => stream.Read(new byte[1], 0, 1);
            Action seek = () => stream.Seek(0, System.IO.SeekOrigin.Begin);
            Action setLength = () => stream.SetLength(1);
            Action getLength = () => _ = stream.Length;
            Action getPosition = () => _ = stream.Position;
            Action setPosition = () => stream.Position = 0;

            // Assert
            read.Should().Throw<InvalidOperationException>();
            seek.Should().Throw<InvalidOperationException>();
            setLength.Should().Throw<InvalidOperationException>();
            getLength.Should().Throw<InvalidOperationException>();
            getPosition.Should().Throw<InvalidOperationException>();
            setPosition.Should().Throw<InvalidOperationException>();
        }
    }
}
