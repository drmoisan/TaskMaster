# Feature Audit — utilities-coverage (2026-03-14T01-49)

## Scope and baseline

- **Feature folder:** `docs/features/active/2026-03-13-utilities-coverage-65/`
- **Current branch inspected:** `feature/utilities-coverage-65`
- **Base branch assumption:** `main` (no `${input:PRBaseBranch}` was provided)
- **Primary evidence sources:**
  - Active scoping docs in `docs/features/active/2026-03-13-utilities-coverage-65/`
  - Canonical evidence under `evidence/baseline/` and `evidence/qa-gates/`
- **PR context note:** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are stale for this feature review, so they were not treated as authoritative without corroborating feature-folder evidence.

## Acceptance criteria inventory

Authoritative criteria were extracted from:

- `docs/features/active/2026-03-13-utilities-coverage-65/spec.md`
- `docs/features/active/2026-03-13-utilities-coverage-65/user-story.md`

## Acceptance criteria evaluation

| Criterion | Status | Evidence | Verification command(s) | Notes |
|---|---|---|---|---|
| Every public class/method in `UtilitiesCS/Extensions/` has corresponding tests in `UtilitiesCS.Test/Extensions/` | **PARTIAL** | Coverage improved substantially, but several extension files remain below target, including `ArrayExtensions.cs` (**77.70%**), `IEnumerableExtensions.cs` (**68.65%**), `AsyncSerialization.cs` (**13.86%**), and `IAsyncEnumerableExtensions.cs` (**20.24%**). | Coverage evidence review; `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug` | Corresponding tests exist for many extension areas, but the evidence does not support “every public class/method” completion. |
| Every public class/method in `UtilitiesCS/HelperClasses/` has corresponding tests in `UtilitiesCS.Test/HelperClasses/` | **PARTIAL** | Multiple helper files remain below target or unverified, e.g. `FileSystemInfoWrapper.cs` (**0%**), `ThemeControlGroup.cs` (**0%**), `QfcTipsDetails.cs` (**0%**), while others are well-covered. | Coverage evidence review | Directory breadth improved, but completion claim is not met. |
| Every public class/method in `UtilitiesCS/Threading/` has corresponding tests in `UtilitiesCS.Test/Threading/` | **PARTIAL** | Several threading files remain at **0%** or well below target, including `ApplicationIdleTimer.cs`, `AsyncMultiTasker.cs`, `IdleActionQueue.cs`, `IdleAsyncQueue.cs`, `ProgressTrackerAsync.cs`, and `ThreadMonitor.cs`. | Coverage evidence review; MSTest pass | Some threading types were covered, but not the entire intended surface. |
| Every public class/method in `UtilitiesCS/ReusableTypeClasses/` has corresponding tests in `UtilitiesCS.Test/ReusableTypeClasses/` | **PARTIAL** | Coverage is strong for some types (`Matrix`, `DenMatrix`, `TimerWrapper`, etc.) but low for others such as `SCODictionary.cs` (**3.02%**), `ScoCollection.cs` (**3.28%**), `SloLinkedList.cs` (**12.12%**), and `SmartSerializable*` files. | Coverage evidence review | Broad progress, but not complete. |
| Every public class/method in `UtilitiesCS/NewtonsoftHelpers/` has corresponding tests in `UtilitiesCS.Test/NewtonsoftHelpers/` | **PARTIAL** | Some files are fully covered (`AllInclusiveBinder`, `KnownTypesBinder`, `ScoDictionaryConverter`), but others remain low or untested (`DerivedCompositionConverter_ConcurrentDictionary.cs` **0%**, `NConsoleTraceWriter.cs` **0%**, `NonRecursiveConverter.cs` **0%**). | Coverage evidence review | Partial completion only. |
| Per-file line coverage ≥80% for each testable UtilitiesCS source file | **FAIL** | `evidence/qa-gates/final-per-file-coverage.2026-03-14T01-42.md` shows **72 / 256** non-excluded files meeting target and **184** below threshold. | Coverage artifact inspection | This is the primary feature-completion blocker. |
| All tests pass with zero failures | **PASS** | Review rerun: **1006 total**, **1003 passed**, **0 failed**, **3 skipped**. Canonical artifact also records **0 failed**. | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug` | Passes on failures; note there are still 3 skipped tests. |
| Tests are independent, isolated, fast, and deterministic | **PARTIAL** | Timing and environment coupling still exist (`Thread.Sleep` in `TimedBatchAction_Tests.cs:84`; repo filesystem lookup in `DirectoryInfoWrapper_Tests.cs:92` and `FileInfoWrapper_Tests.cs:83`; wall clock use in `MyFileSystemInfoTests.cs:28-29`). | Static inspection plus full MSTest run | The suite is passing, but the criterion is not fully satisfied. |
| No external dependencies (no file I/O, no network, no COM interop) in tests—all mocked | **FAIL** | Some tests touch the live filesystem (`DirectoryInfoWrapper_Tests`, `FileInfoWrapper_Tests`). `StoresWrapperTests.cs:71` also contains an ignored Outlook COM-dependent test placeholder. | Static inspection | The branch largely avoids external systems, but not completely. |
| Tests mirror the production directory structure in `UtilitiesCS.Test` | **PASS** | The feature adds or expands tests across `Extensions/`, `HelperClasses/`, `ReusableTypeClasses/`, `EmailIntelligence/`, `OutlookObjects/`, `Dialogs/`, and `Threading/`, matching the production layout. | Directory inspection | This criterion is met. |
| Full C# toolchain passes: format → analyzers → nullable build → test | **PASS** | Review reran all four commands successfully on the current branch. | `dotnet format TaskMaster.sln --verify-no-changes --no-restore`; analyzer MSBuild; nullable MSBuild; MSTest script | Current branch state passes the toolchain, even though the feature’s recorded Phase 8 loop was not fully reconciled in the plan doc. |

## Summary

- **Overall feature readiness:** **NEEDS REVISION**
- **Top gaps preventing PASS:**
  1. The documented ≥80% per-file coverage goal is not met.
  2. Several acceptance criteria claim complete directory-wide test coverage that the evidence does not support.
  3. A subset of tests still violates the feature’s isolation/no-file-I/O expectations.

## Recommended follow-up verification

Because the failures are concrete rather than merely unverified, the next verification steps should happen **after remediation**:

1. Re-run coverage collection and regenerate `final-per-file-coverage.*.md` after the missing low-coverage files are addressed.
2. Re-run the full toolchain in a single contiguous pass and update the plan/evidence so Phase 8 reflects the real final state.
3. Re-review the filesystem/time-coupled tests after they are converted to fully isolated patterns.
