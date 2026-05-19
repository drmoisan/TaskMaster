# Feature Audit — quickfiler-navigation-key-collision-111 (2026-03-27T13-28)

- **Feature folder:** `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/`
- **Branch:** `bug/quickfiler-navigation-key-collision-111` @ `40f176c1cd207a5a5971698d0e9ae762080de926`
- **Base branch:** `main` @ `cb6a6edd11590c245d36ccba16ca5c4c6732ce8f`
- **Work mode:** `minor-audit`
- **AC source:** `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md`
- **Supersedes:** `feature-audit.2026-03-27T13-11.md`

## 1. Scope and baseline

| Field | Value |
|---|---|
| Base branch | `main` @ `cb6a6edd11590c245d36ccba16ca5c4c6732ce8f` |
| Head commit | `40f176c1cd207a5a5971698d0e9ae762080de926` |
| Merge base | `cb6a6edd11590c245d36ccba16ca5c4c6732ce8f` |
| AC source | `issue.md` (`Work Mode: minor-audit`) |
| Evidence — primary | `issue.md`, `plan.2026-03-27T12-45.md`, feature-folder evidence artifacts, live `git diff main...HEAD`, and fresh review-time C# QA reruns |
| Evidence — secondary | Stale `artifacts/pr_context.summary.txt` / `artifacts/pr_context.appendix.txt` were inspected only to confirm they are not authoritative for this branch/base pair |
| Production files changed | `QuickFiler/Controllers/KbdActions.cs` |
| Test files changed | `QuickFiler.Test/Controllers/KbdActionsTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj` |

## 2. Acceptance criteria inventory (authoritative)

Extracted from `issue.md` § `## Acceptance Criteria`:

| ID | Criterion | Source |
|---|---|---|
| AC-1 | `KbdActions<string, KaStringAsync, Func<string, Task>>` no longer treats distinct stored keys `1`, `01`, and `10` as duplicates for the same `SourceId` during storage operations. | `issue.md` |
| AC-2 | An exact duplicate stored key for the same `SourceId` still throws `ArgumentException`. | `issue.md` |
| AC-3 | Runtime keyboard-input matching semantics based on `KaStringAsync.KeyEquals` remain available for filtering and lookup behavior. | `issue.md` |
| AC-4 | The repository C# QA loop passes for the change: format, analyzer build, nullable/type-check build, and MSTest with coverage. | `issue.md` |

## 3. Acceptance criteria evaluation

| ID | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|---|---|---|---|---|
| AC-1 | Distinct stored keys no longer collide during storage operations | PASS | `KbdActions.cs` now uses `StoredKeyEquals()` based on `EqualityComparer<TKey>.Default.Equals` in both `Add` overloads and `Remove`; `KbdActionsTests.Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate` proves `10` and `1` coexist. | `git diff --unified=3 main...HEAD -- QuickFiler/Controllers/KbdActions.cs`; `git diff --unified=3 main...HEAD -- QuickFiler.Test/Controllers/KbdActionsTests.cs`; canonical pass-after evidence in `p2-t4-tests-with-coverage.2026-03-27T13-08.md` | The issue text also names `01`; exact string equality generalizes to that literal even though the focused regression names `10` and `1`. |
| AC-2 | Exact duplicate stored key still throws `ArgumentException` | PASS | `KbdActionsTests.Add_WhenSourceAndStoredKeyAreExactDuplicate_ThrowsArgumentException` covers a second `"1"` registration for the same `SourceId` and asserts `ArgumentException` with the expected message pattern. | `git diff --unified=3 main...HEAD -- QuickFiler.Test/Controllers/KbdActionsTests.cs`; canonical pass-after evidence in `p2-t4-tests-with-coverage.2026-03-27T13-08.md` | This directly verifies the negative path requested by the issue. |
| AC-3 | Runtime keyboard-input matching semantics remain available | PASS | `ContainsKey`, `FilterKeys`, `Find`, and `FindIndex` still use `KeyEquals`; `FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics` proves substring-based filtering remains available for lookup/filter behavior while storage uses exact identity. | `git diff --unified=3 main...HEAD -- QuickFiler/Controllers/KbdActions.cs`; `git diff --unified=3 main...HEAD -- QuickFiler.Test/Controllers/KbdActionsTests.cs` | No `QfcCollectionController.cs` compatibility change was required. |
| AC-4 | Repository C# QA loop passes | PASS | Canonical feature QA artifacts record `EXIT_CODE: 0` for formatter, analyzer build, nullable build, and coverage-enabled MSTest. The live review-time reruns also completed successfully. | `dotnet tool run csharpier check .`; `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`; `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`; `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` | Canonical QA artifact `p2-t4-tests-with-coverage.2026-03-27T13-08.md` reports `2877 total`, `2875 passed`, `2 skipped`, `0 failed`, overall line coverage `61.61%`. |

## 4. Acceptance criteria check-off update

No checkbox edits were required in `issue.md` during this re-review.

All four acceptance criteria were already marked `[x]` in the authoritative source, and the current review confirms that state is accurate.

## 5. Summary

**Overall feature readiness: PASS**

All four authoritative acceptance criteria in `issue.md` are satisfied for the current `main...HEAD` range. The feature branch is scoped correctly, the evidence chain is consistent with the plan, and the repository C# QA loop is green. No additional remediation is required.

Recommended follow-up steps:

1. Optionally refresh the shared `artifacts/pr_context.*` bundle before PR creation if the collector command becomes available.
2. Optionally add a direct `"01"` literal regression or a `Remove()` regression in a future hardening pass; these are not required for the current acceptance criteria to pass.

## 6. Acceptance Criteria Status

- Source: `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md`
- Total AC items: `4`
- Checked off (delivered): `4`
- Remaining (unchecked): `0`
- Items remaining: `None`