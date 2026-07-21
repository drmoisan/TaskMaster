# Policy Compliance Audit — outlook-store-exclusion (Issue #328)

- Timestamp: 2026-07-15T21-22
- Reviewer: feature-review
- Base branch (resolved): `main`
- Merge-base SHA: `26905c4b737b7fb20cf4e05b92d44fdefb18894e`
- Head SHA: `c0414696f91f03b1ca8e5b33f6920473c9178da8`
- Work Mode: `full-feature` (AC sources: `spec.md`, `user-story.md`)
- Scope: full branch diff vs merge-base (feature-vs-base). Not narrowed to any plan, task, or phase.

## Executive Summary

The branch delivers the Issue #328 store-exclusion feature: an additive `ExcludedStoreIds`
exact-match StoreID exclusion checked first in `StoresWrapper`, routing of the four `Session.Stores`
bypass sites through the centralized `ShouldIncludeStore` predicate, and a settings-UI checkbox that
toggles the current store's StoreID and persists through the existing serialization path. The
implementation matches the spec, the code is well-documented, and the C# toolchain (csharpier,
analyzer build, nullable/TreatWarningsAsErrors build) passes per executor evidence with a functionally
green MSTest suite (4611/4611 without coverage instrumentation).

Overall verdict: PARTIAL. One coverage FAIL requires disposition before merge readiness:
the canonical C# coverage artifact expected by the review procedure (`artifacts/csharp/coverage.xml`)
is absent, and one modified class (`StoreWrapper`) carries branch coverage of 64.81%, below the 75%
branch floor (pre-existing; baseline 65.38%). The independently-verified feature-evidence Cobertura
confirms line coverage clears the floor for every touched non-exempt first-party class. These are
procedural / pre-existing coverage findings, not code-correctness defects. See Section 5 and the
remediation inputs.

## 1. Scope and Baseline

- Range audited: `26905c4b737b7fb20cf4e05b92d44fdefb18894e..c0414696f91f03b1ca8e5b33f6920473c9178da8`.
- Changed languages with files in the branch diff: C# only (23 `.cs` files; 4 `.csproj`). No
  TypeScript, Python, or PowerShell files changed.
- Coverage enforcement is therefore mandatory for C# and only C#.

### 1.1 PR-context summary correction (recurring C#-as-docs misclassification)

The auto-generated `artifacts/pr_context.summary.txt` "Changed files overview" reported
`Core logic changes: 0 files` and classified all changes as `Docs/templates/agents/tooling`. That is a
misclassification: the diff contains 15 production C# files and 8 test C# files. The summary was
corrected in place (annotated `CORRECTED BY feature-review 2026-07-15T21-22`). C# is treated as an
in-scope changed language and its coverage is enforced below regardless of the original overview.

## 2. Toolchain Compliance (C#)

Evidence source: executor QA-gate artifacts under
`docs/features/active/2026-07-15-outlook-store-exclusion-328/evidence/qa-gates/`. These are read as
pre-existing evidence per the feature-review workflow (the reviewer does not re-run the C# toolchain;
msbuild/vstest are not available on this review host).

