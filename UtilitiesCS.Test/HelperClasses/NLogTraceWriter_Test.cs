using System;
using System.Diagnostics;
using log4net;
using log4net.Repository.Hierarchy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Serialization;

namespace UtilitiesCS.Test.HelperClasses
{
    [TestClass]
    public class NLogTraceWriter_Test
    {
        private MockRepository mockRepository;
        private Mock<ILog> mockLogger;
        private Mock<NLogTraceWriter> mockTraceWriter;
        private System.IO.TextWriter originalOut;

        [TestInitialize]
        public void TestInitialize()
        {
            this.originalOut = Console.Out;
            Console.SetOut(new DebugTextWriter());
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockLogger = SetupLogger();
            this.mockTraceWriter = this.mockRepository.Create<NLogTraceWriter>();
            this.mockTraceWriter.CallBase = true;
            this.mockTraceWriter.SetupGet(x => x.Logger).Returns(this.mockLogger.Object);
        }

        internal Mock<ILog> SetupLogger()
        {
            var l = this.mockRepository.Create<ILog>();
            l.Setup(x => x.Error(It.IsAny<object>(), It.IsAny<Exception>()))
                .Callback<object, Exception>((message, ex) => PrintLogCall("Error", message, ex));
            l.Setup(x => x.Warn(It.IsAny<object>(), It.IsAny<Exception>()))
                .Callback<object, Exception>((message, ex) => PrintLogCall("Warn", message, ex));
            l.Setup(x => x.Info(It.IsAny<object>(), It.IsAny<Exception>()))
                .Callback<object, Exception>((message, ex) => PrintLogCall("Info", message, ex));
            l.Setup(x => x.Debug(It.IsAny<object>(), It.IsAny<Exception>()))
                .Callback<object, Exception>((message, ex) => PrintLogCall("Debug", message, ex));

            return l;
        }

        internal void PrintLogCall(string loggerName, object message, Exception ex)
        {
            Console.WriteLine($"Logger:    {loggerName}");
            Console.WriteLine($"Message:   {message}");
            Console.WriteLine($"Exception: {ex}");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Console.SetOut(this.originalOut);
        }

        [TestMethod]
        public void Trace_TraceLevel_Error()
        {
            // Arrange
            var traceWriter = this.mockTraceWriter.Object;
            var message = "Test Error Message";
            var ex = new Exception("Test Exception");

            // Act
            traceWriter.Trace(TraceLevel.Error, message, ex);

            // Assert
            mockLogger.Verify(x => x.Error(It.IsAny<object>(), It.IsAny<Exception>()), Times.Once);
        }

        [TestMethod]
        public void Trace_TraceLevel_Warning()
        {
            // Arrange
            var traceWriter = this.mockTraceWriter.Object;
            var message = "Test Warning Message";
            Exception ex = null;

            // Act
            traceWriter.Trace(TraceLevel.Warning, message, ex);

            // Assert
            mockLogger.Verify(x => x.Warn(It.IsAny<object>(), It.IsAny<Exception>()), Times.Once);
        }

        [TestMethod]
        public void Trace_TraceLevel_Info()
        {
            // Arrange
            var traceWriter = this.mockTraceWriter.Object;
            var message = "Test Info Message";
            Exception ex = null;

            // Act
            traceWriter.Trace(TraceLevel.Info, message, ex);

            // Assert
            mockLogger.Verify(x => x.Info(It.IsAny<object>(), It.IsAny<Exception>()), Times.Once);
        }

        [TestMethod]
        public void Trace_TraceLevel_Verbose()
        {
            // Arrange
            var traceWriter = this.mockTraceWriter.Object;
            var message = "Test Verbose Message";
            Exception ex = null;

            // Act
            traceWriter.Trace(TraceLevel.Verbose, message, ex);

            // Assert
            mockLogger.Verify(x => x.Debug(It.IsAny<object>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}
