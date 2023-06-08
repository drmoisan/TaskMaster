using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Office.Interop.Outlook;
using System;
using TaskVisualization;
using UtilitiesCS;
using Moq;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics.Eventing.Reader;
using ToDoModel;
using System.Security.Cryptography;

namespace TaskVisualization.Test
{
    
    public class MoqOlToDo
    {
        public PropertyAccessor MockPA()
        {
            const string PA_TOTAL_WORK = "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/81110003";
            var mockPA = new Mock<PropertyAccessor>();
            mockPA.Setup(x => x.GetProperty(PA_TOTAL_WORK)).Returns(20);
            return mockPA.Object;
        }

        public UserProperty MockProperty<T>(T value, string FieldName, OlUserPropertyType OlFieldType = OlUserPropertyType.olText)
        {
            var mockUser = new Mock<UserProperty>();
            mockUser.Setup(x => x.Name).Returns(FieldName);
            mockUser.Setup(x => x.Type).Returns(OlFieldType);
            mockUser.Setup(x => x.Value).Returns(value);
            return mockUser.Object;
        }

        //internal UserProperty FindProperty(string Name, object Custom = null)
        //{
        //    switch (Name)
        //    {
        //        case "TagProgram":
        //            return MockProperty<string>("TestProgram", "TagProgram", OlUserPropertyType.olText);
        //        case "AB":
        //            return MockProperty<Boolean>(true, "AB", OlUserPropertyType.olYesNo);
        //        case "EC2":
        //            return MockProperty<Boolean>(true, "EC2", OlUserPropertyType.olYesNo);
        //        case "EC":
        //            return MockProperty<string>("+", "EC", OlUserPropertyType.olText);
        //        case "EcState":
        //            return MockProperty<string>("+", "EcState", OlUserPropertyType.olText);
        //        default:
        //            return null;
        //    }
        //}

        internal UserProperty FindProperty(string Name, object Custom = null)
        {
            IEnumerator uPpty = UserPropertyCollection();
            UserProperty result = null;
            while (uPpty.MoveNext())
            {
                UserProperty current = (UserProperty)uPpty.Current;
                if (current.Name == Name) { result = current; break; }
            }
            return result;
        }

        internal IEnumerator UserPropertyCollection()
        {
            UserProperty TagProgram = MockProperty<string>("TestProgram", "TagProgram", OlUserPropertyType.olText);
            UserProperty AB = MockProperty<Boolean>(true, "AB", OlUserPropertyType.olYesNo);
            UserProperty EC2 = MockProperty<Boolean>(true, "EC2", OlUserPropertyType.olYesNo);
            UserProperty EC = MockProperty<string>("+", "EC", OlUserPropertyType.olText);
            UserProperty EcState = MockProperty<string>("+", "EcState", OlUserPropertyType.olText);

            yield return TagProgram;
            yield return AB;
            yield return EC2;
            yield return EC;
            yield return EcState;
        }

        internal UserProperties MockUserProperties()
        {
            var mockUserProperties = new Mock<UserProperties>();
            mockUserProperties.As<IEnumerable>().Setup(x => x.GetEnumerator()).Returns(UserPropertyCollection());
            //mockUserProperties.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object[]>()))
            //    .Returns(FindProperty(It.IsAny<string>(), It.IsAny<object[]>()));
            //mockUserProperties.Setup(x => x.Find(It.IsAny<string>(), It.IsAny<object[]>()))
            //    .Returns((string y, object z) => FindProperty(y,z));
            mockUserProperties.Setup(x => x.Find(It.Is<string>(s => s == "TagProgram"), It.IsAny<object[]>()))
                .Returns(MockProperty<string>("TestProgram", "TagProgram", OlUserPropertyType.olText));
            
            mockUserProperties.Setup(x => x.Find(It.Is<string>(s => s == "AB"), It.IsAny<object[]>()))
                .Returns(MockProperty<Boolean>(true, "AB", OlUserPropertyType.olYesNo));

            mockUserProperties.Setup(x => x.Find(It.Is<string>(s => s == "EC2"), It.IsAny<object[]>()))
                .Returns(MockProperty<Boolean>(true, "EC2", OlUserPropertyType.olYesNo));

            mockUserProperties.Setup(x => x.Find(It.Is<string>(s => s == "EC"), It.IsAny<object[]>()))
                .Returns(MockProperty<string>("+", "EC", OlUserPropertyType.olText));

            mockUserProperties.Setup(x => x.Find(It.Is<string>(s => s == "EcState"), It.IsAny<object[]>()))
                .Returns(MockProperty<string>("+", "EcState", OlUserPropertyType.olText));

            return mockUserProperties.Object;
        }

