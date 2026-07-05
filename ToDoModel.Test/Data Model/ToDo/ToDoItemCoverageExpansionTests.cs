using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;

namespace ToDoModel.Test
{
    [TestClass]
    public class ToDoItemCoverageExpansionTests
    {
        private static readonly DateTime CreatedAt = new DateTime(2026, 7, 4, 9, 30, 0);
        private static readonly DateTime InitialStart = new DateTime(2026, 7, 5, 8, 0, 0);
        private static readonly DateTime InitialDue = new DateTime(2026, 7, 6, 17, 0, 0);

        [TestMethod]
        public void ConstructorWithString_LoadsIdentifierDefaultsWithoutOutlookAccess()
        {
            var item = new ToDoItem("A1B2");

            item.ToDoID.Should().Be("A1B2");
            item.ActiveBranch.Should().BeFalse();
            item.ExpandChildren.Should().BeEmpty();
            item.ExpandChildrenState.Should().BeEmpty();
            item.MetaTaskLvl.Should().BeEmpty();
            item.MetaTaskSubject.Should().BeEmpty();
        }

        [TestMethod]
        public void ConstructorWithFlaggableItem_LoadsFieldsAndCustomState()
        {
            var flaggable = CreateFlaggableItem(
                activeBranch: true,
                expandChildren: "+",
                expandChildrenState: "-",
                visibleTreeState: 5
            );

            var item = new ToDoItem(flaggable.Object);

            item.TaskSubject.Should().Be("Original task");
            item.Priority.Should().Be(OlImportance.olImportanceNormal);
            item.TaskCreateDate.Should().Be(CreatedAt);
            item.StartDate.Should().Be(InitialStart);
            item.ActiveBranch.Should().BeTrue();
            item.EC2.Should().BeTrue();
            item.ExpandChildren.Should().Be("-");
            item.ExpandChildrenState.Should().Be("-");
            item.VisibleTreeState.Should().Be(5);
            item.get_VisibleTreeStateLVL(1).Should().BeTrue();
            item.get_VisibleTreeStateLVL(2).Should().BeFalse();
            item.get_VisibleTreeStateLVL(3).Should().BeTrue();
        }

        [TestMethod]
        public void SettableProperties_UpdateCachedStateAndSaveBackToFlaggableItem()
        {
            var flaggable = CreateFlaggableItem();
            var item = new ToDoItem(flaggable.Object);
            var newDue = new DateTime(2026, 8, 1, 12, 0, 0);

            item.TaskSubject = "Updated task";
            item.Priority = OlImportance.olImportanceHigh;
            item.Complete = true;
            item.FlagAsTask = true;
            item.DueDate = newDue;
            item.TotalWork = 45;
            item.ReminderTime = new DateTime(2026, 7, 31, 9, 0, 0);

            item.TaskSubject.Should().Be("Updated task");
            item.Priority.Should().Be(OlImportance.olImportanceHigh);
            item.Complete.Should().BeTrue();
            item.FlagAsTask.Should().BeTrue();
            item.DueDate.Should().Be(newDue);
            item.TotalWork.Should().Be(45);
            flaggable.Object.TaskSubject.Should().Be("Updated task");
            flaggable.Object.Importance.Should().Be(OlImportance.olImportanceHigh);
            flaggable.Object.Complete.Should().BeTrue();
            flaggable.Object.FlagAsTask.Should().BeTrue();
            flaggable.Object.DueDate.Should().Be(newDue);
            flaggable.Object.TotalWork.Should().Be(45);
            flaggable.Verify(x => x.Save(), Times.AtLeast(6));
        }

        [TestMethod]
        public void DateAndStatusBoundaries_UseDefaultsAndBooleanTransitions()
        {
            var flaggable = CreateFlaggableItem(
                dueDate: DateTime.MinValue,
                complete: false,
                flagAsTask: false
            );
            var item = new ToDoItem(flaggable.Object);
            var boundaryDueDate = DateTime.Parse("1/1/4501");

            item.DueDate.Should().Be(DateTime.MinValue);
            item.Complete.Should().BeFalse();
            item.FlagAsTask.Should().BeFalse();

            item.DueDate = boundaryDueDate;
            item.Complete = true;
            item.FlagAsTask = true;

            item.DueDate.Should().Be(boundaryDueDate);
            item.Complete.Should().BeTrue();
            item.FlagAsTask.Should().BeTrue();
        }

