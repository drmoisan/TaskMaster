using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

        [TestMethod]
        public void Imconcattb_WithValidBitmaps_ReturnsCombinedBitmap()
        {
            // Arrange
            var stripper = new ImageStripper();
            var top = CreateBitmap(width: 4, height: 3, color: Color.Red);
            var bottom = CreateBitmap(width: 4, height: 2, color: Color.Blue);

            // Act
            var result = stripper.imconcattb(top, bottom);

            // Assert
            result.Width.Should().Be(4);
            result.Height.Should().Be(5);
        }

        [TestMethod]
        public void Imconcattb_WithSmallBitmaps_ReturnsCombined()
        {
            // Arrange
            var stripper = new ImageStripper();
            var top = CreateBitmap(width: 2, height: 1, color: Color.Red);
            var bottom = CreateBitmap(width: 2, height: 1, color: Color.Blue);

            // Act
            var result = stripper.imconcattb(top, bottom);

            // Assert
            result.Width.Should().Be(2);
            result.Height.Should().Be(2);
        }

        [TestMethod]
        public void Imconcatlr_WithValidBitmaps_ReturnsCombinedBitmap()
        {
            // Arrange
            var stripper = new ImageStripper();
            var left = CreateBitmap(width: 3, height: 4, color: Color.Red);
            var right = CreateBitmap(width: 2, height: 4, color: Color.Blue);

            // Act
            var result = stripper.imconcatlr(left, right);

            // Assert
            result.Width.Should().Be(5);
            result.Height.Should().Be(4);
        }

        [TestMethod]
        public void ExtractOcrInfo_WithEmptyBitmapList_ReturnsEmptyResult()
        {
            // Arrange
            var stripper = new ImageStripper();

            // Act
            var (text, tokens) = stripper.extract_ocr_info(new List<Bitmap>());

            // Assert
            text.Should().BeEmpty();
            tokens.Should().BeEmpty();
        }

        [TestMethod]
        public void GetStream_WithBytes_ReturnsMemoryStream()
        {
            // Arrange
            var stripper = new ImageStripper();
            var bytes = new byte[] { 1, 2, 3 };

            // Act
            var stream = stripper.GetStream(bytes);

            // Assert
            stream.Should().NotBeNull();
            stream.Length.Should().Be(3);
        }

        [TestMethod]
        public void GetImage_WithValidStream_ReturnsImage()
        {
            // Arrange
            var stripper = new ImageStripper();
            var bitmap = CreateBitmap(width: 2, height: 2, color: Color.White);
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            // Act
            var image = stripper.GetImage(ms);

            // Assert
            image.Should().NotBeNull();
            image.Width.Should().Be(2);
            image.Height.Should().Be(2);
        }

        [TestMethod]
        public void Constructor_WithCacheFile_DoesNotThrow()
        {
            // Act
            System.Action act = () => new ImageStripper("test-cache");

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Analyze_WithTesseractAndAttachment_WhenNoImagesExtracted_ReturnsTokensOnly()
        {
            // Arrange
            var stripper = new ImageStripper();
            var attachment = CreateAttachment(
                size: 10,
                data: new byte[] { 0xFF, 0xFE },
                attachmentType: OlAttachmentType.olByValue
            );

            // Act
            var (text, tokens) = stripper.analyze("Tesseract", new List<object> { attachment });

            // Assert
            tokens.Should().Contain(t => t.StartsWith("invalid-image:"));
        }

        [TestMethod]
        public void Analyze_WithTesseractAndValidImageAttachment_ReturnsNoTextFoundToken()
        {
            // Arrange
            var stripper = new ImageStripper();
            var attachment = CreateAttachmentFromBitmap(
                CreateBitmap(width: 8, height: 8, color: Color.White)
            );

            // Act
            var (text, tokens) = stripper.analyze("Tesseract", new List<object> { attachment });

            // Assert
            text.Should().NotBeNull();
            tokens.Should().Contain("image-text:no text found");
        }

        [TestMethod]
        public void ExtractOcrInfo_WithBitmap_WhenNoTextIsDetected_ReturnsNoTextToken()
        {
            // Arrange
            var stripper = new ImageStripper();
            using var bitmap = CreateBitmap(width: 8, height: 8, color: Color.White);

            // Act
            var (text, tokens) = stripper.extract_ocr_info(new List<Bitmap> { bitmap });

            // Assert
            text.Should().NotBeNull();
            tokens.Should().Contain("image-text:no text found");
        }

        [TestMethod]
        public void GetFrameWithText_WithMultiFrameImage_ReturnsSelectedFrame()
        {
            // Arrange
            var stripper = new ImageStripper();
            var bytes = CreateMultiFrameTiffBytes();
            using var stream = new MemoryStream(bytes);
            using var image = Image.FromStream(stream);

            // Act
            var frames = stripper.SeperateMultiFrame(image).ToList();
            try
            {
                var selectedFrame = stripper.GetFrameWithText(image);

                // Assert
                stripper.IsMultiFrameImage(image).Should().BeTrue();
                frames.Should().HaveCount(2);
                selectedFrame.Should().NotBeNull();
                selectedFrame.Width.Should().Be(4);
                selectedFrame.Height.Should().Be(4);
            }
            finally
            {
                foreach (var frame in frames)
                {
                    frame.Dispose();
                }
            }
        }

        [TestMethod]
        public void PilDecodeParts_WithMultiFrameAttachment_ReturnsSelectedImage()
        {
            // Arrange
            var stripper = new ImageStripper();
            var attachment = CreateAttachment(
                size: 128,
                data: CreateMultiFrameTiffBytes(),
                attachmentType: OlAttachmentType.olByValue
            );

            // Act
            var (images, tokens) = stripper.PIL_decode_parts(new List<object> { attachment });

            // Assert
            images.Should().ContainSingle();
            images[0].Width.Should().Be(4);
            images[0].Height.Should().Be(4);
            tokens.Should().BeEmpty();
        }

        [TestMethod]
        public void PilDecodeParts_WithEmptyAttachmentData_AddsInvalidImageToken()
        {
            // Arrange
            var stripper = new ImageStripper();
            var attachment = CreateAttachment(
                size: 10,
                data: Array.Empty<byte>(),
                attachmentType: OlAttachmentType.olByValue
            );

            // Act
            var (images, tokens) = stripper.PIL_decode_parts(new List<object> { attachment });

            // Assert
            tokens.Should().Contain("invalid-image:olByValue");
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

        private static byte[] CreateMultiFrameTiffBytes()
        {
            using var first = CreateBitmap(width: 4, height: 4, color: Color.White);
            using var firstGraphics = Graphics.FromImage(first);
            firstGraphics.FillRectangle(Brushes.Black, 0, 0, 4, 4);
            firstGraphics.FillRectangle(Brushes.White, 0, 0, 4, 1);

            using var second = CreateBitmap(width: 4, height: 4, color: Color.White);
            using var secondGraphics = Graphics.FromImage(second);
            secondGraphics.FillRectangle(Brushes.Black, 0, 0, 4, 4);
            secondGraphics.FillRectangle(Brushes.White, 0, 0, 2, 2);

            var codec = ImageCodecInfo.GetImageEncoders().Single(x => x.MimeType == "image/tiff");
            using var stream = new MemoryStream();
            using var encoderParameters = new EncoderParameters(1);

            encoderParameters.Param[0] = new EncoderParameter(
                Encoder.SaveFlag,
                (long)EncoderValue.MultiFrame
            );
            first.Save(stream, codec, encoderParameters);

            encoderParameters.Param[0] = new EncoderParameter(
                Encoder.SaveFlag,
                (long)EncoderValue.FrameDimensionPage
            );
            first.SaveAdd(second, encoderParameters);

            encoderParameters.Param[0] = new EncoderParameter(
                Encoder.SaveFlag,
                (long)EncoderValue.Flush
            );
            first.SaveAdd(encoderParameters);

            return stream.ToArray();
        }
    }
}
