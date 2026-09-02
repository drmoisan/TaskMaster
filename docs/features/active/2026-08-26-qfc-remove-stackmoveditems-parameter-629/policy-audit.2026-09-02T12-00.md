# Policy Compliance Audit: Remove `stackMovedItems` parameter from `MoveEmailsAsync` (#629)

**Audit Date:** 2026-09-02
**Feature Folder:** `docs/features/active/2026-08-26-qfc-remove-stackmoveditems-parameter-629`
**Base Branch:** `main`
**Head Branch:** `feature/qfc-remove-stackmoveditems-parameter-629`
**Language Scope:** C# only (no Python, PowerShell, Bash, or JSON production files touched)

---

## Executive Summary

This change removes the unused `stackMovedItems` parameter from
`IQfcCollectionController.MoveEmailsAsync` and its implementation, updates the sole call site, and
updates four affected tests. Five production/test files were touched; no new files, no dependency
changes, no configuration changes. A full CSharpier → analyzer → nullable → MSTest toolchain pass
completed clean (0 format diffs, 0 analyzer errors, 0 nullable errors, 6949/6949 tests passing).
Coverage evidence is documented in `evidence/qa-gates/p2-t6-coverage-delta.md`, using the reliable
`lines-covered`/`lines-valid` sums rather than the root Cobertura rate attributes (known unreliable
per open defects #529/#530).

### Coverage Evidence Checklist

- [x] Baseline coverage captured before implementation: `evidence/baseline/p0-t7-baseline-coverage.md`
- [x] Final coverage captured after implementation: `evidence/qa-gates/p2-t5-final-coverage.md`
- [x] Delta computed using reliable sums, not root rate attributes: `evidence/qa-gates/p2-t6-coverage-delta.md`
- [x] No regression on changed lines (line coverage +0.0078pp; branch delta −0.0121pp is noise, no
      branching logic touched)

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

- **Independence:** All four modified tests set up their own controller instance via
  `QfcCollectionControllerTestSupport.CreateUninitializedController()`; none share mutable state.
- **Isolation:** Each test targets one behavior of `MoveEmailsAsync` (empty-collection no-op,
  cancellation propagation, no-second-subject-read-after-failure, null-group-lookup no-op, undo-handoff
  setup/verify). No test was broadened to cover more than its original scope.
- **Fast execution:** No I/O, no sleeps, no real timers introduced or removed.
- **Determinism:** No randomness, no wall-clock reads, no `Thread.Sleep`/`Task.Delay` added.
- **Readability:** Test names describe the scenario in behavior terms
  (`MoveEmailsAsync_WithEmptyItemGroupsToMove_DoesNotThrow`).

### 1.2 Coverage and Scenarios

- Positive flow: `MoveEmailsAsync()` with a non-empty `_itemGroupsToMove` — unchanged, exercised by
  pre-existing tests not touched by this change.
- Negative/edge flow: empty `_itemGroupsToMove` early-return — preserved by the rewritten
  `MoveEmailsAsync_WithEmptyItemGroupsToMove_DoesNotThrow` (see `evidence/other/p1-t5-test-disposition.md`
  for why rewrite, not delete, was chosen).
- Error handling: cancellation propagation and no-second-subject-read-after-failure tests are unchanged
  in behavior, only in call shape (`MoveEmailsAsync()` instead of `MoveEmailsAsync(null)`).
- State transition: undo-handoff setup/verify in `QfcFormControllerUndoHandoffTests.cs` confirms the
  call site invokes the zero-argument overload.
- No concurrency behavior is affected by this change.

### 1.2.1 Per-Language Coverage Comparison

| Metric | Baseline (P0-T7) | Final (P2-T5) | Delta |
|---|---|---|---|
| Line coverage | 55088 / 64506 = 85.3963% | 55093 / 64506 = 85.4041% | +5 lines, +0.0078pp |
| Branch coverage | 13173 / 16576 = 79.4703% | 13171 / 16576 = 79.4582% | −2 branches, −0.0121pp (noise; no branch logic touched) |

Full analysis: `evidence/qa-gates/p2-t6-coverage-delta.md`.

### 1.3 Test Structure and Diagnostics

- All four modified tests follow Arrange–Act–Assert. FluentAssertions `.Should().NotThrowAsync(because: ...)`
  and `Times.Never`/`Times.Once` Moq verifications carry explicit rationale strings.

### 1.4 External Dependencies and Environment

- No test depends on a database, network, remote API, or external process. No temporary files created
  or used, in production code or test code.

### 1.5 Policy Audit Requirement

- This document satisfies the policy-audit requirement for this change prior to PR authoring.

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

- Objective clarified from `issue.md` (pre-existing, promoted record) and this feature's own
  `spec.md`/`user-story.md`/`plan.2026-09-02T11-00.md`, authored during this run since the feature
  folder held only an `issue.md` and empty scaffolds at the start.
- Atomic plan `plan.2026-09-02T11-00.md` documents the phased approach (Phase 0 baseline, Phase 1
  implementation, Phase 2 QA); all P0/P1/P2 tasks are checked off with evidence citations.

### 2.2 Design Principles

- **Simplicity first:** the change is a pure signature simplification — remove one unused parameter,
  update the three sites that pass it, no new abstractions introduced.
- **Reusability:** N/A — no new reusable logic was introduced.
- **Extensibility:** `IQfcCollectionController` remains a single-implementation, first-party interface;
  removing an unused parameter does not reduce extensibility.
- **Separation of concerns:** unaffected — the undo-stack population logic already lived in
  `EmailFiler.PushToUndoStack`, not in `MoveEmailsAsync`; this change only removes a parameter that was
  never read.

### 2.3 Module & File Structure

- No file exceeds the 500-line repository limit as a result of this change (all five touched files were
  edited in place with a net negative or near-zero line delta).
- No new public surface area was added; one public method's signature was narrowed (parameter removed).

### 2.4 Naming, Docs, and Comments

- `IQfcCollectionController.MoveEmailsAsync` doc comment rewritten as an `<remarks>` block that explains
  *why* no undo-stack argument is needed (citing issue #629 and the actual population path via
  `Globals.AF.MovedMails`), rather than describing *what* the method does.
- No cryptic abbreviations introduced.

### 2.5 After Making Changes — Toolchain Execution

Full toolchain executed in order, restarting from step 1 on any failure or auto-fix (none occurred):

1. `dotnet tool run csharpier check .` — exit 0 (`evidence/qa-gates/p2-t2-csharpier-check.md`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — exit 0, 0 errors (`evidence/qa-gates/p2-t3-analyzer-build.md`)
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` — exit 0, 0 errors (`evidence/qa-gates/p2-t4-nullable-build.md`)
4. `vstest.console.exe` (via `scripts/vscode/Invoke-MSTestWithCoverage.ps1`) — 6949/6949 passing
   (`evidence/qa-gates/p2-t5-final-coverage.md`)

All four stages passed in a single pass; no restart was required.

### 2.6 Summarize and Document

- This audit, `code-review.2026-09-02T12-00.md`, and `feature-audit.2026-09-02T12-00.md` constitute the
  summary and documentation step. `spec.md`'s Definition of Done and Acceptance Criteria are checked off
  with evidence citations.

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3A: Python Code Change Policy Compliance

Not applicable. No Python files were touched by this change.

### Section 3B: PowerShell Code Change Policy Compliance

Not applicable. No PowerShell files were touched by this change.

### Section 3C: Bash Script Policy Compliance

Not applicable. No Bash files were touched by this change.

### Section 3D: JSON Configuration Policy Compliance

Not applicable. No JSON configuration files were touched by this change.

### Section 3E: C# Code Change Policy Compliance

This is the applicable language section for this change. Per `CLAUDE.md`'s C# Code Change Policy:

#### 3E.1 Tooling & Baseline

- **Formatting:** `dotnet tool run csharpier check .` — exit 0, no diffs (`evidence/qa-gates/p2-t2-csharpier-check.md`).
- **Linting/analyzers:** `.NET analyzers` via `/t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — exit 0, 0 errors
  (`evidence/qa-gates/p2-t3-analyzer-build.md`).
- **Type checking / nullable:** `/t:Rebuild ... /p:TreatWarningsAsErrors=true` — exit 0, 0 errors
  (`evidence/qa-gates/p2-t4-nullable-build.md`). Neither touched file carries `#nullable enable`, so no
  file newly opted into nullable-flow analysis as a side effect of this change; none was removed either.

#### 3E.2 C# Design & Type-Safety Principles

- **Strong contracts:** `MoveEmailsAsync()` remains an explicit, documented public contract on
  `IQfcCollectionController`; the `<remarks>` block documents the invariant that the undo stack is
  populated elsewhere.
- **Null-safety:** removing a parameter cannot introduce a new null-state; no nullable warning was
  introduced (confirmed by the clean nullable build).
- **Composition over inheritance:** not implicated — no inheritance relationships were touched.
- **Async/resource safety:** `MoveEmailsAsync` remains `async`/`Task`-returning; no `IDisposable`
  resources are involved in the touched lines.

#### 3E.3 C# Error Handling and Logging

- No exception-handling behavior changed. The one comment referencing `TraceUtility.LogMethodCall`
  was updated to drop the removed argument (`TraceUtility.LogMethodCall(stackMovedItems);` →
  `TraceUtility.LogMethodCall();`) — it was already a comment (dead code), not an active logging call;
  no logging behavior changed.

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4A: Python Unit Test Policy Compliance

Not applicable. No Python tests were touched by this change.

### Section 4B: PowerShell Unit Test Policy Compliance

Not applicable. No PowerShell (Pester) tests were touched by this change.

### Section 4C: C# Unit Test Policy Compliance

This is the applicable language section for this change. Per `CLAUDE.md`'s C# Unit Test Policy:

#### 4C.1 Framework Selection

- All four modified test files use **MSTest** (`[TestClass]`/`[TestMethod]` from
  `Microsoft.VisualStudio.TestTools.UnitTesting`), consistent with the existing test files. No xUnit or
  NUnit was introduced.

#### 4C.2 C#-Specific Libraries and Conventions

- **Mocking:** the two `Setup`/`Verify` sites updated in `QfcFormControllerUndoHandoffTests.cs` use
  **Moq** (`Mock<IQfcCollectionController>`), matching the existing pattern in that file.
- **Assertions:** the rewritten test in `QfcCollectionControllerDefects468MoveTests.cs` uses
  **FluentAssertions** (`.Should().NotThrowAsync(because: ...)`), matching the file's existing style.

#### 4C.3 Running the Toolchain

- `vstest.console.exe` via `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — 6949/6949 tests passing at
  baseline and final (`evidence/baseline/p0-t7-baseline-coverage.md`,
  `evidence/qa-gates/p2-t5-final-coverage.md`).

---

## 5. Test Coverage Detail

### `MoveEmailsAsync` (4 tests directly, 1 indirectly)

- `MoveEmailsAsync_WithEmptyItemGroupsToMove_DoesNotThrow` (rewritten from
  `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack`) — covers the early-return branch
  when `_itemGroupsToMove` is empty.
- `MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException` — call-shape fix only, no
  behavioral change.
- `MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime` — call-shape fix only, no
  behavioral change.
- `MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow` — call-shape fix only, no behavioral
  change.
- `QfcFormControllerUndoHandoffTests` (setup + one `Times.Never` verify) — indirectly exercises the
  call site in `QfcFormController.EventHandlers.cs`.

---

## 6. Test Execution Metrics

| Run | Total | Passing | Failing | Skipped |
|---|---|---|---|---|
| Baseline (P0-T7) | 6949 | 6949 | 0 | 0 |
| Final (P2-T5) | 6949 | 6949 | 0 | 0 |

No test count change: 3 tests had only their call shape fixed (no new test, no deleted test), 1 test
was renamed/rewritten in place (not added or removed as a count), and 2 mock sites in a fifth test file
were updated in place.

---

## 7. Code Quality Checks

| Check | Status | Evidence |
|---|---|---|
| CSharpier format | PASS | `evidence/qa-gates/p2-t2-csharpier-check.md`, exit 0 |
| .NET analyzers | PASS | `evidence/qa-gates/p2-t3-analyzer-build.md`, exit 0, 0 errors |
| Nullable/type-check | PASS | `evidence/qa-gates/p2-t4-nullable-build.md`, exit 0, 0 errors |
| MSTest suite | PASS | `evidence/qa-gates/p2-t5-final-coverage.md`, 6949/6949 |
| Coverage regression | PASS (improved) | `evidence/qa-gates/p2-t6-coverage-delta.md` |

---

## 8. Gaps and Exceptions

### Identified Gaps

None. The change is a same-process signature simplification with no behavioral branch depending on the
removed parameter's value.

### Approved Exceptions

None required.

### Removed/Skipped Tests

None removed. One test was rewritten in place (see `evidence/other/p1-t5-test-disposition.md`); its
early-return-branch coverage is preserved under a new name and assertion shape.

---

## 9. Summary of Changes

### Files Modified

- `QuickFiler/Interfaces/IQfcCollectionController.cs` — removed the `stackMovedItems` parameter from the
  interface declaration; rewrote the doc comment as a `<remarks>` block.
- `QuickFiler/Controllers/QfcCollectionController.cs` — removed the parameter, the `_ = stackMovedItems;`
  discard statement, and updated the stale trace-log comment; rewrote the doc comment.
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` — updated the sole call site to
  `await _groups.MoveEmailsAsync();`.
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` — updated 3 call sites,
  rewrote 1 test (see disposition note above).
- `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` — updated 1 `Setup` and 1 `Verify`
  call shape.

No other production, test, or configuration files were touched. Verified against `origin/main` footprint
prediction in `evidence/other/p1-t6-footprint-check.md`.

---

## 10. Compliance Verdict

### Overall Status: FULLY COMPLIANT

#### General Code Change Policy (Section 2)

Compliant. Toolchain executed in the required order, passed in a single pass, no restarts needed.

#### Language-Specific Code Change Policy (Section 3E, C#)

Compliant. CSharpier, analyzer, and nullable gates all pass at exit 0 with 0 errors.

#### General Unit Test Policy (Section 1)

Compliant. All modified tests are independent, isolated, fast, deterministic, and readable; scenario
coverage for the touched behavior (early-return branch) is preserved.

#### Language-Specific Unit Test Policy (Section 4C, C#)

Compliant. MSTest, Moq, and FluentAssertions used consistently with existing file conventions.

### Metrics Summary

- Files changed: 5 production/test files (0 new files).
- Tests: 6949/6949 passing, no count change.
- Coverage: line +0.0078pp, branch −0.0121pp (noise, no branch logic touched).
- Toolchain: 1 pass, 0 restarts.

### Recommendation

Proceed to PR. No outstanding gaps or exceptions.

---

## Appendix A: Test Inventory

### Complete Test List (files touched by this change)

- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs`:
  `MoveEmailsAsync_WithEmptyItemGroupsToMove_DoesNotThrow`,
  `MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException`,
  `MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime`,
  `MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow`.
- `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs`: all tests in the file (2 mock
  sites updated; full file re-run as part of the suite).

## Appendix B: Toolchain Commands Reference

```
# Formatting
dotnet tool run csharpier check .

# Linting / Analyzers
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking / Nullable
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true

# Testing
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage
```
