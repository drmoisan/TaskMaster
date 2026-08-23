# [P3-T8] Phase 3 Gate — Open-Pipeline Suites Green

- **Issue:** #438
- **Task:** [P3-T8]
- **Timestamp:** 2026-08-08T11-41

## Command 1 — scoped pipeline suites

`pwsh -NoProfile -Command "& 'C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:\"FullyQualifiedName~BreadcrumbDropDown|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbItemViewerLifecycleCoordinator\" ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0

```
Total tests: 128
     Passed: 128
     Failed: 0
```

Zero failures. Coverage of this filter includes `BreadcrumbDropDownHostTests` (+ new Part2), `BreadcrumbDropDownOpenCoordinatorTests` (+ Part2 + new Part3), `BreadcrumbDropDownIntegrationTests`, `BreadcrumbDropDownLifecycle*Tests`, `BreadcrumbPendingOpenCloseTests`, `BreadcrumbSelectorOpenRetryTests`, and `BreadcrumbItemViewerLifecycleCoordinatorTests`.

## Command 2 — existing-test-file modification audit

`pwsh -NoProfile -Command "git diff --stat -- QuickFiler.Test/Viewers/"`

- **EXIT_CODE:** 0

```
 .../Viewers/BreadcrumbDropDownHostTests.cs           |  2 +-
 .../BreadcrumbDropDownOpenCoordinatorTests.cs        | 16 ++++++++++++++++
 .../BreadcrumbItemViewerLifecycleCoordinatorTests.cs | 20 +++++++++++++++++++-
 .../Viewers/BreadcrumbSelectorOpenRetryTests.cs      | 16 ++++++++++++++++
 4 files changed, 52 insertions(+), 2 deletions(-)
