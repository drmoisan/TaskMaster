# Code Review — issue #810 (quickfiler-teardown-and-dropdown-residuals)

- **Timestamp:** 2026-09-08T20-15
- **Branch:** `bug/quickfiler-teardown-and-dropdown-residuals-810` @ `a9d11dd2048e98e4f66cee05dc6a0f30a20a4407`
- **Base:** `origin/main` @ `0e9c95a5dd45104d82f46fd801973a6bc068f25f`
- **Reviewed:** the full branch diff (89 paths), with the sixteen production and test files read in place on disk rather than only in diff form
- **Verdict:** ACCEPT. 0 blocking findings, 5 non-blocking findings, 4 observations.

## 1. What the change does

Seven small changes across five production files plus one new file, each traceable to one acceptance criterion:

1. `ParkFocusAndCancelSelectors` takes a required `bool honourSelfInflictedGuard`; the deactivation caller passes `true`, the Cancel teardown stage passes `false` through an explicit lambda.
2. `QfcHomeController.Cleanup()` nulls `_tokenSource` after disposing it and nulls `_datamodel` beside its five sibling fields.
3. `QfcFormController.Cleanup()` wraps its body in `try` and invokes the ribbon-release callback from a `finally` that reads the field into a local, clears the field, then invokes the local.
4. `FinishClose` clears `IsCommitPending` as the final element of its `CompleteAll` operation list.
5. Two stale comments corrected; one dead `internal` accessor deleted.
6. `FinishClose` and `RestoreAfterOpenFailure` relocate between partial-class parts to stay under the 500-line ceiling.
7. The popup-owner store and its disjunction move out of a coverage-exempt `Form` subclass into a new `BreadcrumbPopupOwnerRegistry`.

## 2. Targeted verifications the caller asked for

### 2.1 AC1 — does the guard narrowing preserve the issue-677 keyboard-lock contract?

Verified. The guard survives with unchanged polarity and unchanged position, and the deactivation caller keeps it active.

`QuickFiler/Controllers/QfcFormController.Deactivate.cs:122-128`:

```csharp
if (
    honourSelfInflictedGuard
    && _formViewer?.IsDeactivationSelfInflictedByOwnPopup == true
)
{
    return;
}
```

Checked against every item on the plan's D16 prohibited list, by reading the file rather than the diff:

| Prohibited change | Present? | Evidence |
|---|---|---|
| Guard deleted, or its condition made unconditionally false | No | The guard stands at `:122-128` with `_formViewer?.IsDeactivationSelfInflictedByOwnPopup == true` intact as the second conjunct |
| Polarity of `IsDeactivationSelfInflictedByOwnPopup` inverted | No | `QfcFormViewer.cs:239` forwards to `_breadcrumbPopupOwners.AnyOpen`, and `BreadcrumbPopupOwnerRegistry.AnyOpen` is `_owners.Values.Any(popupIsOpen => popupIsOpen())`, which is `false` for an empty store — the same value the old `Dictionary.Values.Any(...)` returned, and the GENUINE case |
| Guard moved above the focus-parking step | No | Focus parking is at `:100-103`; the guard is at `:122`. Order unchanged |
| `MayTakeFocus`, `FocusPending()`, `FocusAnchorIfPermitted()` or the `ItemViewer.Breadcrumb.cs` wiring touched | No | None appears in the branch diff; `ItemViewer.Breadcrumb.cs` is not a changed path |
| `ParkFocusOffWebView2()` removed from either path | No | Still called at `Deactivate.cs:102`, still asserted `Times.Once` by the new AC1 test |
| An `ActionCancelAsync` stage removed or reordered | No | `EventHandlers.cs:143-149` retains `reset-keyboard`, `park-focus`, `unregister-handlers`, `hide-form` in that order, then `quiesce-loader` and `groups-cleanup` |
| `"park-focus"` stage literal changed | No | `EventHandlers.cs:145` still reads `"park-focus"`, which `QfcFormControllerCancelTeardownTests.MarkerParkFocus` compares against |

Caller census: a repository-wide search for `ParkFocusAndCancelSelectors` returns exactly two invocations — `Deactivate.cs:27` with `honourSelfInflictedGuard: true` and `EventHandlers.cs:146` with `false` — plus three `<see cref>` references and two diagnostic string literals, none of which is a call. The spec's "exactly two callers" claim holds after the change.

