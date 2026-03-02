using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS;
using UtilitiesCS.OutlookObjects.Store;

namespace UtilitiesCS.OutlookObjects.Store.UnitTests
{
    /// <summary>
    /// Unit tests for the StoresWrapper class.
    /// </summary>
    [TestClass]
    public class StoresWrapperTests
    {
        /// <summary>
        /// Tests that CreateAsync throws when a cancelled CancellationToken is provided.
        /// Expected: TaskCanceledException or OperationCanceledException should be thrown.
        /// </summary>
        [TestMethod]
        public async Task CreateAsync_CancelledToken_ThrowsTaskCanceledException()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            var cancelledTokenSource = new CancellationTokenSource();
            cancelledTokenSource.Cancel();
            var cancelledToken = cancelledTokenSource.Token;

            // Act
            Func<Task> act = async () => await StoresWrapper.CreateAsync(mockGlobals.Object, cancelledToken);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        /// <summary>
        /// Tests that CreateAsync throws NullReferenceException when null globals are provided.
        /// Expected: NullReferenceException should be thrown when Init() tries to access Globals.Ol.
        /// </summary>
        [TestMethod]
        public async Task CreateAsync_NullGlobals_ThrowsNullReferenceException()
        {
            // Arrange
            IApplicationGlobals nullGlobals = null;
            var cancellationToken = CancellationToken.None;

            // Act
            Func<Task> act = async () => await StoresWrapper.CreateAsync(nullGlobals, cancellationToken);

            // Assert
            await act.Should().ThrowAsync<NullReferenceException>();
        }