```

Exactly the four files sanctioned by D3 and D7. `BreadcrumbDropDownIntegrationTests.cs` has **no diff**.

### `BreadcrumbDropDownHostTests.cs` — one token, verified

```diff
@@ -18 +18 @@ namespace QuickFiler.Test.Viewers
-    public sealed class BreadcrumbDropDownHostTests
+    public sealed partial class BreadcrumbDropDownHostTests
```

One line, one token. No test method touched.

### The three fake-implementer files — additive only, verified

Filtering the combined diff of the three files for any added or removed line containing `[TestMethod]`, `public void `, `Should()`, `Verify(`, or `Assert.` returns **zero lines**. Every change is confined to the private fake host classes: a `RequestedTakeFocus` recording list plus a 4-parameter `OpenAsync` overload, with the pre-existing 3-parameter method becoming a one-line delegation to it with `takeFocus: true`. No test method was added, removed, weakened, or altered.

## Deviation D11 — explicit interface implementation of the 4-parameter overload

**Observed problem.** `BreadcrumbDropDownHostTests.cs:342-350` reaches the host by reflection with `host.GetType().GetMethod("OpenAsync")`, an overload-name lookup with no parameter-type array. `Type.GetMethod(String)` throws `AmbiguousMatchException` when more than one method matches. Adding a second public `OpenAsync` to the concrete `BreadcrumbDropDownHost` made that call ambiguous and broke **8 pre-existing tests** plus 2 new ones:

```
Total tests: 21   Passed: 11   Failed: 10
System.Reflection.AmbiguousMatchException: Ambiguous match found.
  (OpenAsync_CreatesToolStripControlHostAndUsesCalculatedScreenPlacement,
   ExplicitCommitAndUncommittedClose_HaveDistinctCallbacks,
   OpenAndClose_TransferFocusIntoPendingOptionAndBackToAnchor,
   OpenAsync_WhenAlreadyOpen_FocusesPendingWithoutRecreatingOrShowing,
   OpenAsync_ZeroWorkingArea_RestoresSelectionAndFocus,
   OpenAsync_ShowFailure_ClosesUncommittedAndRetainsTheFailure,
   NativeClosedEvent_CancelsOnceAndIgnoresLaterCloseNotifications,
   ResetAndDispose_HandleOpenOrPartialStateAndRejectLaterUse)
```

**Resolution.** The intent-carrying overload is implemented as an **explicit interface implementation** on `BreadcrumbDropDownHost`:

```csharp
public Task<bool> OpenAsync(Rectangle a, Rectangle w, Size s)
    => OpenWithFocusIntentAsync(a, w, s, true);

Task<bool> IBreadcrumbDropDownHost.OpenAsync(Rectangle a, Rectangle w, Size s, bool takeFocus)
    => OpenWithFocusIntentAsync(a, w, s, takeFocus);
```

The concrete type therefore keeps exactly one public `OpenAsync`, `GetMethod("OpenAsync")` is unambiguous again, and all 8 pre-existing tests pass with the file byte-identical apart from its sanctioned `partial` token.

**Why this and not a test edit.** The alternative was editing the private `Open` helper in `BreadcrumbDropDownHostTests.cs` to pass an explicit parameter-type array. That edit is outside the enumerated set of sanctioned structural edits (spec AC-11 and plan D3/D4 permit only additive fake-implementer members and one-token `partial` keywords), so it was rejected. The chosen resolution is also independently justified by `.claude/rules/csharp.md` § Coding Standards: "Keep public API surface intentional and minimal." Every consumer of the focus intent — `BreadcrumbDropDownOpenCoordinator` and the tests — holds an `IBreadcrumbDropDownHost`, so a second public overload on the concrete type would widen the public surface for no caller.

**Contract impact: none.** Spec AC-10 constrains the `IBreadcrumbDropDownHost` contract, which gains exactly one additive overload with the 3-parameter member delegating with `takeFocus: true`. That is satisfied. No existing signature is removed or altered.

**Verification after the change:** `FullyQualifiedName~BreadcrumbDropDownHostTests` -> `Total tests: 21, Passed: 21`, EXIT_CODE 0.

## Deviation D12 — P3-T4 lifetime plumbing landed inside the P3-T1..T3 compile unit

The plan's compile-unit note places the solution-build gate at P3-T3. P3-T2 requires the flag to be "passed to the open lifetime", which cannot compile unless `BreadcrumbDropDownOpenLifetime.OpenAsync`/`OpenCoreAsync` already accept it. The parameter threading and the `takeFocus` guard on `FocusCurrentSurface` therefore landed with P3-T2, and P3-T4's specified relocation of `FocusCurrentSurface` into the new `BreadcrumbDropDownOpenLifetime.Focus.cs` partial was performed in the same compile unit. Both P3-T3 and P3-T4 gates were then run and returned EXIT_CODE 0. Sequencing only; no content, contract, or acceptance criterion differs from the plan.

## Files delivered in Phase 3

| File | Change |
|---|---|
| `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` | **additive** 4-parameter `OpenAsync` overload (68 lines) |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | one-token `partial`; 3-parameter `OpenAsync` body relocated out (463 lines) |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | **new** — both entry points plus the private `OpenWithFocusIntentAsync` body |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | one-token `partial`; `takeFocus` threaded through `OpenAsync`/`OpenCoreAsync`; `FocusCurrentSurface` relocated out (459 lines) |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs` | **new** — `FocusCurrentSurface` with the conditional `_host.FocusPending()` guard |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | latch field, `LatchNextOpenTakesNoFocus()`, `NextOpenTakesNoFocus`, latch-consuming dispatch in `BeginOpenCore` (355 lines) |
| `QuickFiler/QuickFiler.csproj` | two `<Compile Include>` entries |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` | one-token `partial` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` | **new** — 8 focus-count tests |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` | **new** — 6 latch tests |
| the three fake-implementer files | additive `RequestedTakeFocus` + 4-parameter `OpenAsync` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | two `<Compile Include>` entries |

All files verified under the 500-line ceiling (largest: `BreadcrumbSelectorOpenRetryTests.cs` at 477).

## GUI-seam compliance

`BreadcrumbDropDownHostTests`'s `Harness` injects `showPopup`, `focusPending`, and `focusAnchor` as counting delegates, so the real host never shows a native popup or focuses a real control. The open-coordinator harness uses a hand-written `ControlledHost` and a capturing synchronization context drained explicitly. No window is created by any test added in this phase.

## Result

- **Output Summary:** EXIT_CODE 0 with 128 of 128 pipeline tests passing. The only existing test files with diffs are the four sanctioned by D3/D7; `BreadcrumbDropDownIntegrationTests.cs` is untouched; a targeted diff filter confirms no test-method line was added, removed, or altered in any of them. Two deviations are recorded: D11 (explicit interface implementation, adopted to keep 8 pre-existing tests passing byte-unmodified) and D12 (P3-T4 plumbing sequenced into the P3-T1..T3 compile unit). Accept criteria met.