The design choice of a required parameter over a defaulted one is correct and load-bearing: `EventHandlers.cs` previously passed the method group to `System.Action`, and C# method-group conversion does not apply optional-argument defaults, so a defaulted parameter would have failed with CS0123. The change to an explicit lambda is the minimum that satisfies that constraint.

Named-argument call sites (`honourSelfInflictedGuard: true` / `: false`) are the right call for a bare `bool` parameter and make both sites self-documenting at the point of use.

### 2.2 AC4 — is exactly-once invocation achieved on every path, including a throwing callback?

Verified. `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:262-272`:

```csharp
finally
{
    System.Action parentCleanup = _parentCleanup;
    _parentCleanup = null;
    parentCleanup?.Invoke();
}
```

Path analysis, all four cases:

| Path | Callback invocations | Why |
|---|---|---|
| Body completes normally | 1 | `finally` runs once; field cleared first |
| Body throws (for example the planted `_formViewer.Dispose()` fault at `:253`) | 1 | `finally` runs on the exceptional path; this is the hole the criterion names |
| Callback itself throws | 1 | The field is already `null` when `Invoke()` is entered, so no later pass can re-invoke |
| `Cleanup()` called a second time | 0 more | The field is `null` from the first pass regardless of how that pass ended |

The read-into-local-then-clear ordering is what makes case 3 hold; a `finally { _parentCleanup?.Invoke(); _parentCleanup = null; }` would close case 2 but not case 3. The implementation matches the mechanism the plan specified.

Two supporting checks:

- The invariant test `Cleanup_SourceContainsNoSynchronousWait` reads the whole file text and rejects `.Wait(`, `.Result`, `Thread.Sleep` and `Task.Delay`. The restructure introduces none of the four; it adds only a `try`, a `finally`, a local and a field write. The test passes in `[P3-T6]` (8 of 8 in the class).
- The same idiom already exists in this repository at `QuickFiler/Controllers/EfcFormController.cs:305-306`, so the fix matches an established sibling pattern rather than inventing one.

The regression test discriminates properly: it plants a throwing `Dispose()`, drives two passes, asserts both propagate the planted exception, and asserts the counter is exactly 1 across both. The fail-before artifact records the counter at 0 with the mechanism named (`:251` throws, no `try`, `:259` never reached).

### 2.3 AC5 — is the latch clear an element of the operation list, and does the test discriminate?

Verified on both halves.

`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:133-162` places `() => IsCommitPending = false` as the fourth element of the `CompleteAll(params Action[])` argument list, not as a statement after the call. That placement matters because `CompleteAll` (`BreadcrumbDropDownHost.cs:432-451`) runs every operation, retains the first failure, reports the rest, and rethrows the first failure after the loop — so a trailing statement would be skipped whenever an earlier operation threw, while a list element always runs.

Ordering within the list is correct and deliberate: the latch is read by element 2 and cleared by element 4, so the clear cannot change the decision made by the close that consumed the latch. What it fixes is that the latch cannot survive that close.

The regression test discriminates. `RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch` sets the latch, drives a native close (which consumes it and, after the fix, clears it), asserts `CancelCount == 0` at that point, then calls `RestoreAfterOpenFailure()` and asserts `CancelCount == 1`. Without the fix the latch would still be `true` at the second close, the cancel would again be suppressed, and the count would stay 0 — which is exactly what the fail-before artifact records. Driving two closes is necessary rather than incidental: a single-close version asserting a cancel would be unsatisfiable even after the fix.

Regression risk in the other direction was checked: after an `ExplicitCommit` close, `FinishClose` now also clears the latch. No path re-reads the latch within the same popup lifetime after that point, because `Close`, `CompleteClose` and `OnDropDownClosed` all guard on `OpenState`/`IsPendingClose` and return early once the lifetime has ended, and `ShowPopup` clears the latch again at the next native show. The 48-of-48 pass across `BreadcrumbDropDownCloseOrderingTests`, `BreadcrumbDropDownHostTests` and `BreadcrumbPendingOpenCloseTests` corroborates this.

### 2.4 Design B relocation — is it behaviour-preserving?

