using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class ImageStripper_Tests
    {
        [TestMethod]
        public void Analyze_WithUnsupportedEngine_ReturnsEmptyResult()
        {
            // Arrange
            var stripper = new ImageStripper();

            // Act
            var (text, tokens) = stripper.analyze("OtherEngine", new List<object>());

            // Assert
            text.Should().BeEmpty();
            tokens.Should().BeEmpty();
        }

        [TestMethod]
        public void Analyze_WithNoParts_ReturnsEmptyResult()
        {
            // Arrange
            var stripper = new ImageStripper();

            // Act
            var (text, tokens) = stripper.analyze("Tesseract", new List<object>());

            // Assert
            text.Should().BeEmpty();
            tokens.Should().BeEmpty();
        }

        [TestMethod]
        public void PilDecodeParts_WithNullAttachmentData_AddsInvalidImageToken()
        {
            // Arrange
            var stripper = new ImageStripper();
            var attachment = CreateAttachment(
                size: 10,
                data: null,
                attachmentType: OlAttachmentType.olByValue
            );

            // Act
            var (images, tokens) = stripper.PIL_decode_parts(new List<object> { attachment });

            // Assert
            images.Should().BeEmpty();
            tokens.Should().Contain("invalid-image:olByValue");
        }

        [TestMethod]
        public void PilDecodeParts_WithOversizedAttachment_AddsBigImageToken()
        {
            // Arrange
            var stripper = new ImageStripper();
            var attachment = CreateAttachment(
                size: SpamBayesOptions.max_image_size + 1,
                data: Array.Empty<byte>(),
                attachmentType: OlAttachmentType.olEmbeddeditem
            );

            // Act
            var (images, tokens) = stripper.PIL_decode_parts(new List<object> { attachment });

            // Assert
            images.Should().BeEmpty();
            tokens.Should().Contain("image:big");
        }

        [TestMethod]
        public void PilDecodeParts_WithNullParts_ThrowsArgumentNullException()
        {
            // Arrange
            var stripper = new ImageStripper();

            // Act
            System.Action act = () => stripper.PIL_decode_parts(parts: null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void PilDecodeParts_WithMixedContent_ConcatenatesImagesIntoSingleBitmap()
        {
            // Arrange
            var stripper = new ImageStripper();
            var first = CreateAttachmentFromBitmap(
                CreateBitmap(width: 2, height: 3, color: Color.Blue)
            );
            var second = CreateAttachmentFromBitmap(
                CreateBitmap(width: 4, height: 3, color: Color.Green)
            );
            var third = CreateAttachmentFromBitmap(
                CreateBitmap(width: 5, height: 2, color: Color.Red)
            );

            // Act
            var (images, tokens) = stripper.PIL_decode_parts(
                new List<object>
                {
                    "<html><body><img src='cid:test' /></body></html>",
                    first,
                    second,
                    third,
                }
            );

            // Assert
            images.Should().ContainSingle();
            images[0].Width.Should().Be(6);
            images[0].Height.Should().Be(5);
            tokens.Should().BeEmpty();
        }

        private static IAttachment CreateAttachment(
            int size,
            byte[] data,
            OlAttachmentType attachmentType
        )
        {
            var mock = new Mock<IAttachment>(MockBehavior.Strict);
            mock.SetupGet(x => x.Size).Returns(size);
            mock.SetupGet(x => x.AttachmentData).Returns(data);
            mock.SetupGet(x => x.Type).Returns(attachmentType);
            return mock.Object;
        }

        private static IAttachment CreateAttachmentFromBitmap(Bitmap bitmap)
        {
            return CreateAttachment(
                size: bitmap.Width * bitmap.Height,
                data: ConvertBitmapToPngBytes(bitmap),
                attachmentType: OlAttachmentType.olByValue
            );
        }

        private static Bitmap CreateBitmap(int width, int height, Color color)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(color);
            }

            return bitmap;
        }

        private static byte[] ConvertBitmapToPngBytes(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
