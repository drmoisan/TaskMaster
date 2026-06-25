using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Folder;

namespace TaskMaster.Test.AppGlobals
{
    [TestClass]
    public sealed class AppOlObjectsFolderTreeServiceTests
    {
        [TestMethod]
        public void FolderTreeService_ReturnsSingleSessionScopedInstance()
        {
            var service = new Mock<IOutlookFolderTreeService>(MockBehavior.Strict);
            var sut = new TestableAppOlObjects(service.Object);

            var first = sut.FolderTreeService;
            var second = sut.FolderTreeService;

            first.Should().BeSameAs(service.Object);
            second.Should().BeSameAs(first);
            sut.LoadCount.Should().Be(1);
            service.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Dispose_DisposesCachedFolderTreeServiceOnce()
        {
            var service = new Mock<IOutlookFolderTreeService>(MockBehavior.Strict);
            service.Setup(x => x.Dispose());
            var sut = new TestableAppOlObjects(service.Object);

            _ = sut.FolderTreeService;
            sut.Dispose();
            sut.Dispose();

            service.Verify(x => x.Dispose(), Times.Once);
            service.VerifyNoOtherCalls();
        }

        private sealed class TestableAppOlObjects : AppOlObjects
        {
            private readonly IOutlookFolderTreeService _service;

            internal TestableAppOlObjects(IOutlookFolderTreeService service)
                : base(null, Mock.Of<IApplicationGlobals>())
            {
                _service = service ?? throw new ArgumentNullException(nameof(service));
            }

            internal int LoadCount { get; private set; }

            protected internal override IOutlookFolderTreeService LoadFolderTreeService()
            {
                LoadCount++;
                return _service;
            }
        }
    }
}
