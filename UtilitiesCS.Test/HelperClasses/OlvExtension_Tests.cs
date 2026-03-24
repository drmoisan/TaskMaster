using System;
using System.Drawing;
using System.Threading;
using BrightIdeasSoftware;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.HelperClasses
{
    /// <summary>
    /// Unit tests for <see cref="OlvExtension"/>.
    ///
    /// Purpose:
    ///     Covers the <c>AutoScaleColumnsToContainer</c> extension method on
    ///     <see cref="ObjectListView"/>, verifying proportional column resizing and
    ///     graceful handling of an empty column list.
    ///
    /// Constraints:
    ///     ObjectListView is a WinForms control; all tests run on a dedicated STA
    ///     thread and surface any exception to the main thread for MSTest to record.
    /// </summary>
    [TestClass]
    public class OlvExtension_Tests
    {
        /// <summary>
        /// Verifies that <see cref="OlvExtension.AutoScaleColumnsToContainer"/>
        /// resizes each column proportionally to fill the control's container width.
        ///
        /// Purpose:
        ///     Exercises the main scaling branch: total column width differs from the
        ///     container width, so each column must be scaled by the ratio
        ///     <c>containerWidth / totalColumnWidth</c>.
        ///
        /// Returns:
        ///     Asserts each column width equals <c>Math.Round(originalWidth *
        ///     containerWidth / totalWidth)</c>.
        ///
        /// Side Effects:
        ///     Creates and disposes a transient WinForms ObjectListView and two
        ///     OLVColumns; no persistent state is left.
        /// </summary>
        [TestMethod]
        public void AutoScaleColumnsToContainer_WithTwoColumns_ScalesWidthsProportionally()
        {
            int colAWidthAfterScale = 0;
            int colBWidthAfterScale = 0;
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                ObjectListView olv = null;
                try
                {
                    // Arrange: container width = 400; two equal columns each 100 wide
                    // total column width = 200; scale factor = 400/200 = 2×
                    // expected result: colA = 200, colB = 200
                    olv = new ObjectListView();
                    olv.Size = new Size(400, 100);

                    var colA = new OLVColumn("ColA", null);
                    colA.Width = 100;
                    var colB = new OLVColumn("ColB", null);
                    colB.Width = 100;

                    olv.Columns.Add(colA);
                    olv.Columns.Add(colB);

                    // Act
                    olv.AutoScaleColumnsToContainer();

                    // Capture widths for assertion outside the STA thread
                    colAWidthAfterScale = colA.Width;
                    colBWidthAfterScale = colB.Width;
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    if (olv != null)
                    {
                        olv.Dispose();
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert
            caughtException.Should().BeNull("AutoScaleColumnsToContainer should not throw with valid columns");

            // Math: 100 * 400 / 200 = 200 for each column (double rounding: Math.Round(200.0) = 200)
            colAWidthAfterScale.Should().Be(200, "column A must be scaled from 100 to 200 when container is 2× the total column width");
            colBWidthAfterScale.Should().Be(200, "column B must be scaled from 100 to 200 when container is 2× the total column width");
        }

        /// <summary>
        /// Verifies that calling <see cref="OlvExtension.AutoScaleColumnsToContainer"/>
        /// on an <see cref="ObjectListView"/> with no columns is a no-op and does not throw.
        ///
        /// Purpose:
        ///     Exercises the guard branch inside <c>AutoScaleColumnsToContainer</c>:
        ///     when <c>colswidth == 0</c> (no columns), the scaling loop is skipped
        ///     and the method returns silently.
        ///
        /// Side Effects:
        ///     Creates and disposes a transient WinForms ObjectListView; no persistent
        ///     state is left.
        /// </summary>
        [TestMethod]
        public void AutoScaleColumnsToContainer_WithNoColumns_DoesNotThrow()
        {
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                ObjectListView olv = null;
                try
                {
                    // Arrange: no columns added; colswidth will be 0 inside the method
                    olv = new ObjectListView();
                    olv.Size = new Size(400, 100);

                    // Act: should return silently without entering the scaling loop
                    olv.AutoScaleColumnsToContainer();
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    if (olv != null)
                    {
                        olv.Dispose();
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert
            caughtException.Should().BeNull("AutoScaleColumnsToContainer must be a no-op and not throw when there are no columns");
        }
    }
}
