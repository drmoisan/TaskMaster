using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using UtilitiesCS.NewtonsoftHelpers.Sco;
using Newtonsoft.Json;
using UtilitiesCS.ReusableTypeClasses;
using System.Windows.Input;
using FluentAssertions;
using UtilitiesCS.NewtonsoftHelpers;
using System.Threading.Tasks;
using static System.Resources.ResXFileRef;

namespace UtilitiesCS.Test.NewtonsoftHelpers
{
    [TestClass]
    public class ScoDictionaryConverterTests
    {
        private MockRepository mockRepository;
        private Mock<Microsoft.Office.Interop.Outlook.Application> mockApplication;
        private IApplicationGlobals globals;


        [TestInitialize]
        public void TestInitialize()
        {
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            mockApplication = mockRepository.Create<Microsoft.Office.Interop.Outlook.Application>();
            globals = new TaskMaster.ApplicationGlobals(mockApplication.Object, true);

        }

        internal class TestDerived : ScoDictionaryNew<string, int>
        {
            public string AdditionalField1 { get; set; }
            private int AdditionalField2;
            private string _additionalField3;
            public string AdditionalField3 { get => _additionalField3; set => _additionalField3 = value; }

            public TestDerived() { }

            public TestDerived Init(IApplicationGlobals globals)
            {
                AdditionalField1 = "Test";
                AdditionalField2 = 42;
                AdditionalField3 = "Test3";
                this.TryAdd("key1", 1);
                this.TryAdd("key2", 2);

                var settings = GetSettings(globals);                
                this.Config.JsonSettings = settings;
                return this;
            }

            public int GetAdditionalField2() => AdditionalField2;

            public static JsonSerializerSettings GetJsonSettings(IApplicationGlobals globals) { return new TestDerived().GetSettings(globals); }
            private JsonSerializerSettings GetSettings(IApplicationGlobals globals)
            {
                var settings = new JsonSerializerSettings()
                {
                    //TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    TraceWriter = new NLogTraceWriter()
                };
                settings.Converters.Add(new AppGlobalsConverter(globals));
                settings.Converters.Add(new FilePathHelperConverter(globals.FS));
                
                return settings;
            }


        }



        //[TestMethod]
        //public void ReadJson_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var scoDictionaryConverter = this.CreateScoDictionaryConverter();
        //    JsonReader reader = null;
        //    Type typeToConvert = null;
        //    TDerived existingValue = null;
        //    bool hasExistingValue = false;
        //    JsonSerializer serializer = null;

        //    // Act
        //    var result = scoDictionaryConverter.ReadJson(
        //        reader,
        //        typeToConvert,
        //        existingValue,
        //        hasExistingValue,
        //        serializer);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        [TestMethod]
        public void TypedConverter_IntegrationTest_SerializeAndDeserialize()
        {
            // Arrange
            var expected = new TestDerived().Init(globals);
            expected.Config.JsonSettings.Converters.Add(new ScoDictionaryConverter<TestDerived, string, int>());
            
            // Act
            var json = expected.SerializeToString();
            Console.WriteLine(json);

            // Sequential actions to do without custom converter
            // var wrap = JsonConvert.DeserializeObject<WrapperScoDictionary<TestDerived, string, int>>(json, settings);
            // var actual = wrap.ToDerived();

            // Direct action with custom converter
            //var actual = JsonConvert.DeserializeObject<TestDerived>(json, settings);
            
            // Static class deserialization with custom converter
            var settings = TestDerived.GetJsonSettings(globals);
            settings.Converters.Add(new ScoDictionaryConverter<TestDerived, string, int>());
            var actual = SmartSerializable.DeserializeObject<TestDerived>(json, settings);

            // Assert

            actual.Should().BeEquivalentTo(expected);
                        
        }

        [TestMethod]
        public void UntypedConverter_IntegrationTest_SerializeAndDeserialize()
        {
            // Arrange
            var expected = new TestDerived().Init(globals);
            expected.Config.JsonSettings.Converters.Add(new ScoDictionaryConverter());

            // Act
            var json = expected.SerializeToString();
            Console.WriteLine(json);

            // Sequential actions to do without custom converter
            // var wrap = JsonConvert.DeserializeObject<WrapperScoDictionary<TestDerived, string, int>>(json, settings);
            // var actual = wrap.ToDerived();

            // Direct action with custom converter
            //var actual = JsonConvert.DeserializeObject<TestDerived>(json, settings);

            // Static class deserialization with custom converter
            var settings = TestDerived.GetJsonSettings(globals);
            settings.Converters.Add(new ScoDictionaryConverter());
            var actual = SmartSerializable.DeserializeObject<TestDerived>(json, settings);

            // Assert

            actual.Should().BeEquivalentTo(expected);

        }

        [TestMethod]
        public void WriteJson_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var expected = new TestDerived().Init(globals);

            //// Act
            //scoDictionaryConverter.WriteJson(
            //    writer,
            //    value,
            //    serializer);

            //// Assert
            //Assert.Fail();
            //this.mockRepository.VerifyAll();
        }
    }
}