Verified. The move is between two parts of the same `public sealed partial class BreadcrumbDropDownHost`, both in namespace `QuickFiler.Viewers`, both carrying `#nullable enable`.

- Accessibility unchanged: `private void FinishClose(...)` and `internal void RestoreAfterOpenFailure()`, identical to the removed declarations.
- Every member the two methods reach — `CompleteAll`, `DropDown`, `OpenState`, `CloseNative`, `FocusAnchorIfPermitted`, `_cancelSelection`, `IsCommitPending` — is a member of the same class, so no `using` directive was added and no symbol resolution changed. `CompleteAll` itself stayed behind in `BreadcrumbDropDownHost.cs` and is reachable across parts.
- The three call sites resolve unchanged: `BreadcrumbDropDownHost.cs:415` (`CompleteClose`), `BreadcrumbDropDownHost.Diagnostics.cs:75` (`OnDropDownClosed`), and `Open.cs:174` (`RestoreAfterOpenFailure`). That is the "three call sites" figure the spec asserts.
- No test resolves either method by file path. The reflection lookup in `CloseOrderingHostHarness.RaiseNativeClose` targets `OnDropDownClosed` on the type, which is file-agnostic; its failure message names `BreadcrumbDropDownHost.Diagnostics.cs`, a file the relocation did not touch.
- The one textual departure from a verbatim move is disclosed in `evidence/qa-gates/p4-t8-layout-decision.md`: a stale `:486-487` line citation was dropped from a comment because the move renumbered the cited method. Dropping a locator that would otherwise be wrong in both files is the right call.

The relocation was required rather than preferred: Design A measured 504 lines after CSharpier, over the 500 ceiling. The measurement was taken and recorded before the branch was chosen, which is the decision rule the spec set.

### 2.5 `BreadcrumbPopupOwnerRegistry` against design, naming, null-handling and test policy

`QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` (61 lines):

- **Design.** A cohesive type with one responsibility, `internal sealed`, minimal surface (`Register`, `AnyOpen`), no inheritance, no framework coupling beyond `Control` as an identity key. It is the smallest extraction that makes the AC2 derivation measurable, and it deliberately adds no speculative `Unregister`. Correct application of "simplicity first" and "keep the public surface intentional and minimal".
- **Naming.** `PascalCase` type and members, `camelCase` parameters, `_owners` private field. Names are descriptive; no abbreviation.
- **Documentation.** The type doc states why the extraction exists; `Register` documents both null cases and, in a `<remarks>`, why assigning by key rather than adding is load-bearing. `AnyOpen` documents the empty-store polarity. This is "comment why, not what" done properly.
- **Semantics preserved.** `_owners[itemViewer] = popupIsOpen` replaces rather than appends, and `Dictionary<Control, Func<bool>>` uses reference equality because `Control` does not override `Equals` — the same behaviour the field had inside `QfcFormViewer`.
- **Test policy.** MSTest attributes, Moq for the two `Func<bool>` predicates, FluentAssertions throughout, explicit Arrange/Act/Assert, one scenario per test, six cases covering the empty, single-false, single-true, two-owner, replacement and null-argument paths. No handle is created, no window is shown, no temporary file is used, and each `Control` is disposed. Measured coverage of the new module is 100 percent of 9 measurable lines against a 90 percent bar.
- **Polarity test.** `AnyOpen_WithNoRegistration_ReportsFalse` is the case that pins the issue-677-relevant polarity, and its doc says so. Good choice: this is the assertion that would fail first if a future refactor inverted the derivation.

One defect found in this file, CR-2 below.

### 2.6 File ceiling, independence, determinism, temporary files

All independently measured and clean; see `policy-audit.2026-09-08T20-15.md` sections 7.1 and 8. Maximum changed-file size is 498 lines. No added test uses a sleep, a delay, a wall-clock wait, a retry, mutable global state that is not restored, or the filesystem.

## 3. Non-blocking findings

### CR-1 — Minor — the sibling ribbon-release callback in `QfcHomeController` still has the defect AC4 fixed in `QfcFormController`

**Location:** `QuickFiler/Controllers/QfcHomeController.cs:403-407`

