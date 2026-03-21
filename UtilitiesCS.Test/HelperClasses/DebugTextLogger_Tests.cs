using System;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;
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

        [TestMethod]
        public void Write_ShouldWriteBytesWithoutThrowing()
        {
            // Arrange
            using var logger = new DebugTextLogger();
            var stream = logger.BaseStream;

            // Act
            Action act = () =>
            {
                var bytes = System.Text.Encoding.Unicode.GetBytes("test");
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            };

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Write_WithVariousCharSequences_ShouldNotThrow()
        {
            // Arrange
            using var logger = new DebugTextLogger();

            // Act
            Action act = () =>
            {
                logger.Write('A');
                logger.Write("Test string");
                logger.Write(new char[] { 'x', 'y', 'z' });
                logger.Flush();
            };

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void DebugTextWriter_Constructor_ShouldEnableAutoFlushAndExposeWritableStream()
        {
            // Arrange
            using var writer = new DebugTextWriter();

            // Assert
            writer.AutoFlush.Should().BeTrue();
            writer.BaseStream.CanWrite.Should().BeTrue();
            writer.BaseStream.CanRead.Should().BeFalse();
            writer.BaseStream.CanSeek.Should().BeFalse();
        }

        [TestMethod]
        public void DebugTextWriter_BaseStream_ShouldThrowForUnsupportedOperationsAndAllowWrites()
        {
            // Arrange
            using var writer = new DebugTextWriter();
            var stream = writer.BaseStream;

            // Act
            Action read = () => stream.Read(new byte[1], 0, 1);
            Action seek = () => stream.Seek(0, SeekOrigin.Begin);
            Action setLength = () => stream.SetLength(1);
            Action getLength = () => _ = stream.Length;
            Action getPosition = () => _ = stream.Position;
            Action setPosition = () => stream.Position = 0;
            Action write = () =>
            {
                var bytes = System.Text.Encoding.Unicode.GetBytes("writer");
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            };

            // Assert
            read.Should().Throw<InvalidOperationException>();
            seek.Should().Throw<InvalidOperationException>();
            setLength.Should().Throw<InvalidOperationException>();
            getLength.Should().Throw<InvalidOperationException>();
            getPosition.Should().Throw<InvalidOperationException>();
            setPosition.Should().Throw<InvalidOperationException>();
            write.Should().NotThrow();
        }
    }
}
