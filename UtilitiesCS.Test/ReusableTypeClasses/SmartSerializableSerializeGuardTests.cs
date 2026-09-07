using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UtilitiesCS.Interfaces;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.Test.TestHelpers;

namespace UtilitiesCS.Test.ReusableTypeClasses
{
    /// <summary>
    /// Guard and flush behaviour of <see cref="SmartSerializable{T}"/> (issue #797, AC2 and AC4).
    /// AC2: the empty-or-null disk path must log an error rather than return silently. AC4: an
    /// explicit save must write synchronously, without waiting for the deferred three-second timer,
    /// while the deferred path for every other caller is unchanged.
    ///
    /// This file declares its own harness and its own probe item type because the established
    /// harness is a private nested class inside SmartSerializable_Tests.cs and is not reachable from
    /// another file. No temporary file is created: writes are captured through the injectable
    /// stream-writer seam into a MemoryStream, and the timer is a deterministic manual-fire double.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class SmartSerializableSerializeGuardTests
    {
        /// <summary>
        /// A fixed, non-existent path. It is never opened because every write in this file goes
        /// through the injected stream-writer seam.
        /// </summary>
        private const string FakeSettingsPath = @"X:\FakeAppData\TaskMaster\GuardProbe.json";

        #region AC2 — the empty-or-null path guard logs an error and arms no timer

        [TestMethod]
        public void Serialize_WithEmptyDiskPath_LogsErrorAndArmsNoTimer()
        {
            // Arrange
            var parent = new SerializeGuardProbeItem { Name = "empty-path" };
            var harness = new SerializeGuardHarness(parent);
            using var timerStub = new ManualFireTimerWrapper();
            var timerFactoryCallCount = 0;
            harness.SetTimerFactory(_ =>
            {
                timerFactoryCallCount++;
                return timerStub;
            });
            harness.Config.Disk.FilePath = string.Empty;

            var appender = AttachRootMemoryAppender(out var restore);
            try
            {
                // Act
                harness.Serialize();

                // Assert
                ProbeErrorEvents(appender)
                    .Should()
                    .NotBeEmpty(
                        "an empty Config.Disk.FilePath must be reported at error level, not "
                            + "swallowed by a silent return (AC2)."
                    );
                timerFactoryCallCount.Should().Be(0, "the rejecting path arms no timer.");
                timerStub.Started.Should().BeFalse();
            }
            finally
            {
                restore();
            }
        }

        [TestMethod]
        public void Serialize_WithNullDiskPath_LogsErrorAndArmsNoTimer()
        {
            // Arrange: the pre-fix guard compared only against the empty string, so a null path
            // passed it and reached the write path. FilePathHelper can assign a null file path in
            // its property-changed handler, so this case is genuinely reachable.
            var parent = new SerializeGuardProbeItem { Name = "null-path" };
            var harness = new SerializeGuardHarness(parent);
            using var timerStub = new ManualFireTimerWrapper();
            var timerFactoryCallCount = 0;
            harness.SetTimerFactory(_ =>
            {
                timerFactoryCallCount++;
                return timerStub;
            });
            harness.Config.Disk.FilePath = null;

            var appender = AttachRootMemoryAppender(out var restore);
            try
            {
                // Act
                harness.Serialize();

                // Assert
                ProbeErrorEvents(appender)
                    .Should()
                    .NotBeEmpty(
                        "a null Config.Disk.FilePath must be reported at error level (AC2)."
                    );
                timerFactoryCallCount.Should().Be(0, "the rejecting path arms no timer.");
                timerStub.Started.Should().BeFalse();
            }
            finally
            {
                restore();
            }
        }

        #endregion AC2

        #region AC4 — the explicit save flushes synchronously; the deferred path is unchanged

        [TestMethod]
        public void SerializeNow_WithConfiguredPath_WritesWithoutFiringTimer()
        {
            // Arrange
            var parent = new SerializeGuardProbeItem { Name = "explicit-save" };
            var harness = new SerializeGuardHarness(parent);
            using var timerStub = new ManualFireTimerWrapper();
            var writerCallCount = 0;
            harness.SetTimerFactory(_ => timerStub);
            harness.SetCreateStreamWriter(_ =>
            {
                writerCallCount++;
                return new StreamWriter(new MemoryStream(), Encoding.UTF8, 1024, leaveOpen: false);
            });
            harness.Config.Disk.FilePath = FakeSettingsPath;

            // Act
            harness.SerializeNow();

            // Assert
            writerCallCount
                .Should()
                .Be(
                    1,
                    "the explicit save must write inline so a save is not lost when the host "
                        + "process exits inside the deferred window (AC4)."
                );
            timerStub
                .Started.Should()
                .BeFalse("the explicit save does not arm the deferred timer.");
            timerStub.FireCount.Should().Be(0);
        }

        [TestMethod]
        public void Serialize_WithConfiguredPath_StillRequiresTimerFireToWrite()
        {
            // Arrange: pins the unchanged behaviour of the deferred path for every other caller.
            var parent = new SerializeGuardProbeItem { Name = "deferred-save" };
            var harness = new SerializeGuardHarness(parent);
            using var timerStub = new ManualFireTimerWrapper();
            var writerCallCount = 0;
            harness.SetTimerFactory(_ => timerStub);
            harness.SetCreateStreamWriter(_ =>
            {
                writerCallCount++;
                return new StreamWriter(new MemoryStream(), Encoding.UTF8, 1024, leaveOpen: false);
            });
            harness.Config.Disk.FilePath = FakeSettingsPath;

            // Act
            harness.Serialize();

            // Assert: nothing is written until the timer fires.
            timerStub.Started.Should().BeTrue("the deferred path arms the single-shot timer.");
            writerCallCount.Should().Be(0, "the deferred path defers the write.");

            timerStub.FireElapsed();

            writerCallCount.Should().Be(1, "firing the timer performs the deferred write.");
        }

        #endregion AC4

        #region Log capture helpers

        /// <summary>
        /// Attaches an in-memory appender to the root logger of the repository that owns
        /// <see cref="SmartSerializable{T}"/>.
        ///
        /// The serializer initialises its logger from the declaring type reported by reflection over
        /// a member of a generic type, which resolves to the generic type definition rather than to
        /// any closed constructed type. One logger therefore serves every instantiation and its name
        /// carries no type argument, so an appender attached to the full name of a closed
        /// constructed serializer type would be a different logger and would capture nothing.
        /// Attaching to the root logger is correct under either resolution.
        /// </summary>
        /// <param name="restore">
        /// Receives the action that detaches the appender and restores the root logger's previous
        /// level and the repository's previous configured flag. Attaching to the root logger and
        /// marking the repository configured are process-wide mutations that must not outlive the
        /// test.
        /// </param>
        /// <returns>The attached appender.</returns>
        private static MemoryAppender AttachRootMemoryAppender(out Action restore)
        {
            var appender = new MemoryAppender();
            appender.ActivateOptions();

            var repositoryAssembly = typeof(SmartSerializable<SerializeGuardProbeItem>).Assembly;
            var hierarchy = (Hierarchy)LogManager.GetRepository(repositoryAssembly);
            var root = hierarchy.Root;
            var previousLevel = root.Level;
            var previousConfigured = hierarchy.Configured;

            root.Level = Level.Debug;
            hierarchy.Configured = true;
            root.AddAppender(appender);

            restore = () =>
            {
                root.RemoveAppender(appender);
                root.Level = previousLevel;
                hierarchy.Configured = previousConfigured;
            };

            return appender;
        }

        /// <summary>
        /// Selects the captured error-level events whose rendered message names this file's own
        /// probe item type.
        ///
        /// The assertion is existence rather than an exact count. The run settings this plan uses
        /// impose a class-level parallel scope, so a concurrently running class can only add events.
        /// The probe item type name occurs nowhere else in this test project, so no concurrent class
        /// can contribute a matching event.
        /// </summary>
        private static LoggingEvent[] ProbeErrorEvents(MemoryAppender appender)
        {
            return appender
                .GetEvents()
                .Where(loggingEvent =>
                    loggingEvent.Level >= Level.Error
                    && loggingEvent.RenderedMessage != null
                    && loggingEvent.RenderedMessage.Contains(nameof(SerializeGuardProbeItem))
                )
                .ToArray();
        }

        #endregion Log capture helpers

        #region Harness and probe type

        /// <summary>
        /// Exposes the protected stream-writer and timer-factory seams so the writes and the
        /// deferred timer can be driven deterministically without touching disk or the clock.
        /// </summary>
        private sealed class SerializeGuardHarness : SmartSerializable<SerializeGuardProbeItem>
        {
            public SerializeGuardHarness(SerializeGuardProbeItem parent)
                : base(parent) { }

            public void SetCreateStreamWriter(Func<string, StreamWriter> createStreamWriter) =>
                CreateStreamWriter = createStreamWriter;

            public void SetTimerFactory(Func<TimeSpan, ITimerWrapper> timerFactory) =>
                TimerFactory = timerFactory;
        }

        /// <summary>
        /// Minimal <see cref="ISmartSerializable{T}"/> implementation used only by this file. Its
        /// name occurs nowhere else in this test project, so it uniquely identifies the log events
        /// this file asserts on.
        /// </summary>
        private sealed class SerializeGuardProbeItem : ISmartSerializable<SerializeGuardProbeItem>
        {
            public SerializeGuardProbeItem()
            {
                Config = new NewSmartSerializableConfig();
            }

            public NewSmartSerializableConfig Config { get; set; }

            public string Name { get; set; }

            // Required by ISmartSerializable<T> : INotifyPropertyChanged. This probe never raises
            // it, so CS0067 fires; the interface makes deletion impossible, so the suppression is
            // scoped to the single member.
#pragma warning disable CS0067
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

            public SerializeGuardProbeItem Deserialize(string fileName, string folderPath) => new();

            public SerializeGuardProbeItem Deserialize(
                string fileName,
                string folderPath,
                bool askUserOnError
            ) => new();

            public SerializeGuardProbeItem Deserialize(
                string fileName,
                string folderPath,
                bool askUserOnError,
                JsonSerializerSettings settings
            ) => new();

            public SerializeGuardProbeItem Deserialize<U>(SmartSerializable<U> loader)
                where U : class, ISmartSerializable<U>, new() => new();

            public SerializeGuardProbeItem Deserialize<U>(
                SmartSerializable<U> loader,
                bool askUserOnError,
                Func<SerializeGuardProbeItem> altLoader
            )
                where U : class, ISmartSerializable<U>, new() => altLoader?.Invoke() ?? new();

            public Task<SerializeGuardProbeItem> DeserializeAsync<U>(SmartSerializable<U> config)
                where U : class, ISmartSerializable<U>, new() =>
                Task.FromResult(new SerializeGuardProbeItem());

            public Task<SerializeGuardProbeItem> DeserializeAsync<U>(
                SmartSerializable<U> config,
                bool askUserOnError
            )
                where U : class, ISmartSerializable<U>, new() =>
                Task.FromResult(new SerializeGuardProbeItem());

            public Task<SerializeGuardProbeItem> DeserializeAsync<U>(
                SmartSerializable<U> config,
                bool askUserOnError,
                Func<SerializeGuardProbeItem> altLoader
            )
                where U : class, ISmartSerializable<U>, new() =>
                Task.FromResult(altLoader?.Invoke() ?? new SerializeGuardProbeItem());

            public SerializeGuardProbeItem DeserializeObject(
                string json,
                JsonSerializerSettings settings
            ) => JsonConvert.DeserializeObject<SerializeGuardProbeItem>(json, settings);

            public void Serialize() { }

            public void Serialize(string filePath) { }

            public void SerializeThreadSafe(string filePath) { }
        }

        #endregion Harness and probe type
    }
}
