using System;
using System.Runtime.InteropServices;
using Exchange.Export.MAPIMessageConverter;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public class MAPIMethodsTests
    {
        [TestMethod]
        public void GuidFields_ShouldMatchKnownConverterSessionIdentifiers()
        {
            MAPIMethods
                .CLSID_IConverterSession.Should()
                .Be(new Guid("4e3a7680-b77a-11d0-9da5-00c04fd65685"));
            MAPIMethods
                .IID_IConverterSession.Should()
                .Be(new Guid("4b401570-b77b-11d0-9da5-00c04fd65685"));
        }

        [TestMethod]
        public void InterfaceAndEnumTypes_ShouldExposeExpectedInteropMetadata()
        {
            typeof(MAPIMethods.IConverterSession).IsInterface.Should().BeTrue();
            typeof(MAPIMethods.IMessage)
                .GetCustomAttributes(typeof(ComImportAttribute), inherit: false)
                .Should()
                .ContainSingle();
            typeof(MAPIMethods.MAPITOMIMEFLAGS).IsEnum.Should().BeTrue();
            typeof(MAPIMethods.CLSCTX).IsEnum.Should().BeTrue();
            MAPIMethods
                .CLSCTX.CLSCTX_ALL.HasFlag(MAPIMethods.CLSCTX.CLSCTX_REMOTE_SERVER)
                .Should()
                .BeTrue();
            ((int)MAPIMethods.ENCODINGTYPE.IET_LAST).Should().Be(13);
            ((int)MAPIMethods.MIMESAVETYPE.SAVE_RFC1521).Should().Be(1);
        }
    }
}
