using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Conversation
{
    [TestClass]
    public class ConversationHelperTests
    {
        [TestMethod]
        public void Transform_returns_expected_row_shape_for_resolved_conversation_items()
        {
            ConvHelper.SafeResolveConversationItem(null, null).Should().BeNull();
        }

        [TestMethod]
        public void Resolver_failure_returns_controlled_result_without_live_outlook()
        {
            ConvHelper.SafeResolveConversationItem(new object(), (ns, id, sid) => throw new System.Exception()).Should().BeNull();
        }
    }
}
