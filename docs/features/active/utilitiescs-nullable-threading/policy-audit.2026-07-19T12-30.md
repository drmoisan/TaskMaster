# Policy Audit — utilitiescs-nullable-threading (Issue #369)

- Timestamp: 2026-07-19T12-30
- Reviewer: feature-review agent
- Feature branch: `feature/utilitiescs-nullable-threading-369`
- Head: `911cfd18ba89f136c38887464ec3fb97226bd771`
- Base branch (resolved): `origin/epic/utilitiescs-nullable-remediation-integration`
- Merge-base: `6d4da8bb4d881dc26c421440464ce5575e3fb15f`
- Integration tip: `52c1d7cff2e9942a39cc07a748c845bde95a8ad2`
- Diff reviewed: `git diff origin/epic/utilitiescs-nullable-remediation-integration...HEAD`
- Work mode: `full-feature` (AC sources: `spec.md` `## Definition of Done` + `user-story.md` `## Acceptance Criteria`)

## Executive Summary

This is a C# nullable-reference-type remediation of `UtilitiesCS/Threading/` delivered as a single
feature commit under a per-file `#nullable enable` opt-in. The change set is annotation and
null-safety only: nullable annotations (`?`), justified null-forgiving operators (`!`) with
why-comments, and `= null!` field initializers. Independent inspection of the full branch diff
confirms no behavior change, no API-signature semantics change, and no change to locking, ordering,
scheduling, single-shot-guard, `Interlocked`/`volatile`, timer arm/re-arm, `SynchronizationContext`,
`Dispatcher` sequencing, or store-lockup-watchdog concurrency semantics.

Overall policy verdict: **PASS** with one pre-existing, correctly-flagged file-size exception
(`TimeOutTask.cs`, 976 lines) that is out of remediable scope for an annotation-only change, and one
non-blocking procedural recommendation (generate the canonical `artifacts/csharp/coverage.xml`
JaCoCo artifact in the epic capstone/CI). No blocking findings.

Blocking findings: 0.

## Scope and Baseline

- Changed code files: 25 hand-written `.cs` files under `UtilitiesCS/Threading/` (all pre-existing,
  all MODIFIED; zero new source files; zero deleted files).
- Changed non-code files: feature docs (`spec.md`, `user-story.md`, `plan.*.md`) and evidence
  markdown/cobertura under `docs/features/active/utilitiescs-nullable-threading/evidence/`.
- Not changed (verified via `git diff --name-only`): no `.csproj`, no `.sln`, no `*.Designer.cs`,
  no `.resx`, no `.github/workflows/**`, no `scripts/benchmarks/**`.
- Only language with changed code files in the branch diff: **C#**. TypeScript, Python, and
  PowerShell have zero changed files on this branch.

## Rejected Scope Narrowing

No caller instruction attempted to restrict the audit to a plan/task/phase subset, to a subset of
changed files, or to suppress a required check for a language present in the diff. The caller's
guidance on the pragma-only nullable verification command and on the changed-line coverage basis is
a documented, maintainer-ratified policy deviation for this epic child (recorded below under
Toolchain), not a scope narrowing. Nothing to reject.

## 1. Toolchain Compliance (C#)

Order per CLAUDE.md CUT3: csharpier -> analyzer/codestyle build -> type-check build -> vstest with
coverage. Evidence inspected from the executor's final QC run (not re-executed by the reviewer).

