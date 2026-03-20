using System;
using System.Collections.ObjectModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class FlagTranslator_Tests
    {
        private string _storedStr = "";
        private bool _lastPrefix;
        private ObservableCollection<string> _storedList = new();

        private FlagTranslator CreateTestTranslator()
        {
            return new FlagTranslator(
                getStrFunc: prefix => prefix ? $"P:{_storedStr}" : _storedStr,
                setStrFunc: (prefix, val) => { _lastPrefix = prefix; _storedStr = val; },
                getListFunc: prefix => prefix ? new ObservableCollection<string> { $"P:{_storedList[0]}" } : _storedList,
                setListFunc: (prefix, val) => { _lastPrefix = prefix; _storedList = val; }
            );
        }

        [TestMethod]
        public void DefaultConstructor_ShouldCreateInstanceWithNullDelegates()
        {
            var ft = new FlagTranslator();

            ft.Identifier.Should().Be("not set");
            ft.GetStrFunc.Should().BeNull();
        }

        [TestMethod]
        public void Constructor_WithDelegates_ShouldSetAllFunctions()
        {
            _storedStr = "hello";
            _storedList = new ObservableCollection<string> { "a" };
            var ft = CreateTestTranslator();

            ft.GetStrFunc.Should().NotBeNull();
            ft.SetStrFunc.Should().NotBeNull();
            ft.GetListFunc.Should().NotBeNull();
            ft.SetListFunc.Should().NotBeNull();
        }

        [TestMethod]
        public void Constructor_NullGetStrFunc_ShouldThrowArgumentNullException()
        {
            Action act = () => new FlagTranslator(
                null,
                (p, v) => { },
                p => new ObservableCollection<string>(),
                (p, v) => { }
            );

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_NullSetStrFunc_ShouldThrowArgumentNullException()
        {
            Action act = () => new FlagTranslator(
                p => "",
                null,
                p => new ObservableCollection<string>(),
                (p, v) => { }
            );

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_NullGetListFunc_ShouldThrowArgumentNullException()
        {
            Action act = () => new FlagTranslator(
                p => "",
                (p, v) => { },
                null,
                (p, v) => { }
            );

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_NullSetListFunc_ShouldThrowArgumentNullException()
        {
            Action act = () => new FlagTranslator(
                p => "",
                (p, v) => { },
                p => new ObservableCollection<string>(),
                null
            );

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Identifier_SetAndGet_ShouldWork()
        {
            var ft = new FlagTranslator();

            ft.Identifier = "myFlag";

            ft.Identifier.Should().Be("myFlag");
        }

        [TestMethod]
        public void AsStringNoPrefix_Get_ShouldCallGetStrFuncWithFalse()
        {
            _storedStr = "test";
            _storedList = new ObservableCollection<string> { "a" };
            var ft = CreateTestTranslator();

            ft.AsStringNoPrefix.Should().Be("test");
        }

        [TestMethod]
        public void AsStringWithPrefix_Get_ShouldCallGetStrFuncWithTrue()
        {
            _storedStr = "test";
            _storedList = new ObservableCollection<string> { "a" };
            var ft = CreateTestTranslator();

            ft.AsStringWithPrefix.Should().Be("P:test");
        }

        [TestMethod]
        public void AsStringNoPrefix_Set_ShouldCallSetStrFuncWithFalse()
        {
            _storedStr = "";
            _storedList = new ObservableCollection<string> { "a" };
            var ft = CreateTestTranslator();

            ft.AsStringNoPrefix = "newValue";

            _lastPrefix.Should().BeFalse();
            _storedStr.Should().Be("newValue");
        }

        [TestMethod]
        public void AsStringWithPrefix_Set_ShouldCallSetStrFuncWithTrue()
        {
            _storedStr = "";
            _storedList = new ObservableCollection<string> { "a" };
            var ft = CreateTestTranslator();

            ft.AsStringWithPrefix = "newValue";

            _lastPrefix.Should().BeTrue();
            _storedStr.Should().Be("newValue");
        }

        [TestMethod]
        public void AsListNoPrefix_Get_ShouldCallGetListFuncWithFalse()
        {
            _storedStr = "";
            _storedList = new ObservableCollection<string> { "item1" };
            var ft = CreateTestTranslator();

            var result = ft.AsListNoPrefix;

            result.Should().Contain("item1");
        }

        [TestMethod]
        public void AsListWithPrefix_Get_ShouldCallGetListFuncWithTrue()
        {
            _storedStr = "";
            _storedList = new ObservableCollection<string> { "item1" };
            var ft = CreateTestTranslator();

            var result = ft.AsListWithPrefix;

            result.Should().Contain("P:item1");
        }

        [TestMethod]
        public void ToString_ShouldReturnAsStringNoPrefix()
        {
            _storedStr = "hello";
            _storedList = new ObservableCollection<string> { "a" };
            var ft = CreateTestTranslator();

            ft.ToString().Should().Be("hello");
        }

        [TestMethod]
        public void AsString_ShouldReturnAsStringNoPrefix()
        {
            _storedStr = "world";
            _storedList = new ObservableCollection<string> { "a" };
            var ft = CreateTestTranslator();

            ft.AsString().Should().Be("world");
        }
    }
}
