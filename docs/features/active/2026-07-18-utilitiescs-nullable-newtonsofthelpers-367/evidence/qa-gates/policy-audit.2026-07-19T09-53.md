# Policy Audit — utilitiescs-nullable-newtonsofthelpers (#367)

- Timestamp: 2026-07-19T09-53
- Reviewer: feature-reviewer
- Work Mode: full-feature
- Branch under review: `feature/utilitiescs-nullable-newtonsofthelpers-367` @ `c9284b30`
- Base for diff: `origin/epic/utilitiescs-nullable-remediation-integration` (epic integration branch; merge-base `6d4da8bb`)
- Scope: C# nullable-reference-type remediation via per-file `#nullable enable` opt-in across `UtilitiesCS/NewtonsoftHelpers/` (recursive, incl. `MonoExtension/` and `SDIL Reader/`). Annotation and null-safety only.

## Executive Summary

Overall policy verdict: PASS.

The branch delivers annotation-only nullable remediation across exactly the 19 in-scope `UtilitiesCS/NewtonsoftHelpers/` production `.cs` files. No project- or solution-level `<Nullable>` element was introduced; no `.csproj`, `.sln`, `packages.config`, or `.claude/rules/*` file was modified; no files were added or removed. The full C# toolchain (csharpier -> analyzer/codestyle build -> pragma-only nullable/TreatWarningsAsErrors build -> vstest with coverage) passes on the final pass. All 4511 UtilitiesCS tests pass. Coverage shows no regression on changed lines.

One non-blocking policy observation is recorded: three wrapper files remain above the 500-line limit as a pre-existing condition (documented and flagged, splitting is out of the annotation-only scope). No blocking findings.

## Scope and Baseline

- Resolved base branch: `origin/epic/utilitiescs-nullable-remediation-integration` (epic integration base, as directed; this child has `depends_on: []` in the epic manifest).
- Merge-base: `6d4da8bb4d881dc26c421440464ce5575e3fb15f`.
- Branch diff (code): 19 `UtilitiesCS/NewtonsoftHelpers/` production `.cs` files. Non-code diff: feature docs (`spec.md`, `user-story.md`, `plan.*.md`), evidence artifacts under the feature `evidence/` tree, and executor agent-memory entries.
- Verified independently via `git diff --name-only origin/epic/utilitiescs-nullable-remediation-integration...HEAD`: exactly the 19 in-scope `.cs` files changed under `UtilitiesCS/NewtonsoftHelpers/`; no `.csproj`, `.sln`, `packages.config`, `SVGControl/**`, or `.claude/rules/*` change.

## Rejected Scope Narrowing

None. The caller prompt supplied the correct full branch-vs-base audit scope (epic integration base, all changed files) and included documented per-child deviation context (pragma-only verification, pre-existing SVGControl CS0649). No instruction attempted to narrow the audit below the full branch diff. The pragma-only nullable command and the SVGControl CS0649 disposition are legitimate, maintainer-documented mechanics recorded in `evidence/other/maintainer-flags.2026-07-19T08-48.md`, not scope narrowing of the review.

## Evidence Location Compliance

- `validate_evidence_locations.py` equivalent check performed via `git diff --name-only ... | grep -E '^artifacts/(baselines|qa|evidence|coverage)/'`: no matches. All feature evidence is written under the canonical `docs/features/active/2026-07-18-utilitiescs-nullable-newtonsofthelpers-367/evidence/<kind>/` tree (`baseline/`, `qa-gates/`, `regression-testing/`, `other/`).
- No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` conditions encountered. This audit's own artifacts are written under `.../evidence/qa-gates/`, which is not a forbidden `artifacts/...` prefix per `enforce-evidence-locations.ps1`.

## 1. General Code Change Policy

| # | Rule | Verdict | Evidence |
|---|---|---|---|
| 1.1 | Simplicity first; no cleverness | PASS | Minimal per-file annotation edits; no new indirection. |
| 1.2 | No behavior change / no refactor beyond nullable | PASS (1 minor observation) | Diff is `#nullable enable` pragmas, `?` annotations, `!` with `// why` comments, `= null!` initializers. One behavior-preserving pattern-match combine in `FilePathHelperConverter.GetErrorMessage` and one modifier reorder in `NonRecursiveConverter` (see code-review OBS-1/OBS-2), both null-flow/analyzer-driven and behavior-identical. |
| 1.3 | Error handling: fail fast, no silent swallow | PASS | No catch/throw changes; `!` sites preserve prior runtime behavior. |
| 1.4 | File size <= 500 lines | PARTIAL (non-blocking, pre-existing) | See section 1.2.2. |
| 1.5 | Public API compatibility | PASS | Framework-override signatures matched to Newtonsoft.Json 13.0.4; public return/param nullability annotated to reflect actual runtime null behavior; no signature semantics changed. |
| 1.6 | Dependencies unchanged | PASS | No package or `.csproj` reference changes. |

