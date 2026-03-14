using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    [TestClass]
    public class SerializableList_Tests
    {
        [TestMethod]
        public void DefaultConstructorAndCoreListOperations_WorkLikeAList()
        {
            // Arrange
            var list = new SerializableList<string>();

            // Act
            list.Add("alpha");
            list.Insert(1, "gamma");
            list.Insert(1, "beta");
            list[2] = "delta";
            var removed = list.Remove("beta");
            var containsDelta = list.Contains("delta");
            var indexOfDelta = list.IndexOf("delta");

            // Assert
            list.Count.Should().Be(2);
            list.Should().Equal("alpha", "delta");
            removed.Should().BeTrue();
            containsDelta.Should().BeTrue();
            indexOfDelta.Should().Be(1);
            list.IsReadOnly.Should().BeFalse();
        }

        [TestMethod]
        public void IEnumerableConstructor_LoadsLazySequenceOnFirstUse()
        {
            // Arrange
            var list = new SerializableList<int>(Enumerable.Range(1, 4));

            // Act
            var count = list.Count;
            var values = list.ToList();

            // Assert
            count.Should().Be(4);
            values.Should().Equal(1, 2, 3, 4);
        }

        [TestMethod]
        public void CopyToRemoveAtClearAndFromList_UpdateCollectionState()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 1, 2, 3, 4 });
            var copied = new int[6];

            // Act
            list.CopyTo(copied, 1);
            list.RemoveAt(1);
            list.FromList(new List<int> { 9, 8 });
            var valuesAfterFromList = list.ToList();
            list.Clear();

            // Assert
            copied.Should().Equal(0, 1, 2, 3, 4, 0);
            valuesAfterFromList.Should().Equal(9, 8);
            list.Count.Should().Be(0);
        }

        [TestMethod]
        public void FindIndexOverloadsAndEnumeration_ReturnExpectedMatches()
        {
            // Arrange
            var list = new SerializableList<string>(new List<string> { "ant", "bear", "cat", "dog", "emu" });

            // Act
            var firstThreeLetterIndex = list.FindIndex(value => value.Length == 3);
            var laterThreeLetterIndex = list.FindIndex(2, value => value.Length == 3);
            var rangedIndex = list.FindIndex(1, 3, value => value.StartsWith("d", StringComparison.Ordinal));
            var enumerated = list.ToArray();

            // Assert
            firstThreeLetterIndex.Should().Be(0);
            laterThreeLetterIndex.Should().Be(2);
            rangedIndex.Should().Be(3);
            enumerated.Should().Equal("ant", "bear", "cat", "dog", "emu");
        }

        [TestMethod]
        public void Add_RaisesPropertyChangedForAdd()
        {
            // Arrange
            var list = new SerializableList<string>();
            var raisedNames = new List<string>();
            list.PropertyChanged += (_, args) => raisedNames.Add(args.PropertyName);

            // Act
            list.Add("value");

            // Assert
            raisedNames.Should().ContainSingle().Which.Should().Be(nameof(SerializableList<string>.Add));
        }

        [TestMethod]
        public void FilenameAndFolderpath_ComposeFilepath()
        {
            // Arrange
            var list = new SerializableList<string>();
            var folder = @"C:\Example";

            // Act
            list.Filename = "items.json";
            list.Folderpath = folder;

            // Assert
            list.Filepath.Should().Be(Path.Combine(folder, "items.json"));
        }

        [TestMethod]
        public void Filepath_SetToExistingFolderWithoutExtension_ThrowsArgumentException()
        {
            // Arrange
            var list = new SerializableList<string>();
            var existingFolder = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

            // Act
            Action act = () => list.Filepath = existingFolder;

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Folder Path*");
        }

        [TestMethod]
        public void JsonRoundTrip_PreservesItems()
        {
            // Arrange
            var source = new SerializableList<int>(new List<int> { 2, 1, 3 })
            {
                Filename = "values.json",
                Folderpath = @"C:\Lists"
            };
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            // Act
            var json = JsonConvert.SerializeObject(source, settings);
            var roundTrip = JsonConvert.DeserializeObject<SerializableList<int>>(json, settings);

            // Assert
            roundTrip.Should().NotBeNull();
            roundTrip!.ToList().Should().Equal(2, 1, 3);
            roundTrip.Filename.Should().BeEmpty();
            roundTrip.Folderpath.Should().BeEmpty();
            roundTrip.Filepath.Should().BeEmpty();
        }

        [TestMethod]
        public void Sort_OrdersValuesUsingComparableImplementation()
        {
            // Arrange
            var list = new SerializableList<int>(new List<int> { 4, 1, 3, 2 });

            // Act
            list.Sort();

            // Assert
            list.ToList().Should().Equal(1, 2, 3, 4);
        }
    }
}