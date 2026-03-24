using System;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;

namespace UtilitiesCS.Test.HelperClasses
{
    /// <summary>
    /// Unit tests for <see cref="DgvForm"/>.
    ///
    /// Purpose:
    ///     Verify that the resize-end event path in DgvForm executes without throwing
    ///     and that the form can be constructed normally on an STA thread.
    ///
    /// Note: DgvForm is a thin WinForms designer shell; its only non-designer method
    ///     is the ResizeEnd handler, which writes diagnostic output via Debug.WriteLine.
    ///     Tests verify the public surface (construction) and the resize path via reflection.
    /// </summary>
    [TestClass]
    public class DvgForm_Tests
    {
        [TestMethod]
        public void DgvForm_ResizeEnd_DoesNotThrow()
        {
            // Arrange: WinForms controls must be created on an STA thread.
            Exception caughtException = null;

            var thread = new Thread(() =>
            {
                DgvForm form = null;
                try
                {
                    // Arrange: construct on the STA thread.
                    form = new DgvForm();

                    // Obtain the private ResizeEnd handler via reflection to invoke
                    // it directly without needing a message loop or visible window.
                    MethodInfo handler = typeof(DgvForm).GetMethod(
                        "DgvForm_ResizeEnd",
                        BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    handler
                        .Should()
                        .NotBeNull("DgvForm_ResizeEnd must exist as a private instance method");

                    // Act: invoke the resize-end handler with a synthetic EventArgs.
                    // This path only calls Debug.WriteLine and must not throw.
                    handler.Invoke(form, new object[] { form, EventArgs.Empty });
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    if (form != null)
                    {
                        form.Dispose();
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert: no exception should have escaped the resize-end handler.
            caughtException
                .Should()
                .BeNull("the resize-end handler should complete without throwing");
        }
    }
}
