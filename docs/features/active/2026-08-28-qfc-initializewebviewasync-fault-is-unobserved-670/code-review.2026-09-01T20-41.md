# Code Review — issue #670 (`bug/qfc-initializewebviewasync-fault-is-unobserved-670`)

- **Timestamp:** 2026-09-01T20-41
- **HEAD:** `bb4dbaade9c9a90c0e1e5c61ea78041aa0c1892f`
- **Base:** `988d35a8f8eb7436cc46a9f6424db917ed93807a`
- **Verdict:** PASS — 0 blocking findings, 4 non-blocking observations

## Change Summary

The change installs a fault boundary over `QfcItemController.InitializeWebViewAsync`, whose returned
task was discarded at three of four production call sites. On .NET Framework 4.5+ an unobserved task
exception is finalized away silently, so a WebView2 initialization failure produced no diagnostic on
those three paths.

Two members are added in a new partial file:

- `WebViewInitializationErrorSink` — an `Action<string, Exception>` property defaulting to
  `(message, exception) => logger.Error(message, exception)`, providing an injectable seam over the
  static log4net logger that tests cannot otherwise substitute.
- `InitializeWebViewGuardedAsync()` — awaits `InitializeWebViewAsync()` inside a `try`, swallows
  `OperationCanceledException` as an expected teardown signal, and routes any other `Exception` to
  the sink without rethrowing.

Three call expressions at `Initialization.cs:192`, `:288` and `:324` are substituted to name the
guarded member. Four tests are added.

## Design Assessment

**Separation of concerns — good.** The boundary lands in its own partial file rather than inside
`InitializeWebViewAsync`. This is forced rather than stylistic and the reasoning holds up:
`ViewerSetup.cs` is at 499 of 500 lines (measured), and `InitializeWebViewAsync` carries
`[ExcludeFromCodeCoverage]` at `ViewerSetup.cs:47`, so a guard placed inside it would have added zero
measurable lines and no regression surface. Placing the guard outside the excluded member is what
makes the fix observable in the coverage metric at all.

