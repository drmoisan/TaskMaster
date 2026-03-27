using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class ComStreamWrapper_Tests
    {
        #region Properties

        [TestMethod]
        public void CanRead_ReturnsTrue()
        {
            var mockStream = new Mock<IStream>();
            using var wrapper = new ComStreamWrapper(mockStream.Object);

            wrapper.CanRead.Should().BeTrue();
        }

        [TestMethod]
        public void CanSeek_ReturnsTrue()
        {
            var mockStream = new Mock<IStream>();
            using var wrapper = new ComStreamWrapper(mockStream.Object);

            wrapper.CanSeek.Should().BeTrue();
        }

        [TestMethod]
        public void CanWrite_ReturnsTrue()
        {
            var mockStream = new Mock<IStream>();
            using var wrapper = new ComStreamWrapper(mockStream.Object);

            wrapper.CanWrite.Should().BeTrue();
        }

        #endregion

        #region Position

        [TestMethod]
        public void Position_SetAndGet()
        {
            var mockStream = new Mock<IStream>();
            using var wrapper = new ComStreamWrapper(mockStream.Object);

            wrapper.Position = 42;
            wrapper.Position.Should().Be(42);
        }

        #endregion

        #region Flush

        [TestMethod]
        public void Flush_CallsCommitOnSource()
        {
            var mockStream = new Mock<IStream>();
            using var wrapper = new ComStreamWrapper(mockStream.Object);

            wrapper.Flush();
            mockStream.Verify(s => s.Commit(0), Times.Once);
        }

        #endregion

        #region SetLength

        [TestMethod]
        public void SetLength_CallsSetSizeOnSource()
        {
            var mockStream = new Mock<IStream>();
            using var wrapper = new ComStreamWrapper(mockStream.Object);

            wrapper.SetLength(1024);
            mockStream.Verify(s => s.SetSize(1024), Times.Once);
        }

        #endregion

        #region Write

        [TestMethod]
        public void Write_WithOffset_ThrowsNotImplemented()
        {
            var mockStream = new Mock<IStream>();
            using var wrapper = new ComStreamWrapper(mockStream.Object);

            Action act = () => wrapper.Write(new byte[] { 1, 2, 3 }, 1, 2);
            act.Should().Throw<NotImplementedException>();
        }

        [TestMethod]
        public void Write_ZeroOffset_CallsWriteOnSource()
        {
            var mockStream = new Mock<IStream>();
            using var wrapper = new ComStreamWrapper(mockStream.Object);
            var buffer = new byte[] { 1, 2, 3 };

            wrapper.Write(buffer, 0, 3);
            mockStream.Verify(s => s.Write(buffer, 3, IntPtr.Zero), Times.Once);
        }

        #endregion

        #region Read

        [TestMethod]
        public void Read_WithOffset_ThrowsNotImplemented()
        {
            var mockStream = new Mock<IStream>();
            using var wrapper = new ComStreamWrapper(mockStream.Object);

            Action act = () => wrapper.Read(new byte[10], 1, 5);
            act.Should().Throw<NotImplementedException>();
        }

        #endregion
    }
}
