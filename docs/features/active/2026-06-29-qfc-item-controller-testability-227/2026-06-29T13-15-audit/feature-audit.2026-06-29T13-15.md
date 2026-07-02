# Feature Audit: QfcItemController / IItemViewer Testability Refactor (Issue #227)

**Audit Date:** 2026-06-29
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main`
**Head Branch:** `TaskMaster-wt-2026-06-29-09-38`
**Work Mode:** `full-feature`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `main` (commit `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
- **Head branch/commit:** `TaskMaster-wt-2026-06-29-09-38` (commit `bcc7d7e32a12693b732d5c5e133a681890bec412`)
- **Merge base:** `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/**`
  - Additional evidence: `git diff --name-status 4611fd60..bcc7d7e3`; direct `awk`/`grep` inspection of changed `.cs` files
- **Feature folder used:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
- **Requirements source:** `spec.md` (AC1–AC7). `user-story.md` is the second full-feature source but is **absent** for this feature; `spec.md` is therefore the sole authoritative AC source for this run.
- **Work mode resolution note:** `issue.md` carries the explicit marker `- Work Mode: full-feature`. Per the work-mode contract, full-feature resolves to `spec.md` and `user-story.md`; `user-story.md` does not exist, so AC evaluation uses `spec.md` only, recorded here.
- **Scope note:** Audit scope is the full branch diff against the merge base (19 changed C# files: 16 production, 3 test). The PR-context summary overview misclassified the C# changes as docs ("Core logic changes: 0 files"); the audit uses the git diff as the authoritative scope source. No caller scope narrowing was supplied.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/spec.md` — primary (and only available) source
- `docs/features/active/2026-06-29-qfc-item-controller-testability-227/user-story.md` — expected secondary source under full-feature, **absent**

### Acceptance criteria (from spec.md)

1. AC1: `QfcItemController` is split into partial-class files, each under 500 lines, with a logical responsibility-based structure; no behavior change; all existing tests pass.
2. AC2: `private ItemViewer _itemViewer` and the public constructor parameters are changed to `IItemViewer`; `Mock<IItemViewer>` is injectable into the controller.
3. AC3: `IItemViewer` is narrowed to intent-level members (display-state properties, command events, intent methods); raw clickable/raw control types are removed from the interface; `ItemViewer.cs` provides forwarding implementations and remains `[ExcludeFromCodeCoverage]`.
4. AC4: Test files mirror the new partial-class structure (one test file per testable cluster), each under 500 lines, with explicit csproj entries.
5. AC5: Coverage of the affected testable (non-exempt) denominator is >= 80%; new/extracted code >= 90%; changed lines do not regress. Repo-wide floor handled under the authority-scoped exception precedent; exemption boundary ratified by the maintainer.
6. AC6: No production file modified in this cycle exceeds 500 lines after the change.
7. AC7: Full C# toolchain passes in order — csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest with coverage — with no regressions.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | Split into < 500-line partials; logical structure; no behavior change; existing tests pass | PASS | 10 partials (main 294 + 9 clusters; largest Initialization 398); 233/233 tests pass | `awk 'END{print NR}' QfcItemController.*.cs`; `final-tests-coverage.2026-06-29T12-50.md` | Verbatim move; `partial` modifier added. |
| 2 | `_itemViewer` field + ctor params → `IItemViewer`; `Mock<IItemViewer>` injectable | PASS | `IItemViewer.cs` narrowed; new tests inject `Mock<IItemViewer>` | `git diff IItemViewer.cs`; EventWiring/FolderHandling tests use `Mock<IItemViewer>` | Construction sites remain compatible (`ItemViewer : IItemViewer`). |
| 3 | `IItemViewer` narrowed to intent members; raw control types removed; `ItemViewer` forwards and stays `[ExcludeFromCodeCoverage]` | PASS | Raw `ButtonSVG`/`ComboBox`/`WebView2`/`FastObjectListView`/`OLVColumn`/`TableLayoutPanel`/`ToolStripMenuItemCb` removed; four `ItemViewer.*.cs` forwarding partials; `ItemViewer.cs:19-20` `[ExcludeFromCodeCoverage]` | `git diff IItemViewer.cs`; `grep ExcludeFromCodeCoverage ItemViewer.cs` | Forwarding placed in same-class partials (intent of AC3 met). |
| 4 | Test files mirror partial structure; each < 500 lines; explicit csproj entries | PASS | 6 new test files (87–192 lines); `QuickFiler.Test.csproj` `+6` entries | `awk` line counts; `git diff QuickFiler.Test.csproj` | 201 baseline tests preserved + 32 new. |
| 5 | Affected testable non-exempt ≥80%; new/extracted ≥90%; no changed-line regression; repo-wide under exception; exemption boundary ratified | PARTIAL | ≥80% floor MET (484/585 = 82.74%); no changed-line regression (strictly additive); ≥90% sub-target UNMET (aggregate 82.74%); 103-method exemption boundary unratified | `coverage-delta.2026-06-29T12-50.md`; `exemption-boundary.2026-06-29T12-40.md` | PASS-with-documented-exception on floor + no-regression. ≥90% residual deferred to #197 (structurally un-coverable inline UI/COM lambda bodies + Dispatcher-bound render). Maintainer ratification of the exemption boundary outstanding. Not a code defect. |
| 6 | No modified production file exceeds 500 lines | PASS | All 17 production files < 500 (largest Initialization 398; ItemViewer.cs at baseline 436) | `awk 'END{print NR}'` each file; `final-file-sizes.2026-06-29T12-50.md` | `QfcCollectionController.cs` (2296) not split — pre-existing debt, Non-Goal, net-neutral. |
| 7 | Full C# toolchain passes in order, no regressions | PASS | csharpier check, analyzers, nullable/TWAE, vstest all EXIT_CODE 0 at final gate; p1–p8 gates clean | `final-{csharpier,analyzers,nullable,tests-coverage}.2026-06-29T12-50.md` | Verified from executor evidence; not re-run in review environment. |

---

## Summary

**Overall Feature Readiness:** NEEDS REVISION

The feature is functionally complete and behavior-preserving. Six of seven acceptance criteria
(AC1–AC4, AC6, AC7) PASS with independent verification. AC5 is PARTIAL: its ≥80% testable-
denominator floor and no-changed-line-regression sub-claims are MET, but two items prevent a clean
PASS — the ≥90% new/extracted sub-target is unmet (deferred to #197, residual structurally
un-coverable), and the 103-method exemption boundary awaits maintainer ratification. A separate
process gap (the canonical `artifacts/csharp/coverage.xml` is absent) is recorded in the policy
audit. None of these is a code defect or a behavior-change risk.

**Criteria summary:**
- **PASS:** 6 criteria (AC1, AC2, AC3, AC4, AC6, AC7)
- **PARTIAL:** 1 criterion (AC5)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. AC5 ≥90% new/extracted sub-target unmet (aggregate 82.74%); deferred to #197.
2. Maintainer ratification of the 103-method `[ExcludeFromCodeCoverage]` boundary is outstanding (gates AC5 checkoff).
3. Canonical C# coverage artifact `artifacts/csharp/coverage.xml` absent (policy-audit FAIL on artifact presence).

**Recommended follow-up verification steps:**

1. Generate the canonical Cobertura `artifacts/csharp/coverage.xml` via the documented #223 cycle-1 procedure and re-confirm the repo-wide first-party figure.
2. Obtain maintainer ratification of the exemption boundary (produce a `maintainer-decision` artifact analogous to #223), then re-check AC5 in `spec.md`.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- PASS criteria already represented as checked boxes in `spec.md` (AC1–AC4, AC6, AC7) require no change.
- AC5 is PARTIAL and remains unchecked.

No checkbox change was made to `spec.md` in this review: AC1–AC4, AC6, and AC7 are already `- [x]`
in the source, and AC5 is `- [ ]` and correctly remains unchecked because it is PARTIAL (≥90%
sub-target unmet and exemption-boundary ratification outstanding).

### AC Status Summary

- Source: `docs/features/active/2026-06-29-qfc-item-controller-testability-227/spec.md`
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining: AC5 (affected testable non-exempt ≥80% MET; ≥90% new/extracted sub-target unmet and deferred to #197; exemption boundary awaiting maintainer ratification)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 7 | 6 | 1 | Checkbox-backed; authoritative for full-feature in absence of user-story.md |
| `user-story.md` | 0 | 0 | 0 | Absent; expected secondary full-feature source not present for this feature |
