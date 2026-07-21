# Policy Audit — utilitiescs-nullable-ci-capstone (Issue #376)

- Timestamp: 2026-07-20T06-00
- Feature folder: `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/`
- Branch: `feature/utilitiescs-nullable-ci-capstone-376`
- Diff base (resolved per `pr-base-branch-merge-base`, NOT `main`): `bfcdb394b19a12ca2345bf740a4efffe0ad6e330` (`origin/epic/utilitiescs-nullable-remediation-integration` tip; this feature merges to the epic integration branch, not `main`)
- Head: uncommitted working-tree state on top of `bfcdb394` (`git log bfcdb394..HEAD` is empty — all reviewed work is currently uncommitted in the worktree; see Procedural Notes)
- Reviewer artifacts consulted: `artifacts/pr_context.summary.txt` (hand-authored fallback this session, no `collect_pr_context` MCP tool available — see Procedural Notes), full `git diff bfcdb394` inspected directly file-by-file
- Work mode: `full-feature` (per `issue.md` marker) — AC source: `spec.md` + `user-story.md`, both present, both fully checked

## Executive Summary

This capstone (Wave 2 of the `utilitiescs-nullable-remediation` epic) edits one CI workflow step to
drop the global `/p:Nullable=enable` override (AC1), executes and evidences a genuine-enforcement
verification of the resulting per-file pragma gate (AC2), and — after discovering the pragma gate had
never actually been run to a clean `EXIT_CODE: 0` on the fanned-in integration tip — performs a large
(61-file) annotation/pragma-suppression/dead-code-deletion remediation pass across eight first-party
projects to make the gate genuinely testable. All seven acceptance criteria are independently verified
against on-disk evidence and are PASS. No `.claude/rules/*` file is edited. No `<Nullable>` element is
added to `UtilitiesCS.csproj` or `SVGControl.csproj`. The full toolchain (csharpier, analyzers build,
pragma nullable gate, MSTest+coverage) completes clean in a final pass. Coverage shows a marginal
increase (83.88%→83.89% line, 76.36%→76.37% branch), not a regression.

Two items remain open as **pre-merge obligations, not defects in this feature's delivered work**: (1)
the `modified-workflow-needs-green-run` gate (a green CI run against the branch head, an
execution/merge-time obligation correctly recorded as unsatisfied, not falsely claimed complete), and
(2) generation of the canonical `artifacts/csharp/coverage.xml` artifact path for automated tooling
(the feature's own Cobertura evidence is complete and used for this audit's independent verification).
Both are recorded in `remediation-inputs.2026-07-20T06-00.md`.

**Overall disposition: PASS, with two pre-merge procedural obligations (not code-quality or AC
defects). Blocking findings against the feature's own delivered work: 0.**

## Rejected Scope Narrowing

No caller instruction in this session's task attempted to narrow the audit to a plan/task/phase
subset, mark any language "out of scope"/"informational only," or instruct skipping a toolchain or
coverage check for a language with changed files on the branch. The task prompt explicitly directed
full-branch-diff scope (`bfcdb394...HEAD`) and enumerated specific verification points to check
independently rather than take on faith. Nothing to record under this heading.

## 1. General Code Change Policy Compliance (`.claude/rules/general-code-change.md`)

