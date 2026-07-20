# Policy Compliance Audit — utilitiescs-nullable-residuals (Issue #375)

- Timestamp: 2026-07-19T12-11
- Branch under review: `feature/utilitiescs-nullable-residuals-375`
- Diff base (epic-integration tip): `dffadd5a102884dd811ed5731477de18417594f1`
- Feature HEAD: `c413e61cb32002bd802c4dc8e1f07f5a70729e55`
- Work Mode: `full-feature` (AC sources: `spec.md` + `user-story.md`)
- Scope command: `git diff dffadd5a102884dd811ed5731477de18417594f1..HEAD`

## Scope and Baseline

The audit scope is the full branch diff against the resolved base commit `dffadd5a`. The diff comprises:

- 37 hand-written `UtilitiesCS/*.cs` source files (per-file `#nullable enable` opt-in; annotation-only).
- 5 baseline/other evidence files, 11 qa-gate evidence files, and 2 Cobertura coverage XML artifacts under the canonical `docs/features/active/utilitiescs-nullable-residuals/evidence/` tree.
- `spec.md`, `user-story.md`, `plan.2026-07-18T23-13.md` (AC check-offs and plan progress).
- 2 `.claude/agent-memory/atomic-executor/` markdown files (executor memory notes; documentation only, no code).

No `*.csproj`, `*.sln`, `*.props`, `*.targets`, or `*.Designer.cs` file is modified on the branch (verified by name-only diff filter).

### PR Context Artifacts

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are absent in this worktree. Scope was derived directly from the authoritative base commit `dffadd5a` supplied by the delegating context and confirmed via `git diff`. This is a legitimate authoritative scope source under the Scope Invariant. Absent a pr_context summary, the coverage-gate hook enumerates zero changed languages from that file; the C# coverage verdicts below are nonetheless recorded explicitly.

## Rejected Scope Narrowing

None detected. No caller instruction attempted to narrow the audit to a plan/task/phase subset, mark any language out of scope, or skip a required check. The delegating context correctly identifies the full 37-file branch diff as the scope. The single non-code narrowing present in evidence — the SVGControl CS0649 being out of scope for #375 — is a legitimate ownership boundary (owned by epic child #368) documented and verified as pre-existing, not a coverage-scope narrowing.

## Evidence Location Compliance

