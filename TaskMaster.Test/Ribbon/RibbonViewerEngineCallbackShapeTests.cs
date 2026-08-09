using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for issues #505, #506, and #518: the Office callback signatures of the
    /// Spam Config and Triage Config toggle checkboxes, and the pre-<c>SetGlobals</c> degradation
    /// of the two <c>getPressed</c> callbacks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// VSTO binds ribbon callbacks by name and signature and silently ignores a mismatch: the code
    /// compiles, Office queries nothing, and no error is reported. A signature defect is therefore
    /// invisible at runtime, which is why these reflection pins exist — they convert a silent
    /// binding failure into a failing build.
    /// </para>
    /// <para>
    /// Every parameter type is compared by <see cref="Type.FullName"/> rather than with
    /// <c>typeof</c>. <c>TaskMaster.Test.csproj</c> carries no reference to the Office
    /// (Microsoft.Office.Core) primary interop assembly, and a legacy non-SDK
    /// <c>ProjectReference</c> does not flow that reference to the compiler. <c>office.dll</c> is
    /// present in the test output directory and in the GAC, so runtime reflection resolves the
    /// type at test time.
    /// </para>
    /// <para>
    /// The callback names are read from the embedded ribbon document rather than hard-coded, so a
    /// rename in either the XML or the viewer is caught as a resolution failure.
    /// </para>
    /// </remarks>
    [TestClass]
    public class RibbonViewerEngineCallbackShapeTests
    {
        private const string ResourceName = "TaskMaster.Ribbon.RibbonExplorer.xml";

        private const string RibbonControlTypeName = "Microsoft.Office.Core.IRibbonControl";

        /// <summary>
        /// The two Office <c>checkBox</c> ids whose callbacks this fixture pins. They are
        /// deliberately outside <c>EngineCommandCatalog</c>: catalog membership implies
        /// readiness-gated <c>getEnabled</c> semantics, which is the wrong predicate for a
        /// configuration toggle.
        /// </summary>
        private static readonly string[] ToggleControlIds =
        {
            "SpamBayesEnabledToggle",
            "TriageEnabledToggle",
        };

        /// <summary>
        /// The two save-info command handlers whose shape changes with this fix. Their method
        /// names are pinned by <c>onAction</c> in the ribbon XML and do not match their control
        /// ids (<c>GetSaveState</c> / <c>TriageGetSaveState</c>).
        /// </summary>
        private static readonly string[] ShowSaveInfoHandlerNames =
        {
            "GetSaveLocation_Click",
            "TriageGetSaveLocation_Click",
        };

        /// <summary>
        /// The <c>getPressed</c> callback of each toggle checkbox must expose the exact Office
        /// contract: a public instance method returning <see cref="bool"/> with a single
        /// <c>IRibbonControl</c> parameter. An <c>async Task&lt;bool&gt;</c> declaration is the
        /// #505 defect and never binds.
        /// </summary>
        [TestMethod]
        public void ToggleGetPressedCallbacks_MatchOfficeCheckBoxGetPressedSignature()
        {
            // Arrange
            var document = LoadRibbonDocument();

            foreach (var controlId in ToggleControlIds)
            {
                // Act
                var callbackName = ResolveCallbackName(document, controlId, "getPressed");
                var callback = GetPublicInstanceMethod(callbackName);

                // Assert
                callback
                    .Should()
                    .NotBeNull(
                        "'{0}' is declared as the getPressed callback of '{1}' and must be a public instance method on RibbonViewer",
                        callbackName,
                        controlId
                    );
                callback!
                    .ReturnType.Should()
                    .Be<bool>(
                        "Office requires a synchronous bool getPressed callback for '{0}'; an async Task<bool> is silently ignored",
                        controlId
                    );

                var parameters = callback.GetParameters();
                parameters
                    .Should()
                    .ContainSingle(
                        "the Office getPressed callback for '{0}' takes exactly one parameter",
                        controlId
                    );
                parameters[0]
                    .ParameterType.FullName.Should()
                    .Be(
                        RibbonControlTypeName,
                        "the single parameter of '{0}' must be the Office IRibbonControl",
                        callbackName
                    );
            }
        }

        /// <summary>
        /// The <c>onAction</c> callback of each toggle checkbox must expose the Office
        /// <c>checkBox</c> contract: <c>void (IRibbonControl, bool)</c>. This shape is already
        /// correct before the fix and is pinned so the #506 rewrite cannot regress it.
        /// </summary>
        [TestMethod]
        public void ToggleOnActionCallbacks_MatchOfficeCheckBoxOnActionSignature()
        {
            // Arrange
            var document = LoadRibbonDocument();

            foreach (var controlId in ToggleControlIds)
            {
                // Act
                var callbackName = ResolveCallbackName(document, controlId, "onAction");
                var callback = GetPublicInstanceMethod(callbackName);

                // Assert
                callback
                    .Should()
                    .NotBeNull(
                        "'{0}' is declared as the onAction callback of '{1}' and must be a public instance method on RibbonViewer",
                        callbackName,
                        controlId
                    );
                AssertCheckBoxOnActionParameters(callback!, callbackName);
            }
        }

        /// <summary>
        /// Invoked before <c>SetGlobals</c> has assigned <c>Globals</c>, each <c>getPressed</c>
        /// callback must degrade to an unchecked box rather than raising
        /// <see cref="NullReferenceException"/> from a null <c>Engines</c> dereference (#518).
        /// </summary>
        /// <remarks>
        /// The callback is invoked by reflection so this test compiles unchanged across the
        /// signature change. While the pre-fix declaration returns <c>Task&lt;bool&gt;</c> the
        /// fault is captured inside the returned task, so the task must be awaited for the defect
        /// to surface; once the signature is synchronous the returned value is asserted directly.
        /// </remarks>
        [TestMethod]
        public async Task GetPressedCallbacks_BeforeSetGlobals_ReturnFalseWithoutThrowing()
        {
            // Arrange
            var document = LoadRibbonDocument();

            foreach (var controlId in ToggleControlIds)
            {
                var callbackName = ResolveCallbackName(document, controlId, "getPressed");
                var callback = GetPublicInstanceMethod(callbackName);
                callback
                    .Should()
                    .NotBeNull("'{0}' must exist to be invoked", callbackName);

                // A bare RibbonController leaves Globals unassigned, so RibbonController.Engines
                // yields null — exactly the pre-SetGlobals state the ribbon can be polled in.
                var viewer = new RibbonViewer(new RibbonController());
                object invocationResult = null;

                // Act
                Func<Task> act = async () =>
                {
                    invocationResult = callback!.Invoke(viewer, new object[] { null });
                    if (invocationResult is Task pending)
                    {
                        await pending.ConfigureAwait(false);
                    }
                };

                // Assert
                await act.Should()
                    .NotThrowAsync(
                        "'{0}' must degrade to an unchecked box before SetGlobals rather than dereferencing a null Engines",
                        callbackName
                    );

                if (callback!.ReturnType == typeof(bool))
                {
                    invocationResult
                        .Should()
                        .Be(
                            false,
                            "a never-primed engine key must report the toggle as unchecked for '{0}'",
                            callbackName
                        );
                }
            }
        }

        /// <summary>
        /// Both toggle click handlers must be awaited <c>async void</c> handlers rather than
        /// fire-and-forget <c>void</c> methods that discard the toggle task (#506). The
        /// compiler-emitted <see cref="AsyncStateMachineAttribute"/> is the observable proof that
        /// the handler awaits.
        /// </summary>
        [TestMethod]
        public void ToggleClickHandlers_AreAsyncVoidAwaitedShape()
        {
            // Arrange
            var document = LoadRibbonDocument();

            foreach (var controlId in ToggleControlIds)
            {
                // Act
                var handlerName = ResolveCallbackName(document, controlId, "onAction");
                var handler = GetPublicInstanceMethod(handlerName);

                // Assert
                handler.Should().NotBeNull("'{0}' must exist on RibbonViewer", handlerName);
                AssertCheckBoxOnActionParameters(handler!, handlerName);
                AssertAwaitedAsyncVoidShape(handler!, handlerName);
            }
        }

        /// <summary>
        /// The two <c>ShowSaveInfo</c> command handlers must also become awaited <c>async void</c>
        /// handlers, matching the sibling <c>*SaveNetwork_Click</c> / <c>*SaveLocal_Click</c>
        /// shape, because their engine work is now deferred into a gated lambda (#518).
        /// </summary>
        [TestMethod]
        public void ShowSaveInfoHandlers_AreAsyncVoidAwaitedShape()
        {
            foreach (var handlerName in ShowSaveInfoHandlerNames)
            {
                // Act
                var handler = GetPublicInstanceMethod(handlerName);

                // Assert
                handler.Should().NotBeNull("'{0}' must exist on RibbonViewer", handlerName);

                var parameters = handler!.GetParameters();
                parameters
                    .Should()
                    .ContainSingle("'{0}' takes exactly one parameter", handlerName);
                parameters[0]
                    .ParameterType.FullName.Should()
                    .Be(
                        RibbonControlTypeName,
                        "the single parameter of '{0}' must be the Office IRibbonControl",
                        handlerName
                    );
                AssertAwaitedAsyncVoidShape(handler, handlerName);
            }
        }

        /// <summary>
        /// Asserts the Office <c>checkBox</c> <c>onAction</c> parameter list:
        /// <c>(IRibbonControl, bool)</c>, with a <c>void</c> return.
        /// </summary>
        private static void AssertCheckBoxOnActionParameters(MethodInfo handler, string handlerName)
        {
            handler
                .ReturnType.Should()
                .Be(
                    typeof(void),
                    "Office requires a void onAction callback for a checkBox; '{0}' must not return a value",
                    handlerName
                );

            var parameters = handler.GetParameters();
            parameters
                .Should()
                .HaveCount(
                    2,
                    "the Office checkBox onAction callback '{0}' takes (IRibbonControl, bool)",
                    handlerName
                );
            parameters[0]
                .ParameterType.FullName.Should()
                .Be(
                    RibbonControlTypeName,
                    "the first parameter of '{0}' must be the Office IRibbonControl",
                    handlerName
                );
            parameters[1]
                .ParameterType.Should()
                .Be<bool>(
                    "the second parameter of '{0}' must be the pressed state",
                    handlerName
                );
        }

        /// <summary>
        /// Asserts the awaited <c>async void</c> shape: a <c>void</c> return plus the
        /// compiler-emitted <see cref="AsyncStateMachineAttribute"/>, which is present only when
        /// the method body actually contains an <c>await</c>.
        /// </summary>
        private static void AssertAwaitedAsyncVoidShape(MethodInfo handler, string handlerName)
        {
            handler
                .ReturnType.Should()
                .Be(
                    typeof(void),
                    "'{0}' is an Office callback and must return void",
                    handlerName
                );
            handler
                .GetCustomAttribute<AsyncStateMachineAttribute>()
                .Should()
                .NotBeNull(
                    "'{0}' must await its work rather than discarding the returned Task; the compiler emits AsyncStateMachineAttribute only for an async body",
                    handlerName
                );
        }

        /// <summary>
        /// Resolves a callback name from a named control's attribute in the embedded ribbon
        /// document, failing with a specific message when the control or the attribute is absent.
        /// </summary>
        private static string ResolveCallbackName(
            XDocument document,
            string controlId,
            string attributeName
        )
        {
            var element = document
                .Descendants()
                .SingleOrDefault(candidate => candidate.Attribute("id")?.Value == controlId);
            element
                .Should()
                .NotBeNull("control '{0}' must exist in the ribbon XML", controlId);

            var attribute = element!.Attribute(attributeName);
            attribute
                .Should()
                .NotBeNull(
                    "control '{0}' must declare a '{1}' callback",
                    controlId,
                    attributeName
                );

            return attribute!.Value;
        }

        /// <summary>
        /// Looks up a public instance method on <c>RibbonViewer</c> by the exact name Office would
        /// bind against.
        /// </summary>
        private static MethodInfo GetPublicInstanceMethod(string methodName)
        {
            return typeof(RibbonViewer).GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.Instance
            );
        }

        /// <summary>
        /// Loads the embedded Explorer ribbon document from the production assembly, so the tests
        /// read the same bytes Outlook does.
        /// </summary>
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
    }
}