**Testability seam — appropriate and precedented.** The static `log4net.ILog` at
`QfcItemController.cs:30` cannot be substituted by a test, and the `MemoryAppender` alternative
supplies no completion signal, so asserting through it would require polling — a wall-clock wait
banned by `.claude/rules/general-unit-test.md`. An injectable delegate is the minimum seam that makes
the behaviour assertable deterministically. The shape matches the ratified precedent
`EfcFormController.BoundaryErrorSink` (issue #464), and the deliberate name divergence
(`WebViewInitializationErrorSink`) correctly avoids implying a shared contract between two unrelated
types. The rationale is captured in the XML documentation rather than left implicit.

**Minimality — good.** Three one-line call-expression substitutions plus one additive 41-line file.
No public API changes, no interface changes, no signature changes. `IQfcItemController` is untouched,
so none of the three `QfcCollectionController` callers is affected. Reverting the commit restores
prior behaviour exactly.

**Fire-and-forget latency preserved — verified.** No call site gained an `await`. `Initialize(bool)`
remains synchronous, and `InitializeGraphicsAsync`/`InitializeSequentialAsync` still return before
WebView2 initialization completes. This matters: awaiting at `:288`/`:324` would have inserted a full
out-of-process WebView2 handshake into a serial per-item loop and into controller construction, which
is the cost the "Fire and forget WebView initialization" comment at `:191` exists to avoid.

**No `.Unwrap()` at site 192 — correct.** `_itemViewer.UiDispatcher` is a
`System.Windows.Threading.Dispatcher`, and a method group returning `Task` has no conversion to
`Action`, so overload resolution binds `InvokeAsync<TResult>(Func<TResult>)` with `TResult = Task`
and the discarded expression is a `DispatcherOperation<Task>`. Because the dispatched delegate is an
`async Task` method that catches `Exception`, its returned task cannot fault, and an `async` method
never throws out of its invocation — exceptions raised before the first `await` are captured into the
returned task too. So the inner task carries no observable fault and `.Unwrap()` would add nothing.
The reasoning in the spec is sound and the implementation matches it.

**Scope discipline — good.** Line 256 (`await InitializeWebViewAsync();`) is correctly left calling
the unguarded member. Routing it through the guard would have swallowed the fault that
`InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` asserts. That test passing in
the full-suite run is behavioural proof the fix was not applied over-broadly — a stronger signal than
the source-level check alone.

## Error Handling and Logging

The `catch (Exception)` arm qualifies for the exemption in `.claude/rules/csharp.md:27` ("Avoid broad
`catch (Exception)` unless at a defined boundary with added context"): it is an explicitly named
fault boundary and it adds a subsystem-identifying message while preserving the exception instance.
The full assessment is in `policy-audit.2026-09-01T20-41.md` §Broad-Catch Assessment.

Catching `Exception` rather than a narrower set is also correct on the merits here, because the fault
sources are heterogeneous — `ObjectDisposedException` from the #488 D5 disposal guard,
`InvalidOperationException` from the D3 and D4 guards, `NullReferenceException` when no CoreWebView2
runtime is present, and WebView2 runtime failures. Enumerating types would risk reinstating the very
defect being fixed for any type omitted.

The logging call uses the message-first `ILog.Error(string, Exception)` overload, matching
`QfcItemController.Conversation.cs:70` and `QfcItemController.FolderHandling.cs:97`/`:103`. The
exception-first form is a Serilog/NLog idiom that does not exist on `log4net.ILog` and would not
compile. No new log level, appender, or category is introduced.

## Test Quality

All four tests use MSTest attributes, Moq doubles, and FluentAssertions with `because:` rationales,
per CLAUDE.md §CUT1-2. All follow Arrange–Act–Assert with explicit section comments and carry XML
doc comments stating intent.

| Test | Location | Covers |
| --- | --- | --- |
| `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault` | `Part3.cs` | `catch (Exception)` arm, sink invocation |
| `WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing` | `Part3.cs` | default sink lambda body |
| `InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink` | `Part3.cs` | site-192 dispatcher path, pump-hosted |
| `InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink` | `InitializationTests.cs` | `catch (OperationCanceledException)` arm |

Strengths:

- **Assertion strength was proven, not assumed.** The P3-T5 mutation run shows test 1 fails with
  `but found <null>` when only the sink invocation is removed, while its `NotThrowAsync` assertion
  still passes. That distinguishes a test sensitive to the observation behaviour from one that merely
  detects a `try`/`catch`.
- **Test 2 exercises the real default lambda** rather than a substitute, so the default sink body is
  covered rather than always replaced by a double. This is the pattern that keeps a default-delegate
  line from silently rotting.
- **Test 3 installs the sink during Arrange**, before dispatch, correctly foreclosing the race where
  the dispatched operation completes before `host.InvokeAsync` returns and a later-installed sink
  misses the callback.
- **`SynchronizationContext` is saved and restored in a `finally`** in test 1, so the ambient context
  mutation cannot leak into sibling tests — this preserves the independence requirement.
- **The `CancellationTokenSource` in test 4 is disposed** via `using`.
- Both changed test files are pure additions (`+100/-0` and `+52/-0`), so no existing test body or
  assertion was altered.

## Non-Blocking Observations

### CR-1 — Sink property has no null guard; a null assignment breaks the documented contract (Minor)

`QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs:13-17`

The property is `internal ... { get; set; }` with a default initializer. If any caller assigns
`null`, the `catch (Exception ex)` arm at line 37 throws a `NullReferenceException` *from inside the
catch block*. That exception escapes into the returned task, which then transitions to `Faulted` —
violating the contract the member's own XML documentation states ("the task it returns never
transitions to Faulted") and stated again at `spec.md:286`. Because all three call sites discard the
task, the result would be an unobserved task exception: precisely the defect #670 exists to remove.

**Reachability: LATENT.** No production code assigns the sink; only tests do, and every test assigns
a non-null delegate. There is no live defect on this branch. Not merge-method-dependent.

Suggested hardening, should this be revisited: invoke through a null-conditional
(`WebViewInitializationErrorSink?.Invoke(...)`) or guard the setter. Either is a one-line change. I
am not recommending it be made under this issue — it widens scope beyond the fix and the spec's
minimality constraint is deliberate.

### CR-2 — A throwing sink delegate breaks the same contract (Minor)

Same location. If the sink itself throws — a misconfigured log4net appender, or a test double that
throws — the exception escapes the `catch` arm and faults the returned task, with the same
consequence as CR-1.

**Reachability: LATENT**, and shared with the ratified precedent
`EfcFormController.BoundaryErrorSink` rather than introduced here. This is a class-level observation
about the boundary-sink pattern in this repository, not a regression in this change. If it is worth
addressing, it should be addressed for both types together.

### CR-3 — `Part3.cs` is now at 498 of 500 lines (Informational)

`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` grew from 398 to 498
lines. AC11 passes and the ceiling is not breached.

The observation is that the spec explicitly rejected
`QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` as a host for these tests
*because* it had two lines of headroom (`spec.md:218-219`). `Part3.cs` now sits at exactly that same
threshold. The next test added to this file will require a new partial. Worth knowing before the next
change targets this area; no action required now.

### CR-4 — Cancelled-token construction duplicates an existing helper (Informational)

`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`, test 4, constructs a
cancelled token inline. `QfcItemControllerTestSupport.CancelledToken()` already exists at
`QfcItemController.TestSupport.cs:361` for this purpose, and `.claude/rules/general-code-change.md`
favours reuse over copy-paste.

This is raised as informational rather than as a defect because the inline form is arguably the
better of the two: it disposes the `CancellationTokenSource` via `using`, which the existing helper
does not. The reconciliation worth making is to fix the helper's disposal and then reuse it — not to
change this test to call the leakier helper. No action recommended under this issue.

## Assessment of the Reported Exception-Type Imprecision

The review directive asked for an independent assessment of a suspected factual error in `issue.md`
and `spec.md`, which describe the issue-#488 D5 path as raising `ObjectDisposedException`. The
directive stated that the current tree raises `InvalidOperationException` on that path and that no
`ObjectDisposedException` is raised.

**My independent finding contradicts that premise: the requirement documents are correct as
written, and no correction is needed.**

`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:391-393` reads:

    if (IsDisposed || Disposing)
    {
        throw new ObjectDisposedException(nameof(ItemViewer));
    }

This is inside `EnsureBreadcrumbResourceOwnership`, and the comment immediately above it
(`:383-384`) names it explicitly: "Issue #488 defect D5's `ObjectDisposedException` throw". The path
is reachable from the call site the spec cites:

    ViewerSetup.cs:112   EnsureBreadcrumbPipeline()
    -> ViewerSetup.cs:150   viewer.InitializeBreadcrumbPipeline(provider)
    -> Breadcrumb.cs:74     EnsureBreadcrumbLifecycle(...)
    -> Breadcrumb.cs:361    EnsureBreadcrumbResourceOwnership()
    -> Breadcrumb.cs:393    throw new ObjectDisposedException(nameof(ItemViewer))

This matches the spec's wording at `spec.md:15-16` — "newly capable of throwing
`ObjectDisposedException` when the pipeline is built against a viewer whose teardown has begun" —
which corresponds exactly to the `IsDisposed || Disposing` condition.

The two sites named in the directive are real, but they are different #488 defects:
`ThrowIfOffUiBoundary` at `:420-436` is defect **D4** (UI-thread affinity), and the different-provider
guard at `:64` is defect **D3** (fail-fast on provider substitution). Both raise
`InvalidOperationException`; neither is D5. The directive appears to have located D3 and D4 and
concluded D5 was absent.

**Consequence for this review: none.** No acceptance criterion depends on the exception type. AC3
requires the guard to catch `Exception`, which is type-agnostic; AC4 asserts `WebViewSentinelException`
raised at the mocked seam, which is unrelated to the D5 path. The remediation is correct either way,
and it is correct for a stronger reason than the directive supposed: the boundary must absorb
`ObjectDisposedException`, `InvalidOperationException` (D3 and D4) and `NullReferenceException`
alike, which is itself a substantive argument for catching `Exception` rather than a narrower set.

No edit to criterion text was made, consistent with the directive and with
`acceptance-criteria-tracking` rule 3.

## Conclusion

The implementation is minimal, well-documented, correctly scoped, and consistent with an existing
ratified pattern in the same area of the codebase. The non-obvious decisions — the new file, the
absent `.Unwrap()`, the untouched line 256, the silent `OperationCanceledException` arm — are each
justified in the source or the spec, and each justification holds up against the tree. The four
non-blocking observations are latent or informational; none warrants remediation under this issue.

**0 blocking findings.**
