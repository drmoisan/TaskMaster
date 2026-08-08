using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Unit tests for the issue #503 post-initialization refresh decision. Office caches each
    /// <c>getEnabled</c> response per control until the add-in invalidates it, so the set of
    /// invalidated control ids is what determines whether the eight engine-backed buttons become
    /// enabled after <c>InitAsync()</c> succeeds.
    /// </summary>
    [TestClass]
    public class EngineCommandRefreshPlannerTests
    {
        [TestMethod]
        public void InvalidateAll_InvokesDelegateOnceForEachEngineBackedControlId()
        {
            // Arrange
            var invalidated = new List<string>();

            // Act
            EngineCommandRefreshPlanner.InvalidateAll(invalidated.Add);

            // Assert: SET equality, never an ordered sequence. Office documents callback ordering
            // as unspecified, so no test may depend on invalidation order.
            invalidated
                .Should()
                .BeEquivalentTo(
                    EngineCommandCatalog.ControlIds,
                    "every engine-backed control must be invalidated exactly once, and no other"
                );
            invalidated
                .Should()
                .HaveCount(
                    EngineCommandCatalog.ControlIds.Count,
                    "a control invalidated twice would signal a duplicate catalog entry"
                );
        }

        [TestMethod]
        public void InvalidateAll_WithNullDelegate_ThrowsArgumentNullException()
        {
            // Arrange, Act
            Action act = () => EngineCommandRefreshPlanner.InvalidateAll(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("invalidateControl");
        }
    }
}
