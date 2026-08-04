using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Svg;

namespace SVGControl.Test
{
    /// <summary>
    /// Regression tests for issue #418. The byte-array <see cref="SvgRenderer"/> constructors must
    /// degrade to a null document instead of throwing when the SVG payload cannot be parsed.
    /// Two distinct failure shapes are covered: malformed input, where the underlying parser
    /// throws, and element-free input, where the parser returns null without throwing.
    /// </summary>
    [TestClass]
    public class SvgRendererParseContractTests
    {
        private static readonly Size TargetSize = new Size(16, 16);

        private static byte[] MalformedSvgBytes()
        {
            return Encoding.ASCII.GetBytes("this is not xml");
        }

        [TestMethod]
        public void Constructor_WithMalformedBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull()
        {
            // Arrange
            byte[] malformed = MalformedSvgBytes();
            SvgRenderer renderer = null;

            // Act
            Action act = () =>
                renderer = new SvgRenderer(malformed, TargetSize, AutoSize.MaintainAspectRatio);

            // Assert
            act.Should()
                .NotThrow(
                    "issue #418 requires the byte-array constructor to degrade rather than throw when the payload cannot be parsed"
                );
            renderer.Should().NotBeNull("the constructor completed without throwing");
            renderer
                .Document.Should()
                .BeNull(
                    "a failed parse must leave the document null instead of being dereferenced"
                );
        }

        [TestMethod]
        public void Constructor_WithMalformedBytesAndMargin_DoesNotThrowAndLeavesDocumentNull()
        {
            // Arrange
            byte[] malformed = MalformedSvgBytes();
            SvgRenderer renderer = null;

            // Act
            Action act = () =>
                renderer = new SvgRenderer(
                    malformed,
                    TargetSize,
                    new Padding(2),
                    AutoSize.MaintainAspectRatio
                );

            // Assert
            act.Should()
                .NotThrow(
                    "the four-argument overload must degrade identically to the three-argument overload"
                );
            renderer.Should().NotBeNull("the constructor completed without throwing");
            renderer
                .Document.Should()
                .BeNull(
                    "a failed parse must leave the document null instead of being dereferenced"
                );
        }

        [TestMethod]
        public void Constructor_WithEmptyBytesAndNoMargin_DoesNotThrowAndLeavesDocumentNull()
        {
            // Arrange — an empty payload is a distinct failure shape from malformed input: the XML
            // reader finds no root element. Either shape must degrade to a null document rather
            // than reaching the caller.
            SvgRenderer renderer = null;

            // Act
            Action act = () =>
                renderer = new SvgRenderer(
                    Array.Empty<byte>(),
                    TargetSize,
                    AutoSize.MaintainAspectRatio
                );

            // Assert
            act.Should().NotThrow("an empty payload must not surface as a constructor exception");
            renderer.Should().NotBeNull("the constructor completed without throwing");
            renderer.Document.Should().BeNull("the parser produced no document for empty input");
        }

        [TestMethod]
        public void Constructor_WithEmptyBytesAndMargin_DoesNotThrowAndLeavesDocumentNull()
        {
            // Arrange
            SvgRenderer renderer = null;

            // Act
            Action act = () =>
                renderer = new SvgRenderer(
                    Array.Empty<byte>(),
                    TargetSize,
                    new Padding(2),
                    AutoSize.MaintainAspectRatio
                );

            // Assert
            act.Should()
                .NotThrow("the four-argument overload must tolerate an empty payload identically");
            renderer.Should().NotBeNull("the constructor completed without throwing");
            renderer.Document.Should().BeNull("the parser produced no document for empty input");
        }

        [TestMethod]
        public void GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument()
        {
            // Arrange — the shipped default image is a known-good payload, so this is the success
            // path that proves the tolerant parse still parses rather than always degrading.
            byte[] valid = Defaults.GetDefault.SvgImage;

            // Act
            SvgDocument document = SvgRenderer.GetSvgDocument(valid);

            // Assert
            document
                .Should()
                .NotBeNull("a well-formed SVG payload must still produce a parsed document");
        }

        [TestMethod]
        public void GetSvgDocument_WithNullPayload_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () => SvgRenderer.GetSvgDocument(null);

            // Assert — a null argument is a caller defect, not a parse failure, so it fails fast
            // rather than degrading to a null document.
            act.Should()
                .Throw<ArgumentNullException>(
                    "a null payload is an argument-contract violation, distinct from an unparsable payload"
                );
        }

        [TestMethod]
        public void TryGetSvgDocument_WithNullPayload_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () => SvgRenderer.TryGetSvgDocument(null, out _, out _);

