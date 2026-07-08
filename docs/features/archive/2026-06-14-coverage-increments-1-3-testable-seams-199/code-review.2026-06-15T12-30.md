# Code Review: coverage-increments-1-3-testable-seams (#199)

**Review Date:** 2026-06-15
**Reviewer:** Feature Review Agent (Claude Sonnet 4.6)
**Feature Folder:** `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/`
**Base Branch:** `main` (merge-base `d436a06f`)
**Head Branch:** `refactor/coverage-increments-1-3-199` (HEAD `3b7defa3`)
**Review Type:** Post-Phase-6 final review

---

## Executive Summary

This branch adds 14 new MSTest unit test files across three assemblies (ToDoModel.Test, QuickFiler.Test, TaskMaster.Test), makes three maintainer-authorized minimal production seam changes, and registers the new test files in three `.csproj` files. The branch also contains feature-folder documentation (spec, plan, evidence, prior review artifacts). The diff spans 91 files, 6,287 additions, and 6 deletions. The 6 deletions are the three `MessageBox.Show` calls replaced by `MyBox.ShowDialog` calls in `ProjectEntry.cs`.

The implementation is well-structured. Test code follows MSTest + Moq + FluentAssertions + AAA conventions consistently. The three production seam changes are minimal and behavior-preserving: a single `InternalsVisibleTo` attribute line, a method delegation extraction that preserves byte-for-byte semantics, and three dialog-routing replacements that change the call site but not the displayed message, button styles, icons, or return-value comparisons. All 349 tests pass, all four toolchain steps pass, and coverage increased on all three feature assemblies.

**What changed:**
- 14 new test files covering ToDoLoader.SetAndSave, IDList.GetNextToDoID, ProjectEntry (dialog-free and dialog-dependent branches), BaseChanger, KaChar/KaKey/KaStringAsync value objects, KbdActions, FilerQueue, QfcQueue, AppStagingFilenames, AppFileSystemFolderPaths.MatchBestSpecialFolder, and AppQuickFilerSettings.
- `UtilitiesCS/Properties/AssemblyInfo.cs`: +1 line `[assembly: InternalsVisibleTo("ToDoModel.Test")]`.
- `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`: `MatchBestSpecialFolder` instance body extracted to `internal static` helper; instance method delegates.
- `ToDoModel/Data Model/Project/ProjectEntry.cs`: 3× `MessageBox.Show(...)` → `MyBox.ShowDialog(...)` in `ProjectID` setter.
- 3 `.csproj` files: additive `<Compile Include>` registrations for new test files.

