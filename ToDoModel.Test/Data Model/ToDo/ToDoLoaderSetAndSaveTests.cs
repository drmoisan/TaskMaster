using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToDoModel.Data_Model.ToDo;

namespace ToDoModel.Test
{
    /// <summary>
    /// Unit tests for the four <see cref="ToDoLoader.SetAndSave{T}"/> overloads. The class is
    /// internal and reachable via InternalsVisibleTo("ToDoModel.Test"). The Outlook-free
    /// constructor takes two delegates (an Outlook saver and an is-read-only predicate), so the
    /// type is exercised without any live Outlook dependency. Tests cover positive flows
    /// (setter/saver invoked, ref value assigned), negative flows (null setter throws when not
    /// read-only, null saver is tolerated), and edge flows (read-only guard suppresses the
    /// setter/saver).
    /// </summary>
    [TestClass]
    public class ToDoLoaderSetAndSaveTests
    {
        private static ToDoLoader CreateLoader(Action olSaver, bool isReadOnly) =>
            new ToDoLoader(olSaver, () => isReadOnly);

        // ---- Overload: SetAndSave<T>(ref T, T, Action<T>) ----

        [TestMethod]
        public void SetAndSaveRefSetterOnly_NotReadOnly_AssignsRefAndInvokesSetterAndOlSaver()
        {
            // Arrange
            bool olSaverInvoked = false;
            var loader = CreateLoader(() => olSaverInvoked = true, isReadOnly: false);
            int variable = 1;
            int? setterValue = null;

            // Act
            loader.SetAndSave(ref variable, 42, v => setterValue = v);

            // Assert
            variable.Should().Be(42, "the ref variable is always assigned the new value");
            setterValue.Should().Be(42, "the objectSetter must be invoked with the supplied value");
            olSaverInvoked
                .Should()
                .BeTrue("the ref+setter overload delegates to OlSaver as the saver");
        }

        [TestMethod]
        public void SetAndSaveRefSetterOnly_ReadOnly_AssignsRefButSkipsSetterAndSaver()
        {
            // Arrange
            bool olSaverInvoked = false;
            var loader = CreateLoader(() => olSaverInvoked = true, isReadOnly: true);
            int variable = 1;
            bool setterInvoked = false;

            // Act
            loader.SetAndSave(ref variable, 42, v => setterInvoked = true);

            // Assert
            variable.Should().Be(42, "the ref variable is assigned before the read-only guard");
            setterInvoked.Should().BeFalse("the read-only guard suppresses the objectSetter");
            olSaverInvoked.Should().BeFalse("the read-only guard suppresses the saver");
        }

        // ---- Overload: SetAndSave<T>(ref T, T, Action<T>, System.Action) ----

        [TestMethod]
        public void SetAndSaveRefSetterSaver_NotReadOnly_AssignsRefInvokesSetterAndSuppliedSaver()
        {
            // Arrange
            var loader = CreateLoader(() => { }, isReadOnly: false);
            string variable = "old";
            string setterValue = null;
            bool saverInvoked = false;

            // Act
            loader.SetAndSave(ref variable, "new", v => setterValue = v, () => saverInvoked = true);

            // Assert
            variable.Should().Be("new");
            setterValue.Should().Be("new", "the objectSetter receives the supplied value");
            saverInvoked.Should().BeTrue("the supplied objectSaver is invoked when not read-only");
        }

        [TestMethod]
        public void SetAndSaveRefSetterSaver_NotReadOnly_NullSetter_ThrowsArgumentNullException()
        {
            // Arrange
            var loader = CreateLoader(() => { }, isReadOnly: false);
            int variable = 0;

            // Act
            Action act = () => loader.SetAndSave(ref variable, 7, null, () => { });

            // Assert
            act.Should()
                .Throw<ArgumentNullException>(
                    "a null objectSetter is rejected explicitly when not read-only"
                );
            variable.Should().Be(7, "the ref variable is assigned before the null check");
        }

        [TestMethod]
        public void SetAndSaveRefSetterSaver_NotReadOnly_NullSaver_InvokesSetterWithoutThrowing()
        {
            // Arrange
            var loader = CreateLoader(() => { }, isReadOnly: false);
            int variable = 0;
            bool setterInvoked = false;

            // Act
            Action act = () => loader.SetAndSave(ref variable, 9, v => setterInvoked = true, null);

            // Assert
            act.Should().NotThrow("a null objectSaver is guarded, not dereferenced");
            setterInvoked.Should().BeTrue("the setter still runs when the saver is null");
            variable.Should().Be(9);
        }