```csharp
finally
{
    ParentCleanup?.Invoke();
    logger.Info("Home cleanup complete; ribbon release callback invoked.");
}
```

`ParentCleanup` is an `internal System.Action { get; set; }` auto-property (`:154`) that `Cleanup()` never clears. A second direct call to `QfcHomeController.Cleanup()` therefore invokes the ribbon-release callback a second time — the double-release case whose mirror image AC4 closed one level down by reading the field into a local and clearing it before invoking.

Why it is not a finding against the delivery: AC4 is worded over `QfcFormController.Cleanup()` and that method is fixed. AC3 touches this method but is worded over the token source. Neither criterion reaches this line.

Why it is worth recording anyway:

- The chain is `QfcFormController.Cleanup` → `finally` → `QfcHomeController.Cleanup` (wired at `QfcHomeController.cs:142`, which passes `Cleanup` as the form controller's `parentCleanup`) → `finally` → the ribbon release. The AC4 fix guarantees the middle link runs exactly once, which reduces but does not eliminate the exposure; a direct second call to the home `Cleanup()` still double-releases.
- The AC3 test cannot see it. `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs:115` asserts `parentCleanup.Verify(x => x.Invoke(), Times.Once)` and the second `controller.Cleanup()` is at `:120`, after that assertion. Moving the `Times.Once` verification below the second pass would fail today.
- The fix is already written elsewhere in the same solution: `EfcFormController.cs:305-306` uses the read-and-clear idiom, and `QfcFormController.SetupDisposal.cs:269-271` now does too.

**Recommendation:** promote as a follow-up issue (report-only item 10). The change is three lines and mirrors code that now exists twice in the same repository.

### CR-2 — Minor — nullable annotations on `Register` contradict its documented contract

**Location:** `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs:1` (the pragma) and `:42-50` (the `Register` declaration and its guard)

The file opens with `#nullable enable`, so `internal void Register(Control itemViewer, Func<bool> popupIsOpen)` declares both parameters non-nullable. The body then guards `if (itemViewer == null || popupIsOpen == null) return;`, and the XML doc states in two places that a null value "is ignored rather than rejected" because the registration hop runs from a form lookup that can legitimately find no form. The tests pass `null` to both parameters and assert no throw.

To a nullable-aware reader the guard is unreachable defensive code, and a future caller in a `#nullable enable` file would get CS8625 for the call the type is documented to accept. Neither happens today only because both current callers — `QfcFormViewer.cs` and `BreadcrumbPopupOwnerRegistryTests.cs` — sit in files without the pragma, so no diagnostic fires and the analyzer and nullable gates are clean.

`.claude/rules/csharp.md` "Null safety" requires optional values to be modelled with nullable annotations *and* guard clauses; here only the guard clause is present.

**Recommendation:** declare `Register(Control? itemViewer, Func<bool>? popupIsOpen)`. That makes the annotation match the documented and tested contract with no behaviour change. Not blocking: the delivered form is correct at runtime and passes both builds.

### CR-3 — Minor — stale line-count citation in a doc comment on a file this change modified

**Location:** `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:11`

> "Held on a second partial-class part so `BreadcrumbDropDownHost.cs` (480 lines) stays clear of the repository's 500-line ceiling."

The cited figure was already wrong before this branch (the file was 496) and is now 459. The comment is directly about the ceiling constraint that drove the Design B relocation into this very file, so it is the one comment a future reader is most likely to trust. The relocation was the natural moment to correct it.

**Recommendation:** either update the figure or, better, drop the parenthetical so the comment cannot go stale again. Pre-existing inaccuracy, so not attributed to this delivery.

### CR-4 — Minor — a throwing ribbon-release callback masks an in-flight teardown exception

**Location:** `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:262-272`

If the `try` body throws and the callback then also throws, C# `finally` semantics discard the original exception and propagate the callback's. `RunTeardownStage("controller-cleanup", Cleanup)` logs one exception, so the first failure — the one that describes what actually went wrong during teardown — is lost.

This is a genuine consequence of the chosen shape, and it is the standard trade-off for invoking from a `finally`. The exactly-once property AC4 requires is unaffected. The alternative (a nested `try`/`catch` around the invoke that aggregates or logs the secondary failure) would add branching to a method whose simplicity is itself a policy value, and the case requires two simultaneous faults.

**Recommendation:** record only. If it is ever addressed, a `catch` around `parentCleanup?.Invoke()` that logs the secondary exception and rethrows nothing would preserve the primary.

### CR-5 — Informational — `_datamodel = null` adds one more unguarded post-cleanup read surface

**Location:** `QuickFiler/Controllers/QfcHomeController.cs:391`

`_datamodel` is read without a null-conditional at six sites: `QfcHomeController.Iteration.cs:16, 22, 74/78` and `QfcHomeController.cs:251, 281, 300`. After this change a post-cleanup entry into any of them raises `NullReferenceException` where it previously reached a cleaned-but-non-null datamodel.

Three facts bound the risk, and none of them was assumed:

- `IterateQueueAsync` opens with `Token.ThrowIfCancellationRequested()` (`Iteration.cs:14`), and the Cancel teardown cancels the source in its first stage (`EventHandlers.cs:133`), so the cancelled-token throw precedes the field read on the path that actually reaches teardown.
- The same exposure already exists for the five sibling fields nulled beside it; `_formController` is read unguarded at `Iteration.cs:23` and `:78`. The change adds an instance of an existing pattern rather than a new class of risk.
- The nulling is what makes AC3 work: `ActionCancelAsync`'s `_parent?.TokenSource?.Cancel()` and `_parent?.DataModel?.QuiesceLoaderAsync(...)` both become no-ops after cleanup instead of reaching a disposed source.

**Recommendation:** none for this branch. The structural fix is report-only item 2 (token-source ownership redesign), which would remove the shared-field pattern entirely.

## 4. Observations

- **OBS-1.** `artifacts/pr_context.summary.txt` reports `Core logic changes: 0 files` and files all sixteen C# and project changes under `Docs/templates/agents/tooling`. The coverage hook derives its language set from that file, so the automated C# gate does not run on this branch. Generator defect, recurring, not attributable to the delivery. Recorded in the policy audit section 4.5.
- **OBS-2.** The `[P1-T10]` declared-versus-observed exit-code divergence is correctly handled: the expectation is mechanically derived, the baseline failure is demonstrably intermittent (three passes and one failure across four observations of the same test), the run was 1381 of 1381 green, and recording the divergence rather than rewriting the expectation is the choice that keeps every other expectation in the plan falsifiable. Full reasoning in the policy audit section 11.
- **OBS-3.** Three rule sources state different repository coverage floors (85/75 versus 80/90). Pre-existing and unreconciled; both readings are reported.
- **OBS-4.** Ten report-only items (the delivery's nine plus CR-1) are recorded in `evidence/issue-updates/issue-810.2026-09-08T10-32.md` but no follow-up issues exist, because the executing agent had no `gh` access and correctly refused to fabricate issue numbers. Prose in a feature folder does not survive merge, so this needs the caller's action.

## 5. What was done well

Recorded because these are choices worth repeating, not as praise:

- **Fail-before evidence names a mechanism, not just a count.** Each fail-before artifact states which line produced the failure and why, so the test is shown to discriminate against the specific defect rather than merely to have been red at some point.
- **The AC2 evidence is a non-edit.** Treating "this file stays byte-unmodified and green" as the criterion, and proving it with both a name-listing diff and a porcelain status (because the first cannot see an untracked replacement), is a stronger form of regression fence than any assertion inside the file could be.
- **The layout decision was measured, not assumed.** The research artifact asserted relocation was required; the spec demoted that to a conditional and required a post-CSharpier measurement; the measurement came back 504 and selected the relocation on evidence.
- **The one textual departure from a verbatim move was disclosed.** A dropped line citation is trivial, and disclosing it is what makes the "byte-identical apart from this" claim checkable.
- **Comments were corrected rather than left to rot.** AC6 exists only to fix two comments that had become false and to delete an accessor with no readers; carrying that work rather than deferring it is why the next reader of `FinishClose` will not be misled.

## 6. Verdict

**ACCEPT — 0 blocking findings.**

Five non-blocking findings (CR-1 through CR-5) and four observations are recorded above. CR-1 is the only one that describes a live defect in production code, it is outside every criterion's wording, and it should be promoted as a follow-up issue rather than fixed on this branch.
