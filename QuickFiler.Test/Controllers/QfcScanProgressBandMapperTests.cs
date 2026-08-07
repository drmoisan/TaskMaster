using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Issue #424 coverage for <see cref="QfcScanProgressBandMapper"/>, the pure mapping of the
    /// confidence gate's <c>(scanned, accepted, quantity)</c> signal onto the 0-30 progress band.
    /// The mapper is <c>internal</c> and reachable directly because <c>QuickFiler</c> declares
    /// <c>InternalsVisibleTo("QuickFiler.Test")</c>, so no reflection is needed. Deterministic: no
    /// clock, no I/O, no COM, no temp files.
    /// </summary>
    [TestClass]
    public class QfcScanProgressBandMapperTests
    {
        /// <summary>Creates a mapper over a recording sink.</summary>
        private static QfcScanProgressBandMapper CreateMapper(
            out List<(double Value, string Label)> reports
        )
        {
            var recorded = new List<(double Value, string Label)>();
            reports = recorded;
            return new QfcScanProgressBandMapper((value, label) => recorded.Add((value, label)));
        }

        [TestMethod]
        public void Constructor_NullReport_ThrowsArgumentNullException()
        {
            // Act — `System.Action` avoids the CS0104 ambiguity with Outlook's Action type.
            System.Action act = () => new QfcScanProgressBandMapper(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("report");
        }

        [TestMethod]
        public void Constructor_NonNullReport_DoesNotThrow()
        {
            // Act
            System.Action act = () => new QfcScanProgressBandMapper((value, label) => { });

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Report_QuantityZero_MapsToZero()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act
            mapper.Report(scanned: 5, accepted: 3, quantity: 0);

            // Assert
            reports.Should().ContainSingle().Which.Value.Should().Be(0);
        }

        [TestMethod]
        public void Report_QuantityNegative_MapsToZero()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act
            mapper.Report(scanned: 5, accepted: 3, quantity: -4);

            // Assert
            reports.Should().ContainSingle().Which.Value.Should().Be(0);
        }

        [TestMethod]
        public void Report_ZeroAccepted_MapsToZeroWithScanningLabel()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act
            mapper.Report(scanned: 7, accepted: 0, quantity: 5);

            // Assert
            (double Value, string Label) only = reports.Should().ContainSingle().Subject;
            only.Value.Should().Be(0);
            only.Label.Should().Be("Scanning for high-confidence items (7 scanned, 0 accepted)");
        }

        [TestMethod]
        public void Report_MidBand_MapsProportionallyIntoTheBand()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act — 2 of 5 accepted maps to round(30 * 2 / 5) == 12.
            mapper.Report(scanned: 9, accepted: 2, quantity: 5);

            // Assert
            reports.Should().ContainSingle().Which.Value.Should().Be(12d);
        }

        [TestMethod]
        public void Report_AcceptedEqualsQuantity_MapsToBandCeiling()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act
            mapper.Report(scanned: 12, accepted: 5, quantity: 5);

            // Assert
            reports.Should().ContainSingle().Which.Value.Should().Be(30d);
        }

        [TestMethod]
        public void Report_AcceptedExceedsQuantity_ClampsToBandCeiling()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act
            mapper.Report(scanned: 20, accepted: 9, quantity: 5);

            // Assert
            reports.Should().ContainSingle().Which.Value.Should().Be(30d);
        }

        [TestMethod]
        public void Report_NegativeAcceptedCount_ClampsToZero()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act — a negative accepted count would otherwise compute a negative band value.
            mapper.Report(scanned: 3, accepted: -3, quantity: 5);

            // Assert
            reports.Should().ContainSingle().Which.Value.Should().Be(0);
        }

        [TestMethod]
        public void Report_WhenComputedValueWouldDecrease_HoldsThePreviousValue()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act — a regressing accepted count must not move the bar backwards.
            mapper.Report(scanned: 4, accepted: 4, quantity: 5); // round(24) == 24
            mapper.Report(scanned: 5, accepted: 1, quantity: 5); // would be 6

            // Assert
            reports.Should().HaveCount(2);
            reports[0].Value.Should().Be(24d);
            reports[1].Value.Should().Be(24d, "the mapped value must never decrease");
        }

        [TestMethod]
        public void Report_AcrossARisingSequence_IsMonotonicAndStaysInsideTheBand()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act
            for (int scanned = 1; scanned <= 12; scanned++)
            {
                mapper.Report(scanned, accepted: scanned / 3, quantity: 4);
            }

            // Assert
            reports.Should().HaveCount(12);
            reports.Should().OnlyContain(r => r.Value >= 0 && r.Value <= 30);
            for (int i = 1; i < reports.Count; i++)
            {
                reports[i]
                    .Value.Should()
                    .BeGreaterThanOrEqualTo(reports[i - 1].Value, "the sequence is monotonic");
            }
        }

        [TestMethod]
        public void Report_LabelFormat_CarriesScannedAndAcceptedCounts()
        {
            // Arrange
            QfcScanProgressBandMapper mapper = CreateMapper(
                out List<(double Value, string Label)> reports
            );

            // Act
            mapper.Report(scanned: 1, accepted: 0, quantity: 4);
            mapper.Report(scanned: 12, accepted: 3, quantity: 4);

            // Assert
            reports[0]
                .Label.Should()
                .Be("Scanning for high-confidence items (1 scanned, 0 accepted)");
            reports[1]
                .Label.Should()
                .Be("Scanning for high-confidence items (12 scanned, 3 accepted)");
        }
    }
}
