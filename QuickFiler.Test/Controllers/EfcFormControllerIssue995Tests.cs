using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickFiler.Controllers;
using QuickFiler.Viewers;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace QuickFiler.Test.Controllers
{
    /// <summary>Issue 995 confirmation subscription ownership and existing OK boundary.</summary>
    [TestClass]
    public class EfcFormControllerIssue995Tests
    {
        [TestMethod]
        public void ConfirmationWiring_NullRouter_IsRejected()
        {
            var controller = CreateController();
            Action configure = () => controller.ConfigureBreadcrumbConfirmation(null);

            configure.Should().Throw<ArgumentNullException>().WithParameterName("router");
        }

        [TestMethod]
        public async Task ConfirmationWiring_RequestsOkOnceAndDoesNotDuplicateSubscription()
        {
            var controller = CreateController();
            var router = await CreateSelectedRouter();
            int calls = 0;
            EventHandler ok = (sender, args) => calls++;
            controller.ConfigureBreadcrumbConfirmation(router, ok);
            controller.ConfigureBreadcrumbConfirmation(router, ok);

            await Confirm(router);

            calls.Should().Be(1);
            controller.Cleanup();
        }

        [TestMethod]
        public async Task CleanupAndReconfiguration_DetachOldRouterConfirmation()
        {
            var controller = CreateController();
            var oldRouter = await CreateSelectedRouter();
            var currentRouter = await CreateSelectedRouter();
            int calls = 0;
            EventHandler ok = (sender, args) => calls++;
            controller.ConfigureBreadcrumbConfirmation(oldRouter, ok);
            controller.ConfigureBreadcrumbConfirmation(currentRouter, ok);

            await Confirm(oldRouter);
            calls.Should().Be(0);
            await Confirm(currentRouter);
            calls.Should().Be(1);
            controller.Cleanup();
            controller.Cleanup();
            await Confirm(currentRouter);

            calls.Should().Be(1, "cleanup must release the active router subscription");
        }

        [TestMethod]
        public async Task DefaultConfirmationWiring_UsesExistingOkFaultBoundaryOnce()
        {
            var previousContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                var controller = CreateController();
                var router = await CreateSelectedRouter();
                int faults = 0;
                controller.BoundaryErrorSink = (message, exception) => faults++;
                controller.ConfigureBreadcrumbConfirmation(router);

                // The established all-fields-null controller fault is caught by ButtonOkClickAsync.
                await Confirm(router);

                faults.Should().Be(1);
                controller.Cleanup();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        private static EfcFormController CreateController()
        {
            var constructor = typeof(EfcFormController).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null
            );
            constructor.Should().NotBeNull();
            return (EfcFormController)constructor.Invoke(Array.Empty<object>());
        }

        private static async Task<BreadcrumbBridgeRouter> CreateSelectedRouter()
        {
            var provider = new Mock<IFolderHierarchyProvider>();
            var host = new Mock<IBreadcrumbWebHost>();
            host.SetupGet(value => value.IsCoreInitialized).Returns(true);
            var router = new BreadcrumbBridgeRouter(
                provider.Object,
                host.Object,
                new BreadcrumbMessageCodec(),
                new BreadcrumbHtmlRenderer(),
                new BreadcrumbOutboundQueue(host.Object)
            );
            await router.BindRowsAsync(
                new[] { "Clients" },
                Array.Empty<FolderScore>(),
                CancellationToken.None
            );
            await router.ProcessInboundAsync("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}");
            return router;
        }

        private static Task Confirm(BreadcrumbBridgeRouter router) =>
            router.ProcessInboundAsync("{\"type\":\"rowConfirmed\",\"rowId\":\"row-0\"}");
    }
}
