# Code Review — F5 disabled-stores-settings-ui (Issue #265)

- Branch: `feature/disabled-stores-settings-ui-265` @ HEAD `abe278ec`
- Diff range: `872eafb4..HEAD`
- Timestamp: 2026-07-08T04-24
- Reviewer: feature-review agent

## Executive Summary

The change is cohesive, well-documented, and follows the existing `StoreWrapperController` + `IViewer`
precedent. Decision logic is isolated in a controller behind an interface seam, with the single live
`DataGridView.DataSource` write confined to the WinForms-exempt viewer. Error handling uses the established
log4net + `MyBox` boundary, and the reenable path re-fetches state on both success and failure so the grid
cannot drift from the service. Naming, XML docs, file sizes, and null-safety are consistent with repository
standards. No Blocking or non-blocking-PARTIAL code-quality findings were identified; two Advisory items are
recorded for future polish.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Advisory | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs` | `Dgv_CellContentClick` -> `_ = ReenableAsync(row)` | Fire-and-forget of the returned `Task`; exceptions are contained inside `ReenableAsync` (try/catch/finally), so nothing escapes, but the discard is a deliberate async-void-equivalent | Keep as-is; the `ReenableInFlight` guard plus internal try/finally make this safe. Optionally add a code comment cross-referencing the contained failure path (already partially present) | WinForms synchronous event handlers cannot await; the pattern is contained and matches repo precedent | code lines 89-93 |
| Advisory | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs` | `ReenableAsync` finally block | The `Viewer.InvokeRequired == true` marshaling branch has no unit test (branch coverage 50% on the async fragment) | Add one test with a mocked `InvokeRequired = true` verifying `Viewer.Invoke` is used | Improves branch coverage of the refresh path; line coverage (authoritative here) already passes | policy-audit §6 |
| PASS | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs` | class | Single responsibility, small focused methods, guard clauses, log4net logging, `[ExcludeFromCodeCoverage]` on the WinForms `Launch()` shell only | none | Meets General/C# design and error-handling policy | source review |
| PASS | `UtilitiesCS/OutlookObjects/Store/DisabledStoreRow.cs` | class | Pure POCO view-model, no WinForms/Outlook/I/O dependency; clear XML docs | none | Correct separation of pure data from framework glue | source review |
| PASS | `UtilitiesCS/OutlookObjects/Store/IDisabledStoresViewer.cs` | interface | Minimal `internal` seam extending `IForm`; `BindRows` isolates the live data-source write | none | Correct DI-seam design; keeps grid write in exempt code | source review |
| PASS | `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs` | class | Behavior-preserving extraction of readiness logic into a shared `internal static` helper; 100% covered | none | Reuse without duplication; simplicity-first | source review; readiness-extraction evidence |
| PASS | `UtilitiesCS/OutlookObjects/Store/DisabledStoresViewer.cs` + `.Designer.cs` | Form + Designer | Thin shell; wires `Dgv.CellContentClick` to the controller; `CellFormatting` renders the future-sessions distinction keyed on `IsFutureSession`; column order (DisplayName=0, Scope=1, Reenable=2) matches `ReenableColumnIndex` | none | WinForms-exempt glue kept minimal | Designer review |
| PASS | `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | `EvaluateLaunchReadiness` | Reduced to a one-line delegation with unchanged signature/accessibility/return type | none | Behavior-preserving; existing 51 tests pass unmodified | diff; readiness-extraction evidence |
| PASS | `TaskMaster/Ribbon/RibbonController.cs`, `RibbonViewer.cs`, `RibbonExplorer.xml` | ribbon wiring | Additive `DisabledStoresSettings` button/callback/dispatch mirroring the Folder Settings path; both classes carry class-level `[ExcludeFromCodeCoverage]` | none | Additive, no behavioral risk to existing buttons | diff |
| PASS | `UtilitiesCS.Test/OutlookObjects/Store/DisabledStoresControllerTests.cs` | test class | AAA structure, descriptive names, Moq seams, `MyBox.DialogInvoker` seam, reflection helpers mirroring `StoreWrapperViewerTests`; deterministic (completed/faulted Tasks) | none | Meets General/C# unit-test policy | source review; controller-tests-pass |

## Detailed Notes

### Correctness of the reenable/refresh ordering

`ReenableAsync` calls the service inside try/catch and performs `PopulateRows()` in `finally`, so the grid
is re-projected from `GetDisabledStores()` on both the success and failure paths. This is the intended
"re-fetch after action" invariant and it is directly asserted by both the success test
(`GetDisabledStores` Times.Once + `BindRows` Times.Once) and the failure test (MyBox invoked once,
`GetDisabledStores` still Times.Once). The `ReenableInFlight` guard prevents overlapping in-flight reenables
on rapid double-clicks.

### Concurrency / thread marshaling

The finally block honors the `Viewer.InvokeRequired`/`Viewer.Invoke(...)` convention already used by
`StoreWrapperController`'s folder-picker handlers before touching viewer state from the continuation. The
`Invoke(() => PopulateRows())` shape compiles against the same `IForm`-derived seam the existing controller
uses, so it is consistent with proven precedent.

### Style consistency

Naming (`PascalCase` types/members, `camelCase` locals), explicit `using` directives, and XML documentation
on non-obvious public members all match repository conventions. The controller documents the non-obvious
CS0053 reconciliation (Viewer made `internal` because `IDisabledStoresViewer` is `internal`) inline, which
is the correct place for a "why" comment.

## Verdict

PASS. Zero Blocking findings, zero non-blocking-PARTIAL findings. Two Advisory items for optional
follow-up (fire-and-forget comment clarity; `InvokeRequired=true` branch test).