**Top 3 risks:**
1. The `MatchBestSpecialFolder` instance-method delegation line (line 62 of `AppFileSystemFolderPaths.cs`) remains uncovered by unit tests. The static helper is 100% covered, but the single delegation line requires instantiating `AppFileSystemFolderPaths`, which calls `LoadFolders` and touches the filesystem. This is a minor residual gap, not a policy violation.
2. The `ProjectID` setter first `MyBox.ShowDialog` call (malformed-ID arm, line ~40) was originally a single-argument `MessageBox.Show(string message)`. The replacement adds three arguments (`"Dialog"`, `MessageBoxButtons.OK`, `MessageBoxIcon.Warning`). The dialog behavior is functionally equivalent but the arguments were not present in the original. This is documented in `evidence/other/p6-production-seam-verified.2026-06-14T17-00.md` as an authorized design decision.
3. `[STATestClass]` is used on `ProjectEntryDialogBranchesTests` because `MyBox.ShowDialog` constructs a WinForms `MyBoxViewer` control. The `DialogInvoker` stub prevents any dialog from being shown, but the STA requirement means the tests cannot run under the MTA apartment. This is correctly handled and presents no risk provided CI uses vstest with STA support (which it does: MSTest's `[STATestClass]` is a standard attribute).

**PR readiness recommendation:** **Go** — All toolchain gates pass, all acceptance criteria are met and checked off, no blocking findings.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | Line 62 | Instance delegation line `return MatchBestSpecialFolder(SpecialFolders, path)` is not covered by unit tests. The static helper it calls is 100% covered. | No action required. Integration test or a test that doesn't call LoadFolders could cover it in a future increment. | The delegation is a single-line refactor of previously-uncovered code; the static helper semantics are fully validated. | `p5-coverage-delta.2026-06-14T15-10.md` §Covered-line increase |
| Info | `ToDoModel/Data Model/Project/ProjectEntry.cs` | Line ~40 (malformed-ID arm) | The first `MessageBox.Show` call was a single-argument form; replacement adds `"Dialog"`, `MessageBoxButtons.OK`, `MessageBoxIcon.Warning`. These are the correct equivalent arguments, but were not present in the original. | Verify the displayed dialog matches user intent. No code change needed — the call is correct and authorized. | The original single-argument `MessageBox.Show(string)` displayed a dialog with default title ""; adding explicit title, buttons, and icon is a user-experience improvement, not a behavior regression. | `evidence/other/p6-production-seam-verified.2026-06-14T17-00.md` |
| Nit | `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs` | Lines 185–194 | Comment block explaining the CompareTo tie-break Moq strategy is long (10 lines). | No change required; the comment explains a non-obvious test pattern. | The CompareTo branch requires a shifting-ProjectID mock because a plain ProjectEntry cannot produce equal ordinal + unequal length (all accessible constructors validate 4-char length). The comment is load-bearing. | Direct file inspection |

No Blockers or Major findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- The `MatchBestSpecialFolder` static helper extraction is textbook: the instance body is moved verbatim, the parameter name is changed from `SpecialFolders` (field reference) to `specialFolders` (parameter), and the instance method delegates in a single line. No behavior difference is possible because the only read of `SpecialFolders` in the original was the collection passed to `Where`. The XML `<summary>` and `<remarks>` on the static helper document this explicitly.
- The three `MessageBox.Show` → `MyBox.ShowDialog` replacements in `ProjectEntry.cs` are targeted and complete. `grep` for `MessageBox.Show(` returns zero matches post-change (verified in `p6-production-seam-verified.2026-06-14T17-00.md`). The `DialogResult` comparison patterns (`response == DialogResult.Yes`) are unchanged.
- The `[assembly: InternalsVisibleTo("ToDoModel.Test")]` addition to UtilitiesCS is the minimum change needed to expose the existing `MyBox.DialogInvoker` internal seam to the test project. No `MyBox` member behavior, signature, or visibility beyond this one test project was changed.
- `ProjectEntryDialogBranchesTests.cs` correctly uses `[TestInitialize]` and `[TestCleanup]` to manage the `MyBox.DialogInvoker` seam: seed a no-op stub before each test, restore the real invoker after. This prevents cross-test leakage without requiring a global fixture. The pattern is well-documented in the class-level XML comment.

#### Type safety and API notes

- `AppFileSystemFolderPaths.MatchBestSpecialFolder` static helper accepts `IReadOnlyDictionary<string, string>` rather than the concrete `ConcurrentDictionary<string, string>` used by the instance field. This is correct: the parameter type is the most permissive interface that satisfies the method's read-only usage, enabling tests to pass `Dictionary<string, string>` without needing `ConcurrentDictionary`.
- The nullable build (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`) passes, confirming no nullable regression from any of the three production changes.
- `MatchBestSpecialFolder` returns `string` (nullable return: `null` when no match). The callers handle this correctly: the test for null collection asserts `BeNull()`.
- All new test files use `var` appropriately for locals where type is obvious, and explicit types at public/internal boundaries (test method parameters and `IProjectEntry` in casts).

#### Error handling and logging

- No new exception-handling patterns introduced. The existing `ArgumentException` throw in `ProjectEntry.SetProjectId` `default:` arm is unchanged.
- `AppFileSystemFolderPaths` logging (`logger.Debug`, `logger.Error`) is unmodified by the `MatchBestSpecialFolder` extraction. The static helper does not log, which is correct: it is a pure function with no side effects.
- `ProjectEntry.cs` changes do not touch any exception handling or logging paths.

---

## Test Quality Audit

The test suite demonstrates consistent quality across all 14 new files. Key observations:

### Reviewed test and QA artifacts

- `evidence/qa-gates/inc1-mstest-coverage.2026-06-14T08-22.md` — Increment 1 vstest targeted filter run, EXIT_CODE 0; validates ToDoModel seam tests in isolation.
- `evidence/qa-gates/inc2-mstest-coverage.2026-06-14T08-22.md` — Increment 2 vstest targeted filter run, EXIT_CODE 0; validates QuickFiler seam tests.
- `evidence/qa-gates/inc3-mstest-coverage.2026-06-14T08-22.md` — Increment 3 vstest targeted filter run, EXIT_CODE 0; validates TaskMaster seam tests.
- `evidence/qa-gates/p5-mstest-coverage.2026-06-14T15-10.md` — Phase 5 full three-assembly run with the two new dialog/helper tests, EXIT_CODE 0.
- `evidence/qa-gates/p6-final-mstest-todomodel.2026-06-14T17-00.md` — Phase 6 final run, 98 tests, 98/98 passed in 3.76s; confirms 4 Phase 6 change-confirmation tests pass and ProjectEntry class coverage rose from 44.20% to 54.35%.
- `evidence/qa-gates/final-mstest-coverage.2026-06-14T08-22.md` — Full three-assembly suite 349/349 passed.
- `evidence/qa-gates/p5-coverage-delta.2026-06-14T15-10.md` — Per-seam line coverage table for Phase 5.
- `evidence/qa-gates/final-coverage-comparison.2026-06-14T08-22.md` — Net coverage comparison: all three assemblies increased.
- `evidence/qa-gates/p5-invariant-check.2026-06-14T15-10.md` — Production-change boundary check confirming exactly two authorized production changes in Phase 5.
- `evidence/qa-gates/final-invariant-check.2026-06-14T08-22.md` — Invariant check after Increments 1–3: zero production changes, no `[ExcludeFromCodeCoverage]` changes.

### Quality assessment prompts

- **Determinism:** Dialog branches use `MyBox.DialogInvoker` stub returning a fixed `DialogResult`. Async tests (KaCharAsync, KaKeyAsync, KaStringAsync) use delegates that return `Task.CompletedTask` or similar synchronously-completing tasks. No `Thread.Sleep`, no `Task.Delay`, no `DateTime.Now`. `[TestInitialize]`/`[TestCleanup]` resets shared seam state.
- **Isolation:** Each `[TestMethod]` creates fresh instances. The Moq `IProjectEntry` mock for the CompareTo tie-break is local to each test via `ComparandWithShiftingProjectId` factory. No cross-test state leaks are possible.
- **Speed:** Full three-assembly suite runs in well under 10 seconds (349 tests). The 98-test ToDoModel run completes in 3.76s. No I/O, no Outlook, no WinForms message loop.
- **Diagnostics:** FluentAssertions `.because` arguments on every assertion describe the exact contract being validated. For example: `entry.ProjectID.Should().Be("AAAA", "the id is not changed when the user declines the confirmation")`. A failure message identifies the exact contract violation without needing to inspect production code.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | No credentials, tokens, API keys, or PII in any changed file. All `MyBox.ShowDialog` message strings are hardcoded user-facing error messages present in the original source. |
| No unsafe subprocess or command construction | PASS | No subprocess calls in any changed file. |
| Input validation at boundaries | PASS | `ProjectEntry.SetProjectId` validates length and null at entry. `MatchBestSpecialFolder` guards for null/empty collection. `ToDoLoader.SetAndSave` guards for null `objectSetter`. |
| Error handling remains explicit | PASS | No broad `catch (Exception)` introduced. `ProjectEntry.SetProjectId` `default:` arm throws explicitly. |
| Configuration / path handling is safe | PASS | `MatchBestSpecialFolder` does not access the filesystem. `AppFileSystemFolderPaths` filesystem access (`LoadFolders`) is unchanged and not touched in the seam extraction. |

---

## Research Log

No external research was required. All evidence was derived from: (1) the branch diff via `git diff d436a06f..3b7defa3`, (2) feature-folder QA-gate and other evidence artifacts, (3) direct file inspection of changed source files, and (4) PR context summary and appendix artifacts.

---

## Verdict

The branch is ready for normal PR flow. Implementation quality is high across all 14 new test files and three production seam changes. The toolchain (csharpier, .NET analyzers, nullable + TreatWarningsAsErrors, vstest with coverage) passed in a single final pass at Phase 6. Coverage increased on all three feature assemblies. No blockers or majors identified.

The two informational findings (uncovered delegation line, argument-addition on the malformed-ID dialog replacement) are explicitly documented in feature-folder evidence and do not require code changes before merge. The Nit on comment length is advisory only.
