using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Ribbon
{
    /// <summary>
    /// Regression tests for the Explorer-ribbon command boundary. A ribbon callback is an
    /// <c>async void</c> handler, so an exception escaping it reaches Outlook as an unhandled
    /// exception rather than a diagnosable message.
    /// </summary>
    /// <remarks>
    /// The TaskMaster test assembly carries no reference to the QuickFiler assembly, so no test in
    /// this file depends on any QuickFiler type. The boundary is exercised through injected sinks
    /// and a supplied <see cref="Func{Task}"/>, which is host-neutral.
    /// </remarks>
    [TestClass]
    public class RibbonCommandBoundaryTests
    {
        private const string CommandName = "QuickFiler";

        /// <summary>Builds a boundary that records each sink invocation.</summary>
        private static RibbonCommandBoundary BuildBoundary(
            List<string> logged,
            List<string> presented
        ) =>
            new RibbonCommandBoundary(
                (commandName, failure) => logged.Add($"{commandName}|{failure.Message}"),
                message => presented.Add(message)
            );

        /// <summary>
        /// Runs the boundary and absorbs anything it propagates, so a test asserting sink behaviour
        /// is not masked by the propagation defect that
        /// <see cref="RunAsync_ActionThrows_DoesNotPropagateToCaller"/> pins on its own.
        /// </summary>
        private static async Task RunAbsorbingAsync(
            RibbonCommandBoundary boundary,
            Func<Task> action
        )
        {
            try
            {
                await boundary.RunAsync(CommandName, action);
            }
            catch (System.Exception)
            {
                // Deliberately absorbed; propagation is a sibling test's assertion.
            }
        }

        /// <summary>An action that fails with a distinctive message.</summary>
        private static Func<Task> FailingAction(string message) =>
            () => Task.FromException(new InvalidOperationException(message));

        /// <summary>AC5: a command that succeeds must reach neither failure sink.</summary>
        [TestMethod]
        public async Task RunAsync_ActionSucceeds_InvokesNeitherSink()
        {
            // Arrange
            var logged = new List<string>();
            var presented = new List<string>();
            var boundary = BuildBoundary(logged, presented);

            // Act
            await boundary.RunAsync(CommandName, () => Task.CompletedTask);

            // Assert
            logged.Should().BeEmpty("a successful command is not a failure");
            presented.Should().BeEmpty("a successful command must show no error dialog");
        }

        /// <summary>AC5: a failing command must be logged exactly once, with full detail.</summary>
        [TestMethod]
        public async Task RunAsync_ActionThrows_InvokesLogSinkOnce()
        {
            // Arrange
            var logged = new List<string>();
            var presented = new List<string>();
            var boundary = BuildBoundary(logged, presented);

            // Act
            await RunAbsorbingAsync(boundary, FailingAction("command failure"));

            // Assert
            logged
                .Should()
                .ContainSingle("the boundary must log the failure exactly once")
                .Which.Should()
                .Contain("command failure", "the log entry must carry the exception detail");
        }

        /// <summary>AC5: a failing command must reach the presentation sink exactly once.</summary>
        [TestMethod]
        public async Task RunAsync_ActionThrows_InvokesPresentationSinkOnce()
        {
            // Arrange
            var logged = new List<string>();
            var presented = new List<string>();
            var boundary = BuildBoundary(logged, presented);

            // Act
            await RunAbsorbingAsync(boundary, FailingAction("command failure"));

            // Assert
            presented.Should().ContainSingle("the user must be told once that the command failed");
        }

        /// <summary>
        /// AC5: the boundary must contain the failure. An exception escaping here would reach
        /// Outlook through an <c>async void</c> ribbon callback as an unhandled exception.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_ActionThrows_DoesNotPropagateToCaller()
        {
            // Arrange
            var logged = new List<string>();
            var presented = new List<string>();
            var boundary = BuildBoundary(logged, presented);

            // Act
            Func<Task> act = () => boundary.RunAsync(CommandName, FailingAction("command failure"));

            // Assert
            await act.Should()
                .NotThrowAsync("an async void ribbon callback cannot observe a propagated failure");
        }

        /// <summary>
        /// AC5: a presentation sink that itself fails must be contained too, so a broken dialog
        /// cannot convert a handled command failure back into an unhandled exception.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate()
        {
            // Arrange
            var logged = new List<string>();
            var boundary = new RibbonCommandBoundary(
                (commandName, failure) => logged.Add(commandName),
                message => throw new InvalidOperationException("presentation sink failure")
            );

            // Act
            Func<Task> act = () => boundary.RunAsync(CommandName, FailingAction("command failure"));

            // Assert
            await act.Should()
                .NotThrowAsync("a failing presentation sink must not escape the boundary");
        }

        /// <summary>
        /// AC11: an <see cref="AggregateException"/> must be presented with its inner detail. The
        /// wrapper's own message, "One or more errors occurred.", tells the user nothing.
        /// </summary>
        [TestMethod]
        public async Task RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail()
        {
            // Arrange
            var logged = new List<string>();
            var presented = new List<string>();
            var boundary = BuildBoundary(logged, presented);
            var inner = new InvalidOperationException("inner detail 798");

            // Act
            await RunAbsorbingAsync(
                boundary,
                () => Task.FromException(new AggregateException(inner))
            );

            // Assert
            presented
                .Should()
                .ContainSingle("the failure must be presented once")
                .Which.Should()
                .Contain("inner detail 798", "the dialog must render inner exception detail")
                .And.NotBe(
                    "One or more errors occurred.",
                    "the AggregateException summary alone is not actionable"
                );
        }

        /// <summary>The three QuickFiler-family handlers AC5 routes through the boundary.</summary>
        private static readonly string[] NamedQuickFilerHandlers =
        {
            "QuickFiler_Click",
            "QuickFilerHighConfidence_Click",
            "SortEmail_Click",
        };

        /// <summary>
        /// AC5/AC11: the three named handlers must keep the awaited <c>async void</c> shape Office
        /// binds against, and <c>RibbonViewer</c> must hold the boundary they route through.
        /// </summary>
        /// <remarks>
        /// VSTO binds ribbon callbacks by name and signature and silently ignores a mismatch, so a
        /// shape defect is invisible at runtime. The compiler-emitted
        /// <see cref="AsyncStateMachineAttribute"/> is the observable proof that a handler awaits
        /// rather than discarding the returned task.
        /// </remarks>
        [TestMethod]
        public void NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary()
        {
            // Arrange
            var viewer = typeof(RibbonViewer);

            // Act / Assert
            foreach (var handlerName in NamedQuickFilerHandlers)
            {
                var handler = viewer.GetMethod(
                    handlerName,
                    BindingFlags.Public | BindingFlags.Instance
                );
                handler.Should().NotBeNull("'{0}' must exist on RibbonViewer", handlerName);
                handler
                    .ReturnType.Should()
                    .Be(
                        typeof(void),
                        "'{0}' is an Office callback and must return void",
                        handlerName
                    );
                handler
                    .GetCustomAttribute<AsyncStateMachineAttribute>()
                    .Should()
                    .NotBeNull(
                        "'{0}' must await its work rather than discarding the returned Task",
                        handlerName
                    );
            }

            var boundaryFields = viewer
                .GetFields(
                    BindingFlags.Public
                        | BindingFlags.NonPublic
                        | BindingFlags.Instance
                        | BindingFlags.Static
                )
                .Where(field => field.FieldType == typeof(RibbonCommandBoundary))
                .ToList();
            boundaryFields
                .Should()
                .NotBeEmpty("RibbonViewer must hold the boundary the three handlers route through");
        }
    }
}