| Stage | Command | Result | Evidence | Verdict |
|---|---|---|---|---|
| Format | `dotnet tool run csharpier check .` | EXIT 0; 1406 files checked, 0 unformatted | `evidence/qa-gates/final-csharpier.2026-07-19T11-05.md` | PASS |
| Analyzer / codestyle | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0; 0 new analyzer diagnostics vs P0-T3 baseline | `evidence/qa-gates/final-analyzer-build.2026-07-19T11-05.md` | PASS |
| Type-check (nullable) | `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` (pragma-only, NO `/p:Nullable=enable`) | 0 CS86xx across all 25 opted-in files; residual EXIT 1 confined to pre-existing vendored SVGControl CS0649 | `evidence/qa-gates/final-nullable-build.2026-07-19T11-05.md` | PASS (see deviation note) |
| Tests + coverage | `dotnet-coverage collect ... vstest.console.exe UtilitiesCS.Test.dll` | 4511 passed, 0 failed | `evidence/qa-gates/final-coverage.2026-07-19T11-05.md` | PASS |

### Documented, maintainer-ratified toolchain deviation (not a finding)

The nullable type-check gate for this child intentionally uses the pragma-only build
`msbuild TaskMaster.sln /t:Rebuild /p:TreatWarningsAsErrors=true` WITHOUT `/p:Nullable=enable`. This
is mandated by the epic's per-file opt-in architecture (spec.md Constraints item 1): forcing
`/p:Nullable=enable` project-wide would surface the whole epic's ~2131 CS86xx across ~234 files as
false failures unrelated to #369. The absence of project-wide `/p:Nullable=enable` and the absence of
a `<Nullable>` element in `UtilitiesCS.csproj` are therefore expected and correct, not defects.

### Pre-existing vendored SVGControl CS0649 (not a blocker for this base)

The literal solution `/t:Rebuild ... /p:TreatWarningsAsErrors=true` exits 1 on a pre-existing
vendored `SVGControl` CS0649 (unassigned fields promoted by TreatWarningsAsErrors), present in the
P0 baseline (`evidence/baseline/nullable-build-baseline.2026-07-19T08-51.md`) and unrelated to
`Threading/`. The executor confirmed zero CS86xx via a scoped genuine recompile of
`UtilitiesCS.csproj` under the same pragma-only TreatWarningsAsErrors. This PR targets the epic
integration branch, on which `ci.yml` does not run (it triggers only on PRs to main/development), so
the SVGControl CS0649 is not a required-check blocker for this child. Disposition: pre-existing,
out of remediable scope for #369, non-blocking for this base. Recommend tracking at the epic level.

## 2. General Code Change Policy

| Rule | Assessment | Verdict |
|---|---|---|
| Simplicity / no behavior change | Diff is annotation-only; no logic, control-flow, or API-shape changes introduced | PASS |
| Separation of concerns | Unchanged; no restructuring | PASS |
| Error handling / fail-fast | Preserved exactly; `timer!` retains NRE-if-unassigned behavior rather than swallowing via `timer?.` (`AsyncMultiTasker.cs`) | PASS |
| Public API compatibility | Public return types preserved; `TimeOutTask` retains `Task<TResult>` via `result = default!` / `return result!` rather than widening to `Task<TResult?>` | PASS |
| Naming / comments | Justified `!` sites carry brief why-comments explaining the invariant | PASS |
| File size (<= 500 lines) | 24 of 25 files under limit; `TimeOutTask.cs` = 976 lines — PRE-EXISTING breach (975 pre-change + 1 pragma line), flagged not fixed | PARTIAL (non-blocking; see finding F1) |

`ApplicationIdleTimer.cs` (481->482) and `AsyncMultiTasker.cs` (465->469) remain under 500 lines
post-annotation; no annotation-induced breach occurred (verified by direct line count on HEAD).

## 3. General Unit Test Policy / Coverage

Coverage is mandatory for every language with changed files. C# is the only such language.
Coverage was verified by inspecting the executor's Cobertura artifact in the canonical evidence
location; the reviewer independently re-parsed the Cobertura XML root and confirmed the figures
below rather than trusting only the evidence markdown.

