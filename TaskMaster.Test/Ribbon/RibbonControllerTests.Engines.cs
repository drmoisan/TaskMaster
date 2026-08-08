using System;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;

namespace TaskMaster.Test.Ribbon
{
    public partial class RibbonControllerTests
    {
        /// <summary>
        /// Regression test for issue #507: reading <see cref="RibbonController.Engines"/> before
        /// <c>Globals</c> has been assigned (i.e. before <c>SetGlobals</c> has run) must return
        /// <c>null</c> rather than throwing a <see cref="NullReferenceException"/>. A bare
        /// <c>new RibbonController()</c> leaves <c>Globals</c> at its default (unassigned) value.
        /// </summary>
        [TestMethod]
        public void Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing()
        {
            // Arrange: construct a controller directly, without CreateController() and without
            // setting Globals, so Globals remains unassigned.
            var controller = new RibbonController();

            // Act
            System.Action act = () =>
            {
                var result = controller.Engines;
                result.Should().BeNull();
            };

            // Assert: reading Engines must not throw, and must yield null.
            act.Should().NotThrow();
        }

        /// <summary>
        /// Regression test for issue #507: when <c>Globals</c> is assigned, <c>Engines</c> must
        /// continue to forward the value of <c>Globals.Engines</c> (no behavior regression for the
        /// assigned path). Uses reference equality against a distinguishable mock instance to prove
        /// forwarding, not merely a null-to-null coincidence. Sets <c>Globals.Engines</c> via
        /// property-based reflection because <c>ApplicationGlobals.Engines</c> is
        /// <c>public IAppItemEngines Engines { get; private set; }</c>, mirroring the reflection
        /// pattern <see cref="CreateController"/> already uses to set the <c>Globals</c> property
        /// itself.
        /// </summary>
        [TestMethod]
        public void Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines()
        {
            // Arrange
            var controller = CreateController();
            var expectedEngines = new Mock<IAppItemEngines>().Object;
            var globals = (ApplicationGlobals)
                typeof(RibbonController)
                    .GetProperty(
                        "Globals",
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
                    )
                    .GetValue(controller);
            typeof(ApplicationGlobals)
                .GetProperty(
                    "Engines",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
                )
                .SetValue(globals, expectedEngines);

            // Act
            var result = controller.Engines;

            // Assert: the property must forward the exact assigned instance.
            result.Should().BeSameAs(expectedEngines);
        }
    }
}
