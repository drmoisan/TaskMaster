using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Ribbon
{
    public partial class RibbonExplorerXmlTests
    {
        [TestMethod]
        public void RibbonExplorerXml_BuildTriageClassifierIsInSettingsFolderClassifierMenu()
        {
            // Arrange
            var document = LoadRibbonDocument();

            // Act
            var settingsMenu = document
                .Descendants(CustomUiNs + "menu")
                .SingleOrDefault(menu => menu.Attribute("id")?.Value == "Settings");
            var folderClassifierMenu = settingsMenu
                ?.Descendants(CustomUiNs + "menu")
                .SingleOrDefault(menu => menu.Attribute("id")?.Value == "FolderClassifier");
            var buildTriageButton = folderClassifierMenu
                ?.Elements(CustomUiNs + "button")
                .SingleOrDefault(button =>
                    button.Attribute("label")?.Value == "Build Triage Classifier"
                );

            // Assert
            buildTriageButton
                .Should()
                .NotBeNull(
                    "Build Triage Classifier must be available at TaskMaster > Settings > Folder Classifier"
                );
            buildTriageButton!
                .Attribute("onAction")
                .Should()
                .NotBeNull("the ribbon button must bind to a RibbonViewer callback");
            buildTriageButton!
                .Attribute("onAction")!
                .Value.Should()
                .Be("BuildTriageClassifier_Click");
        }
    }
}