### 1.2.1 Coverage rows (per language with changed files)

Coverage assessment for this child applies the changed-line no-regression rule and the targeted `NewtonsoftHelpers/` figure, per the annotation-only nature of the work on already-covered lines.

- C# / .NET coverage: PASS. Targeted `UtilitiesCS/NewtonsoftHelpers/` production line-rate 93.71% -> 93.81% (dedup 1876/2002 -> 1893/2018); no regression on changed lines. Evidence: `evidence/qa-gates/coverage-delta.2026-07-19T08-48.md`, `evidence/qa-gates/final-coverage.2026-07-19T08-48.md`.
  - Baseline: 93.71% (NewtonsoftHelpers targeted line-rate)
  - Post-change: 93.81% (NewtonsoftHelpers targeted line-rate)
  - Change: +0.10 percentage points (no regression on changed lines)
  - Disposition: PASS
  - New/changed-code coverage: 93.81% line-rate on the touched files; the changed executable lines (`= null!` initializers, split `!`-bearing expressions) sit on already-covered paths and are counted covered.
  - Evidence: `evidence/qa-gates/coverage-delta.2026-07-19T08-48.md`
- C# / .NET repo-wide context (UtilitiesCS.Test scope): line-rate 72.07% -> 72.07% (98272/136359 -> 98286/136375); branch-rate 48.45% -> 48.45%. This figure is the pre-existing repository baseline; it is unchanged by #367 (delta +0.0000181 line, +0.0000788 branch, both marginal increases). Under the COM/VSTO/WinForms testable-denominator exemption ratified for this repository and the per-child epic deviation, the repo-wide floor is enforced at the epic capstone, not this annotation-only Wave-0 child. Verdict for #367: PASS on the applicable changed-line no-regression and targeted-file criteria.
- No `artifacts/csharp/coverage.xml` was generated for this child by design (per maintainer instruction: the completion hook applies a fixed 85% floor to that path against the pre-existing repo-wide percentage, which is not the enforcement basis for this annotation-only child). The authoritative coverage substrate is the feature Cobertura pair under `evidence/baseline/` and `evidence/qa-gates/`.

No other languages have changed files in the branch diff. TypeScript: no changed files (no verdict required). Python: no changed files. PowerShell: no changed files.

### 1.2.2 File-size limit (500 lines) — non-blocking pre-existing observation

Three wrapper files exceed the 500-line production-file limit:

| File | Base-branch lines | HEAD lines | Status |
|---|---|---|---|
| `WrapperScoDictionary.cs` | 644 | 649 | Pre-existing >500; +5 from pragma + `// why` comments |
| `WrapperPeopleScoDictionaryNew.cs` | 606 | 615 | Pre-existing >500; +9 from pragma + `// why` comments |
| `WrapperScDictionary.cs` | 519 | 524 | Pre-existing >500; +5 from pragma + `// why` comments |

Verdict: PARTIAL, dispositioned NON-BLOCKING. All three already exceeded 500 lines on the base branch. The growth is limited to the mandated `#nullable enable` pragma and `// why` comments. Splitting the files is a refactor explicitly excluded by the annotation-only scope (and would itself violate the "no refactor" constraint). This is documented and flagged in `evidence/other/maintainer-flags.2026-07-19T08-48.md` (P6-T4), consistent with the sibling #364 `PrettyPrint.cs` handling. This does not count toward the blocking total; it is carried forward as a maintainer-level pre-existing debt item.

## 2. C# Code Change Policy

| # | Rule | Verdict | Evidence |
|---|---|---|---|
| 2.1 | csharpier formatting | PASS | `dotnet csharpier check .` EXIT 0, `Checked 1406 files`, zero unformatted. `evidence/qa-gates/final-csharpier.2026-07-19T08-48.md`. |
| 2.2 | .NET analyzers / codestyle | PASS | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` EXIT 0; 16 warnings all pre-existing and located in test projects, zero in `NewtonsoftHelpers/`. `evidence/qa-gates/final-analyzer-build.2026-07-19T08-48.md`. |
| 2.3 | Nullable / type-check (pragma-only for this child) | PASS | `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649;CS0618;CS0168` EXIT 0, zero CS86xx across all 19 opted-in files (CS86xx remains fatal). `evidence/qa-gates/final-nullable-build.2026-07-19T08-48.md`. |
| 2.4 | Null-safety by default; annotate to actual behavior | PASS | Framework overrides matched to Newtonsoft.Json 13.0.4; `!` used only with documented behavior-preserving `// why` justification. |
| 2.5 | No project-level `<Nullable>` | PASS | `grep -nE "<Nullable>"` returns no match in `UtilitiesCS.csproj` and `TaskMaster.sln`. `evidence/qa-gates/csproj-no-nullable.2026-07-19T08-48.md`. |

