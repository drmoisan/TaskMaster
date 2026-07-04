using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            item.ExpandChildren.Should().Be("+");
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
            };

            var mock = new Mock<UserProperties>(MockBehavior.Loose);
            mock.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(
                    (string name, object custom) => properties.FirstOrDefault(p => p.Name == name)
                );
            mock.Setup(x =>
                    x.Add(
                        It.IsAny<string>(),
                        It.IsAny<OlUserPropertyType>(),
                        It.IsAny<object>(),
                        It.IsAny<object>()
                    )
                )
                .Returns(
                    (
                        string name,
                        OlUserPropertyType type,
                        object addToFolderFields,
                        object displayFormat
                    ) =>
                    {
                        var property = MockProperty(name, null, type);
                        properties.Add(property);
                        return property;
                    }
                );
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
