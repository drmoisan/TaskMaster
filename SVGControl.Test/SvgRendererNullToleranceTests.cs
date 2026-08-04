using System;
using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SVGControl.Test
{
    /// <summary>
    /// AC-4 contracts for issue #418: the consumers that already treat a missing document as a
    /// normal state must keep behaving exactly as before the fix. A null document is a supported
    /// state of <see cref="SvgRenderer"/>, not an error, so neither the property setter, nor
    /// <c>Render()</c>, nor the <see cref="SvgImageSelector"/> surface may throw because of it.
    /// </summary>
    [TestClass]
    public class SvgRendererNullToleranceTests
    {
        private static readonly Size TargetSize = new Size(32, 32);

        private static SvgRenderer CreateRendererWithoutDocument()
        {
            // The outer/margin/autoSize constructor parses nothing, so the renderer starts in the
            // null-document state deterministically and without depending on the SVG parser.
            return new SvgRenderer(TargetSize, new Padding(0), AutoSize.MaintainAspectRatio);
        }

        private static SvgImageSelector CreateSelectorWithDefaultImage()
        {
            return new SvgImageSelector(
                TargetSize,
                new Padding(0),
                AutoSize.MaintainAspectRatio,
                true
            );
        }

        [TestMethod]
        public void DocumentSetter_AssignedNull_SucceedsAndLeavesDocumentNull()
        {
            // Arrange
            SvgRenderer renderer = CreateRendererWithoutDocument();

            // Act
            Action act = () => renderer.Document = null;

            // Assert — the setter's non-null branch draws the document to measure it; the null
            // branch must skip that dereference rather than guarding it after the fact.
            act.Should().NotThrow("a null document is a supported state, not an error, per AC-4");
            renderer.Document.Should().BeNull("the assigned null must be observable");
        }

        [TestMethod]
        public void Render_WithNullDocument_ReturnsNull()
        {
            // Arrange
            SvgRenderer renderer = CreateRendererWithoutDocument();
            renderer.Document = null;

            // Act
            Bitmap rendered = renderer.Render();

            // Assert — returning null rather than throwing is the pre-existing tolerant contract
            // that AC-4 requires the issue #418 fix to preserve.
            try
            {
                rendered.Should().BeNull("Render must degrade to null when there is no document");
            }
            finally
            {
                // Defensive: if the contract ever regresses and a bitmap is produced, it is still
                // released rather than leaked into the test host.
                rendered?.Dispose();
            }
        }

        [TestMethod]
        public void SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull()
        {
            // Arrange — SetDefaultImage routes through SvgRenderer.GetSvgDocument, the tolerant
            // parse whose null-returning contract AC-4 preserves. The renderer field is private, so
            // the document's presence is observed through Render(), which returns null if and only
            // if the document is null.
            SvgImageSelector selector = CreateSelectorWithDefaultImage();

            // Act
            selector.SetDefaultImage();
            Bitmap rendered = selector.Render();

            // Assert
            try
            {
                rendered
                    .Should()
                    .NotBeNull(
                        "the built-in default image is well-formed, so the tolerant parse must produce a document"
                    );
            }
            finally
            {
                rendered?.Dispose();
            }
        }

        [TestMethod]
        public void DefaultImageConstructor_DoesNotThrow()
        {
            // Arrange
            SvgImageSelector selector = null;

            // Act
            Action act = () => selector = CreateSelectorWithDefaultImage();

            // Assert — this constructor is the designer-host path from issue #418: it forwards the
            // default payload to the byte-array SvgRenderer constructor, which previously
            // dereferenced a swallowed null and surfaced an opaque NullReferenceException.
            act.Should()
                .NotThrow(
                    "the default-image constructor must complete even when the payload cannot be parsed"
                );
            selector.Should().NotBeNull("the constructor completed without throwing");
        }

        [TestMethod]
        public void UseDefaultImageSetterToFalse_DoesNotThrowAndRecordsTheNewValue()
        {
            // Arrange
            SvgImageSelector selector = CreateSelectorWithDefaultImage();

            // Act
            Action act = () => selector.UseDefaultImage = false;

            // Assert — the setter's document-clearing branch is guarded by _relativeImagePath being
            // "" or "(none)". That field is never assigned on any live path (a pre-existing
            // condition documented by the CS0649 suppression at SvgImageSelector.cs:62-65), so a
            // freshly constructed selector does not enter the branch. What AC-4 requires here, and
            // what this test proves, is that the setter completes without throwing and records the
            // new value; asserting a clear that the pre-existing guard prevents would assert
            // behavior the production code does not have.
            act.Should().NotThrow("toggling the default image off must not throw, per AC-4");
            selector.UseDefaultImage.Should().BeFalse("the setter must record the assigned value");
        }
    }
}