| Item | Baseline (P0-T5) | Post-change (P9-T4) | Basis |
|---|---|---|---|
| Repo-wide C# line-rate | 72.067% (98270/136359) | 72.069% (98283/136373) | Cobertura root `<coverage>` (reviewer-verified) |
| Repo-wide C# branch-rate | 48.44% (12288/25366) | 48.44% (12288/25366) | Cobertura root `<coverage>` (reviewer-verified) |
| Threading production aggregate line-rate | 81.85% (3855/4710) | 81.93% (3890/4748) | targeted production subset |
| Tests | — | 4511 passed / 0 failed | `final-coverage` |

C# coverage verdict — changed-line no-regression gate: **PASS**. Rationale and disposition:

- These 25 files are all MODIFIED pre-existing files, not new files; no new source file was added,
  so the 90% new-file threshold does not engage.
- The edits are annotation-only and preferred `?` / `= null!` / justified `!` over new
  `if (x is null)` guards, so no new runtime branch/executable line was introduced. Reviewer-verified
  from the Cobertura root: covered lines rose 98270 -> 98283 and the Threading production aggregate
  rose 81.85% -> 81.93%; no previously-covered executable line became uncovered. The single per-file
  rate movement (`ProgressTrackerPane.cs` 0.761 -> 0.753) is a denominator-growth artifact inside an
  already-uncovered constructor (its covered count rose 178 -> 180), not a regression on a changed
  line. Evidence: `evidence/qa-gates/coverage-delta.2026-07-19T11-05.md` (independently corroborated).
- Repo-wide C# line-rate (72.07%) is a pre-existing baseline characteristic of the whole solution and
  is unchanged-to-slightly-up by this feature; it is not introduced or worsened here. Per CLAUDE.md's
  80% testable-denominator floor with the ratified COM/VSTO/WinForms exemption, the Threading module
  aggregate of 81.93% clears the 80% floor for this heavily UI-thread/COM-bound module. Repo-wide
  uplift toward the 85% general figure is the epic capstone's responsibility, tracked in
  `feature/csharp-coverage-uplift`.

Canonical coverage-artifact location: the canonical `artifacts/csharp/coverage.xml` (JaCoCo) is not
present; coverage was instead verified from the executor's Cobertura at
`docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/final-coverage.2026-07-19T11-05.cobertura.xml`
(a canonical evidence location) plus its baseline sibling. This is a procedural gap, recorded as
non-blocking recommendation R1 below; the coverage data itself exists and was reviewer-verified.

## 4. C# Code Change / Unit Test Policy

| Rule | Assessment | Verdict |
|---|---|---|
| Nullable annotations reflect actual runtime null behavior (contracts) | `CurrentStoreContext.Current`/`_current` -> `string?` matches documented "null = no context"; `LockupAttribution.StoreIdentity` -> `string?` chain feeds the watchdog | PASS |
| `volatile` / single-writer discipline untouched | `CurrentStoreContext._current` keeps `volatile`; only the element type gains `?` | PASS |
| net481 profile respected | No `System.Diagnostics.CodeAnalysis` post-condition attributes introduced; zero CS86xx reached with `?`, `= null!`, justified `!` | PASS |
| MSTest + Moq + FluentAssertions | No test files changed; existing suite green (4511/0) | PASS (n/a for new tests) |
| Justified `!` documented | why-comments present at `StoreLockupResponder` `displayName!`, `AsyncMultiTasker` `timer!`, `ProgressTrackerPane` constructor | PASS |

## 5. Concurrency-Semantics Preservation (C3 module)

Reviewer inspected the diff of the concurrency-critical files directly:

- `StoreLockupResponder.cs`: the four documented null-store-model branches (no-context guard,
  unresolved-sentinel guard, `<Stores-enumeration>` phase guard, already-disabled guard) are
  unchanged in order and content. Only annotations were applied around them (`StoreLockupNotifier?`,
  `Action<string>?` ctor params; `displayName!` at the `_notify(...)` site). Verified against diff
  and `evidence/other/maintainer-flags.2026-07-19T10-00.md` (P7-T7). PASS.
