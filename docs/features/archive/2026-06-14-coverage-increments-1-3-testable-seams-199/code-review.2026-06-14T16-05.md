# Code Review: coverage-increments-1-3-testable-seams (#199) — Phase 5 re-review

**Review Date:** 2026-06-14
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199`
**Feature Folder Selection Rule:** Suffix `-199` matches the canonical issue number and the branch's primary scoping doc (spec.md).
**Base Branch:** `origin/main` @ `d436a06f`
**Head Branch:** `refactor/coverage-increments-1-3-199` @ `aa3a7542`
**Review Type:** Re-review after maintainer-authorized Phase 5 production seams

---

## Executive Summary

This re-review covers the full branch diff `d436a06f..aa3a7542` after the Phase 5 scope change, which the maintainer authorized (`remediation-inputs.2026-06-14T15-10.md`) to close the AC1/AC3 Flag-and-Stop coverage gaps that prior cycles left open under the test-only constraint. Phase 5 lifted the spec's zero-production-change Non-Goal for exactly two narrow seams, both verified by source inspection:

1. `UtilitiesCS/Properties/AssemblyInfo.cs` — adds `[assembly: InternalsVisibleTo("ToDoModel.Test")]` (non-executable; exposes the existing internal `MyBox.DialogInvoker` seam to the test project).
2. `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` — extracts the pure matching logic into an `internal static MatchBestSpecialFolder(IReadOnlyDictionary<string,string>, string)` helper; the public instance method now delegates. The helper body is byte-for-byte the original matching logic (only the local reference renamed to the parameter). Added `using System.Collections.Generic;`.

The reviewer verified via `git diff d436a06f..aa3a7542 --name-status` that exactly these two production source files changed — no third seam, no `UtilitiesCS/Dialogs/MyBox.cs` edit, no `coverage.config`/`*.runsettings`/`*.props`/`*.targets`/pipeline change, no `[ExcludeFromCodeCoverage]` delta. The extraction preserves behavior exactly (null/empty -> null; ordinal `Contains`; descending value-length order; `FirstOrDefault().Key`). Implementation quality of the new tests is high: AAA structure with descriptive names and FluentAssertions reason strings, deterministic seam injection (`MyBox.DialogInvoker` stub set in `[TestInitialize]` and restored in `[TestCleanup]`), in-memory dictionaries for the folder helper (no `LoadFolders`, no `Directory.CreateDirectory`), and Moq for the `IProjectEntry` comparand.

**What changed in Phase 5:**
- 2 authorized production source files (above), behavior-preserving.
- 2 new test files: `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs` (3 tests) and `TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs` (9 tests).
- 2 additive test-csproj `<Compile Include>` registrations.
- New static helper covered 8/8 lines (100%); `ProjectEntry` malformed-ID branch and CompareTo length tie-break newly covered; 185/185 tests pass.

**Top 3 risks:**
1. The `ProjectEntry` change-confirmation branch remains uncovered (0/28) because committing a changed id runs the `ProjectID` property setter's RAW, un-seamed `MessageBox.Show`, which deadlocks the STA host. This is a real untested production branch; closing it needs a third (currently unauthorized) production seam. Correctly Flag-and-Stopped.
2. `MyBox.DialogInvoker` is process-global static state; the dialog test class mutates it. Correctness depends on `[TestCleanup]` restoring the real invoker — verified present, but a shared-state coupling future tests must respect.
3. Repo-wide C# coverage remains below the 80% floor (pre-existing 197-COV-001 exception), satisfied via the accepted exception, not by reaching the threshold.

**PR readiness recommendation:** **Go** — production change limited to the two maintainer-authorized, behavior-preserving seams; full toolchain green per p5 evidence; new-code coverage 100%; the single residual change-confirmation gap is an acceptable, documented, maintainer-pending Flag-and-Stop.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` | lines 57-91 | The pure-helper extraction is the smallest seam that fits repo design (separation of pure logic from I/O); the instance method delegates with identical semantics. No behavior change. | None required; this is a correctly-scoped, maintainer-authorized seam. | The original instance body returned `bestMatch.Key` (default `KeyValuePair.Key` = null on no match); the helper preserves this byte-for-byte. | `evidence/qa-gates/p5-invariant-check.2026-06-14T15-10.md`; reviewer source read; `git diff` |
| Info | `UtilitiesCS/Properties/AssemblyInfo.cs` | line 18 | The single added `InternalsVisibleTo("ToDoModel.Test")` attribute is non-executable and changes only test-time visibility; the `MyBox.DialogInvoker` seam still defaults to the real dialog in production. | None required; maintainer-authorized. | No `MyBox` member behavior or visibility changes; no runtime effect. | `evidence/baseline/p5-seam-verification-mybox.2026-06-14T15-10.md`; `git diff` |
| Info | `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs` | ChangeId change-confirmation | The `SetProjectId` -> `ChangeId` change-confirmation branch (0/28) is intentionally not covered: committing `ProjectID = newID` runs the property setter's RAW un-seamed `MessageBox.Show`, deadlocking the STA host. | None required; correctly Flag-and-Stopped. Covering it is maintainer follow-up: route the `ProjectID` setter through `MyBox`. | Adding a third production seam without authorization would violate the spec flag-and-stop rule and `csharp.md`. | `evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md`; per-line 0/28 in `p5-coverage.cobertura.xml` |
| Minor | `ToDoModel.Test/Data Model/Project/ProjectEntryDialogBranchesTests.cs` | `[TestInitialize]`/`[TestCleanup]` | The class mutates the process-global static `MyBox.DialogInvoker`; mitigated by seeding in `[TestInitialize]` and restoring the real invoker in `[TestCleanup]`. | Keep the seed/restore guard on any future tests using the `MyBox` seam; consider a shared fixture if more such classes appear. | Shared mutable static is a determinism risk if a future test omits restore; current code restores correctly. | Inspected `TestInitialize_SeedSeam`/`TestCleanup_ResetSeam` (lines 41-52) |
| Info | `ToDoModel.Test/.../ProjectEntryDialogBranchesTests.cs` | `[STATestClass]` | The dialog-branch class runs STA because `MyBox.ShowDialog` constructs a WinForms control; the injected stub means the control is never shown, so no message loop runs. | None required. | STA is required for WinForms type construction even when no dialog is displayed; deterministic because the stub short-circuits before display. | Class attribute line 34; `p5-mstest-coverage` 185/185 pass in 3.90 s |
| Info | `*/`*.Test.csproj | `<Compile Include>` additions | The two test csproj got additive Compile-item lines for the new Phase-5 test files. | None; mechanically required by legacy non-SDK projects, matches the pre-existing pattern; not a production/config/pipeline change. | The production-change constraint targets production source/config/pipeline; test-file registration is not that. | `evidence/qa-gates/p5-invariant-check.2026-06-14T15-10.md` |

