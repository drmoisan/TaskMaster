using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickFiler.Helper_Classes;
using QuickFiler.Interfaces;

namespace QuickFiler.Controllers.Tests
{
    /// <summary>
    /// Structural pin tests for the deliberate three-owner <see cref="IEmailMoveMonitor"/> topology
    /// described by issue #731 finding 1. Each of the three owning controllers constructs its own
    /// monitor instance: <c>EmailMoveMonitor.BeforeItemMove</c> dispatches at most one action per
    /// MailItem via <c>FirstOrDefault</c>, and <c>UnhookAll</c> is instance-scoped and clears the
    /// whole hook list, so collapsing the three instances into a shared singleton would silently
    /// drop sibling owners' actions and unhook them all on any one owner's teardown. These tests
    /// are forward guards against that collapse; they pass on the pre-change tree as well.
    /// </summary>
    [TestClass]
    public class QfcMoveMonitorTopologyTests
    {
        private const string MonitorInitializer = "= new EmailMoveMonitor();";

        /// <summary>
        /// Reads a source file from the QuickFiler Controllers directory, resolved relative to the
        /// test assembly's output directory. Modelled on the existing precedent at
        /// QfcHighConfidencePreFilterTests.ReadControllerSource.
        /// </summary>
        private static string ReadControllerSource(string fileName)
        {
            string path = Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "QuickFiler",
                    "Controllers",
                    fileName
                )
            );
            return File.ReadAllText(path);
        }

        /// <summary>
        /// Collapses every run of whitespace, including newlines, to a single space so that the
        /// assertions below are insensitive to formatter-driven line breaks and indentation.
        /// </summary>
        private static string NormalizeWhitespace(string text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            var builder = new System.Text.StringBuilder(text.Length);
            bool inWhitespaceRun = false;
            foreach (char character in text)
            {
                if (char.IsWhiteSpace(character))
                {
                    inWhitespaceRun = true;
                    continue;
                }

                if (inWhitespaceRun && builder.Length > 0)
                {
                    builder.Append(' ');
                }

                inWhitespaceRun = false;
                builder.Append(character);
            }

            return builder.ToString();
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            int count = 0;
            int index = haystack.IndexOf(needle, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = haystack.IndexOf(needle, index + needle.Length, StringComparison.Ordinal);
            }

            return count;
        }

        /// <summary>
        /// Each of the three owning controller sources declares exactly one EmailMoveMonitor field
        /// initializer. Scenario: read the three owner sources and count the initializer literal.
        /// Expected outcome: exactly one occurrence per file.
        /// </summary>
        [TestMethod]
        public void EachOwnerDeclaresExactlyOneEmailMoveMonitorInitializer()
        {
            // Arrange
            string[] ownerFiles = new[]
            {
                "QfcCollectionController.cs",
                "QfcDatamodel.cs",
                "QfcQueue.cs",
            };

            foreach (string ownerFile in ownerFiles)
            {
                // Act
                string normalized = NormalizeWhitespace(ReadControllerSource(ownerFile));
                int occurrences = CountOccurrences(normalized, MonitorInitializer);

                // Assert
                occurrences
                    .Should()
                    .Be(
                        1,
                        because: "issue #731 finding 1 pins one EmailMoveMonitor instance per owner, "
                            + "and {0} must declare exactly one initializer",
                        ownerFile
                    );
            }
        }

        /// <summary>
        /// No type in the QuickFiler assembly declares more than one IEmailMoveMonitor instance
        /// field, and exactly three types declare one. Scenario: reflect over the assembly that
        /// contains EmailMoveMonitor. Expected outcome: a per-type maximum of one and a total of
        /// three declaring types.
        /// </summary>
        [TestMethod]
        public void NoTypeDeclaresMoreThanOneEmailMoveMonitorField()
        {
            // Arrange
            Type[] types;
            try
            {
                types = typeof(EmailMoveMonitor).Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException loadException)
            {
                types = loadException.Types.Where(type => type != null).ToArray();
            }

            // Act
            var declaringTypes = new List<string>();
            foreach (Type type in types)
            {
                int fieldCount = type.GetFields(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    )
                    .Count(field =>
                        field.DeclaringType == type && field.FieldType == typeof(IEmailMoveMonitor)
                    );

                // Assert (per type)
                fieldCount
                    .Should()
                    .BeLessThanOrEqualTo(
                        1,
                        because: "issue #731 finding 1 pins at most one IEmailMoveMonitor field per "
                            + "type, and {0} declares {1}",
                        type.FullName,
                        fieldCount
                    );

                if (fieldCount == 1)
                {
                    declaringTypes.Add(type.FullName);
                }
            }

            // Assert (aggregate)
            declaringTypes
                .Should()
                .HaveCount(
                    3,
                    because: "issue #731 finding 1 pins the three-owner topology, and the declaring "
                        + "types observed were {0}",
                    string.Join(", ", declaringTypes)
                );
        }
    }
}
