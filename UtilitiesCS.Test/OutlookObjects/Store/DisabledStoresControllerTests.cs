using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.Test.OutlookObjects.Store
{
    /// <summary>
    /// Unit tests for <see cref="DisabledStoresController"/> (issue #265). All logic is exercised
    /// through the <see cref="IDisabledStoresViewer"/> seam with Moq and a mocked
    /// <see cref="IStoreDisableService"/>: no live Outlook, no live <see cref="DataGridView"/>, and
    /// no temporary files. Clicks are driven via a directly-constructed
    /// <see cref="DataGridViewCellEventArgs"/>. Async paths are driven by completed/faulted
    /// <see cref="Task"/> results (no sleeps, delays, or real timers).
    /// </summary>
    [TestClass]
    public class DisabledStoresControllerTests
    {
        private static (
            DisabledStoresController controller,
            Mock<IStoreDisableService> service,
            Mock<IDisabledStoresViewer> viewer
        ) CreateController()
        {
            var service = new Mock<IStoreDisableService>();
            var globals = new Mock<IApplicationGlobals>();
            globals.SetupGet(g => g.StoreDisable).Returns(service.Object);

            var viewer = new Mock<IDisabledStoresViewer>();
            viewer.SetupGet(v => v.InvokeRequired).Returns(false);

            var controller = new DisabledStoresController(globals.Object);
            SetInternalProperty(controller, "Viewer", viewer.Object);
            return (controller, service, viewer);
        }

        private static IReadOnlyCollection<DisabledStoreEntry> Entries(
            params (string name, DisableScope scope)[] items
        )
        {
            var list = new List<DisabledStoreEntry>();
            foreach (var (name, scope) in items)
            {
                list.Add(new DisabledStoreEntry(StoreIdentity.Resolve(name), scope));
            }
            return list;
        }

        // ---- P4-T2: list population (AC2, AC3, AC6) ----

        [TestMethod]
        public void PopulateRows_ProjectsServiceEntriesIntoRows()
        {
            // Arrange: a mix of session-only and future-sessions entries.
            var (controller, service, viewer) = CreateController();
            service
                .Setup(s => s.GetDisabledStores())
                .Returns(
                    Entries(
                        ("Mailbox A", DisableScope.SessionOnly),
                        ("Mailbox B", DisableScope.FutureSessions)
                    )
                );

            // Act
            controller.PopulateRows();

            // Assert
            var rows = GetInternalProperty<List<DisabledStoreRow>>(controller, "Rows");
            rows.Should().HaveCount(2);

            rows[0].Identity.Value.Should().Be("Mailbox A");
            rows[0].DisplayName.Should().Be("Mailbox A");
            rows[0].IsFutureSession.Should().BeFalse();
            rows[0].ScopeLabel.Should().Be("Session Only");

            rows[1].Identity.Value.Should().Be("Mailbox B");
            rows[1].DisplayName.Should().Be("Mailbox B");
            rows[1].IsFutureSession.Should().BeTrue();
            rows[1].ScopeLabel.Should().Be("Future Sessions");

            viewer.Verify(
                v => v.BindRows(It.Is<IList<DisabledStoreRow>>(r => r.Count == 2)),
                Times.Once
            );
        }

        [TestMethod]
        public void PopulateRows_WhenServiceReturnsEmpty_BindsEmptyListWithoutException()
        {
            // Arrange
            var (controller, service, viewer) = CreateController();
            service.Setup(s => s.GetDisabledStores()).Returns(Array.Empty<DisabledStoreEntry>());

            // Act
            Action act = () => controller.PopulateRows();

            // Assert
            act.Should().NotThrow();
            var rows = GetInternalProperty<List<DisabledStoreRow>>(controller, "Rows");
            rows.Should().BeEmpty();
            viewer.Verify(
                v => v.BindRows(It.Is<IList<DisabledStoreRow>>(r => r.Count == 0)),
                Times.Once
            );
        }

        // ---- P4-T3: click resolution (AC4, AC8) ----

        [TestMethod]
        public void Dgv_CellContentClick_OnReenableColumn_InvokesReenableWithRowIdentityOnce()
        {
            // Arrange
            var (controller, service, _) = CreateController();
            service
                .Setup(s => s.GetDisabledStores())
                .Returns(
                    Entries(
                        ("Mailbox A", DisableScope.SessionOnly),
                        ("Mailbox B", DisableScope.FutureSessions)
                    )
                );
            service
                .Setup(s => s.ReenableAsync(It.IsAny<StoreIdentity>()))
                .Returns(Task.CompletedTask);
            controller.PopulateRows();

            var args = new DataGridViewCellEventArgs(controller.ReenableColumnIndex, 1);

            // Act
            controller.Dgv_CellContentClick(null, args);

            // Assert: row index 1 is "Mailbox B"; reenable invoked exactly once with that identity.
            service.Verify(
                s => s.ReenableAsync(It.Is<StoreIdentity>(id => id.Value == "Mailbox B")),
                Times.Once
            );
        }

        [TestMethod]
        public void Dgv_CellContentClick_OnHeaderOrNonButtonColumn_DoesNothing()
        {
            // Arrange
            var (controller, service, _) = CreateController();
            service
                .Setup(s => s.GetDisabledStores())
                .Returns(Entries(("Mailbox A", DisableScope.SessionOnly)));
            controller.PopulateRows();

            // Act: header row (-1) and a non-Reenable column.
            controller.Dgv_CellContentClick(
                null,
                new DataGridViewCellEventArgs(controller.ReenableColumnIndex, -1)
            );
            controller.Dgv_CellContentClick(null, new DataGridViewCellEventArgs(0, 0));

            // Assert
            service.Verify(s => s.ReenableAsync(It.IsAny<StoreIdentity>()), Times.Never);
        }

        [TestMethod]
        public void Dgv_CellContentClick_WhenRowIndexOutOfRange_DoesNotThrow()
        {
            // Arrange
            var (controller, service, _) = CreateController();
            service
                .Setup(s => s.GetDisabledStores())
                .Returns(Entries(("Mailbox A", DisableScope.SessionOnly)));
            controller.PopulateRows();

            var args = new DataGridViewCellEventArgs(controller.ReenableColumnIndex, 5);

            // Act
            Action act = () => controller.Dgv_CellContentClick(null, args);

            // Assert
            act.Should().NotThrow();
            service.Verify(s => s.ReenableAsync(It.IsAny<StoreIdentity>()), Times.Never);
        }

        // ---- P4-T4: reenable success then refetch (AC4, AC5) ----

        [TestMethod]
        public async Task ReenableAsync_OnSuccess_CallsServiceThenRefetchesDisabledStores()
        {
            // Arrange: seed Rows directly so the only GetDisabledStores call is the post-reenable
            // refetch, making the re-fetch assertion unambiguous.
            var (controller, service, viewer) = CreateController();
            var row = new DisabledStoreRow
            {
                Identity = StoreIdentity.Resolve("Mailbox A"),
                DisplayName = "Mailbox A",
                IsFutureSession = false,
                ScopeLabel = "Session Only",
            };
            SetInternalProperty(controller, "Rows", new List<DisabledStoreRow> { row });
            service
                .Setup(s => s.ReenableAsync(It.IsAny<StoreIdentity>()))
                .Returns(Task.CompletedTask);
            service.Setup(s => s.GetDisabledStores()).Returns(Array.Empty<DisabledStoreEntry>());

            // Act
            await controller.ReenableAsync(row);

            // Assert: service reenabled with the row identity, then state re-fetched and rebound.
            service.Verify(
                s => s.ReenableAsync(It.Is<StoreIdentity>(id => id.Value == "Mailbox A")),
                Times.Once
            );
            service.Verify(s => s.GetDisabledStores(), Times.Once);
            viewer.Verify(v => v.BindRows(It.IsAny<IList<DisabledStoreRow>>()), Times.Once);
        }

        // ---- P4-T5: reenable failure surfaced via MyBox, no throw, still refetches (AC7) ----

        [TestMethod]
        public async Task ReenableAsync_WhenServiceThrows_SurfacesViaMyBoxDoesNotThrowAndStillRefetches()
        {
            // Arrange
            var (controller, service, viewer) = CreateController();
            var row = new DisabledStoreRow
            {
                Identity = StoreIdentity.Resolve("Mailbox A"),
                DisplayName = "Mailbox A",
                IsFutureSession = true,
                ScopeLabel = "Future Sessions",
            };
            SetInternalProperty(controller, "Rows", new List<DisabledStoreRow> { row });
            service
                .Setup(s => s.ReenableAsync(It.IsAny<StoreIdentity>()))
                .ThrowsAsync(new InvalidOperationException("reenable failed"));
            service.Setup(s => s.GetDisabledStores()).Returns(Array.Empty<DisabledStoreEntry>());

            var originalInvoker = MyBox.DialogInvoker;
            var invocationCount = 0;
            try
            {
                MyBox.DialogInvoker = _ =>
                {
                    invocationCount++;
                    return DialogResult.OK;
                };

                // Act
                Func<Task> act = async () => await controller.ReenableAsync(row);

                // Assert
                await act.Should().NotThrowAsync();
                invocationCount.Should().Be(1);
                service.Verify(s => s.GetDisabledStores(), Times.Once);
                viewer.Verify(v => v.BindRows(It.IsAny<IList<DisabledStoreRow>>()), Times.Once);
            }
            finally
            {
                MyBox.DialogInvoker = originalInvoker;
            }
        }

        // ---- reflection helpers (mirrors StoreWrapperViewerTests.cs) ----

        private static T GetInternalProperty<T>(object instance, string propertyName)
        {
            var property = instance
                .GetType()
                .GetProperty(
                    propertyName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );
            property.Should().NotBeNull($"property {propertyName} should exist");
            return (T)property!.GetValue(instance);
        }

        private static void SetInternalProperty(object instance, string propertyName, object value)
        {
            var property = instance
                .GetType()
                .GetProperty(
                    propertyName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );
            property.Should().NotBeNull($"property {propertyName} should exist");
            property!.SetValue(instance, value);
        }
    }
}