| Check | Verdict | Evidence |
|---|---|---|
| Simplicity / no unneeded indirection | PASS | All 61 `.cs` diffs inspected are single-line-scope: nullable annotation (`!`), narrow `#pragma warning disable/restore <CODE>` brackets with a rationale comment, or dead-field deletion. No new abstraction introduced. |
| Separation of concerns preserved | PASS | No I/O, UI, or framework-glue boundary changed. |
| Fail-fast error handling preserved | PASS | No `catch` blocks added/removed; guard clauses only added where a pre-existing null-forgiving convention already applied (e.g., `FolderScorer.cs`, `PeopleScoDictionaryNew.cs`). |
| Logging pattern unchanged | PASS | No logging statements added/removed. |
| File size <= 500 lines (no file crosses the limit **because of this feature**) | PASS | Verified via `git show bfcdb394:<path> \| wc -l` vs. current `wc -l` for every changed `.cs` file: zero files crossed the 500-line threshold as a result of this feature's edits (script check, zero matches). Several touched files (`FolderScorer.cs` 664, `SortEmail.cs` 1408, `BayesianPerformanceMeasurement.cs` 1548, `QfcCollectionController.cs` 2328, several `*Tests.cs` files >500) were **already** over 500 lines before this feature touched them — pre-existing condition, not introduced or worsened here, consistent with the epic's existing no-split precedent recorded in `spec.md`'s Maintainer Decision Summary for three other files. |
| No new production/test dependency added | PASS | No `packages.config`/`.csproj` `<PackageReference>`/`<Analyzer Include>` changed. |
| Public API breaking changes avoided | PASS | No method signature changed except `!`-only nullable annotation; `CS0108` interface re-declaration (`IItemViewer.cs`) is pre-existing and untouched in shape, only pragma-bracketed. |
| I/O isolated, no temp files in tests | PASS | No test file touched adds file I/O; edits are `#nullable enable annotations`/`restore annotations` brackets around existing `?`-annotated members, or `!`-forgiving at pre-existing null-literal test call sites. |

## 2. General Unit Test Policy Compliance (`.claude/rules/general-unit-test.md`)