            // Assert
            act.Should()
                .Throw<ArgumentNullException>(
                    "the try-style member guards its arguments rather than reporting them as a parse failure"
                );
        }

        [TestMethod]
        public void TryGetSvgDocument_WithMalformedBytes_ReturnsFalseAndCapturesTheException()
        {
            // Arrange
            byte[] malformed = MalformedSvgBytes();

            // Act
            bool parsed = SvgRenderer.TryGetSvgDocument(
                malformed,
                out SvgDocument document,
                out Exception error
            );

            // Assert
            parsed.Should().BeFalse("malformed input cannot produce a document");
            document.Should().BeNull("a failed parse yields no document");
            error
                .Should()
                .NotBeNull(
                    "issue #418 requires the swallowed exception to be surfaced to the caller rather than discarded"
                );
        }

        [TestMethod]
        public void TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException()
        {
            // Arrange, Act — an empty payload gives the XML reader no root element, so the parser
            // reports failure by raising rather than by returning null. This is measured behavior:
            // XmlException("Root element is missing.") is what the parser produces for empty input.
            bool parsed = SvgRenderer.TryGetSvgDocument(
                Array.Empty<byte>(),
                out SvgDocument document,
                out Exception error
            );

            // Assert
            parsed.Should().BeFalse("an empty payload produces no document");
            document.Should().BeNull("a failed parse yields no document");
            error
                .Should()
                .BeOfType<XmlException>(
                    "an empty payload has no root element, so the XML reader raises rather than returning null"
                );
        }

        [TestMethod]
        public void TryGetSvgDocument_WhenTheParseSeamReturnsNull_ReturnsFalseWithNoCapturedError()
        {
            // Arrange — the element-free path, where the parser reports failure by returning null
            // instead of raising. No plain byte payload reaches it: malformed input and empty input
            // both make the XML reader raise. The delegate seam is therefore the only deterministic
            // way to drive this branch, and it needs no global state to do so.
            var parse = new Mock<Func<byte[], SvgDocument>>();
            parse.Setup(f => f(It.IsAny<byte[]>())).Returns((SvgDocument)null);

            // Act
            bool parsed = SvgRenderer.TryGetSvgDocument(
                MalformedSvgBytes(),
                parse.Object,
                out SvgDocument document,
                out Exception error
            );

            // Assert
            parsed.Should().BeFalse("a null parse result is a failure, not a success");
            document.Should().BeNull("the parser produced no document");
            error
                .Should()
                .BeNull(
                    "the element-free path fails without raising, so no exception exists to report"
                );
        }

        [TestMethod]
        public void GetSvgDocumentOrThrow_WithMalformedBytes_ThrowsWithTheParserExceptionInner()
        {
            // Arrange
            byte[] malformed = MalformedSvgBytes();

            // Act
            Action act = () => SvgRenderer.GetSvgDocumentOrThrow(malformed);

            // Assert
            act.Should()
                .Throw<InvalidOperationException>(
                    "the fail-fast member converts a parse failure into an explicit exception"
                )
                .Which.InnerException.Should()
                .NotBeNull(
                    "the original parser exception must be preserved as the inner exception"
                );
        }

        [TestMethod]
        public void GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner()
        {
            // Arrange, Act
            Action act = () => SvgRenderer.GetSvgDocumentOrThrow(Array.Empty<byte>());

            // Assert — an empty payload raises inside the parser, so the fail-fast member reports it
            // with that exception preserved as the inner exception. The null-InnerException
            // asymmetry belongs to the element-free path, which is covered through the seam by
            // TryGetSvgDocument_WhenTheParseSeamReturnsNull_ReturnsFalseWithNoCapturedError.
            act.Should()
                .Throw<InvalidOperationException>(
                    "the fail-fast member must report the empty-payload failure too"
                )
                .Which.InnerException.Should()
                .BeOfType<XmlException>(
                    "the original parser exception must be preserved as the inner exception"
                );
        }

        [TestMethod]
        public void TryGetSvgDocument_WithInjectedParseSeam_SurfacesTheSameExceptionInstance()
        {
            // Arrange — the delegate seam exists so exact exception identity can be asserted without
            // mutating any global state. Declared without a nullable annotation because
            // SVGControl.Test compiles as C# 7.3; it binds to the Func<byte[], SvgDocument?>
            // parameter because nullability is metadata-only and the CLR type is identical.
            var sentinel = new InvalidTimeZoneException("sentinel parse failure");
            var parse = new Mock<Func<byte[], SvgDocument>>();
            parse.Setup(f => f(It.IsAny<byte[]>())).Throws(sentinel);

            // Act
            bool parsed = SvgRenderer.TryGetSvgDocument(
                MalformedSvgBytes(),
                parse.Object,
                out SvgDocument document,
                out Exception error
            );

            // Assert
            parsed.Should().BeFalse("the injected parse delegate threw");
            document.Should().BeNull("a failed parse yields no document");
            error
                .Should()
                .BeSameAs(
                    sentinel,
                    "the captured exception must be the original instance, not a wrapped or re-created one"
                );
        }
    }
}
