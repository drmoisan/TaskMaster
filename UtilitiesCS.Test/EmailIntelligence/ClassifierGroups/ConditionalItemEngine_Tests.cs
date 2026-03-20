using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS.Test.EmailIntelligence.ClassifierGroups
{
    [TestClass]
    public class ConditionalItemEngine_Tests
    {
        [TestMethod]
        public void DefaultConstructor_CreatesInstance()
        {
            var engine = new ConditionalItemEngine<string>();
            engine.Should().NotBeNull();
        }

        [TestMethod]
        public void ParameterizedConstructor_SetsAllProperties()
        {
            Func<object, Task<bool>> condition = _ => Task.FromResult(true);
            Func<string, Task> action = _ => Task.CompletedTask;

            var engine = new ConditionalItemEngine<string>(
                new object(),
                "TestEngine",
                condition,
                action,
                "Test message"
            );

            engine.EngineName.Should().Be("TestEngine");
            engine.Message.Should().Be("Test message");
            engine.Engine.Should().NotBeNull();
            engine.AsyncCondition.Should().BeSameAs(condition);
            engine.AsyncAction.Should().BeSameAs(action);
        }

        [TestMethod]
        public void Properties_SetAndGet_RoundTrip()
        {
            var engine = new ConditionalItemEngine<string>();
            engine.EngineName = "TestName";
            engine.EngineName.Should().Be("TestName");

            engine.Message = "msg";
            engine.Message.Should().Be("msg");

            engine.Engine = "eng";
            engine.Engine.Should().Be("eng");

            engine.TypedItem = "item";
            engine.TypedItem.Should().Be("item");
        }

        [TestMethod]
        public void Serialize_WithEngineSet_InvokesAction()
        {
            var engine = new ConditionalItemEngine<string>();
            bool called = false;
            engine.SerializationEngine = () => called = true;

            engine.Serialize();
            called.Should().BeTrue();
        }

        [TestMethod]
        public void Serialize_WithoutEngine_DoesNotThrow()
        {
            var engine = new ConditionalItemEngine<string>();
            System.Action act = () => engine.Serialize();
            act.Should().NotThrow();
        }

        [TestMethod]
        public void Config_SetAndGet()
        {
            var engine = new ConditionalItemEngine<string>();
            var mockConfig = new Mock<ISmartSerializableConfig>();
            engine.Config = mockConfig.Object;
            engine.Config.Should().BeSameAs(mockConfig.Object);
        }

        [TestMethod]
        public void EngineInitializer_SetAndGet()
        {
            var engine = new ConditionalItemEngine<string>();
            Func<IApplicationGlobals, Task> initializer = _ => Task.CompletedTask;
            engine.EngineInitializer = initializer;
            engine.EngineInitializer.Should().BeSameAs(initializer);
        }
    }

    [TestClass]
    public class TristateThreshhold_Tests
    {
        [TestMethod]
        public void Constructor_SetsValues()
        {
            var t = new TristateThreshhold(0.7, 0.3);
            t.MinimumTrue.Should().Be(0.7);
            t.MaximumFalse.Should().Be(0.3);
        }
    }
}
