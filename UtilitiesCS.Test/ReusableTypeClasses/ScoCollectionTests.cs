using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using UtilitiesCS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using FluentAssertions;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class ScoCollectionTests
    {
        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

        }

        #region Helper Methods and Classes

        private ScoCollection<string> CreateScoCollection()
        {
            return new ScoCollection<string>();
        }
        private List<string[]> _receivedEvents;


        public void CollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems.Cast<object>().Select(obj => obj.ToString()).StringJoin(",");

            _receivedEvents.Add([e.Action.ToString(), newItems]);

        }

        #endregion Helper Methods and Classes

        [TestMethod]
        public void CollectionChanged_StateUnderTest_AddRaisesEvent()
        {
            // Arrange
            var scoCollection = this.CreateScoCollection();

            //var receivedEvents = new List<string>();
            //scoCollection.CollectionChanged += delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            //{
            //    receivedEvents.Add(e.Action.ToString());
            //};

            _receivedEvents = [];
            scoCollection.CollectionChanged += CollectionChangedHandler;
            List<string[]> expected = [["Add", "One"], ["Add", "Two"]];

            // Act
            scoCollection.Add("One");
            scoCollection.Add("Two");

            // Assert
            Console.WriteLine(expected.ToArray().ToFormattedText(["Action", "New Items"], title: "Expected Events"));

            Console.WriteLine(_receivedEvents.ToArray().ToFormattedText(["Action", "New Items"], title: "Actual Events"));

            _receivedEvents.Should().BeEquivalentTo(expected);

        }
    
    }    
}
