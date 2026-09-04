# Code Review — efc-archiveroot-boundary-sink-defects (Issue #736)

- Date: 2026-09-04
- Branch: `bug/efc-archiveroot-boundary-sink-defects-736`, HEAD `54da9e4d`
- Base: `origin/main` `66749143` (also the merge base)
- Scope: full branch diff, 98 paths; 8 source files + 3 project files under review here

**Overall: PASS. 0 blocking findings. 3 non-blocking findings, 5 observations.**

## Summary judgement

The design is sound and matches the shape the repository's own policy prescribes for host-bound
code: a thin COM-touching wrapper over a delegate-driven core that carries all the decision logic
and is unit-testable with no Outlook process. `RunKbdGuardedAsync` is a single containment point
rather than duplicated try/catch in two overloads. The frozen contracts named by AC9 are genuinely
untouched, verified by reading them rather than by trusting the evidence artifact. Finding 3 is
untouched.

The weakest points are all in the finding-4 user-facing surface: a WinForms construction path
guarded only by app-domain-wide mutable state, and a fault message that now reaches a user-visible
TextBox carrying `ex.Message` from arbitrary exceptions.

## Blocking findings

None.

## Non-blocking findings

### CR-1 (PARTIAL) — `ShowModelessFaultNotice`'s guard is app-domain-wide mutable global state

