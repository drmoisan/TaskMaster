using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tags.Test.Fakes;
using UtilitiesCS;

namespace Tags.Test
{
    [TestClass]
    public class TagControllerTests
    {
        [TestMethod]
        public void CheckboxClick_WithUnprefixedOptionAndPrefix_DoesNotReconstructMissingKey()
        {
            // Arrange: headless fake viewer + injected dialog/draw seams, no live form, no STA apartment.
            var fake = new FakeTagViewer();
            var options = new SortedDictionary<string, bool> { ["Build Team"] = false };
            var prompt = new Mock<IUserPrompt>(MockBehavior.Loose);
            var controller = new TagController(
                fake.Object,
                options,
                null,
                (IPrefix)CreateProgramPrefix(),
                prompt.Object,
                _ => { }
            );

            var optionCheckBox = fake.OptionControls.Single();

            try
            {
                optionCheckBox.Text.Should().Be("Build Team");

                // Act: raise the checkbox Click through the controller's CheckBoxController wiring.
                Action act = () => RaiseClick(optionCheckBox);

                // Assert: the option is toggled using its Tag, not a reconstructed prefixed key.
                act.Should().NotThrow();
                controller.GetSelections().Should().Equal("Build Team");
            }
            finally
            {
                optionCheckBox.Dispose();
            }
        }

        private static object CreateProgramPrefix()
        {
            var tagsAssembly = typeof(TagController).Assembly;
            var utilitiesAssembly = tagsAssembly
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .First(assembly =>
                    string.Equals(assembly.GetName().Name, "UtilitiesCS", StringComparison.Ordinal)
                );
            var outlookAssembly = tagsAssembly
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .First(assembly =>
                    string.Equals(
                        assembly.GetName().Name,
                        "Microsoft.Office.Interop.Outlook",
                        StringComparison.Ordinal
                    )
                );

            var prefixItemType = tagsAssembly.GetType("Tags.PrefixItem", throwOnError: true);
            var prefixTypeEnum = utilitiesAssembly.GetType(
                "UtilitiesCS.PrefixTypeEnum",
                throwOnError: true
            );
            var olCategoryColor = outlookAssembly.GetType(
                "Microsoft.Office.Interop.Outlook.OlCategoryColor",
                throwOnError: true
            );

            var programPrefixType = Enum.Parse(prefixTypeEnum, "Program");
            var noColor = Enum.Parse(olCategoryColor, "olCategoryColorNone");

            return Activator.CreateInstance(
                prefixItemType,
                programPrefixType,
                "Program",
                "TagProgram",
                noColor
            );
        }

        private static void RaiseClick(CheckBox checkBox)
        {
            var invokeOnClick = typeof(Control).GetMethod(
                "InvokeOnClick",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            invokeOnClick.Should().NotBeNull();
            invokeOnClick.Invoke(checkBox, new object[] { checkBox, EventArgs.Empty });
        }
    }
}
