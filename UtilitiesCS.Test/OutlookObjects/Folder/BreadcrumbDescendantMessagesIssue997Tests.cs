using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class BreadcrumbDescendantMessagesIssue997Tests
    {
        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void DescendantAddress_RoundTripsWithoutClientPath(bool toggle)
        {
            var message = new BreadcrumbSelectorDescendantMessage("row", "known-node", toggle);
            string json = BreadcrumbSelectorMessageSerializer.Serialize(message);
            json.Should().NotContain("path");
            BreadcrumbSelectorMessageSerializer.Parse(json).Should().BeEquivalentTo(message);
        }

        [DataTestMethod]
        [DataRow("null")]
        [DataRow("12")]
        [DataRow("false")]
        [DataRow("[]")]
        [DataRow("{}")]
        [DataRow("\" \"")]
        public void DescendantAddress_RejectsInvalidIdentityTokens(string token)
        {
            foreach (string field in new[] { "rowIdentity", "nodeIdentity" })
            {
                string json =
                    "{\"type\":\"selectorDescendantActivate\",\"rowIdentity\":\"row\",\"nodeIdentity\":\"node\"}";
                json = json.Replace(
                    "\"" + field + "\":\"" + (field == "rowIdentity" ? "row" : "node") + "\"",
                    "\"" + field + "\":" + token
                );
                Action parse = () => BreadcrumbSelectorMessageSerializer.Parse(json);
                parse.Should().Throw<FormatException>();
            }
        }

        [TestMethod]
        public void DescendantAddress_RejectsBlankConstructorArguments()
        {
            Action blankRow = () => new BreadcrumbSelectorDescendantMessage(" ", "node");
            Action blankNode = () => new BreadcrumbSelectorDescendantMessage("row", " ");
            blankRow.Should().Throw<ArgumentException>();
            blankNode.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public async Task RecursiveRender_RoundTripsKnownIdentityAndDurableSelection()
        {
            var router = await BreadcrumbDescendantSelectionIssue997Tests.CreateRouterAsync();
            var child = router.Model.Rows[0].DescendantRoot.Children[0];
            child.Expanded = true;
            child.Attach(
                new[]
                {
                    new FolderBreadcrumbSegment(
                        new FolderTreeNodeKey(
                            "store-a",
                            "review",
                            child.Segment.FolderPath + "\\Review"
                        ),
                        "Review",
                        child.Segment.FolderPath + "\\Review",
                        false
                    ),
                }
            );
            router.OpenSelector();
            router.ActivateSelectorDescendant(
                router.Model.Rows[0].Identity,
                child.Children[0].Identity
            );
            var render = (RenderMessage)BreadcrumbBridgeSerializer.Parse(router.RenderJson());
            var rendered = render.Rows[0].Subfolders[0];
            rendered.Identity.Should().Be(child.Identity);
            rendered.Expanded.Should().BeTrue();
            rendered.ChildrenLoaded.Should().BeTrue();
            rendered.Children[0].Selected.Should().BeTrue();
            render.SelectedDescendantIdentity.Should().Be(child.Children[0].Identity);
            string roundTrip = BreadcrumbBridgeSerializer.Serialize(render);
            roundTrip.Should().Be(router.RenderJson());
            await router.RouteAsync(
                "{\"type\":\"affordanceToggle\",\"rowIndex\":0}",
                CancellationToken.None
            );
            var collapsed = (RenderMessage)BreadcrumbBridgeSerializer.Parse(router.RenderJson());
            collapsed.Rows[0].Subfolders.Should().BeEmpty();
            collapsed.SelectedFolder.Should().EndWith("\\Alpha\\Review");
            collapsed.SelectedDescendantIdentity.Should().Be(render.SelectedDescendantIdentity);
        }
    }
}