**File:** `QuickFiler/Controllers/EfcFormController.cs`, `ShowModelessFaultNotice`, the early return.
**Dependent tests:** `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:316`
`BoundaryErrorSink_DefaultDelegate_ReturnsWithoutBlockingTheCallingThread`, and pre-existing
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs:283`
`BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing`.

```csharp
if (System.Windows.Forms.Application.OpenForms.Count == 0)
{
    return;
}
```

**Violated rule:** `.claude/rules/general-unit-test.md` UT1 (Determinism) and UT4 ("Tests must not
rely on mutable global state").

The `<remarks>` block is candid that this early return exists to keep the test host from
constructing a window: *"The early return is load-bearing rather than defensive — the pre-existing
default-delegate test invokes the sink directly in the test host, and without it that invocation
would construct a window on an MSTest thread."* That is accurate, and the candour is welcome. The
problem is that it makes two test outcomes depend on a condition no test controls. In .NET
Framework, `Application.OpenForms` is a single static `FormCollection` for the whole app domain,
populated from `Form.OnHandleCreated`.

Measured exposure, stated precisely: within `QuickFiler.Test` it is nil — a search of the assembly
for `new Form(`, `: Form`, and `OpenForms` returns zero matches, and `WinFormsPumpHost` uses a bare
`new ApplicationContext()` with no main form. The exposure is cross-assembly:
`UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:73` calls `viewer.Show()` on a real `Form`, and
`ClassLevel` parallelization with `Workers=0` is configured in
`scripts/vscode/TaskMaster.cli.runsettings`. Whether the nine assemblies share one testhost process
is decided inside `Invoke-DotnetCoverageCollection`, which this review did not open. The condition
did not fire in the 7013/7013 run.

If it does fire, `ShowModelessFaultNotice` constructs a `Form` and a `TextBox` on a worker thread
with no message pump, and the dependent test asserts `NotThrow`.

**What would close it:** install a capturing `UserFaultNotifier` in both default-sink tests and
restore it in a `finally`, exactly as `BoundaryErrorSink_DefaultDelegate_RoutesThroughTheUserFaultNotifier`
already does. The seam for this already exists and is already used correctly by its sibling; the two
tests simply do not use it. That change removes the dependency on `Application.OpenForms` entirely
and costs nothing in coverage, since `ShowModelessFaultNotice` is `[ExcludeFromCodeCoverage]` anyway.

### CR-2 (PARTIAL) — arbitrary `ex.Message` now reaches a user-visible surface

**Files:** `QuickFiler/Controllers/EfcFormController.cs`, the `TryReportBoundaryFault` call sites
`$"Keyboard dispatch failed: {ex.Message}"` and `$"Breadcrumb bind failed: {ex.Message}"`.

**Rule at issue:** the #602 redaction principle the rest of this change is careful about — the
archive-root diagnostic deliberately withholds the path *because it carries a mailbox address*.

Before this change, `BoundaryErrorSink`'s default was log-only, so `ex.Message` reached a log file.
After it, `DefaultBoundaryErrorSink` forwards the same string to `UserFaultNotifier`, whose default
renders it into a read-only `TextBox` in a modeless window. The interpolated messages are
constructed from whatever exception arrives.

For the exception this item is built around, the outcome is correct: the normalized
`InvalidOperationException` carries `ArchiveRootPathGuard.UnresolvableRule`, which this review read
in full and which contains no path and no address —

> `The Outlook archive root folder could not be resolved in the default store. The path is withheld from this message because it contains a mailbox address.`

For an arbitrary exception it is not guaranteed. `BindBreadcrumbRowsAsync`'s general
`catch (System.Exception ex)` will happily surface an `IOException` or a `COMException` whose
`Message` embeds a filesystem path — and in this codebase a path is exactly what #602 classifies as
address-bearing. The change did not introduce the `{ex.Message}` interpolation (the breadcrumb one
is pre-existing), but it did change where that string is displayed, which is the part that matters.

**What would close it:** pass a redacted, rule-naming summary to `UserFaultNotifier` and keep the
`ex.Message` detail for `logger.Error` only — i.e. give `DefaultBoundaryErrorSink` two strings
rather than forwarding one to both sinks.

### CR-3 (Observation, promoted) — `UnresolvableRule` is reused for a cause it does not describe

**File:** `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs`, the `catch (COMException)` arm.

```csharp
logDiagnostic?.Invoke(ArchiveRootPathGuard.UnresolvableRule);
throw new InvalidOperationException(ArchiveRootPathGuard.UnresolvableRule, comFailure);
```

The constant states the archive root *could not be resolved in the default store*. A transient COM
failure is a different condition: the archive root may exist and be perfectly resolvable, and
Outlook was merely busy. Anyone reading a log line or the new user-facing notice will diagnose a
missing or misconfigured archive folder when the actual cause is transport.

The `InnerException` does distinguish the two, and reusing the constant is a defensible consequence
of AC9 freezing `ArchiveRootPathGuard.cs` — a new constant cannot be added there. But the new
partial file is the natural home for one, and none was added.

**What would close it:** declare a `ComReadFailedRule` constant in `AppOlObjects.ArchiveRoot.cs`
with the same redaction discipline (name the rule, withhold the path), and use it in the COM arm.
Note that test 3, `..._MessageWithholdsPathAndMailboxAddress`, asserts only absence of the path and
address, so it would continue to pass; test 5 asserts `Contain("could not be resolved")` but drives
the *guard's* branch, not the COM branch, so it is unaffected.

## Observations (no action required)

**OBS-1 — the success-path tests genuinely assert behaviour, not just line execution.** This was
checked specifically because coverage-motivated tests often execute a path without pinning it. All
three P6-T13 tests pin the observable effect:

- `RunKbdGuardedAsync_WhenBodyCompletes_InvokesBodyAndReportsNothing` increments `bodyCallCount`
  inside the body and asserts `Be(1)`, plus `sinkCallCount.Should().Be(0)`.
- `KbdExecuteAsync_FuncTaskOverload_WhenToggleSucceeds_AwaitsTheAction` asserts `dispatchCount == 1`,
  `sinkCallCount == 0`, **and** `handler.Verify(k => k.ToggleKeyboardDialogAsync(), Times.Once)`.
- `KbdExecuteAsync_ActionOverload_WhenToggleSucceeds_InvokesTheAction` does the same for the
  synchronous overload.

A guard that silently swallowed its body would fail all three. The gap the mid-run discovery
identified — six tests all driving fault paths, so the guard had never been observed letting a call
through — is genuinely closed, and closed on both overloads rather than only one.

**OBS-2 — the COM normalization contract is correctly implemented.** Verified against source, not
against the evidence artifact. Both reads sit inside one `try`, so a fault on either normalizes
identically. `InnerException` is the original instance (tests assert `BeSameAs`, not `BeOfType`,
which is the stronger assertion). The diagnostic is emitted *before* the throw, matching the frozen
guard's own ordering. `ArchiveRootPathGuard.RequireResolvedArchiveRoot` is deliberately left outside
the `try`, so the guard's own `InvalidOperationException` is not re-wrapped — correct, and pinned by
test 5 asserting `InnerException.Should().BeNull()`. The no-cache conjunct holds structurally: the
getter assigns `_archiveRootPath` only on success, so a throw leaves it null and the next read
retries; test 6 pins this by counting delegate invocations across two calls.

**OBS-3 — `ArchiveRootPathGuard.cs` is unmodified and finding 3 is untouched.** Both verified
directly rather than accepted. `ArchiveRootPathGuard.cs`, `IOlObjects.cs`, and
`AppOlObjectsArchiveRootValidationTests.cs` appear in no hunk of the branch diff.
`EfcDataModel.cs` contains exactly one `catch (InvalidOperationException ex)` at line 287 and zero
`catch (COMException` — the widening AC9 prohibits did not happen. `ActionOkAsync` begins at
`EfcFormController.cs:838`; the four hunks in that file are at 129, 170, 994–1034, and 1126, none of
which intersects it. The single added `.Dispose()` in the file is
`notice.FormClosed += (sender, args) => notice.Dispose();` inside `ShowModelessFaultNotice` — a
self-disposing notification form, not a disposal-ordering change.

**OBS-4 — `EfcDataModel.cs` is at 499 of 500 lines.** This change took it from 485 to 499. It is
compliant, but the next single-line addition anywhere in the file breaks the ceiling. Worth knowing
before the sibling finding-3 item lands.

**OBS-5 — committed evidence retains the absolute path shape, though not the identity.** All 407
`C:\Users\…` occurrences in added content read `C:\Users\REDACTED\repos\TaskMaster\.claude\worktrees\agent-<id>\…`.
The account name is gone — a sweep of all 9,605 added content lines and of all eight commits'
feature-folder trees found zero unredacted tokens, and sanitization was done in-task rather than
after commit, so no pre-sanitization blob is reachable. What remains is drive letter, repo layout,
and worktree id in the four `min.log.txt` extracts and fourteen TRX files. No identity is disclosed
and no rule is violated. Substituting `<repo-root>` would have been tidier.

## Design and best-practice notes

**Good: `AsyncLocal` over a plain static for `UserFaultNotifier`.** The comment gives the reason —
a shared static races under `ClassLevel` parallelization — and names the in-repo precedent
(`MyBox.DialogInvoker` in `UtilitiesCS.Dialogs`). Both tests that mutate it restore the previous
value in a `finally`. The one subtlety, that an `AsyncLocal` write inside an `async` method does not
flow back to the caller, is handled: the test that depends on visibility is deliberately declared
`void` rather than `async Task`, and says so in its `<summary>`.

**Good: a static method group instead of a lambda for the sink default.** `DefaultBoundaryErrorSink`
is a named static method, so the default is greppable, debuggable, and directly invocable — better
than the previous inline lambda, and the reason (an instance property initializer cannot reference
`this`) is stated.

**Good: the `EmailFilerConfig` construction stays inline above the seam.** The seam was cut *below*
the object initializer rather than around it, deliberately, so the initializer's lines stay covered.
Verified: `EfcDataModel.cs:339` (`OlAncestor = olAncestor,`) reads `hits="1"` post-change. The
choice also newly covered two lines the old incidental crash prevented reaching —
`SortEmail.Cleanup_Files();` and `return result;` moved from `hits=0` to `hits=1`.

**Acceptable: `FormatterServices.GetUninitializedObject`** in `AttachSucceedingKeyboardHandler`.
Unusual, and obsolete on modern .NET, but this is net48, the `EfcHomeController` constructor requires
a live Outlook context, only the public `KeyboardHandler` property is then set, and the helper's
`<summary>` states the reason and names the sibling file's precedent for the technique. Factoring
the arrangement into one helper rather than duplicating it across both success-path tests is right.

**Acceptable: exception containment changes the contract of `KbdExecuteAsync`.** Both overloads now
swallow every non-cancellation exception. Call sites were checked: `EfcFormController.cs:723` and
`:783` bind `KbdExecuteAsync(ActionOkAsync)` to keyboard actions, and `:492` / `:569` call
`ActionOkAsync` directly without going through `KbdExecuteAsync`. No caller loses meaningful
propagation, which is the point of finding 2.

**Minor: one AAA comment missing.** `EfcDataModelArchiveRootTests.cs` has 11 `// Arrange`,
11 `// Act`, and 10 `// Assert` markers. Cosmetic; every test does assert.

## Remediation Decision

**No `remediation-inputs` artifact is produced.** Rationale, stated so the orchestrator can override:

- There are **0 blocking findings**.
- The three coverage FAIL rows (policy audit F-1, F-2, and the modified-file floors) are irreducible
  within this item's ratified scope. F-1's three lines are compiler-lifted lambdas inside an
  `[ExcludeFromCodeCoverage]` COM wrapper, which no test can execute without a live Outlook process.
  F-2's three lines are the AC7-mandated seam body, whose only alternative is to re-introduce the
  real `EmailFiler` call that finding 6 exists to stop depending on. The modified-file floors are
  pre-existing debt on files that were 25.69%, 66.20%, and 29.58% *before* this change; two of the
  three improved.
- F-4 (file size) is explicitly out of scope by the spec's Scope & Non-Goals and the plan's D7.
- CR-1, CR-2, CR-3, and F-5 are latent or forward-looking and are better handled as follow-up issues
  than by reopening a delivered, fully green change. Recommended promotions:
  1. Remove the `Application.OpenForms` dependency from the two default-sink tests (CR-1).
  2. Split the user-facing string from the log string in `DefaultBoundaryErrorSink` so arbitrary
     `ex.Message` content is not rendered to the user (CR-2).
  3. Add a distinct `ComReadFailedRule` constant so a transport fault is not reported as an
     unresolvable archive root (CR-3).
  4. Retain a committed summary carrying the msbuild non-vacuity counts, since the two
     `min.log.txt` extracts contain neither literal (F-5).
  5. Split `EfcFormController.cs`, now 1320 lines (F-4) — if a sibling item does not already own it.
