using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Table
{
    [TestClass]
    public class OlTableExtensionsRetryTests
    {
        [TestMethod]
        public void Retry_stops_after_successful_table_call()
        {
            var attempts = 0;
            var result = OlTableExtensions.RunTableRetry(() => { attempts++; return 42; }, 3);
            result.Should().Be(42);
            attempts.Should().Be(1);
        }

        [TestMethod]
        public void Retry_returns_controlled_failure_after_exhaustion()
        {
            var result = OlTableExtensions.RunTableRetry<object>(() => throw new System.InvalidOperationException(), 2);
            result.Should().BeNull();
        }
    }
}