| Check | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism unaffected | PASS | No test logic, assertion, or fixture behavior changed. Diffs to `*.Test` files are `#nullable enable annotations`/`restore annotations` brackets (13+16 occurrences), null-forgiving `!` at 3 deliberate-null call sites, or (in `PeopleScoDictionaryNewTests.cs`) deletion of 3 grep-confirmed-dead unused fields with the commented-out test bodies left byte-identical. |
| Test file location convention | N/A (no new test file added) | This feature adds zero new test files; it edits 15 pre-existing test files in place. |
| No new external-dependency/mocking-policy violation | PASS | No new mock, stub, or dependency introduced. |
| Repo-wide line coverage >= 85%, branch >= 75% (uniform tier rule, `.claude/rules/quality-tiers.md`) | **FAIL (pre-existing, non-regressed)** | Post-change: 83.89% line / 76.37% branch. Line coverage is below the 85% uniform floor; branch coverage clears 75%. Baseline (captured mid-remediation, honestly disclosed as such in the feature's own evidence) was 83.88%/76.36% — i.e., this shortfall **predates** this feature and is **not introduced or worsened** by it (delta is a marginal increase on both metrics). Disposition: non-blocking for this feature — the repo-wide line-coverage gap is a pre-existing condition tracked separately (CLAUDE.md's COM/VSTO/WinForms coverage-exemption authority and the epic's own tracked `feature/csharp-coverage-uplift` line apply to some of this residual); this feature must not be blocked on a pre-existing, unworsened repo-wide metric. See Section 5 for full numeric detail. |
| No regression on changed lines | PASS | All 61 changed `.cs` files' edits are annotation/pragma/dead-code-only; `qc-coverage-delta.2026-07-20T05-20.md` shows a net increase, not a decrease, on both line and branch coverage, and the 5702/5702 MSTest pass count is identical before and after. |
| Coverage-exclusion policy (no production file excluded from measurement) | PASS | No `exclude`/`ExcludeFromCodeCoverage` attribute or coverage-config exclusion entry added by this feature. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 83.88% line (87365/104150) / 76.36% branch (19529/25576). Post-change: 83.89% line (87378/104155) / 76.37% branch (19533/25576). Change: a marginal increase on both metrics — precisely +0.00008 (rounds to +0.01% displayed) line-rate, +0.00016 (+0.02% displayed) branch-rate. New/changed-code coverage: none reported — this feature's changes are nullable-annotation/pragma-suppression/dead-code-deletion edits to existing lines, not new executable production code; no new method or class is added. Disposition: FAIL on the absolute 85% line-coverage floor (pre-existing, non-regressed — see row above), PASS on the 75% branch floor and on no-regression. Evidence: `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/evidence/baseline/baseline-tests-coverage.2026-07-20T00-30.md`, `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/evidence/qa-gates/qc-tests-coverage.2026-07-20T05-15.md`, `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/evidence/qa-gates/qc-coverage-delta.2026-07-20T05-20.md`.
- TypeScript: N/A — no `.ts`/`.tsx` files changed on this branch.
- Python: N/A — no `.py` files changed on this branch.
- PowerShell: N/A — no `.ps1`/`.psm1` files changed on this branch.

### Coverage Artifact Location

- Canonical hook-facing path `artifacts/csharp/coverage.xml`: **absent**. Per the mandatory-artifact rule, this is procedurally a FAIL for the automated-tooling lookup path; recorded as a remediation-required (non-code) item in `remediation-inputs.2026-07-20T06-00.md`.
- Independent verification was instead performed against the feature's own canonical evidence-folder Cobertura artifacts (`evidence/baseline/baseline-coverage.cobertura.xml`, `evidence/qa-gates/qc-coverage.cobertura.xml`), which this repository's established convention treats as the authoritative per-feature coverage record (see `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`). Both files were read directly and their root `<coverage>` attributes cross-checked against the prose in the corresponding `qc-tests-coverage`/`baseline-tests-coverage` markdown artifacts; the numbers match exactly (line-rate `0.838838`→`0.838923`, branch-rate `0.763567`→`0.763724`).

## 3. Language-Specific Code Change Policy Compliance (C# — `.claude/rules/csharp.md` + CLAUDE.md C# section)

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `evidence/qa-gates/qc-csharpier.2026-07-20T05-00.md`: first pass reformatted 3 files (blank-line normalization around pragma brackets), Final QC loop restarted per policy, second pass 0 files reformatted. |
| .NET analyzer diagnostics (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`) | PASS | `evidence/qa-gates/qc-analyzers-build.2026-07-20T05-05.md`: `EXIT_CODE: 0`, 0 errors; one pre-existing, unrelated CS2002 warning flagged, not introduced by this feature. |
| Nullable/type-check gate (this feature's own subject) | PASS | `evidence/qa-gates/qc-nullable-gate.2026-07-20T05-10.md`: `msbuild ... /t:Rebuild ... /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) reaches `EXIT_CODE: 0` solution-wide on the fully-remediated tree, matching the Phase 2 (`P2-T23`) checkpoint. |
| No `<Nullable>` element added to `UtilitiesCS.csproj`/`SVGControl.csproj` (AC5) | PASS | `grep -n "<Nullable>" UtilitiesCS/UtilitiesCS.csproj SVGControl/SVGControl.csproj` returns no matches on the current tree. |
| No `.claude/rules/*` file edited (AC4) | PASS | `git diff bfcdb394 --name-only \| grep -i "\.claude/rules"` returns no matches; the `csharp.md` rules-vs-convention conflict is flagged, not edited, in `spec.md`'s "Rules-vs-convention conflict detail (AC4)" section. |
| No behavior change to production C# code (AC7) | PASS | Individually inspected diffs for `SvgImageSelector.cs` (narrow `#pragma warning disable/restore CS0649` bracket around two never-assigned fields, dead setter left untouched), `PeopleScoDictionaryNew.cs` (`!`-only), `TaskController.Actions.cs` (`#pragma warning disable/restore CS4014` bracket, no `await` added — preserves the exact pre-existing fire-and-forget UI-thread-marshal semantics), `ToDoEvents.Filtering.cs` (`#pragma warning disable/restore CS0618` bracket, `ForEachAwaitAsync` call unchanged), `IItemViewer.cs` (`#pragma warning disable/restore CS0108` bracket around a pre-existing deliberate member re-declaration, no `new`/restructuring added), `FolderScorer.cs` (33 lines changed, all `!`-only), and the batch evidence artifacts for the remaining ~50 files (all reporting "annotation/null-forgiving/guard-clause-only" or "pragma-bracket-only" or "dead-code-deletion-after-zero-live-reference-confirmation"). No control-flow, signature-shape, or logic change found in any inspected diff. |

## 4. Language-Specific Unit Test Policy Compliance (C# — MSTest/Moq/FluentAssertions)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework unchanged | PASS | No new/changed test uses a different framework; edited test files only receive annotation-context brackets or dead-field deletion. |
| Full MSTest suite passes | PASS | `evidence/qa-gates/qc-tests-coverage.2026-07-20T05-15.md`: 5702/5702 passed, identical count to the P0-T10 baseline (`evidence/baseline/baseline-tests-coverage.2026-07-20T00-30.md`). |
| Dead-field deletion (`PeopleScoDictionaryNewTests.cs`) is genuinely dead | PASS | `evidence/remediation-baseline/debt2-layer2-todomodeltest-cs0169.<timestamp>.md` records a grep confirming zero live (non-commented) references to `mockApplication`, `_mockPrefix`, `_peopleScoDictionaryNew` before deletion; the surrounding commented-out test bodies are confirmed byte-identical after the edit. |

## 5. Test Coverage Detail

See Section 2/1.2.1 above for the full numeric comparison. Summary:

- Total tests: 5702 → 5702 (no change in count or pass/fail outcome).
- Line-rate: 0.838838 (83.88%) → 0.838923 (83.89%). Delta: +0.000085 (marginal increase).
- Branch-rate: 0.763567 (76.36%) → 0.763724 (76.37%). Delta: +0.000157 (marginal increase).
- The baseline itself was captured mid-remediation (Phase 1 and 4 of 7 Phase 2 batches already applied), per the baseline artifact's own transparency note — a conservative, not favorable, comparison basis for this feature.

## 6. Test Execution Metrics

| Metric | Baseline (P0-T10) | Post-change (P7-T4) |
|---|---|---|
| Total tests | 5702 | 5702 |
| Passed | 5702 | 5702 |
| Failed | 0 | 0 |
| Run time | 34.6165s | (recorded in `qc-tests-coverage.2026-07-20T05-15.md`) |

## 7. Code Quality Checks

| Check | Result |
|---|---|
| CSharpier format scan | 0 files reformatted on confirming pass (`qc-csharpier.2026-07-20T05-00.md`) |
| Analyzer/code-style build | 0 errors (`qc-analyzers-build.2026-07-20T05-05.md`) |
| Pragma nullable gate (solution-wide) | `EXIT_CODE: 0` (`qc-nullable-gate.2026-07-20T05-10.md`) |
| Confidentiality/secret scan | No secrets, credentials, or `.env` content introduced; all diffs reviewed are code/comment-only |
| Suppression scan (added lines) | 8 new `#pragma warning disable/restore` brackets added across this session (CS0649 x1, CS4014 x1, CS0618 x2, CS0108 x1, MSTEST0032 x1, CS8632/CS8767 annotations-context brackets x~8 files); every occurrence carries an in-code rationale comment per policy, confirmed by direct diff inspection |
| Workflow change scan | `.github/workflows/ci.yml` modified (7 lines changed, 6 removed) — the only workflow file touched; exit-code handling (`if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`) preserved verbatim (`git diff` confirms it is unchanged) |

## 8. Gaps and Exceptions

1. **Repo-wide C# line coverage (83.89%) is below the 85% uniform floor.** Pre-existing (baseline 83.88%), not introduced or worsened by this feature. Non-blocking for this feature's own merge; tracked as a pre-existing repo condition.
2. **`modified-workflow-needs-green-run` is correctly recorded as an unsatisfied, execution/merge-time obligation**, not falsely claimed complete. `evidence/other/green-run-requirement-recorded.2026-07-20T04-50.md` states explicitly: "NOT SATISFIED BY THIS PLAN," assigning the obligation to epic-orchestrator before merge to `main` (or fan-in to the integration branch). This is the correct disposition per this feature's own scope — planning/execution alone cannot trigger a real GitHub Actions run.
3. **Canonical `artifacts/csharp/coverage.xml` is absent.** The feature's own Cobertura evidence (in the canonical `<FEATURE>/evidence/` location per this repo's evidence-and-timestamp conventions) was used for independent verification instead; the automated-tooling-facing canonical path is a separate, non-blocking procedural gap.
4. **Analyzer-package-version drift (16 first-party `.csproj` files)** and **one residual pre-existing CS2002** (duplicate `<Compile>` item, `UtilitiesCS.Test.csproj`) are correctly flagged-only (not fixed) in `spec.md`'s Maintainer Decision Summary, confirmed present on `origin/main` and pre-dating this epic — correctly out of scope for this feature.
5. **The `PeopleScoDictionaryNew.cs` `#nullable disable`/`#nullable enable` island is retained**, not removed — a tested-and-reverted decision (removing it reintroduced 12 CS8644 errors because neither `ScoDictionaryNew<,>` nor `ConcurrentObservableDictionary<,>` carry a nullable pragma despite #366's `where TKey : notnull` constraint). Correctly evidenced and unchanged on disk (confirmed: `git diff` shows only the two unrelated P2-T13 null-forgiving fixes as this file's net change).

## 9. Summary of Changes

- `.github/workflows/ci.yml`: 1 step edited, dropping `/p:Nullable=enable`, preserving `/t:Rebuild`, `/p:TreatWarningsAsErrors=true`, and the exit-code handling verbatim (AC1, AC3).
- `SVGControl/SvgImageSelector.cs`: 1 narrow `#pragma warning disable/restore CS0649` bracket (build-debt fix, prerequisite for AC1/AC2 to be testable).
- 60 further `.cs` files across `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel`, `ToDoModel.Test`, `TaskVisualization`, `TaskMaster`, `TaskMaster.Test`, `QuickFiler`, `QuickFiler.Test`: nullable-annotation (`!`), narrow pragma-suppression bracket, or grep-confirmed-dead-code deletion, discovered via the first-ever genuine full-solution `/t:Rebuild` under `TreatWarningsAsErrors=true` on the fanned-in integration tip.
- `spec.md`, `user-story.md`, `plan.2026-07-19T04-25.md`, `epic.md`: feature-document updates recording the scope-expansion decision, the consolidated Maintainer Decision Summary (AC6), and the AC4/AC5 conflict-flag/optional-flip evaluation.
- No `.claude/rules/*` file, no `<Nullable>` csproj element, no test-behavior change.

## 10. Compliance Verdict

**PASS** for all seven acceptance criteria and all applicable General/C#-specific Code Change and Unit Test Policy checks, with two non-blocking pre-existing/procedural gaps (repo-wide C# line-coverage floor, unsatisfied at 83.89%<85% but pre-existing and non-regressed; canonical coverage-artifact path absent but independently verified via feature evidence) and two correctly-recorded open pre-merge obligations (green CI run; canonical coverage artifact generation) carried forward to `remediation-inputs.2026-07-20T06-00.md`.

**Blocking findings against this feature's own delivered work: 0.**

## Procedural Notes

- **No commits on this branch beyond the base.** `git log bfcdb394..HEAD` is empty; all reviewed work exists as uncommitted changes in the worktree (`git status --porcelain` shows the 72 modified/added-untracked paths enumerated in this audit). This review was performed against the working-tree diff (`git diff bfcdb394`), which is the actual branch state as instructed. This is a process observation, not a code-quality finding — the work must be committed before a PR can be opened against it.
- **PR context artifacts were absent at review start** (no `artifacts/pr_context.summary.txt`/`.appendix.txt`, and no `collect_pr_context` MCP tool available in this session). Per the regeneration instruction, `artifacts/pr_context.summary.txt` was hand-authored this session from `git diff bfcdb394 --numstat`, in the bullet format the coverage-verification hook (`validate-feature-review-coverage.ps1`) expects, to enable automated per-language coverage-row enforcement.

## Appendix A: Toolchain Commands Reference

1. `dotnet tool run csharpier .` (or `csharpier .`)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable` — this feature's own finalized gate)
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` (via `scripts/vscode/Invoke-MSTestWithCoverage.ps1`)

### Coverage Evidence Checklist

- [x] TypeScript coverage artifact present/absent noted — N/A, no TypeScript files changed.
- [x] Python coverage artifact present/absent noted — N/A, no Python files changed.
- [x] PowerShell coverage artifact present/absent noted — N/A, no PowerShell files changed.
- [x] C# coverage artifact present/absent noted — canonical `artifacts/csharp/coverage.xml` absent; feature-evidence Cobertura (`evidence/baseline/baseline-coverage.cobertura.xml`, `evidence/qa-gates/qc-coverage.cobertura.xml`) present and used for independent verification.
