using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.EmailIntelligence;

namespace UtilitiesCS.Test.EmailIntelligence
{
    [TestClass]
    public class CommonWords_Tests
    {
        [TestMethod]
        public void StripCommonWords_WithSentenceAndList_RemovesKnownWordsCaseInsensitively()
        {
            // Arrange
            const string sentence = "Fwd: Update on PLAN";
            IList<string> commonWords = new List<string> { "fwd", "on" };

            // Act
            var stripped = sentence.StripCommonWords(commonWords);

            // Assert
            stripped.Should().Be("update plan");
        }

        [TestMethod]
        public void StripCommonWords_WithCustomTokenizer_UsesProvidedRegex()
        {
            // Arrange
            const string sentence = "R&D update";
            IList<string> commonWords = new List<string> { "update" };
            var tokenizer = new Regex(@"\b[\w&]{2,}\b");

            // Act
            var stripped = sentence.StripCommonWords(commonWords, tokenizer);

            // Assert
            stripped.Should().Be("r&d");
        }

        [TestMethod]
        public void StripCommonWords_WithEmptyTokenArray_ReturnsEmptyArray()
        {
            // Arrange
            var tokens = Array.Empty<string>();
            IList<string> commonWords = new List<string> { "ignored" };

            // Act
            var stripped = tokens.StripCommonWords(commonWords);

            // Assert
            stripped.Should().BeSameAs(tokens);
            stripped.Should().BeEmpty();
        }

        [TestMethod]
        public void StripCommonWords_WithSerializableListOverload_DelegatesToListImplementation()
        {
            // Arrange
            const string sentence = "Re agenda review";
            ISerializableList<string> commonWords = new SerializableStringListStub { "re" };

            // Act
            var stripped = sentence.StripCommonWords(commonWords);

            // Assert
            stripped.Should().Be("agenda review");
        }

        [TestMethod]
        public void StripCommonWords_WithNullSentence_ThrowsArgumentNullException()
        {
            // Arrange
            string sentence = null;
            IList<string> commonWords = new List<string> { "fwd" };

            // Act
            Action act = () => sentence.StripCommonWords(commonWords);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void StripAccents_RemovesAccentedMarks()
        {
            // Arrange
            const string text = "Crème brûlée";

            // Act
            var stripped = text.StripAccents();

            // Assert
            stripped.Should().Be("Creme brulee");
        }

        [TestMethod]
        public void StripAccents2_RemovesCombiningMarks()
        {
            // Arrange
            const string text = "mañana";

            // Act
            var stripped = text.StripAccents2();

            // Assert
            stripped.Should().Be("manana");
        }

        private sealed class SerializableStringListStub : List<string>, ISerializableList<string>
        {
            public string Filename { get; set; }

            public string Filepath { get; set; }

            public string Folderpath { get; set; }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            public void Deserialize() => throw new NotSupportedException();

            public void Deserialize(bool askUserOnError) => throw new NotSupportedException();

            public void Deserialize(string filepath, bool askUserOnError) =>
                throw new NotSupportedException();

            public void Deserialize(
                string filepath,
                CSVLoader<string> backupLoader,
                bool askUserOnError
            ) => throw new NotSupportedException();

            public void Serialize() => throw new NotSupportedException();

            public void Serialize(string filepath) => throw new NotSupportedException();

            public System.Threading.Tasks.Task SerializeAsync() =>
                throw new NotSupportedException();

            public System.Threading.Tasks.Task SerializeAsync(string filepath) =>
                throw new NotSupportedException();

            public List<string> ToList() => new List<string>(this);

            public void FromList(IList<string> value)
            {
                Clear();
                AddRange(value);
                PropertyChanged?.Invoke(
                    this,
                    new System.ComponentModel.PropertyChangedEventArgs(nameof(Count))
                );
            }
        }
    }
}