| Stage | Command | Evidence | Exit | Verdict |
|---|---|---|---|---|
| Format | `csharpier check .` | final-csharpier.2026-07-15T18-45.md | 0 | PASS |
| Lint / analyzers | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | final-analyzer-build.2026-07-15T18-45.md | 0 | PASS |
| Type / nullable | `msbuild TaskMaster.sln /t:Build ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | final-nullable-build.2026-07-15T18-45.md | 0 | PASS |
| Tests | `vstest.console.exe` over UtilitiesCS.Test, TaskMaster.Test, ToDoModel.Test | final-vstest.2026-07-15T18-45.md | functional pass | PASS |

Test note: the non-instrumented run is 4611/4611 passing (0 functional failures, 1 skipped). The 19
failures observed only under `dotnet-coverage` instrumentation are the documented pre-existing
Deedle/FSharp DataFrame instrumentation flakiness (nondeterministic set), not regressions introduced
by this branch. The prior scope-conflict failure
(`LoadProjInfoAsync_RebuildsWhenProjectCountIsZeroAndOutlookApplicationIsAvailable`) is resolved by the
in-scope P4-T4 edit to the `OlObjectsProxy` test double.

## 3. General Code Change Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity / centralization | PASS | Store-access decisions stay in `StoresWrapper.ShouldIncludeStore`; the four bypass sites route through it via a null-safe `.Where` seam; no parallel filtering logic added (confirmed by reading all four sites). |
| Separation of concerns | PASS | `StoreFilterAttribution.Decide` stays pure/COM-free over already-read primitives; COM `StoreID` reads are guarded and isolated at the callers. |
| Error handling / fail-open | PASS | Every `store.StoreID` read is wrapped in try/catch and fails open (never excludes on an unread ID), matching the existing `FilePath` guard. |
| Backward compatibility | PASS | `ExcludedStoreIds` and `StoreWrapper.StoreId` are additive `[JsonProperty]` members; legacy JSON without the keys deserializes to defaults; no new config file/key. |
| Public API compatibility | PASS with note | `StoreIsIncluded` gains two optional trailing params (source-compatible); `ProjectData.Rebuild(Application)` is preserved and delegates to the new overload; `Decide` gains required leading params (compile-checked, in-assembly + tests only). |
| File size <= 500 lines | PASS with documented exception | See Section 4. |

## 4. File-Size Compliance

Independently verified line counts at HEAD:

- `StoreWrapperController.cs` 477, `StoresWrapper.cs` 443, `ToDoEvents.cs` 467,
  `StoreFilterAttribution.cs` 175, `StoreFilterAttributionTests.cs` 486, `StoresWrapper.Filtering.cs`
  108, `ToDoEvents.Filtering.cs` 102 — all within the 500-line limit.
- `TaskMaster/AppGlobals/AppToDoObjects.cs` is 503 lines. This exceeds 500 but is a pre-existing
  condition: the baseline at the merge-base is also 503 lines, and this branch changed only two
  `ProjectData.Rebuild` call-site arguments (diff `+2/-2`), adding no lines. Not a new violation;
  disposition: pre-existing, documented, no growth. Verdict for this feature: PASS (no file grew
  past the limit).

The relocation of `StoreIsIncluded` into the new `StoresWrapper.Filtering.cs` partial and the
`ToDoEvents` filtering surface into `ToDoEvents.Filtering.cs` partial are behavior-preserving splits
made specifically to keep both parent files under 500 lines. Confirmed behavior-preserving by diff
inspection.

## 5. Coverage Verification (C#)

Mandatory for C# (changed files present). The canonical review coverage artifact path for C# is
`artifacts/csharp/coverage.xml`.

### 5.1 Canonical artifact presence

`artifacts/csharp/coverage.xml` is ABSENT on this review host. Per the mandatory coverage rule
("if no coverage artifact is found for a language that has changed files, flag as FAIL"), the C#
coverage verdict is FAIL on artifact-presence grounds. Disposition: procedural. Coverage was in fact
produced by the executor and written under the feature evidence folder
(`evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml`), which the reviewer parsed
independently below. The remediation item is to place/emit the coverage at the canonical path so the
review procedure and the coverage hook can verify it directly.

### 5.2 Independent per-class verification (feature-evidence Cobertura)

Parsed directly from `evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml` by the reviewer
(class `line-rate` / `branch-rate` attributes):

| Class (non-exempt, touched) | Line coverage | Branch coverage | Line floor 85% | Branch floor 75% |
|---|---|---|---|---|
| StoreFilterAttribution | 100.00% | 96.88% | PASS | PASS |
| StoresWrapper (primary partial) | 98.42% | 89.13% | PASS | PASS |
| StoresWrapper.Filtering (partial) | 95.83% | 85.71% | PASS | PASS |
| StoreWrapperController | 95.89% | 85.38% | PASS | PASS |
| StoreWrapper | 95.31% | 64.81% | PASS | FAIL |

C# line coverage: every touched non-exempt first-party class clears the 85% line floor (>= 95%).
Verdict for C# line coverage: PASS.

C# branch coverage: four of five classes clear the 75% branch floor. `StoreWrapper` branch coverage is
64.81%, below the 75% branch floor. Verdict for C# branch coverage: FAIL. Context: this is
pre-existing (baseline branch-rate 65.38%; the change added the guarded `StoreId` capture branches
whose true/false arms are exercised, and the class-level percentage dipped 0.57 points because the
branch denominator grew, per the coverage-delta evidence). It is not a newly-uncovered changed branch,
but the class remains below the branch floor.

### 5.3 New-code and changed-line coverage

New-code line coverage: the StoreID branch in `Decide` / `ShouldIncludeStore` / `StoreIsIncluded`,
`ExcludedStoreIds`, the `StoreWrapper.StoreId` capture, and the controller methods
(`BindExcludeStoreCheckbox`, `ExcludeStoreSelectionChanged`, `ApplyExcludeStoreSelection`) are all
exercised by the new tests; the four changed classes hold >= 95% line coverage, so new-code line
coverage is >= 90%. Changed-line coverage: no changed line is uncovered (per-class line rates are
flat-to-up). Verdict: PASS.

### 5.4 Repo-wide C# coverage

The whole-process Cobertura line-rate (62.86%) is not a valid repo-wide first-party denominator: it
mixes vendored Deedle/FSharp/Swordfish/SVGControl modules whose loaded set varies run to run
(documented `dotnet-coverage` denominator nondeterminism). The 80/85% floor applies to first-party
production modules, whose touched classes are verified above at >= 95% line coverage. A stable
repo-wide first-party C# line-coverage number is not recomputable from the artifacts on this host;
the authoritative repo-wide gate is the PR CI coverage run. Verdict for repo-wide C# line coverage on
the touched first-party surface: PASS on the measured classes; the repo-wide aggregate is deferred to
CI and is not asserted here.

### 5.5 C# coverage verdict (summary row)

C# coverage: FAIL. Drivers: (1) canonical coverage artifact `artifacts/csharp/coverage.xml` absent
(procedural); (2) `StoreWrapper` branch coverage 64.81% below the 75% branch floor (pre-existing).
Line coverage on all touched non-exempt classes is PASS (>= 95%). This FAIL is dispositioned as
procedural/pre-existing rather than a code-correctness defect; see remediation inputs.

### 5.6 Coverage exemptions applied

`TreeOfToDoItems` and `ToDoEvents` (including `ToDoEvents.Filtering.cs`) carry `[ExcludeFromCodeCoverage]`
per the WinForms/COM-bound exemption; their filter effect is tested behaviorally through the
non-exempt `StoresWrapper` seam and `StoreFilterRoutingTests`. `StoreWrapperViewer(.Designer).cs` are
WinForms form/Designer files under the WinForms exemption. No production source file is excluded from
coverage via config; the exemptions are attribute-based on host-bound types, consistent with policy.

## 6. Unit Test Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| Framework MSTest | PASS | New tests use `[TestClass]`/`[TestMethod]`. |
| Moq for mocking | PASS | `Mock<...>` used for `IApplicationGlobals`/`Outlook.Store`/viewer collaborators. |
| FluentAssertions | PASS | `.Should()` assertions throughout the new test files. |
| Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, `Path.GetTemp*`, `Guid.NewGuid`, or `new Random()` in changed test files (grep clean). No temp files. |
| Scenario completeness | PASS | Positive/negative/edge cases present: exact vs near-miss StoreID, OrdinalIgnoreCase, empty/whitespace entries ignored, fail-open on unreadable StoreID, precedence, UI bind/mutate/no-op/fail-safe, fail-open null wrapper at bypass sites. |
| Test file size <= 500 | PASS | Largest changed test file is `StoreFilterAttributionTests.cs` at 486 lines. |

## 7. Additional Policy Rules

### 7.1 Evidence Location Compliance

All evidence artifacts are written under the canonical `<FEATURE>/evidence/<kind>/` tree
(`evidence/baseline/`, `evidence/qa-gates/`, `evidence/issue-updates/`, `evidence/other/`). A scan of
the branch diff for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/` returned zero matches. Verdict: PASS. (Note: the absent `artifacts/csharp/coverage.xml`
in Section 5.1 is a review coverage-artifact location, not an evidence-location violation.)

