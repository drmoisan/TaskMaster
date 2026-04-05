using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RuntimeMarshal = System.Runtime.InteropServices.Marshal;

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

        #region P75

        /// <summary>
        /// Verifies that <see cref="ComStreamWrapper.Read"/> with offset 0 forwards the
        /// buffer and count to the underlying <see cref="IStream.Read"/>.
        ///
        /// Acceptance: supplies a mocked IStream, calls Read with offset 0, and asserts the
        /// mock's Read received the expected buffer and count.
        /// </summary>
        [TestMethod]
        public void Read_ZeroOffset_ForwardsBufferAndCountToMockedStream()
        {
            // Arrange: set up the mock to write the call count into pcbRead so Read() returns 4.
            var mockStream = new Mock<IStream>();
            mockStream
                .Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<IntPtr>()))
                .Callback<byte[], int, IntPtr>(
                    (buf, count, ptr) => RuntimeMarshal.WriteInt32(ptr, count)
                );
            using var wrapper = new ComStreamWrapper(mockStream.Object);
            byte[] buffer = new byte[4];

            // Act: offset 0 is the only supported path.
            int result = wrapper.Read(buffer, 0, 4);

            // Assert: the forwarded buffer and count match; result equals the written count.
            result.Should().Be(4);
            mockStream.Verify(s => s.Read(buffer, 4, It.IsAny<IntPtr>()), Times.Once);
        }

        /// <summary>
        /// Verifies that <see cref="ComStreamWrapper.Seek"/>, <see cref="ComStreamWrapper.Length"/>,
        /// and <see cref="ComStreamWrapper.Position"/> round-trip correctly through the mock.
        ///
        /// Seek with <see cref="SeekOrigin.Begin"/> (dwOrigin = 0) at offset 100 should:
        ///   - return the value the COM stream writes into plibNewPosition (100),
        ///   - update Position to offset + (int)SeekOrigin.Begin = 100 + 0 = 100.
        /// Length should reflect the cbSize value configured on the Stat mock.
        /// </summary>
        [TestMethod]
        public void Seek_Length_Position_RoundTripThroughMockedStream()
        {
            // Arrange: mock Stat to return cbSize = 512.
            var mockStream = new Mock<IStream>();
            var statResult = new STATSTG { cbSize = 512L };
            mockStream.Setup(s => s.Stat(out statResult, It.IsAny<int>()));

            // Set up Seek to echo the offset back into plibNewPosition.
            mockStream
                .Setup(s => s.Seek(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<IntPtr>()))
                .Callback<long, int, IntPtr>(
                    (offset, origin, ptr) => RuntimeMarshal.WriteInt64(ptr, offset)
                );
            using var wrapper = new ComStreamWrapper(mockStream.Object);

            // Act: seek to offset 100 from the beginning; read back Length.
            long seekResult = wrapper.Seek(100L, SeekOrigin.Begin);
            long length = wrapper.Length;

            // Assert: Seek returns 100, Position advances to 100 + 0 = 100, Length is 512.
            seekResult.Should().Be(100L);
            wrapper.Position.Should().Be(100L);
            length.Should().Be(512L);
        }

        #endregion
    }
}
