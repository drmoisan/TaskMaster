using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// Deterministic unit tests for the pure <see cref="StoreLockupAttribution"/> formatter
    /// (issue #264). No log4net, COM, or clock dependency — a plain string assertion on the
    /// formatter output with culture-invariant numeric formatting.
    /// </summary>
    [TestClass]
    public class StoreLockupAttributionTests
    {
        [TestMethod]
        public void FormatLine_ValidIdentity_Disabled_ProducesExactLine()
        {
            // Act
            var line = StoreLockupAttribution.FormatLine(
                "Mailbox A",
                TimeSpan.FromMilliseconds(6000),
                autoDisabled: true
            );

            // Assert
            line.Should().Be("[store-lockup] identity=Mailbox A stallMs=6000.0 autoDisabled=true");
        }

        [TestMethod]
        public void FormatLine_NotDisabled_RendersAutoDisabledFalse()
        {
            // Act
            var line = StoreLockupAttribution.FormatLine(
                "Mailbox B",
                TimeSpan.FromMilliseconds(5500),
                autoDisabled: false
            );

            // Assert
            line.Should()
                .Be("[store-lockup] identity=Mailbox B stallMs=5500.0 autoDisabled=false");
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void FormatLine_NullOrEmptyIdentity_RendersNullToken(string identity)
        {
            // Act
            var line = StoreLockupAttribution.FormatLine(
                identity,
                TimeSpan.FromMilliseconds(7000),
                autoDisabled: true
            );

            // Assert
            line.Should().Be("[store-lockup] identity=<null> stallMs=7000.0 autoDisabled=true");
        }

        [TestMethod]
        public void FormatLine_UsesInvariantCultureForNumericFormatting()
        {
            // Act: a fractional millisecond value formats with a '.' decimal separator regardless of
            // the ambient culture.
            var line = StoreLockupAttribution.FormatLine(
                "Mailbox C",
                TimeSpan.FromTicks(
                    TimeSpan.TicksPerMillisecond * 1234 + TimeSpan.TicksPerMillisecond / 2
                ),
                autoDisabled: true
            );

            // Assert
            line.Should().Be("[store-lockup] identity=Mailbox C stallMs=1234.5 autoDisabled=true");
        }
    }
}
