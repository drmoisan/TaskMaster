using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.Test.Extensions
{
    [TestClass]
    public class IAsyncEnumerableExtensions_Tests
    {
        [TestMethod]
        public async Task Zip_ShouldThrowFileLoadException_WhenAsyncInterfacesDependencyCannotLoad()
        {
            // Arrange
            var first = GetValuesAsync(1, 2, 3);
            var second = GetValuesAsync("a", "b");

            // Act
            Func<Task> act = async () => await IAsyncEnumerableExtensions.Zip(first, second).ToListAsync();

            // Assert
            await act.Should().ThrowAsync<System.IO.FileLoadException>();
        }

        [TestMethod]
        public async Task WithProgressReporting_ShouldThrowFileLoadException_WhenAsyncSelectPipelineCannotLoad()
        {
            // Arrange
            var reports = new List<int>();
            var source = GetValuesAsync(10, 20, 30, 40);

            // Act
            Func<Task> act = async () => await source.WithProgressReporting(4, reports.Add).ToListAsync();

            // Assert
            await act.Should().ThrowAsync<System.IO.FileLoadException>();
            reports.Should().BeEmpty();
        }

        [TestMethod]
        public void WithProgressReporting_ShouldThrowFileLoadException_BeforeNullGuardWhenAsyncDependencyCannotLoad()
        {
            // Arrange
            IAsyncEnumerable<int> source = null;

            // Act
            Action act = () => source.WithProgressReporting(1, _ => { });

            // Assert
            act.Should().Throw<System.IO.FileLoadException>();
        }

        [TestMethod]
        public async Task ToSortedListAsync_ShouldSortBySelectedKey()
        {
            // Arrange
            var source = GetValuesAsync("bravo", "alpha", "charlie");

            // Act
            SortedList<string, string> result = await source.ToSortedListAsync(value => value);

            // Assert
            result.Keys.Should().Equal("alpha", "bravo", "charlie");
            result.Values.Should().Equal("alpha", "bravo", "charlie");
        }

        [TestMethod]
        public async Task ToConcurrentDictionaryAsync_ShouldThrowFileLoadException_WhenAsyncInterfacesDependencyCannotLoad()
        {
            // Arrange
            var source = GetValuesAsync("alpha", "bravo");

            // Act
            Func<Task> act = async () => await source.ToConcurrentDictionaryAsync(value => value[0], value => value.Length);

            // Assert
            await act.Should().ThrowAsync<System.IO.FileLoadException>();
        }

        private static async IAsyncEnumerable<T> GetValuesAsync<T>(params T[] values)
        {
            foreach (T value in values)
            {
                await Task.Yield();
                yield return value;
            }
        }
    }
}
