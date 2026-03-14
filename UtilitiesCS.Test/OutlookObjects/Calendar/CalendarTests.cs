using System.Collections;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Calendar
{
    [TestClass]
    public class CalendarTests
    {
        [TestMethod]
        public void GetCalendar_WhenCalendarNameMatches_ReturnsMatchingFolder()
        {
            // Arrange
            var matchingCalendar = CreateFolder("Team Calendar");
            var otherCalendar = CreateFolder("Personal");
            var session = CreateSession(otherCalendar.Object, matchingCalendar.Object);

            // Act
            var result = UtilitiesCS.Calendar.GetCalendar("Team Calendar", session.Object);

            // Assert
            result.Should().BeSameAs(matchingCalendar.Object);
        }

        [TestMethod]
        public void GetCalendar_WhenNoCalendarNameMatches_ReturnsNull()
        {
            // Arrange
            var firstCalendar = CreateFolder("Personal");
            var secondCalendar = CreateFolder("Archive");
            var session = CreateSession(firstCalendar.Object, secondCalendar.Object);

            // Act
            var result = UtilitiesCS.Calendar.GetCalendar("Team Calendar", session.Object);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetCalendar_WhenCalendarNameIsNull_MatchesFolderWithNullName()
        {
            // Arrange
            var unnamedCalendar = CreateFolder(null);
            var namedCalendar = CreateFolder("Named");
            var session = CreateSession(namedCalendar.Object, unnamedCalendar.Object);

            // Act
            var result = UtilitiesCS.Calendar.GetCalendar(null, session.Object);

            // Assert
            result.Should().BeSameAs(unnamedCalendar.Object);
        }

        private static Mock<NameSpace> CreateSession(params OutlookFolder[] calendars)
        {
            var calendarRoot = new Mock<OutlookFolder>();
            var folders = new Mock<Folders>();
            var session = new Mock<NameSpace>();
            var collection = new ArrayList(calendars);

            folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());
            calendarRoot.SetupGet(x => x.Folders).Returns(folders.Object);
            session.Setup(x => x.GetDefaultFolder(OlDefaultFolders.olFolderCalendar)).Returns(calendarRoot.Object);

            return session;
        }

        private static Mock<OutlookFolder> CreateFolder(string name)
        {
            var folder = new Mock<OutlookFolder>();
            folder.SetupGet(x => x.Name).Returns(name);
            return folder;
        }
    }
}