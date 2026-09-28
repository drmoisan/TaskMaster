using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Bootstrap
{
    /// <summary>
    /// Reflection-shape regression guards for the eager installation point and for the
    /// declarative binding hardening.
    /// </summary>
    /// <remarks>
    /// Both assertions are on structure rather than on a prose phrase, so neither can be
    /// broken by reformatting. The configuration assertion reads the deployed configuration
    /// image in the add-in's output directory, which is the file the CLR actually consults,
    /// rather than the source file it is copied from.
    /// </remarks>
    [TestClass]
    public class AddInEagerInstallShapeTests
    {
        private const string AddInConfigFileName = "TaskMaster.dll.config";
        private const string NetstandardIdentityName = "netstandard";
        private const string NetstandardPublicKeyToken = "cc7b13ffcd2ddd51";
        private const string NeutralCulture = "neutral";
        private const string ExpectedOldVersion = "0.0.0.0-2.1.0.0";
        private const string ExpectedNewVersion = "2.0.0.0";

        /// <summary>
        /// Guards the eager install point against deletion. Declaring an explicit static
        /// constructor clears <c>beforefieldinit</c>, which is what makes the installation
        /// ordering precise rather than "at or before first use".
        /// </summary>
        [TestMethod]
        public void ThisAddIn_HasExplicitStaticConstructor()
        {
            // Arrange
            Type addIn = typeof(global::TaskMaster.ThisAddIn);

            // Act
            bool beforeFieldInit = addIn.Attributes.HasFlag(TypeAttributes.BeforeFieldInit);
            ConstructorInfo typeInitializer = addIn.TypeInitializer;

            // Assert
            beforeFieldInit
                .Should()
                .BeFalse(
                    "an explicit static constructor clears beforefieldinit, which is what "
                        + "pins the installer to run before the VSTO runtime constructs the "
                        + "add-in instance"
                );
            typeInitializer
                .Should()
                .NotBeNull("the eager installation point is the type initializer");
        }

        /// <summary>
        /// Guards the declarative hardening. Asserts on element and attribute values, never
        /// on a text phrase.
        /// </summary>
        [TestMethod]
        public void AppConfig_DeclaresNetstandardRedirect()
        {
            // Arrange
            string configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                AddInConfigFileName
            );
            File.Exists(configPath)
                .Should()
                .BeTrue(
                    "the deployed add-in configuration image must be present in the test "
                        + "output directory; its absence is a build-configuration problem "
                        + "rather than a binding defect"
                );

            // Act
            XDocument document = XDocument.Load(configPath);
            XElement redirect = document
                .Descendants()
                .Where(e => e.Name.LocalName == "dependentAssembly")
                .Select(e => new
                {
                    Identity = e.Elements()
                        .FirstOrDefault(c => c.Name.LocalName == "assemblyIdentity"),
                    Binding = e.Elements()
                        .FirstOrDefault(c => c.Name.LocalName == "bindingRedirect"),
                })
                .Where(pair =>
                    pair.Identity != null
                    && string.Equals(
                        (string)pair.Identity.Attribute("name"),
                        NetstandardIdentityName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Select(pair => pair.Binding)
                .FirstOrDefault();

            XElement identity = document
                .Descendants()
                .FirstOrDefault(e =>
                    e.Name.LocalName == "assemblyIdentity"
                    && string.Equals(
                        (string)e.Attribute("name"),
                        NetstandardIdentityName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            // Assert
            identity.Should().NotBeNull("the netstandard identity must be declared");
            ((string)identity.Attribute("publicKeyToken")).Should().Be(NetstandardPublicKeyToken);
            ((string)identity.Attribute("culture")).Should().Be(NeutralCulture);

            redirect.Should().NotBeNull("the netstandard identity must carry a redirect");
            ((string)redirect.Attribute("oldVersion")).Should().Be(ExpectedOldVersion);
            ((string)redirect.Attribute("newVersion")).Should().Be(ExpectedNewVersion);
        }
    }
}