All evidence artifacts on the branch are written under the canonical `docs/features/active/utilitiescs-nullable-residuals/evidence/<kind>/` path (`baseline/`, `other/`, `qa-gates/`). A name-only diff scan for non-canonical evidence paths (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`) returned zero matches. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose. `validate_evidence_locations.py` is not present in this repository; the manual scan is the substitute and shows no violations. Verdict: PASS.

## Policy Verdicts

| # | Policy area | Verdict | Evidence |
|---|---|---|---|
| P1 | General Code Change — annotation-only, no behavior/design change | PASS | Full source diff is `#nullable enable` pragmas, `?`/`= null!`/`!` annotations, one scoped `#nullable disable`/`enable` region; no new types, methods, guards, or control flow. |
| P2 | General Code Change — 500-line file limit | PASS (with sanctioned pre-existing flags) | `qc-line-count`: only 3 files exceed 500 (MeetingItemHelper 849, RecipientStatic 774, UserDefinedFields 725); all pre-existing breaches, flagged not split per spec AC8/Maintainer item 6. Next-largest non-breach is SmithWaterman at 377. No file newly crosses 500. |
| P3 | General Code Change — error handling / guards preserved | PASS | Existing guards (`store?.`, `?? ""`, `?.Dispose()`, `is null`) unchanged; no new runtime guard added (spec Constraint 5). |
| P4 | C# Code Change — nullable approach fits net481 profile | PASS | Zero CS86xx reached with `?`, `= null!`, justified `!` only. No post-condition attributes, no `record`/`record struct`/`init` (net481-unavailable) introduced. |
| P5 | C# Code Change — no project/solution `<Nullable>` flip | PASS | `qc-no-project-nullable`: `<Nullable>` count in `UtilitiesCS.csproj` is 0; csproj and `.sln` unchanged; no build used `/p:Nullable=enable`. |
| P6 | C# Toolchain — format (csharpier) | PASS | `qc-format-csharpier`: `csharpier check .` exits 0; idempotent after one reflow-restart (OneDriveDownloader, FileIO2 wraps). |
| P7 | C# Toolchain — analyzers | PASS | `qc-analyzers`: `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exits 0, 0 errors, 77 pre-existing warnings (not promoted). |
| P8 | C# Toolchain — nullable type-check (pragma-only gate, authoritative) | PASS | `qc-nullable-pragma-gate` section B: isolated `UtilitiesCS.csproj /t:Rebuild /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) exits 0, 0 CS86xx across all 37 files. |
| P9 | C# Toolchain — tests | PASS | `qc-tests-coverage`: 4511 passed / 0 failed / 0 skipped (identical to baseline). |
| P10 | Unit Test Policy — no regression, coverage-neutral | PASS | `qc-coverage-delta`: tests 4511→4511, UtilitiesCS line 88.75%→88.75%, branch 82.51%→82.51% (byte-identical). Annotation-only edits add no executable lines. |
| P11 | Evidence & timestamp conventions | PASS | See Evidence Location Compliance; ISO-8601 timestamps on all artifacts. |
| P12 | Maintainer decisions recorded, not silently resolved | PASS | `qc-maintainer-flags`: all 6 flags present in `spec.md` (lines 224/233/239/245/249/258). |

## Full-Solution Build Note (not a finding)

The mandated full-solution command `msbuild TaskMaster.sln /t:Rebuild /p:TreatWarningsAsErrors=true` exits 1 solely on two pre-existing `SVGControl/SvgImageSelector.cs` CS0649 errors reached through UtilitiesCS's `<ProjectReference>` to SVGControl. The P0-T2 baseline evidence (`baseline-pragma-build.2026-07-19T10-54.md`) confirms these are present before any #375 edit and emit zero CS86xx. SVGControl is owned by epic child #368. This is not introduced by, owned by, or in scope for #375 and is recorded here for provenance only. The authoritative CS86xx signal is the isolated UtilitiesCS build (EXIT 0).

## Coverage Verdicts (per language with changed files)

Coverage evidence is read from the feature evidence Cobertura artifacts (`evidence/baseline/coverage-baseline.cobertura.xml`, `evidence/qa-gates/coverage-postchange.cobertura.xml`). The canonical `artifacts/csharp/coverage.xml` path is intentionally not generated for this child: an aggregate across sibling assemblies instrumented at 0% would misreport the diluted root figure (65.35%) rather than the assembly this child edits.

| Language | Changed files | Line coverage | Branch coverage | New/changed-code coverage | Disposition | Verdict |
|---|---|---|---|---|---|---|
| C# / .NET (UtilitiesCS assembly) | 37 `.cs` | Baseline: 88.75%; Post-change: 88.75%; Change: +0.0000575 | Baseline: 82.51%; Post-change: 82.51%; Change: 0.000000 | 0 new executable lines (annotation-only); changed-line coverage non-regressing | Above 85% line floor and 75% branch floor; no regression on changed lines | PASS |
| TypeScript | 0 | N/A | N/A | N/A | No changed files on branch | N/A |
| Python | 0 | N/A | N/A | N/A | No changed files on branch | N/A |
| PowerShell | 0 | N/A | N/A | N/A | No changed files on branch | N/A |

C# coverage verdict: PASS. The edited UtilitiesCS assembly reports line coverage 88.75% (>= 85%) and branch coverage 82.51% (>= 75%). Because the change is annotation-only it introduces no new executable lines, so the new-code >= 90% obligation applies to zero new executable lines and the changed-line no-regression requirement holds (branch-rate byte-identical). The CLAUDE.md 80/90 vs `.claude/rules/general-unit-test.md` 85/75 threshold-source difference is a pre-existing repository-level conflict flagged at the epic level; the assembly figures clear all four floors regardless.

## Blocking Findings

None. Policy blocking_count for this artifact: 0.
