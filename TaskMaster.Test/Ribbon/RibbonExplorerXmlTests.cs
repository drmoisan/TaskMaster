using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        #region Issue #503 — engine-readiness getEnabled wiring

        /// <summary>
        /// The <c>getEnabled</c> callback name declared in the ribbon XML for every engine-backed
        /// control. Office matches this string against a public instance method on
        /// <see cref="RibbonViewer"/> by name.
        /// </summary>
        private const string EngineCommandGetEnabledCallback = "EngineCommand_GetEnabled";

        /// <summary>
        /// Every engine-backed control id in <c>EngineCommandCatalog</c> must exist in the ribbon
        /// XML and must declare the <c>getEnabled</c> callback. Without the attribute the control
        /// stays clickable for the whole initialization window, which is the #503 defect.
        /// </summary>
        [TestMethod]
        public void RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback()
        {
            // Arrange
            var document = LoadRibbonDocument();

            // Act: index every element that carries an id.
            var elementsById = document
                .Descendants()
                .Where(element => element.Attribute("id") != null)
                .ToDictionary(element => element.Attribute("id")!.Value, element => element);

            // Assert
            foreach (var controlId in EngineCommandCatalog.ControlIds)
            {
                elementsById
                    .Should()
                    .ContainKey(
                        controlId,
                        "the catalog control id '{0}' must exist in the ribbon XML",
                        controlId
                    );
                // Bind the attribute first. A null-conditional dereference here would
                // short-circuit the whole assertion chain, including .Should(), so the
                // test would pass silently on the exact regression it exists to catch.
                var getEnabled = elementsById[controlId].Attribute("getEnabled");
                getEnabled
                    .Should()
                    .NotBeNull(
                        "control '{0}' is engine-backed and must declare a getEnabled callback",
                        controlId
                    );
                getEnabled!
                    .Value.Should()
                    .Be(
                        EngineCommandGetEnabledCallback,
                        "control '{0}' is engine-backed and must be disabled until its engine loads",
                        controlId
                    );
            }
        }

        /// <summary>
        /// No element other than the engine-backed controls may declare the callback. The hazard
        /// this guards against is disabling via a <em>containing menu</em>, which would sweep up
        /// folder-settings and the enable-toggle checkboxes along with the commands. Per-control
        /// gating is different: as of issue #518 the save-location and save-info buttons are
        /// themselves engine-backed catalog members, so they are disabled exactly while they would
        /// otherwise no-op. The two enable-toggle checkboxes remain outside the catalog by design —
        /// they are backed by engine configuration rather than readiness, and a readiness-gated
        /// toggle could never re-enable a disabled engine.
        /// </summary>
        [TestMethod]
        public void RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls()
        {
            // Arrange
            var document = LoadRibbonDocument();

            // Act
            var declaringIds = document
                .Descendants()
                .Where(element =>
                    element.Attribute("getEnabled")?.Value == EngineCommandGetEnabledCallback
                )
                .Select(element => element.Attribute("id")?.Value ?? "(no id)")
                .ToList();

            // Assert: set equality guards against over-disabling the UI.
            declaringIds
                .Should()
                .BeEquivalentTo(
                    EngineCommandCatalog.ControlIds,
                    "only the engine-backed controls may be disabled by the readiness callback"
                );
        }

        /// <summary>
        /// The CustomUI schema exposes <c>getEnabled</c> on controls such as <c>button</c>, but
        /// not on <c>group</c> or <c>tab</c>. A catalog id resolving to a container would make the
        /// whole document illegal, which Outlook rejects all-or-nothing.
        /// </summary>
        [TestMethod]
        public void RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled()
        {
            // Arrange
            var document = LoadRibbonDocument();
            var elementsById = document
                .Descendants()
                .Where(element => element.Attribute("id") != null)
                .ToDictionary(element => element.Attribute("id")!.Value, element => element);

            // Act, Assert
            foreach (var controlId in EngineCommandCatalog.ControlIds)
            {
                elementsById[controlId]
                    .Name.LocalName.Should()
                    .Be(
                        "button",
                        "control '{0}' must be a button; group and tab do not permit getEnabled",
                        controlId
                    );
            }
        }

        /// <summary>
        /// VSTO silently ignores a callback whose signature does not match: the code compiles and
        /// nothing happens when Office queries the control. This pins the exact required shape
        /// <c>public bool GetEnabled(Office.IRibbonControl control)</c>.
        /// </summary>
        /// <remarks>
        /// The parameter type is compared by <see cref="Type.FullName"/> rather than with
        /// <c>typeof</c>: <c>TaskMaster.Test.csproj</c> carries no reference to the
        /// <c>Office</c> (Microsoft.Office.Core) primary interop assembly, and a legacy non-SDK
        /// <c>ProjectReference</c> does not flow that reference to the compiler. <c>office.dll</c>
        /// is present in the test output directory and in the GAC, so the runtime reflection
        /// resolves the type at test time.
        /// </remarks>
        [TestMethod]
        public void RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer()
        {
            // Act
            var callback = typeof(RibbonViewer).GetMethod(
                EngineCommandGetEnabledCallback,
                BindingFlags.Public | BindingFlags.Instance
            );

            // Assert
            callback
                .Should()
                .NotBeNull(
                    "'{0}' must be a public instance method on RibbonViewer",
                    EngineCommandGetEnabledCallback
                );
            callback!.ReturnType.Should().Be<bool>("Office requires a bool getEnabled callback");

            var parameters = callback.GetParameters();
            parameters.Should().ContainSingle("the Office callback takes exactly one parameter");
            parameters[0]
                .ParameterType.FullName.Should()
                .Be(
                    "Microsoft.Office.Core.IRibbonControl",
                    "the single parameter must be the Office IRibbonControl"
                );
        }

        #endregion Issue #503 — engine-readiness getEnabled wiring

        #region Issue #735 — callback name binding

        /// <summary>
        /// Full name of the Office ribbon-control interface that every CustomUI callback takes as
        /// its first parameter. Compared by <see cref="Type.FullName"/> rather than with
        /// <c>typeof</c> because <c>TaskMaster.Test.csproj</c> carries no reference to the
        /// Microsoft.Office.Core primary interop assembly and a legacy non-SDK
        /// <c>ProjectReference</c> does not flow that reference to the compiler.
        /// </summary>
        private const string RibbonControlTypeName = "Microsoft.Office.Core.IRibbonControl";

        /// <summary>
        /// Returns every public instance method declared on <see cref="RibbonViewer"/>, which is
        /// the set Office resolves a CustomUI callback name against at invocation time.
        /// </summary>
        private static MethodInfo[] GetViewerCallbackSurface() =>
            typeof(RibbonViewer).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        /// <summary>
        /// Reports whether an attribute is an Office CustomUI callback binding. The rule — local
        /// name is <c>onAction</c>, <c>onChange</c> or <c>onLoad</c>, or begins with <c>get</c> —
        /// is exact for the 2009 CustomUI schema, in which every <c>get*</c> attribute is a
        /// callback, and stays correct if a new getter such as <c>getVisible</c> is introduced.
        /// </summary>
        private static bool IsCallbackAttribute(XAttribute attribute)
        {
            var localName = attribute.Name.LocalName;
            return localName == "onAction"
                || localName == "onChange"
                || localName == "onLoad"
                || localName.StartsWith("get", StringComparison.Ordinal);
        }

        /// <summary>
        /// Office binds CustomUI callbacks by string name and resolves them reflectively at
        /// invocation time, so a name that matches no method produces no compiler error, no load
        /// error, and no runtime error — the control simply does nothing when clicked. This
        /// enumerates the document rather than asserting a hand-written list, so drift introduced
        /// by a future edit is caught by the same test.
        /// </summary>
        /// <remarks>
        /// Enumeration is over <see cref="XContainer.Descendants()"/>, which yields element nodes
        /// only. XML comment nodes are <c>XComment</c> and carry no attributes, so commented-out
        /// callbacks are excluded structurally with no regular expression. Descendants of an
        /// <see cref="XDocument"/> include the root <c>customUI</c> element, so its <c>onLoad</c>
        /// callback is covered without a special case.
        /// </remarks>
        [TestMethod]
        public void RibbonExplorerXml_EveryCallbackNameResolvesToAPublicRibbonViewerMethod()
        {
            // Arrange
            var document = LoadRibbonDocument();
            var declaredMethodNames = new HashSet<string>(
                GetViewerCallbackSurface().Select(method => method.Name),
                StringComparer.Ordinal
            );

            // Act: collect every distinct callback name bound anywhere in the document.
            var boundCallbackNames = document
                .Descendants()
                .SelectMany(element => element.Attributes())
                .Where(IsCallbackAttribute)
                .Select(attribute => attribute.Value)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var unresolved = boundCallbackNames
                .Where(name => !declaredMethodNames.Contains(name))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            // Assert: report every unresolved name in one message so a single run lists them all.
            unresolved
                .Should()
                .BeEmpty(
                    "every CustomUI callback name must resolve to a public instance method on "
                        + "RibbonViewer; these {0} of {1} bound names do not: {2}",
                    unresolved.Count,
                    boundCallbackNames.Count,
                    string.Join(", ", unresolved)
                );
        }

        /// <summary>
        /// Office invokes a <c>checkBox</c> action callback with the signature
        /// <c>void (IRibbonControl, bool)</c> and silently ignores a method whose shape does not
        /// match. This pins the exact shape whose mis-binding produced the #735 defect: the four
        /// Item Sort Settings check boxes were bound to names that resolved to nothing at all.
        /// </summary>
        [TestMethod]
        public void RibbonExplorerXml_CheckBoxOnActionCallbacksTakeControlAndPressedParameters()
        {
            // Arrange
            var document = LoadRibbonDocument();
            var viewerMethods = GetViewerCallbackSurface();
            var defects = new List<string>();

            // Act
            foreach (var checkBox in document.Descendants(CustomUiNs + "checkBox"))
            {
                var onAction = checkBox.Attribute("onAction");
                if (onAction == null)
                {
                    continue;
                }

                var controlId = checkBox.Attribute("id")?.Value ?? "(no id)";
                var candidates = viewerMethods
                    .Where(method =>
                        string.Equals(method.Name, onAction.Value, StringComparison.Ordinal)
                    )
                    .ToList();

                if (candidates.Count == 0)
                {
                    defects.Add(
                        $"{controlId}: '{onAction.Value}' resolves to no public instance method"
                    );
                    continue;
                }

                if (!candidates.Any(HasCheckBoxActionShape))
                {
                    defects.Add(
                        $"{controlId}: '{onAction.Value}' resolves but its signature is "
                            + $"{DescribeSignature(candidates[0])}"
                    );
                }
            }

            // Assert: report every offending callback in one message.
            defects
                .Should()
                .BeEmpty(
                    "every checkBox onAction callback must be void ({0}, bool); these {1} are not: {2}",
                    RibbonControlTypeName,
                    defects.Count,
                    string.Join("; ", defects)
                );
        }

        /// <summary>
        /// Reports whether a method has the Office check-box action shape
        /// <c>void (IRibbonControl, bool)</c>, comparing the first parameter by full type name.
        /// </summary>
        private static bool HasCheckBoxActionShape(MethodInfo method)
        {
            if (method.ReturnType != typeof(void))
            {
                return false;
            }

            var parameters = method.GetParameters();
            return parameters.Length == 2
                && parameters[0].ParameterType.FullName == RibbonControlTypeName
                && parameters[1].ParameterType == typeof(bool);
        }

        /// <summary>
        /// Renders a method signature for a failure message so the report names the shape that was
        /// found rather than only the shape that was expected.
        /// </summary>
        private static string DescribeSignature(MethodInfo method)
        {
            var parameters = string.Join(
                ", ",
                method.GetParameters().Select(parameter => parameter.ParameterType.FullName)
            );
            return $"{method.ReturnType.FullName} ({parameters})";
        }

        #endregion Issue #735 — callback name binding
    }
}
