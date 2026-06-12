using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for the embedded Explorer ribbon definition
    /// (<c>TaskMaster.Ribbon.RibbonExplorer.xml</c>). Outlook loads custom ribbons all-or-nothing:
    /// a single element that violates the Office CustomUI schema causes the entire
    /// <c>customUI</c> document to be rejected, so every TaskMaster button silently fails to load.
    /// These tests assert the structural rules that the loader enforces so that a malformed ribbon
    /// is caught in CI rather than at runtime.
    /// </summary>
    [TestClass]
    public class RibbonExplorerXmlTests
    {
        private const string ResourceName = "TaskMaster.Ribbon.RibbonExplorer.xml";

        private static readonly XNamespace CustomUiNs =
            "http://schemas.microsoft.com/office/2009/07/customui";

        /// <summary>
        /// Controls permitted as direct children of a <c>menu</c> (<c>CT_Menu</c>) in the Office
        /// 2009 CustomUI schema. Notably, container/input controls such as <c>editBox</c>,
        /// <c>comboBox</c>, <c>dropDown</c>, and <c>box</c> are not permitted inside a menu.
        /// </summary>
        private static readonly HashSet<string> MenuLegalControls = new HashSet<string>(
            StringComparer.Ordinal
        )
        {
            "button",
            "checkBox",
            "gallery",
            "dynamicMenu",
            "menu",
            "splitButton",
            "toggleButton",
            "menuSeparator",
            "control",
        };

        private static XDocument LoadRibbonDocument()
        {
            var assembly = typeof(RibbonController).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceName);
            stream
                .Should()
                .NotBeNull(
                    "the Explorer ribbon must be embedded as '{0}' for Outlook to load it",
                    ResourceName
                );
            using var reader = new StreamReader(stream!);
            return XDocument.Parse(reader.ReadToEnd());
        }

        [TestMethod]
        public void RibbonExplorerXml_IsWellFormedXml()
        {
            // Act
            Action act = () => LoadRibbonDocument();

            // Assert: a parse failure here means the ribbon is malformed at the XML level.
            act.Should().NotThrow();
        }

        [TestMethod]
        public void RibbonExplorerXml_MenusContainOnlyMenuLegalControls()
        {
            // Arrange
            var document = LoadRibbonDocument();

            // Act: collect every child element of every <menu> whose tag is not menu-legal.
            var illegalChildren = document
                .Descendants(CustomUiNs + "menu")
                .SelectMany(menu => menu.Elements())
                .Where(child => !MenuLegalControls.Contains(child.Name.LocalName))
                .Select(child =>
                    $"{child.Name.LocalName}#{child.Attribute("id")?.Value ?? "(no id)"}"
                )
                .ToList();

            // Assert: any disallowed child (for example, an editBox) makes Outlook reject the
            // whole customUI document, so all TaskMaster buttons fail to load.
            illegalChildren
                .Should()
                .BeEmpty(
                    "controls inside a <menu> must be menu-legal; the following are not: {0}",
                    string.Join(", ", illegalChildren)
                );
        }

        /// <summary>
        /// The four TaskMaster custom groups (<c>SpamBayesGroup</c>, <c>Group2</c>,
        /// <c>TriageGroup</c>, <c>UtilitiesGroup</c>) must live under the dedicated custom tab
        /// labeled "Taskmaster" rather than on the built-in Mail tab. This asserts each group
        /// resolves as a descendant <c>group</c> of a <c>tab</c> whose <c>label</c> is "Taskmaster".
        /// </summary>
        [TestMethod]
        public void RibbonExplorerXml_TaskMasterGroupsLiveUnderTaskmasterTab()
        {
            // Arrange
            var document = LoadRibbonDocument();
            var expectedGroupIds = new[]
            {
                "SpamBayesGroup",
                "Group2",
                "TriageGroup",
                "UtilitiesGroup",
            };

            // Act: collect the ids of every <group> that descends from a <tab label="Taskmaster">.
            var taskmasterGroupIds = document
                .Descendants(CustomUiNs + "tab")
                .Where(tab => tab.Attribute("label")?.Value == "Taskmaster")
                .Descendants(CustomUiNs + "group")
                .Select(group => group.Attribute("id")?.Value)
                .ToList();

            // Assert: all four custom groups are children of the Taskmaster tab.
            taskmasterGroupIds
                .Should()
                .Contain(
                    expectedGroupIds,
                    "the four custom groups must be moved under the dedicated Taskmaster tab"
                );
        }

        /// <summary>
        /// After the move, the built-in Mail tab (<c>idMso="TabMail"</c>) must carry no custom
        /// <c>group</c>. This asserts <c>TabMail</c> is either absent from the document or, if
        /// present, has zero <c>group</c> children, so no TaskMaster control remains on the
        /// native Mail tab.
        /// </summary>
        [TestMethod]
        public void RibbonExplorerXml_TabMailCarriesNoCustomGroup()
        {
            // Arrange
            var document = LoadRibbonDocument();

            // Act: find the built-in Mail tab and count its <group> descendants.
            var tabMail = document
                .Descendants(CustomUiNs + "tab")
                .SingleOrDefault(tab => tab.Attribute("idMso")?.Value == "TabMail");

            var tabMailGroupCount = tabMail?.Descendants(CustomUiNs + "group").Count() ?? 0;

            // Assert: TabMail is absent, or present with no custom group.
            tabMailGroupCount
                .Should()
                .Be(
                    0,
                    "the built-in Mail tab must not host any custom TaskMaster group after the move"
                );
        }
    }
}