        internal Mock<MailItem> MailItemMock(string TaskSubject,
                                             OlImportance olImportance,
                                             DateTime creationTime,
                                             DateTime taskStartDate,
                                             OlFlagStatus olFlagStatus,
                                             string categoryNames) 
        {
            var email = new Mock<MailItem>();
            email.Setup(x => x.TaskSubject).Returns(TaskSubject);
            email.Setup(x => x.Subject).Returns(TaskSubject);
            email.Setup(x => x.Importance).Returns(olImportance);
            email.Setup(x => x.CreationTime).Returns(creationTime);
            email.Setup(x => x.TaskStartDate).Returns(taskStartDate);
            email.Setup(x => x.ReminderTime).Returns(new DateTime(4501,01,01));
            email.Setup(x => x.TaskDueDate).Returns(new DateTime(4501, 01, 01));
            email.Setup(x => x.FlagStatus).Returns(olFlagStatus);
            email.Setup(x => x.Categories).Returns(categoryNames);
            email.Setup(x => x.PropertyAccessor).Returns(MockPA());
            email.Setup(x => x.UserProperties).Returns(MockUserProperties());
            //email.Setup(x => x.UserProperties.Find(It.IsAny<string>(), It.IsAny<object[]>())).Returns(FindProperty(It.IsAny<string>(), It.IsAny<object[]>()));

            return email;
        }
        
        internal IEnumerator CategoryCollection()
        {
            var category1 = MockCategory("Tag PROJECT TestProject1");
            var category2 = MockCategory("Tag PROJECT TestProject2");
            var category3 = MockCategory("Tag PROJECT TestProject3");

            yield return category1;
            yield return category2;
            yield return category3;
        }

        internal Category MockCategory(string categoryName)
        {
            var cat = new Mock<Category>();
            cat.Setup(x => x.CategoryID).Returns(categoryName);
            cat.SetupGet(x => x.Name).Returns(categoryName);
            return cat.Object;
        }
        
        internal Categories MockCategories(string categoryString)
        {
            var categories = new Mock<Categories>();
            categories.Setup(x => x.ToString()).Returns(categoryString);
            categories.As<IEnumerable>().Setup(x => x.GetEnumerator()).Returns(CategoryCollection());
            return categories.Object;
        }
        
        internal IEnumerator EmailCollection()
        {
            var email1 = MailItemMock("Task1",OlImportance.olImportanceHigh,DateTime.Now, DateTime.Now, OlFlagStatus.olFlagMarked, "");
            var email2 = MailItemMock("Task2", OlImportance.olImportanceLow, DateTime.Now, DateTime.Now, OlFlagStatus.olFlagMarked, "");

            yield return email1.Object;
            yield return email2.Object;
        }

        internal IApplicationGlobals MockGlobals()
        {
            var mockSelection = new Mock<Selection>();
            mockSelection.As<IEnumerable>().Setup(x => x.GetEnumerator()).Returns(EmailCollection());

            var mockExplorer = new Mock<Explorer>();
            mockExplorer.Setup(x => x.Selection).Returns(mockSelection.Object);

            var mockOlApp = new Mock<Microsoft.Office.Interop.Outlook.Application>();
            mockOlApp.Setup(x => x.ActiveExplorer()).Returns(mockExplorer.Object);

            var mockCategories = new Mock<Categories>();

            var mockNamespaceMAPI = new Mock<Microsoft.Office.Interop.Outlook.NameSpace>();
            mockNamespaceMAPI.Setup(x => x.Categories).Returns(MockCategories("Tag PROJECT TestProject"));

            var mockOlObjects = new Mock<IOlObjects>();
            mockOlObjects.Setup(x => x.App).Returns(mockOlApp.Object);
            mockOlObjects.Setup(x => x.NamespaceMAPI).Returns(mockNamespaceMAPI.Object);
                                    
            var mockGlobals = new Mock<IApplicationGlobals>();
            mockGlobals.Setup(x => x.Ol).Returns(mockOlObjects.Object);

            return mockGlobals.Object;
        }
        
    }
}
