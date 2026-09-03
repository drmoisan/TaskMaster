using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SVGControl.Test
{
    /// <summary>
    /// Structural guard: no live WinForms window type may be compiled into this unit-test
    /// assembly. Reflection is over type metadata only; nothing is instantiated.
    /// </summary>
    [TestClass]
    public class NoLiveFormInTestAssemblyTests
    {
        [TestMethod]
        public void ExecutingAssembly_ContainsNoFormDerivedType()
        {
            // Arrange - metadata only; scoped to the executing assembly, never a referenced one.
            Type formType = typeof(System.Windows.Forms.Form);
            Assembly executing = Assembly.GetExecutingAssembly();

            // Act
            string[] formDerivedTypeNames = GetLoadableTypes(executing)
                .Where(candidate => formType.IsAssignableFrom(candidate))
                .Select(candidate => candidate.FullName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            // Assert
            formDerivedTypeNames
                .Should()
                .BeEmpty(
                    "a unit-test assembly must not compile a live System.Windows.Forms.Form "
                        + "type, but found: "
                        + string.Join(", ", formDerivedTypeNames)
                );
        }

        // Reflection over a large test assembly can hit a single type whose dependencies fail to
        // resolve, and GetTypes then throws for the whole assembly. That would leave this guard
        // permanently red for a reason unrelated to what it measures, so the loaded subset carried
        // on the exception is used instead; its null entries are the types that did not load.
        private static Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(candidate => candidate != null).ToArray();
            }
        }
    }
}
