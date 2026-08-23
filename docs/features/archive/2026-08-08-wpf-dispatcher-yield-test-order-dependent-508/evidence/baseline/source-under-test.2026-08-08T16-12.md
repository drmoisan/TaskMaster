# Baseline Source Under Test (verbatim, pre-change)

Timestamp: 2026-08-08T16-12

Task: [P0-T4]

Captured at HEAD `003c5715055d7d1933db68a742531332756e30b2` with both scoped git gates empty
(see `repo-state.2026-08-08T16-11.md`), so these contents are the merge-base contents.

## 1. `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` — 44 lines

```csharp
#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace UtilitiesCS.OutlookObjects.Folder
{
    /// <summary>
    /// Yields folder tree work through the captured UI dispatcher.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class WpfDispatcherYield : IDispatcherYield
    {
        public async Task YieldAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Prefer the dispatcher already affinitized to this thread so a traversal that the
            // service marshalled onto a captured dispatcher keeps yielding through that same
            // dispatcher. Only a worker thread with no dispatcher of its own falls back to the
            // process-global UI dispatcher, which is the case Dispatcher.Yield() could not serve.
            // UiThread.Dispatcher is set-once state populated by UiThread.Init() and is null
            // outside a live host, so that null state is surfaced as InvalidOperationException to
            // preserve the strict contract callers relied on.
            Dispatcher dispatcher =
                Dispatcher.FromThread(Thread.CurrentThread) ?? UtilitiesCS.UiThread.Dispatcher;
            if (dispatcher is null)
            {
                throw new InvalidOperationException(
                    "The UI dispatcher has not been captured. Call UiThread.Init() before yielding folder tree work."
                );
            }

            await dispatcher.InvokeAsync(
                () => { },
                DispatcherPriority.Background,
                cancellationToken
            );
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
```

Facts fixed by this capture, binding on P2-T14:

- Line 3: `using System.Diagnostics.CodeAnalysis;` (present pre-change, to be removed by P1-T7).
- Line 13: `[ExcludeFromCodeCoverage]` (present pre-change, to be removed by P1-T7).
- Lines 27-28: pre-change resolution expression
  `Dispatcher.FromThread(Thread.CurrentThread) ?? UtilitiesCS.UiThread.Dispatcher`. These are the
  exact expressions the P1-T4 default delegates must reproduce.
- Lines 31-33: exception message text that P1-T5 must keep byte-identical:
  `"The UI dispatcher has not been captured. Call UiThread.Init() before yielding folder tree work."`
- The class declares no constructor pre-change, so it currently has an implicit public
  parameterless constructor. Adding any constructor removes it; P1-T3 restores it explicitly.
- Public surface pre-change: the type `WpfDispatcherYield` (public sealed, implements
  `IDispatcherYield`), the implicit parameterless constructor, and `public async Task YieldAsync(CancellationToken)`.

## 2. `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` — 39 lines

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS.OutlookObjects.Folder;

namespace UtilitiesCS.Test.OutlookObjects.Folder
{
    [TestClass]
    public sealed class WpfDispatcherYieldTests
    {
        [TestMethod]
        public async Task YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield()
        {
            var dispatcherYield = new WpfDispatcherYield();
            using (var source = new CancellationTokenSource())
            {
                source.Cancel();

                await dispatcherYield
                    .Invoking(item => item.YieldAsync(source.Token))
                    .Should()
                    .ThrowAsync<OperationCanceledException>();
            }
        }

        [TestMethod]
        public async Task YieldAsync_WithoutDispatcher_RemainsStrict()
        {
            var dispatcherYield = new WpfDispatcherYield();

            await dispatcherYield
                .Invoking(item => item.YieldAsync(CancellationToken.None))
                .Should()
                .ThrowAsync<InvalidOperationException>();
        }
    }
}
```

The order-dependent test is `YieldAsync_WithoutDispatcher_RemainsStrict` at lines 28-37. It
constructs `new WpfDispatcherYield()` and asserts a throw without arranging either operand of the
`??`, which is the defect.

Note: this file has no `#nullable enable` pre-change (P1-T8 adds it).

## 3. `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21` — parallelization attribute

```csharp
[assembly: Parallelize(
    Workers = 0,
    Scope = Microsoft.VisualStudio.TestTools.UnitTesting.ExecutionScope.ClassLevel
)]
```

`Workers = 0` means "use the processor count", and `Scope = ClassLevel` means test classes run
concurrently. This is the condition under which the executing thread for
`YieldAsync_WithoutDispatcher_RemainsStrict` is nondeterministic. This file is out of scope and
must not be modified.

Output Summary: Captured verbatim pre-change contents of the 44-line production file, the 39-line
test file, and the four-line `Parallelize` attribute. Recorded the byte-identical exception message,
the two `??` operand expressions, and the fact that the pre-change class has only an implicit
parameterless constructor — the three facts P2-T14 compares against.