        /// <summary>
        /// Tests that CreateAsync can be invoked with default CancellationToken.
        /// Note: Full integration test requires Outlook COM objects which cannot be properly mocked.
        /// This test verifies the method can be called with valid parameters but may fail
        /// due to Outlook COM interop dependencies in the Init() method.
        /// To fully test this method, use integration tests with a real Outlook instance.
        /// Expected: Method should attempt to execute but may throw due to unmockable Outlook dependencies.
        /// </summary>
        [TestMethod]
        [Ignore("Requires Outlook COM objects that cannot be properly mocked. Use integration tests instead.")]
        public async Task CreateAsync_ValidGlobalsDefaultToken_ReturnsStoresWrapper()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();
            // Note: Cannot properly mock Globals.Ol.NamespaceMAPI.Stores due to COM interop constraints
            // Full test requires integration testing with actual Outlook instance
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await StoresWrapper.CreateAsync(mockGlobals.Object, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<StoresWrapper>();
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a valid instance.
        /// </summary>
        [TestMethod]
        public void StoresWrapper_ParameterlessConstructor_CreatesValidInstance()
        {
            // Act
            var result = new StoresWrapper();

            // Assert
            result.Should().NotBeNull();
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes default property values correctly.
        /// Verifies ExcludePublicFolderStores is set to true by default.
        /// </summary>
        [TestMethod]
        public void StoresWrapper_ParameterlessConstructor_InitializesExcludePublicFolderStoresToTrue()
        {
            // Act
            var result = new StoresWrapper();

            // Assert
            result.ExcludePublicFolderStores.Should().BeTrue();
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes default property values correctly.
        /// Verifies ExcludeGwsoStores is set to true by default.
        /// </summary>
        [TestMethod]
        public void StoresWrapper_ParameterlessConstructor_InitializesExcludeGwsoStoresToTrue()
        {
            // Act
            var result = new StoresWrapper();

            // Assert
            result.ExcludeGwsoStores.Should().BeTrue();
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes default property values correctly.
        /// Verifies GwsoFilePathContains contains expected default values.
        /// </summary>
        [TestMethod]
        public void StoresWrapper_ParameterlessConstructor_InitializesGwsoFilePathContainsWithDefaults()
        {
            // Act
            var result = new StoresWrapper();

            // Assert
            result.GwsoFilePathContains.Should().NotBeNull();
            result.GwsoFilePathContains.Should().HaveCount(2);
            result.GwsoFilePathContains.Should().Contain(@"\Google\Google Apps Sync\");
            result.GwsoFilePathContains.Should().Contain(@"\Google\Google Workspace Sync\");
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes default property values correctly.
        /// Verifies ExcludedStoreNameContains is initialized as an empty list.
        /// </summary>
        [TestMethod]
        public void StoresWrapper_ParameterlessConstructor_InitializesExcludedStoreNameContainsAsEmpty()
        {
            // Act
            var result = new StoresWrapper();

            // Assert
            result.ExcludedStoreNameContains.Should().NotBeNull();
            result.ExcludedStoreNameContains.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes default property values correctly.
        /// Verifies ExcludedStoreFilePathContains is initialized as an empty list.
        /// </summary>
        [TestMethod]
        public void StoresWrapper_ParameterlessConstructor_InitializesExcludedStoreFilePathContainsAsEmpty()
        {
            // Act
            var result = new StoresWrapper();

            // Assert
            result.ExcludedStoreFilePathContains.Should().NotBeNull();
            result.ExcludedStoreFilePathContains.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes Stores property to null.
        /// Verifies Stores collection is not initialized by the constructor.
        /// </summary>
        [TestMethod]
        public void StoresWrapper_ParameterlessConstructor_StoresPropertyIsNull()
        {
            // Act
            var result = new StoresWrapper();

            // Assert
            result.Stores.Should().BeNull();
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes Globals property to null.
        /// Verifies Globals is not initialized by the parameterless constructor.
        /// </summary>
        [TestMethod]
        public void StoresWrapper_ParameterlessConstructor_GlobalsPropertyIsNull()
        {
            // Act
            var result = new StoresWrapper();

            // Assert
            result.Globals.Should().BeNull();
        }

        /// <summary>
        /// Helper class to expose the internal RewireOlObjectsAsync method for testing.
        /// This class allows testing of internal methods without using reflection.
        /// </summary>
        private class TestableStoresWrapper : StoresWrapper
        {
            public TestableStoresWrapper(IApplicationGlobals globals) : base(globals)
            {
            }

            public new Task RewireOlObjectsAsync(StreamingContext context)
            {
                return base.RewireOlObjectsAsync(context);
            }
        }

        /// <summary>
        /// Tests that ShouldIncludeStore returns false when ExcludePublicFolderStores is true 
        /// and the store is a public folder.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludePublicFolderStoresTrue_StoreIsPublicFolder_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olExchangePublicFolder);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = true
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore continues processing when ExcludePublicFolderStores is false 
        /// and the store is a public folder.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludePublicFolderStoresFalse_StoreIsPublicFolder_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olExchangePublicFolder);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore continues processing when ExcludePublicFolderStores is true 
        /// but the store is not a public folder.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludePublicFolderStoresTrue_StoreIsNotPublicFolder_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = true,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore skips the name check when ExcludedStoreNameContains is null.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludedStoreNameContainsNull_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = null,
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore returns false when the store display name contains 
        /// an excluded string (case-insensitive match).
        /// </summary>
        [TestMethod]
        [DataRow("Excluded", "Test Excluded Store")]
        [DataRow("EXCLUDED", "Test excluded Store")]
        [DataRow("excluded", "Test EXCLUDED Store")]
        public void ShouldIncludeStore_ExcludedStoreNameContainsMatchingName_ReturnsFalse(string excludedPattern, string displayName)
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns(displayName);
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string> { excludedPattern },
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore continues processing when the store display name 
        /// does not contain any excluded strings.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludedStoreNameContainsNonMatchingName_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Valid Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string> { "Excluded", "Forbidden" },
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore skips empty or whitespace entries in ExcludedStoreNameContains.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludedStoreNameContainsEmptyOrWhitespace_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string> { "", "   ", "\t" },
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore handles null DisplayName gracefully when 
        /// ExcludedStoreNameContains is not null.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_StoreDisplayNameIsNull_ExcludedStoreNameContainsNotNull_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns((string)null);
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string> { "Excluded" },
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore handles exceptions when accessing FilePath property.
        /// The method should catch the exception and continue processing with null filePath.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_FilePathThrowsException_ContinuesProcessing_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Throws(new InvalidOperationException("FilePath not available"));

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string> { "SomeExcluded" },
                GwsoFilePathContains = new List<string> { "GWSO" },
                ExcludeGwsoStores = true
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore returns false when ExcludeGwsoStores is true 
        /// and the file path matches a GWSO pattern.
        /// </summary>
        [TestMethod]
        [DataRow(@"C:\Google\Google Apps Sync\mailbox.pst")]
        [DataRow(@"C:\Google\Google Workspace Sync\data.pst")]
        [DataRow(@"c:\google\google apps sync\test.pst")]
        public void ShouldIncludeStore_ExcludeGwsoStoresTrue_FilePathMatchesGwso_ReturnsFalse(string filePath)
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(filePath);

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>
                {
                    @"\Google\Google Apps Sync\",
                    @"\Google\Google Workspace Sync\"
                },
                ExcludeGwsoStores = true
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore continues processing when ExcludeGwsoStores is false 
        /// even if the file path matches a GWSO pattern.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludeGwsoStoresFalse_FilePathMatchesGwso_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Google\Google Apps Sync\mailbox.pst");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>
                {
                    @"\Google\Google Apps Sync\",
                    @"\Google\Google Workspace Sync\"
                },
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore skips GWSO check when filePath is null or whitespace.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludeGwsoStoresTrue_FilePathIsNullOrWhitespace_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns((string)null);

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = null,
                GwsoFilePathContains = new List<string> { @"\Google\Google Apps Sync\" },
                ExcludeGwsoStores = true
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore skips the file path check when ExcludedStoreFilePathContains is null.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludedStoreFilePathContainsNull_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = null,
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore returns false when the file path contains 
        /// an excluded string (case-insensitive match).
        /// </summary>
        [TestMethod]
        [DataRow(@"C:\Excluded\Path\store.pst", "Excluded")]
        [DataRow(@"C:\Test\FORBIDDEN\store.pst", "forbidden")]
        [DataRow(@"C:\temp\Bad\store.pst", "BAD")]
        public void ShouldIncludeStore_ExcludedStoreFilePathContainsMatchingPath_ReturnsFalse(string filePath, string excludedPattern)
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(filePath);

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string> { excludedPattern },
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore skips empty or whitespace entries in ExcludedStoreFilePathContains.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludedStoreFilePathContainsEmptyOrWhitespace_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string> { "", "   ", "\t" },
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore skips excluded file path check when filePath is null or whitespace.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludedStoreFilePathContainsNotNull_FilePathIsNullOrWhitespace_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns((string)null);

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string> { "Excluded" },
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore returns true when all exclusion checks pass.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_AllChecksPass_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Valid Store Name");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Valid\Path\store.pst");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = true,
                ExcludedStoreNameContains = new List<string> { "Excluded", "Forbidden" },
                ExcludedStoreFilePathContains = new List<string> { "Temp", "Bad" },
                GwsoFilePathContains = new List<string>
                {
                    @"\Google\Google Apps Sync\",
                    @"\Google\Google Workspace Sync\"
                },
                ExcludeGwsoStores = true
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore handles multiple matching entries in ExcludedStoreNameContains.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludedStoreNameContainsMultipleMatches_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Excluded Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string> { "Forbidden", "Excluded", "Bad" },
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore handles multiple matching entries in ExcludedStoreFilePathContains.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_ExcludedStoreFilePathContainsMultipleMatches_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Temp\Excluded\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string> { "Bad", "Temp", "Forbidden" },
                GwsoFilePathContains = new List<string>(),
                ExcludeGwsoStores = false
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that ShouldIncludeStore handles GwsoFilePathContains with empty or whitespace entries correctly.
        /// </summary>
        [TestMethod]
        public void ShouldIncludeStore_GwsoFilePathContainsEmptyOrWhitespace_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\Path");

            var wrapper = new StoresWrapper
            {
                ExcludePublicFolderStores = false,
                ExcludedStoreNameContains = new List<string>(),
                ExcludedStoreFilePathContains = new List<string>(),
                GwsoFilePathContains = new List<string> { "", "   ", "\t" },
                ExcludeGwsoStores = true
            };

            // Act
            var result = wrapper.ShouldIncludeStore(mockStore.Object);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that the constructor with a valid IApplicationGlobals instance
        /// successfully creates the object and sets the Globals property correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidGlobals_SetsGlobalsProperty()
        {
            // Arrange
            var mockGlobals = new Mock<IApplicationGlobals>();

            // Act
            var storesWrapper = new StoresWrapper(mockGlobals.Object);

            // Assert
            storesWrapper.Should().NotBeNull();
            storesWrapper.Globals.Should().BeSameAs(mockGlobals.Object);
        }

        /// <summary>
        /// Tests that the constructor with a null IApplicationGlobals parameter
        /// does not throw an exception and sets the Globals property to null.
        /// </summary>
        [TestMethod]
        public void Constructor_WithNullGlobals_SetsGlobalsToNull()
        {
            // Arrange
            IApplicationGlobals nullGlobals = null;

            // Act
            var storesWrapper = new StoresWrapper(nullGlobals);

            // Assert
            storesWrapper.Should().NotBeNull();
            storesWrapper.Globals.Should().BeNull();
        }

        /// <summary>
        /// Tests that a public folder store is excluded when excludePublicFolderStores is true.
        /// Input: Store with ExchangeStoreType = olExchangePublicFolder, excludePublicFolderStores = true.
        /// Expected: Returns false (store is excluded).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_PublicFolderStoreWithExcludeFlagTrue_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olExchangePublicFolder);
            mockStore.Setup(s => s.DisplayName).Returns("Public Folder");
            mockStore.Setup(s => s.FilePath).Returns("C:\\test.pst");

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                new List<string>(),
                excludePublicFolderStores: true,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a public folder store is included when excludePublicFolderStores is false.
        /// Input: Store with ExchangeStoreType = olExchangePublicFolder, excludePublicFolderStores = false.
        /// Expected: Returns true (store is included).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_PublicFolderStoreWithExcludeFlagFalse_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olExchangePublicFolder);
            mockStore.Setup(s => s.DisplayName).Returns("Public Folder");
            mockStore.Setup(s => s.FilePath).Returns("C:\\test.pst");

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a non-public folder store is included regardless of excludePublicFolderStores flag.
        /// Input: Store with ExchangeStoreType = olPrimaryExchangeMailbox, excludePublicFolderStores = true.
        /// Expected: Returns true (store is included).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_NonPublicFolderStoreWithExcludeFlagTrue_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Mailbox");
            mockStore.Setup(s => s.FilePath).Returns("C:\\mailbox.pst");

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                new List<string>(),
                excludePublicFolderStores: true,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is excluded when its DisplayName contains an excluded string.
        /// Input: Store with DisplayName = "Archive Store", excludedStoreNameContains = ["Archive"].
        /// Expected: Returns false (store is excluded).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_DisplayNameContainsExcludedString_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Archive Store");
            mockStore.Setup(s => s.FilePath).Returns("C:\\archive.pst");

            var excludedNames = new List<string> { "Archive" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a store is excluded with case-insensitive DisplayName matching.
        /// Input: Store with DisplayName = "Archive Store", excludedStoreNameContains = ["archive"].
        /// Expected: Returns false (store is excluded, case-insensitive match).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_DisplayNameMatchIsCaseInsensitive_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Archive Store");
            mockStore.Setup(s => s.FilePath).Returns("C:\\archive.pst");

            var excludedNames = new List<string> { "archive" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a store is included when DisplayName does not contain any excluded string.
        /// Input: Store with DisplayName = "Main Store", excludedStoreNameContains = ["Archive"].
        /// Expected: Returns true (store is included).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_DisplayNameDoesNotContainExcludedString_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Main Store");
            mockStore.Setup(s => s.FilePath).Returns("C:\\main.pst");

            var excludedNames = new List<string> { "Archive" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when excludedStoreNameContains is null.
        /// Input: Store with DisplayName = "Test Store", excludedStoreNameContains = null.
        /// Expected: Returns true (store is included).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_ExcludedStoreNameContainsIsNull_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns("C:\\test.pst");

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when excludedStoreNameContains is empty.
        /// Input: Store with DisplayName = "Test Store", excludedStoreNameContains = [].
        /// Expected: Returns true (store is included).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_ExcludedStoreNameContainsIsEmpty_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns("C:\\test.pst");

            var excludedNames = new List<string>();

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when excludedStoreNameContains contains only whitespace strings.
        /// Input: Store with DisplayName = "Test Store", excludedStoreNameContains = ["", "  ", null].
        /// Expected: Returns true (store is included, whitespace entries are ignored).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_ExcludedStoreNameContainsOnlyWhitespace_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns("C:\\test.pst");

            var excludedNames = new List<string> { "", "  ", null };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when DisplayName is null.
        /// Input: Store with DisplayName = null, excludedStoreNameContains = ["Test"].
        /// Expected: Returns true (store is included, null DisplayName doesn't match any exclusion).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_DisplayNameIsNull_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns((string)null);
            mockStore.Setup(s => s.FilePath).Returns("C:\\test.pst");

            var excludedNames = new List<string> { "Test" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is excluded when FilePath contains an excluded GWSO path string.
        /// Input: Store with FilePath containing GWSO path, excludeGwsoStores = true, gwsoFilePathContains = [GWSO marker].
        /// Expected: Returns false (store is excluded).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_FilePathContainsGwsoPathWithExcludeFlagTrue_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("GWSO Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Users\Test\Google\Google Apps Sync\mailbox.ost");

            var gwsoPaths = new List<string> { @"\Google\Google Apps Sync\" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                gwsoPaths,
                excludePublicFolderStores: false,
                excludeGwsoStores: true);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a store is included when FilePath contains GWSO path but excludeGwsoStores is false.
        /// Input: Store with FilePath containing GWSO path, excludeGwsoStores = false.
        /// Expected: Returns true (store is included).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_FilePathContainsGwsoPathWithExcludeFlagFalse_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("GWSO Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Users\Test\Google\Google Apps Sync\mailbox.ost");

            var gwsoPaths = new List<string> { @"\Google\Google Apps Sync\" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                gwsoPaths,
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is excluded when FilePath contains an excluded file path string.
        /// Input: Store with FilePath = "C:\Temp\data.pst", excludedStoreFilePathContains = ["Temp"].
        /// Expected: Returns false (store is excluded).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_FilePathContainsExcludedString_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Temp Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Temp\data.pst");

            var excludedPaths = new List<string> { "Temp" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that file path matching is case-insensitive.
        /// Input: Store with FilePath = "C:\TEMP\data.pst", excludedStoreFilePathContains = ["temp"].
        /// Expected: Returns false (store is excluded, case-insensitive match).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_FilePathMatchIsCaseInsensitive_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Temp Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\TEMP\data.pst");

            var excludedPaths = new List<string> { "temp" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a store is included when FilePath does not contain any excluded string.
        /// Input: Store with FilePath = "C:\Data\store.pst", excludedStoreFilePathContains = ["Temp"].
        /// Expected: Returns true (store is included).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_FilePathDoesNotContainExcludedString_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Data Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Data\store.pst");

            var excludedPaths = new List<string> { "Temp" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when FilePath property throws an exception.
        /// Input: Store where FilePath throws exception, excludedStoreFilePathContains = ["Test"].
        /// Expected: Returns true (store is included, exception is caught and filePath remains null).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_FilePathThrowsException_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Exception Store");
            mockStore.Setup(s => s.FilePath).Throws(new InvalidOperationException("FilePath not available"));

            var excludedPaths = new List<string> { "Test" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when excludedStoreFilePathContains is null.
        /// Input: Store with valid FilePath, excludedStoreFilePathContains = null.
        /// Expected: Returns true (store is included).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_ExcludedStoreFilePathContainsIsNull_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Data\test.pst");

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when excludedStoreFilePathContains contains only whitespace strings.
        /// Input: Store with FilePath, excludedStoreFilePathContains = ["", "  ", null].
        /// Expected: Returns true (store is included, whitespace entries are ignored).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_ExcludedStoreFilePathContainsOnlyWhitespace_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Data\test.pst");

            var excludedPaths = new List<string> { "", "  ", null };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when FilePath is null and excludedStoreFilePathContains has values.
        /// Input: Store with FilePath = null, excludedStoreFilePathContains = ["Test"].
        /// Expected: Returns true (store is included, null FilePath doesn't match exclusions).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_FilePathIsNull_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns((string)null);

            var excludedPaths = new List<string> { "Test" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when FilePath is empty string and excludedStoreFilePathContains has values.
        /// Input: Store with FilePath = "", excludedStoreFilePathContains = ["Test"].
        /// Expected: Returns true (store is included, empty FilePath doesn't match exclusions).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_FilePathIsEmptyString_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns(string.Empty);

            var excludedPaths = new List<string> { "Test" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when FilePath is whitespace and excludedStoreFilePathContains has values.
        /// Input: Store with FilePath = "  ", excludedStoreFilePathContains = ["Test"].
        /// Expected: Returns true (store is included, whitespace FilePath doesn't match exclusions).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_FilePathIsWhitespace_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Test Store");
            mockStore.Setup(s => s.FilePath).Returns("  ");

            var excludedPaths = new List<string> { "Test" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests multiple exclusion conditions applied together.
        /// Input: Public folder store with excluded DisplayName.
        /// Expected: Returns false (store is excluded by public folder type first).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_MultipleExclusionConditions_ReturnsFalseOnFirstMatch()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olExchangePublicFolder);
            mockStore.Setup(s => s.DisplayName).Returns("Archive Public Folder");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Temp\archive.pst");

            var excludedNames = new List<string> { "Archive" };
            var excludedPaths = new List<string> { "Temp" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: true,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a store is excluded by DisplayName when public folder check passes.
        /// Input: Non-public store with excluded DisplayName.
        /// Expected: Returns false (store is excluded by DisplayName).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_ExcludedByDisplayNameAfterPublicFolderCheck_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Archive Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Data\archive.pst");

            var excludedNames = new List<string> { "Archive" };
            var excludedPaths = new List<string> { "Temp" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: true,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a store is excluded by file path when DisplayName check passes.
        /// Input: Store with valid DisplayName but excluded FilePath.
        /// Expected: Returns false (store is excluded by FilePath).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_ExcludedByFilePathAfterDisplayNameCheck_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Valid Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Temp\data.pst");

            var excludedNames = new List<string> { "Archive" };
            var excludedPaths = new List<string> { "Temp" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a store passes all exclusion checks and is included.
        /// Input: Valid store that doesn't match any exclusion criteria.
        /// Expected: Returns true (store is included).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_AllChecksPass_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Main Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Data\main.pst");

            var excludedNames = new List<string> { "Archive" };
            var excludedPaths = new List<string> { "Temp" };
            var gwsoPaths = new List<string> { @"\Google\Google Apps Sync\" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                excludedPaths,
                gwsoPaths,
                excludePublicFolderStores: true,
                excludeGwsoStores: true);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests GWSO exclusion with case-insensitive path matching.
        /// Input: Store with GWSO path in mixed case, gwsoFilePathContains with lowercase.
        /// Expected: Returns false (store is excluded, case-insensitive match).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_GwsoPathMatchIsCaseInsensitive_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("GWSO Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Users\Test\GOOGLE\GOOGLE APPS SYNC\mailbox.ost");

            var gwsoPaths = new List<string> { @"\google\google apps sync\" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                gwsoPaths,
                excludePublicFolderStores: false,
                excludeGwsoStores: true);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that a store is included when gwsoFilePathContains is empty.
        /// Input: Store with GWSO-like path, gwsoFilePathContains = [], excludeGwsoStores = true.
        /// Expected: Returns true (store is included, no GWSO paths to match).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_GwsoFilePathContainsIsEmpty_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Users\Test\Google\Google Apps Sync\mailbox.ost");

            var gwsoPaths = new List<string>();

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                gwsoPaths,
                excludePublicFolderStores: false,
                excludeGwsoStores: true);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that a store is included when gwsoFilePathContains has only whitespace entries.
        /// Input: Store with GWSO-like path, gwsoFilePathContains = ["", "  "], excludeGwsoStores = true.
        /// Expected: Returns true (store is included, whitespace entries are ignored).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_GwsoFilePathContainsOnlyWhitespace_ReturnsTrue()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Users\Test\Google\Google Apps Sync\mailbox.ost");

            var gwsoPaths = new List<string> { "", "  ", null };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                null,
                gwsoPaths,
                excludePublicFolderStores: false,
                excludeGwsoStores: true);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests exclusion list with multiple entries where one matches DisplayName.
        /// Input: Store with DisplayName = "My Archive", excludedStoreNameContains = ["Temp", "Archive", "Old"].
        /// Expected: Returns false (store is excluded by second entry "Archive").
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_MultipleExcludedNamesOneMatches_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("My Archive");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Data\archive.pst");

            var excludedNames = new List<string> { "Temp", "Archive", "Old" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests exclusion list with multiple entries where one matches FilePath.
        /// Input: Store with FilePath containing "Test", excludedStoreFilePathContains = ["Temp", "Test", "Old"].
        /// Expected: Returns false (store is excluded by second entry "Test").
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_MultipleExcludedPathsOneMatches_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Test\data.pst");

            var excludedPaths = new List<string> { "Temp", "Test", "Old" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that partial string matching works for DisplayName.
        /// Input: Store with DisplayName = "2023 Archive Store", excludedStoreNameContains = ["Archive"].
        /// Expected: Returns false (store is excluded, "Archive" is substring of DisplayName).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_PartialStringMatchInDisplayName_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("2023 Archive Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\Data\archive.pst");

            var excludedNames = new List<string> { "Archive" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                excludedNames,
                null,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that partial string matching works for FilePath.
        /// Input: Store with FilePath = "C:\MyTemp\Folder\data.pst", excludedStoreFilePathContains = ["Temp"].
        /// Expected: Returns false (store is excluded, "Temp" is substring of FilePath).
        /// </summary>
        [TestMethod]
        public void StoreIsIncluded_PartialStringMatchInFilePath_ReturnsFalse()
        {
            // Arrange
            var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
            mockStore.Setup(s => s.ExchangeStoreType).Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
            mockStore.Setup(s => s.DisplayName).Returns("Store");
            mockStore.Setup(s => s.FilePath).Returns(@"C:\MyTemp\Folder\data.pst");

            var excludedPaths = new List<string> { "Temp" };

            // Act
            var result = StoresWrapper.StoreIsIncluded(
                mockStore.Object,
                null,
                excludedPaths,
                new List<string>(),
                excludePublicFolderStores: false,
                excludeGwsoStores: false);

            // Assert
            result.Should().BeFalse();
        }
    }
}