        [TestMethod]
        public void VisibilityAndExpandedStateTransitions_SetBitsAndClearChangeMarker()
        {
            var flaggable = CreateFlaggableItem(
                activeBranch: false,
                expandChildren: "",
                expandChildrenState: "",
                visibleTreeState: 0
            );
            var item = new ToDoItem(flaggable.Object);

            item.get_VisibleTreeStateLVL(2).Should().BeFalse();
            item.set_VisibleTreeStateLVL(2, true);
            item.get_VisibleTreeStateLVL(2).Should().BeTrue();
            item.VisibleTreeState.Should().Be(2);
            item.set_VisibleTreeStateLVL(2, false);
            item.get_VisibleTreeStateLVL(2).Should().BeFalse();
            item.VisibleTreeState.Should().Be(0);

            item.ExpandChildren = "+";
            item.ExpandChildrenState = "-";
            item.EC_Change.Should().BeTrue();
            item.EC_Change = false;
            item.ExpandChildrenState.Should().Be("+");
            item.EC_Change.Should().BeFalse();
        }

        [TestMethod]
        public void CloneAndEquals_UseReferenceEqualityForDistinctInstances()
        {
            var item = new ToDoItem(CreateFlaggableItem().Object);

            var clone = (ToDoItem)item.Clone();

            clone.Should().NotBeSameAs(item);
            clone.TaskSubject.Should().Be(item.TaskSubject);
            item.Equals(item).Should().BeTrue();
            item.Equals(clone).Should().BeFalse();
            item.Equals(null).Should().BeFalse();
        }

