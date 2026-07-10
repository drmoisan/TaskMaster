using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// Deterministic tests for <see cref="StoreWrapperInitProbe"/>, the pure COM-free formatter for the
    /// <c>[store-wrapper-init]</c> diagnosis line (issue #211 Phase 3.6). Uses a list-capturing
    /// <see cref="Action{T}"/> sink. No live COM, no live timer, no network/filesystem, no temporary files.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class StoreWrapperInitProbeTests
    {
        [TestMethod]
        public void FormatLine_RepresentativeStore_ProducesExactString()
        {
            // Arrange
            var probe = new StoreWrapperInitProbe(_ => { });

            // Act
            var line = probe.FormatLine("Mailbox - Dan", 67500.0, 7);

            // Assert
            line.Should()
                .Be("[store-wrapper-init] store=Mailbox - Dan totalMs=67500.0 threadId=7");
        }

        [TestMethod]
        public void FormatLine_NullStoreDisplayName_RendersAngleNull()
        {
            // Arrange
            var probe = new StoreWrapperInitProbe(_ => { });

            // Act
            var line = probe.FormatLine(null, 12.3, 1);

            // Assert
            line.Should().Be("[store-wrapper-init] store=<null> totalMs=12.3 threadId=1");
        }

        [TestMethod]
        public void FormatLine_FormatsTotalMsWithF1AndInvariantCulture()
        {
            // Arrange
            var probe = new StoreWrapperInitProbe(_ => { });

            // Act
            var line = probe.FormatLine("S", 1234.56, 3);

            // Assert (F1 rounds to one decimal place; InvariantCulture uses '.' as the separator)
            line.Should().Be("[store-wrapper-init] store=S totalMs=1234.6 threadId=3");
        }

        [TestMethod]
        public void EmitLine_RoutesFormattedLineToSinkExactlyOnce()
        {
            // Arrange
            var captured = new List<string>();
            var probe = new StoreWrapperInitProbe(captured.Add);

            // Act
            probe.EmitLine("Archive", 500.0, 9);

            // Assert
            captured.Should().ContainSingle();
            captured[0].Should().Be("[store-wrapper-init] store=Archive totalMs=500.0 threadId=9");
        }

        [TestMethod]
        public void Constructor_NullEmit_ThrowsArgumentNullException()
        {
            // Arrange / Act
            Action act = () => new StoreWrapperInitProbe(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("emit");
        }
    }
}