---

## Detailed Observations

### Production seam correctness (behavior-preservation)

Read `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` lines 57-91 against the diff. The instance method `MatchBestSpecialFolder(string path)` now reads `return MatchBestSpecialFolder(SpecialFolders, path);`. The static helper guards `specialFolders.IsNullOrEmpty()` -> `null` (identical to the original `SpecialFolders.IsNullOrEmpty()` guard), runs the same `.Where(x => path.Contains(x.Value)).OrderByDescending(x => x.Value.Length).FirstOrDefault()`, and returns `bestMatch.Key`. The only token change is the local reference name (`SpecialFolders` -> the `specialFolders` parameter). The instance `SpecialFolders` dictionary is passed through unchanged, so production callers observe identical results. This is a clean application of the repo's "separate pure logic from I/O" principle and the lightest DI seam (a structural extraction rather than an interface or delegate).

### Test design

`AppFileSystemFolderPathsMatchBestSpecialFolderTests` (9 tests) covers positive, longest-match (descending-length tie-break), ordinal case sensitivity, trailing-separator substring behavior, no-match, null collection, empty collection, empty path, and the documented null-path `NullReferenceException`. Each test constructs an in-memory `Dictionary<string,string>` and calls the static helper directly — no object construction, no `LoadFolders`, no filesystem. This is the correct way to exercise the pure logic deterministically.

`ProjectEntryDialogBranchesTests` (3 tests) injects a non-modal `MyBox.DialogInvoker` stub to cover the malformed-ID branch, and uses a Moq `IProjectEntry` whose `ProjectID` getter returns an ordinal-equal value on the first read and a length-differing value on later reads to reach the `CompareTo` length tie-break in both directions. The class-level XML doc and the in-body comment block (lines 81-98) precisely document why the change-confirmation branch is unreachable within the authorized scope — an accurate technical explanation matching the source.

### Residual gap assessment

The change-confirmation branch is the only targeted path left uncovered. The reviewer confirms the executor's reasoning is correct: the `ProjectID` property setter's `_projectID != value` arm calls a raw `System.Windows.Forms.MessageBox.Show`, not the `MyBox` seam, so injecting the stub cannot suppress it; committing a changed id therefore blocks the STA test thread. Covering it requires routing the property setter through `MyBox` — a third production change beyond the two authorized seams. Per the spec flag-and-stop rule and `csharp.md`'s no-silent-production-change rule, the executor correctly stopped and documented the gap. This is policy-correct behavior, not a defect.

---

## Recommendation

**Go (Ready for merge).** No blocking findings. The Phase-5 production changes are limited to the two maintainer-authorized, behavior-preserving seams (verified by source inspection and the invariant-check evidence). The new tests are deterministic, well-structured, and policy-compliant. The single residual change-confirmation coverage gap is an acceptable, documented, maintainer-pending Flag-and-Stop; the below-floor repo-wide coverage is the accepted #197 exemption that this feature improves. Follow-up (out of scope): route the `ProjectID` setter through `MyBox` under separate maintainer direction; roadmap Increments 4+.