- `AsyncMultiTasker.cs`: `timer!.StopTimer()`/`timer!.Dispose()` in `catch`/`finally` preserve the
  NRE-if-unassigned behavior; not switched to `timer?.`. `timerFactory` params annotated `?` only.
  PASS.
- `CurrentStoreContext.cs`: `volatile string? _current`; `Begin`/`Normalize`/`Scope` signatures
  gain `?` with no logic change. PASS.
- `ApplicationIdleTimer.cs`: reflection lookups gain `EventInfo?`/`FieldInfo?`/`object?` locals and
  `= null!` on `instance`/`_timer`; pre-existing null-guards (`if (idleEventInfo == null) return
  null;`) unchanged. PASS.

## 6. Evidence Location Compliance

- `validate_evidence_locations.py` is not present in this repository; scan performed via
  `git diff --name-only`.
- No branch-diff file is written under `artifacts/baselines/`, `artifacts/qa/`,
  `artifacts/evidence/`, or `artifacts/coverage/`. All feature evidence is under the canonical
  `docs/features/active/utilitiescs-nullable-threading/evidence/<kind>/` tree (baseline, qa-gates,
  regression-testing, other). No violation.

## 7. Modified-Workflow Green-Run Gate

No path under `.github/workflows/**`, `scripts/benchmarks/**`, or `.github/actions/**` is modified in
the branch diff. The `modified-workflow-needs-green-run` rule does not fire. Verdict: PASS (rule not
triggered).

## Findings

- **F1 (PARTIAL, non-blocking):** `TimeOutTask.cs` is 976 lines, exceeding the 500-line limit
  (General Code Change Policy §4). This is a PRE-EXISTING condition (975 lines before this feature;
  the `#nullable enable` pragma adds the single 976th line). Bringing it under 500 requires splitting
  ~15 overloads across files — a refactor explicitly prohibited by the annotation-only mandate. The
  breach is correctly flagged for the maintainer in `spec.md` Constraints item 6, DoD item 8,
  `user-story.md` AC 8, and `evidence/other/maintainer-flags.2026-07-19T10-00.md` (P8-T7). Not
  remediable within this child. Disposition: accept as a documented pre-existing exception; defer the
  split to a separate issue. Non-blocking.

## Recommendations

- **R1 (non-blocking, procedural):** Generate the canonical `artifacts/csharp/coverage.xml` (JaCoCo)
  during the epic capstone / CI coverage step so the automated coverage validator can parse a
  repo-wide C# figure directly. For this child the coverage data exists as Cobertura in the canonical
  evidence location and was reviewer-verified; only the canonical JaCoCo path is absent.
- **R2 (epic-level tracking):** Track the pre-existing vendored SVGControl CS0649 and the
  `TimeOutTask.cs` split as separate epic-level items.

## Appendix A — Coverage Verdict Summary (per language with changed files)

| Language | Changed files | Coverage evidence | Changed-line no-regression | Verdict |
|---|---|---|---|---|
| C# | 25 (all modified) | Cobertura `final-coverage.2026-07-19T11-05.cobertura.xml` + baseline sibling; canonical `artifacts/csharp/coverage.xml` absent | No regression (repo-wide covered lines 98270 -> 98283; Threading aggregate 81.85% -> 81.93%) | PASS |

TypeScript, Python, PowerShell: zero changed files on this branch; no coverage verdict required.

## Appendix B — Command Reference

- Scope: `git diff origin/epic/utilitiescs-nullable-remediation-integration...HEAD`
- Merge-base: `git merge-base HEAD origin/epic/utilitiescs-nullable-remediation-integration`
- Format (executor): `dotnet tool run csharpier check .`
- Analyzer (executor): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Type-check (executor, pragma-only): `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- Coverage (executor): `dotnet-coverage collect --output <cobertura> --output-format cobertura --settings coverage.config -- vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
- Reviewer verification: direct re-parse of Cobertura root `<coverage>` element (baseline vs post-change) and direct line-count of changed `.cs` files on HEAD.
