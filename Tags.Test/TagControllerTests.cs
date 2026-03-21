using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tags;

namespace Tags.Test
{
    [TestClass]
    public class TagControllerTests
    {
        [TestMethod]
        [STAThread]
        public void CheckboxClick_WithUnprefixedOptionAndPrefix_DoesNotReconstructMissingKey()
        {
            var viewer = new TagViewer();
            var options = new SortedDictionary<string, bool> { ["Build Team"] = false };

            using (viewer)
            {
                var controller = (TagController)
                    Activator.CreateInstance(
                        typeof(TagController),
                        viewer,
                        options,
                        null,
                        CreateProgramPrefix()
                    );
                var optionCheckBox = FindOptionCheckBox(viewer);

                optionCheckBox.Text.Should().Be("Build Team");

                Action act = () => RaiseClick(optionCheckBox);

                act.Should().NotThrow();
                controller.GetSelections().Should().Equal("Build Team");
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

        private static CheckBox FindOptionCheckBox(TagViewer viewer)
        {
            var optionsPanel = FindControls<Panel>(viewer)
                .Single(control =>
                    string.Equals(control.Name, "L1v2L2_OptionsPanel", StringComparison.Ordinal)
                );

            return optionsPanel.Controls.OfType<CheckBox>().Single();
        }

        private static IEnumerable<TControl> FindControls<TControl>(Control root)
            where TControl : Control
        {
            if (root is TControl matched)
            {
                yield return matched;
            }

            foreach (Control child in root.Controls)
            {
                foreach (var descendant in FindControls<TControl>(child))
                {
                    yield return descendant;
                }
            }
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