### 7.2 modified-workflow-needs-green-run

The branch diff contains no files matching `.github/workflows/**`, `scripts/benchmarks/**`, or
`.github/actions/**`. The rule does not fire; no green-run evidence is required. Verdict: PASS
(rule not triggered).

## Rejected Scope Narrowing

No caller (orchestrator or otherwise) attempted to narrow the audit scope to a plan, task, phase, or
file subset; the delegating prompt explicitly affirmed full feature-vs-base scope. The only scope-
relevant data-quality issue was the PR-context summary's C#-as-docs misclassification, corrected in
Section 1.1; that is a generator defect, not a caller narrowing instruction. The plan file's trailing
handoff directives are standard planner/executor text and were not treated as narrowing.

## Appendix A — Coverage Verdict Checklist

- C# — canonical artifact `artifacts/csharp/coverage.xml`: FAIL (absent on review host).
- C# — line coverage (touched non-exempt classes, feature-evidence Cobertura): PASS (all >= 95%).
- C# — branch coverage: FAIL (`StoreWrapper` 64.81% below 75% floor; pre-existing; other four PASS).
- C# — new-code line coverage: PASS (>= 90%).
- C# — changed-line regression: PASS (no uncovered changed line).
- TypeScript / Python / PowerShell: no changed files on the branch; coverage not required for these.

## Appendix B — Command Reference

Commands used or cited during this review (check-only, no mutation of source):

- `git merge-base HEAD origin/main` — confirmed base SHA `26905c4b...`.
- `git diff --numstat 26905c4b..HEAD` / `git diff --name-status` — enumerated the full branch diff.
- `git diff 26905c4b..HEAD -- <path>` — read production and test diffs.
- `awk 'END{print NR}' <file>` and `git show 26905c4b:<file> | awk 'END{print NR}'` — file-size checks
  at HEAD and baseline.
- Ripgrep over `evidence/qa-gates/final-coverage.2026-07-15T18-45.cobertura.xml` for
  `<class line-rate=... branch-rate=... name="UtilitiesCS.OutlookObjects.Store.*">` — independent
  per-class coverage verification.
- `grep -rnE "Thread\.Sleep|Task\.Delay|DateTime\.Now|..."` over changed test files — determinism scan.
- Executor toolchain evidence (not re-run here): final-csharpier, final-analyzer-build,
  final-nullable-build, final-vstest, coverage-delta, file-size-check under `evidence/qa-gates/`.