### 2.a Pragma-only verification (deliberate per-child deviation — accepted)

The nullable gate for this child is `msbuild TaskMaster.sln /t:Rebuild /p:TreatWarningsAsErrors=true` WITHOUT `/p:Nullable=enable`. Adding `/p:Nullable=enable` would surface the whole epic's ~2131 CS86xx across ~234 files as false failures. This is a documented, deliberate per-child deviation recorded in `evidence/other/maintainer-flags.2026-07-19T08-48.md` (a) and (b), and is NOT resolved by editing `.claude/rules/*`. The absence of `/p:Nullable=enable` is therefore not a defect. Because `UtilitiesCS` depends on the vendored `SVGControl` project (which fails first under TreatWarningsAsErrors), the authoritative nullable proof is the project-level `UtilitiesCS.csproj` gate that exempts only pre-existing non-nullable codes (CS0649/CS0618/CS0168) and shows zero CS86xx.

### 2.b Pre-existing SVGControl CS0649 (out-of-scope — not a #367 finding)

The solution-level `/t:Rebuild /p:TreatWarningsAsErrors=true` command exits 1 on exactly 2 pre-existing vendored `SVGControl/SvgImageSelector.cs` CS0649 (unused-field) errors. These exist identically on `origin/main`, are not nullable, and the file is untouched by this branch (verified: no `SVGControl/**` change in the diff). Disposition: pre-existing / out of scope; not a blocking finding for #367.

## 3. General Unit Test Policy

| # | Rule | Verdict | Evidence |
|---|---|---|---|
| 3.1 | Existing tests treated as spec; all pass | PASS | 4511/4511 UtilitiesCS tests pass. `evidence/qa-gates/final-coverage.2026-07-19T08-48.md`. |
| 3.2 | No new tests required (annotation-only) | PASS | No test files changed; annotation-only work does not add behavior requiring new tests. Batch regression runs green (`evidence/regression-testing/batch1..8-tests.*.md`). |
| 3.3 | No temp files / external deps introduced | PASS | No test changes. |
| 3.4 | No coverage exclusion of production paths | PASS | No `coverage.config` or exclusion changes in diff. |

## 4. Determinism / CI-Workflow / Benchmark Rules

- `.claude/rules/ci-workflows.md`, `.claude/rules/benchmark-baselines.md`: N/A — no `.github/workflows/**`, no `scripts/benchmarks/**`, and no baseline files changed in the branch diff (verified). No workflow green-run gate applies.

## 5. Tonality Policy

- PASS. Feature docs and evidence use neutral, factual language.

## 6. Assumptions and Unverified Items

- The pragma-only build results, the 4511-test pass count, and the Cobertura line/branch figures are taken from the committed evidence artifacts under the feature `evidence/` tree; spot checks re-ran (csharpier scope reasoning, csproj/sln `<Nullable>` grep, changed-file scope, wrapper line counts, dead-duplicate confirmation) and all corroborated the evidence. A full solution rebuild was not re-executed by this review (not required; evidence verification model).

## 7. Coverage Verdict Summary

| Language | Changed files on branch | Coverage artifact | Repo-wide (context) | Verdict |
|---|---|---|---|---|
| C# / .NET | 19 `.cs` | feature Cobertura (`evidence/baseline` + `evidence/qa-gates`); no `artifacts/csharp/coverage.xml` by design | 72.07% line UtilitiesCS.Test scope, unchanged pre-existing baseline; targeted NewtonsoftHelpers 93.81% | PASS (no regression on changed lines; targeted line-rate clears floor) |
| TypeScript | 0 | n/a | n/a | No changed files |
| Python | 0 | n/a | n/a | No changed files |
| PowerShell | 0 | n/a | n/a | No changed files |

## Appendix A — Commands re-run during this review

- `git diff --name-only origin/epic/utilitiescs-nullable-remediation-integration...HEAD` (scope confirmation)
- `grep -nE "<Nullable>" UtilitiesCS/UtilitiesCS.csproj` / `TaskMaster.sln` (no match)
- wrapper line counts via `awk 'END{print NR}'` (base vs HEAD)
- `git diff --name-only ... -- 'SVGControl/**'` (empty — untouched)
- dead-duplicate confirmation of `ToDoModel/Data Model/People/PeopleScoConverter.cs` (fully commented out) and live registration at `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs:127`

Blocking findings in this artifact: 0.