        [TestMethod]
        public void SetAndSaveRefSetterSaver_ReadOnly_SkipsSetterAndSaverEvenWithNullSetter()
        {
            // Arrange
            var loader = CreateLoader(() => { }, isReadOnly: true);
            int variable = 0;
            bool saverInvoked = false;

            // Act: a null setter does not throw when read-only because the guard is skipped first
            Action act = () => loader.SetAndSave(ref variable, 5, null, () => saverInvoked = true);

            // Assert
            act.Should()
                .NotThrow("the read-only guard returns before the null-setter check executes");
            variable.Should().Be(5, "the ref variable is assigned before the read-only guard");
            saverInvoked.Should().BeFalse("the read-only guard suppresses the saver");
        }

        // ---- Overload: SetAndSave<T>(T, Action<T>) ----

        [TestMethod]
        public void SetAndSaveValueSetterOnly_NotReadOnly_InvokesSetterAndOlSaver()
        {
            // Arrange
            bool olSaverInvoked = false;
            var loader = CreateLoader(() => olSaverInvoked = true, isReadOnly: false);
            int? setterValue = null;

            // Act
            loader.SetAndSave(123, v => setterValue = v);

            // Assert
            setterValue.Should().Be(123);
            olSaverInvoked.Should().BeTrue("the value+setter overload uses OlSaver as the saver");
        }

        [TestMethod]
        public void SetAndSaveValueSetterOnly_ReadOnly_SkipsSetterAndSaver()
        {
            // Arrange
            bool olSaverInvoked = false;
            var loader = CreateLoader(() => olSaverInvoked = true, isReadOnly: true);
            bool setterInvoked = false;

            // Act
            loader.SetAndSave(123, v => setterInvoked = true);

            // Assert
            setterInvoked.Should().BeFalse("the read-only guard suppresses the setter");
            olSaverInvoked.Should().BeFalse("the read-only guard suppresses the saver");
        }

        // ---- Overload: SetAndSave<T>(T, Action<T>, System.Action) ----

        [TestMethod]
        public void SetAndSaveValueSetterSaver_NotReadOnly_InvokesSetterAndSuppliedSaver()
        {
            // Arrange
            var loader = CreateLoader(() => { }, isReadOnly: false);
            string setterValue = null;
            bool saverInvoked = false;

            // Act
            loader.SetAndSave("payload", v => setterValue = v, () => saverInvoked = true);

            // Assert
            setterValue.Should().Be("payload");
            saverInvoked.Should().BeTrue();
        }

        [TestMethod]
        public void SetAndSaveValueSetterSaver_NotReadOnly_NullSetter_ThrowsArgumentNullException()
        {
            // Arrange
            var loader = CreateLoader(() => { }, isReadOnly: false);

            // Act
            Action act = () => loader.SetAndSave(1, null, () => { });

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void SetAndSaveValueSetterSaver_NotReadOnly_NullSaver_InvokesSetterWithoutThrowing()
        {
            // Arrange
            var loader = CreateLoader(() => { }, isReadOnly: false);
            bool setterInvoked = false;

            // Act
            Action act = () => loader.SetAndSave(1, v => setterInvoked = true, null);

            // Assert
            act.Should().NotThrow("a null objectSaver is guarded against, not dereferenced");
            setterInvoked.Should().BeTrue();
        }

        [TestMethod]
        public void SetAndSaveValueSetterSaver_ReadOnly_SkipsSetterAndSaver()
        {
            // Arrange
            var loader = CreateLoader(() => { }, isReadOnly: true);
            bool setterInvoked = false;
            bool saverInvoked = false;

            // Act
            loader.SetAndSave(1, v => setterInvoked = true, () => saverInvoked = true);

            // Assert
            setterInvoked.Should().BeFalse("the read-only guard suppresses the setter");
            saverInvoked.Should().BeFalse("the read-only guard suppresses the saver");
        }

        [TestMethod]
        public void SetAndSaveValueSetterSaver_ValueEqualToExisting_StillInvokesSetterWhenNotReadOnly()
        {
            // Arrange: equal value is not special-cased; the setter is still invoked.
            var loader = CreateLoader(() => { }, isReadOnly: false);
            int observed = -1;

            // Act
            loader.SetAndSave(5, v => observed = v, () => { });

            // Assert
            observed.Should().Be(5, "SetAndSave does not short-circuit on value equality");
        }
    }
}