        [TestMethod]
        public void ConstructorWithNullFlaggableItem_ThrowsBeforeOutlookAccess()
        {
            System.Action act = () => _ = new ToDoItem((IOutlookItemFlaggable)null);

            act.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public async Task ForceSave_WhenReadOnlyPersistsCachedState_RestoresReadOnlyState()
        {
            var flaggable = CreateFlaggableItem();
            var item = new ToDoItem(flaggable.Object)
            {
                ReadOnly = true,
                TaskSubject = "Cached task",
                Priority = OlImportance.olImportanceHigh,
                StartDate = new DateTime(2026, 8, 2, 8, 15, 0),
                DueDate = new DateTime(2026, 8, 4, 17, 30, 0),
                Complete = true,
                FlagAsTask = true,
                TotalWork = 75,
                ActiveBranch = false,
                ExpandChildren = "-",
                ExpandChildrenState = "+",
                EC2 = false,
                VisibleTreeState = 11,
                MetaTaskLvl = "4",
                MetaTaskSubject = "Cached parent",
                ToDoID = "PR0102",
            };

            await item.ForceSave();

            item.ReadOnly.Should().BeTrue();
            flaggable.Object.TaskSubject.Should().Be("Cached task");
            flaggable.Object.Importance.Should().Be(OlImportance.olImportanceHigh);
            flaggable.Object.Complete.Should().BeTrue();
            flaggable.Object.TotalWork.Should().Be(75);
            UserPropertyValue(flaggable.Object, "AB").Should().Be(false);
            UserPropertyValue(flaggable.Object, "EC").Should().Be(string.Empty);
            UserPropertyValue(flaggable.Object, "EcState").Should().Be(string.Empty);
            UserPropertyValue(flaggable.Object, "EC2").Should().Be(false);
            UserPropertyValue(flaggable.Object, "VTS").Should().Be(11);
            UserPropertyValue(flaggable.Object, "Meta Task Level").Should().Be("4");
            UserPropertyValue(flaggable.Object, "Meta Task Subject").Should().Be("Cached parent");
            UserPropertyValue(flaggable.Object, "ToDoID").Should().Be("PR0102");
            flaggable.Verify(x => x.Save(), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task WriteFlagsBatch_WithTranslatorValues_UpdatesUserDefinedFields()
        {
            var flaggable = CreateFlaggableItem();
            var item = new ToDoItem(flaggable.Object) { ReadOnly = true };

            item.Context.AsStringNoPrefix = "Office";
            item.People.AsStringNoPrefix = "Alex";
            item.Topics.AsStringNoPrefix = "Planning";
            item.KB.AsStringNoPrefix = "KB42";
            item.Program.AsStringNoPrefix = "Operations";
            item.Projects.AsStringNoPrefix = "Coverage";
            item.ReadOnly = false;

            await item.WriteFlagsBatch();

            UserPropertyValue(flaggable.Object, "TagContext")
                .Should()
                .BeEquivalentTo(new[] { "Office" });
            UserPropertyValue(flaggable.Object, "TagPeople")
                .Should()
                .BeEquivalentTo(new[] { "Alex" });
            UserPropertyValue(flaggable.Object, "TagTopic")
                .Should()
                .BeEquivalentTo(new[] { "Planning" });
            UserPropertyValue(flaggable.Object, "TagProgram").Should().Be("Operations");
            UserPropertyValue(flaggable.Object, "TagProject")
                .Should()
                .BeEquivalentTo(new[] { "Coverage" });
        }

        [TestMethod]
        public async Task AutoCodeIdAsync_WithNullAndEmptyInputs_ReturnsWithoutIdListAccess()
        {
            var item = new ToDoItem(CreateFlaggableItem().Object);
            var idList = new Mock<IIDList>(MockBehavior.Strict);
            var projectData = new Mock<IProjectData>(MockBehavior.Strict);

            item.IdList = idList.Object;
            await item.AutoCodeIdAsync("PR01", "Coverage");
            item.ProjectData = projectData.Object;
            await item.AutoCodeIdAsync("", "Coverage");
            await item.AutoCodeIdAsync("PR01", "");

            idList.Verify(x => x.GetNextToDoID(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AutoCodeIdAsync_WithEmptyAndExistingIds_UsesBranchSpecificTransitions()
        {
            var emptyIdFlaggable = CreateFlaggableItem();
            var emptyIdItem = new ToDoItem(emptyIdFlaggable.Object)
            {
                ToDoID = "",
                ProjectData = Mock.Of<IProjectData>(),
            };
            var idList = new Mock<IIDList>(MockBehavior.Strict);
            idList.Setup(x => x.GetNextToDoID("PR0100")).Returns("PR0101");
            emptyIdItem.IdList = idList.Object;

            await emptyIdItem.AutoCodeIdAsync("PR01", "Coverage");

            emptyIdItem.ToDoID.Should().Be("PR0101");
            emptyIdItem.EC2.Should().BeTrue();
            idList.Verify(x => x.GetNextToDoID("PR0100"), Times.Once);

            var projectIdItem = new ToDoItem(CreateFlaggableItem().Object)
            {
                ToDoID = "PR01",
                ProjectData = Mock.Of<IProjectData>(),
                IdList = idList.Object,
            };
            await projectIdItem.AutoCodeIdAsync("PR01", "Coverage");

            var existingChildItem = new ToDoItem(CreateFlaggableItem().Object)
            {
                ToDoID = "PR0109",
                ProjectData = Mock.Of<IProjectData>(),
                IdList = idList.Object,
            };
            await existingChildItem.AutoCodeIdAsync("PR01", "Coverage");

            idList.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void ReadOnlyPropertyTransitions_UpdateCachedStateWithoutWritingFields()
        {
            var flaggable = CreateFlaggableItem(
                activeBranch: true,
                expandChildren: "",
                visibleTreeState: 0
            );
            var item = new ToDoItem(flaggable.Object) { ReadOnly = true };

            flaggable.Invocations.Clear();
            item.Today = true;
            item.Bullpin = true;
            item.ActiveBranch = false;
            item.EC3 = true;
            item.EC2 = false;
            item.ExpandChildren = "-";
            item.ExpandChildrenState = "+";
            item.MetaTaskLvl = "5";
            item.MetaTaskSubject = "Read-only parent";

            item.Today.Should().BeTrue();
            item.Bullpin.Should().BeTrue();
            item.ActiveBranch.Should().BeFalse();
            item.EC3.Should().BeTrue();
            item.EC2.Should().BeTrue();
            item.ExpandChildren.Should().Be("-");
            item.ExpandChildrenState.Should().Be("+");
            item.MetaTaskLvl.Should().Be("5");
            item.MetaTaskSubject.Should().Be("Read-only parent");
            flaggable.Verify(x => x.Save(), Times.Never);
        }

        private static Mock<IOutlookItemFlaggable> CreateFlaggableItem(
            bool activeBranch = true,
            bool ec2 = true,
            string expandChildren = "EcVal",
            string expandChildrenState = "EcStateVal",
            int visibleTreeState = 63,
            DateTime? dueDate = null,
            bool complete = false,
            bool flagAsTask = false
        )
        {
            var mock = new Mock<IOutlookItemFlaggable>(MockBehavior.Loose);
            mock.SetupAllProperties();

            mock.Object.Categories = "Category1,Category2";
            mock.Object.Importance = OlImportance.olImportanceNormal;
            mock.Object.TaskSubject = "Original task";
            mock.Object.TaskStartDate = InitialStart;
            mock.Object.DueDate = dueDate ?? InitialDue;
            mock.Object.Complete = complete;
            mock.Object.FlagAsTask = flagAsTask;
            mock.Object.TotalWork = 10;
            mock.Object.ReminderTime = InitialStart;

            mock.SetupGet(x => x.CreationTime).Returns(CreatedAt);
            mock.SetupGet(x => x.EntryID).Returns("entry-coverage-1");
            mock.SetupGet(x => x.InnerObject).Returns(new object());
            mock.SetupGet(x => x.ItemType).Returns(typeof(object));
            mock.SetupGet(x => x.UserProperties)
                .Returns(
                    CreateUserProperties(
                        activeBranch,
                        ec2,
                        expandChildren,
                        expandChildrenState,
                        visibleTreeState
                    )
                );

            return mock;
        }

        private static object UserPropertyValue(IOutlookItemFlaggable flaggableItem, string name)
        {
            return flaggableItem.UserProperties.Find(name).Value;
        }

        private static UserProperties CreateUserProperties(
            bool activeBranch,
            bool ec2,
            string expandChildren,
            string expandChildrenState,
            int visibleTreeState
        )
        {
            var properties = new List<UserProperty>
            {
                MockProperty("AB", activeBranch, OlUserPropertyType.olYesNo),
                MockProperty("EC2", ec2, OlUserPropertyType.olYesNo),
                MockProperty("EC", expandChildren, OlUserPropertyType.olText),
                MockProperty("EcState", expandChildrenState, OlUserPropertyType.olText),
                MockProperty("VTS", visibleTreeState, OlUserPropertyType.olInteger),
                MockProperty("Meta Task Level", "2", OlUserPropertyType.olText),
                MockProperty("Meta Task Subject", "Parent task", OlUserPropertyType.olText),
                MockProperty("ToDoID", "A1B2", OlUserPropertyType.olText),
                MockProperty("TagContext", Array.Empty<string>(), OlUserPropertyType.olKeywords),
                MockProperty("TagPeople", Array.Empty<string>(), OlUserPropertyType.olKeywords),
                MockProperty("TagProject", Array.Empty<string>(), OlUserPropertyType.olKeywords),
                MockProperty("TagProgram", string.Empty, OlUserPropertyType.olText),
                MockProperty("TagTopic", Array.Empty<string>(), OlUserPropertyType.olKeywords),
                MockProperty("KBF", string.Empty, OlUserPropertyType.olText),
                MockProperty("ToDoIdLvl1", string.Empty, OlUserPropertyType.olText),
                MockProperty("ToDoIdLvl2", string.Empty, OlUserPropertyType.olText),
                MockProperty("ToDoIdLvl3", string.Empty, OlUserPropertyType.olText),
                MockProperty("ToDoIdLvl4", string.Empty, OlUserPropertyType.olText),
            };

            var mock = new Mock<UserProperties>(MockBehavior.Loose);
            mock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns((UserProperty)null);
            foreach (var property in properties)
            {
                mock.Setup(x => x.Find(property.Name, It.IsAny<object>())).Returns(property);
            }
            mock.Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(MockProperty("Added", null, OlUserPropertyType.olText));
            mock.As<IEnumerable>()
                .Setup(x => x.GetEnumerator())
                .Returns(() => properties.GetEnumerator());
            mock.Setup(x => x.Count).Returns(() => properties.Count);

            return mock.Object;
        }

        private static UserProperty MockProperty(
            string propertyName,
            object value,
            OlUserPropertyType olPropertyType
        )
        {
            var mockUser = new Mock<UserProperty>(MockBehavior.Loose);
            mockUser.SetupGet(x => x.Name).Returns(propertyName);
            mockUser.SetupGet(x => x.Type).Returns(olPropertyType);
            mockUser.SetupProperty(x => x.Value, value);
            return mockUser.Object;
        }
    }
}
