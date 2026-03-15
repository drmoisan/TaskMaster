using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.MailItem
{
    [TestClass]
    public class MailItemHelperProjectionTests
    {
        [TestMethod]
        public void Projection_returns_subject_and_entry_id_from_mock_mail_item()
        {
            var projection = MailItemHelper.TryProjectMailItemMembers(new { Subject = "S", EntryID = "E" });
            projection.Subject.Should().Be("S");
            projection.EntryId.Should().Be("E");
        }

        [TestMethod]
        public void Projection_returns_safe_defaults_when_member_lookup_fails()
        {
            var projection = MailItemHelper.TryProjectMailItemMembers(new { });
            projection.Subject.Should().BeEmpty();
            projection.EntryId.Should().BeEmpty();
        }
    }
}
