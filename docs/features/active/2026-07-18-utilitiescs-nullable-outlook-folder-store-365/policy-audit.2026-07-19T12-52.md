# Policy Compliance Audit — utilitiescs-nullable-outlook-folder-store (Issue #365)

- Timestamp: 2026-07-19T12-52
- Reviewer: feature-reviewer
- Work mode: full-feature (from `issue.md` marker `- Work Mode: full-feature`)
- Branch: `feature/utilitiescs-nullable-outlook-folder-store-365`
- Base (resolved merge-base): `dffadd5a` (epic integration tip, PR #382 merge). Verified `git merge-base HEAD dffadd5a == dffadd5a`.
- Head: `e00a2c21`
- Audit scope: the full branch diff `dffadd5a..HEAD` (63 production `.cs` files, ~378 insertions / 297 deletions, plus feature docs, evidence, and one evidence `.gitignore`). C# is the only changed source language.

## Executive Summary

The change is annotation-only per-file `#nullable enable` remediation across `UtilitiesCS/OutlookObjects/Folder/` (including `MsgToMime/`) and `UtilitiesCS/OutlookObjects/Store/`. No behavior changes, no refactors, no new features, no new tests, no project/build file edits. Independent verification confirms the change is confined to nullable annotations (`?`), justified null-forgiving operators (`!`), non-null field/property initializers, and behavior-neutral null-guard refinements.

Overall policy verdict: **PASS**, with one non-blocking dispositioned coverage finding (pre-existing legacy VSTO/COM coverage floor) and a set of informational residuals for the maintainer.

## Rejected Scope Narrowing

No caller instruction attempted to narrow the branch-diff audit scope. Two caller clarifications were evaluated and found legitimate (not narrowing):

- The vendored `SVGControl` `CS0649` errors are outside this branch's diff. `SVGControl` files do not appear in `dffadd5a..HEAD` (verified via `git diff --name-only`), so those errors are definitionally pre-existing and belong to sibling feature #368. Excluding them is a correct scope statement, not a narrowing of this feature's own coverage.
- The absence of required CI checks on a PR targeting `epic/utilitiescs-nullable-remediation-integration` is by design for epic children (CI runs only on PRs targeting `main`/`development`). This is a repository CI-configuration fact, not a suppressed defect.

The full branch diff for every changed language was audited.

## Evidence Location Compliance

All evidence artifacts are written under the canonical `docs/features/active/2026-07-18-utilitiescs-nullable-outlook-folder-store-365/evidence/{baseline,qa-gates,regression-testing,other}/` tree. A scan of `dffadd5a..HEAD` for files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` returned zero matches. No evidence-location violations. (`validate_evidence_locations.py` is not present in this worktree; the scan was performed with `git diff --name-only` filtered on the prohibited prefixes.)

## Toolchain Compliance (C# Code Change Policy, CLAUDE.md order)

The reviewer inspects pre-existing evidence artifacts rather than re-running the toolchain (no-mutation review model; msbuild/vstest are not on the review shell PATH).

| Stage | Command (from evidence) | Evidence artifact | Result |
|---|---|---|---|
| Format (csharpier) | `csharpier .` | `evidence/qa-gates/final-csharpier.md` | Reported clean |
| Lint / analyzers | `msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `evidence/qa-gates/final-analyzers.md` | Reported clean for cluster |
| Type-check / nullable gate | `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`) + scoped `UtilitiesCS.csproj /t:Rebuild ... /p:BuildProjectReferences=false` | `evidence/qa-gates/final-nullable-pragma-gate.md` | Zero CS86xx/CS87xx for the 63+18 cluster files; see AC1 |
| Test | `Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test` | `evidence/qa-gates/final-tests-coverage.md` | 4511/4511 passed, exit 0 |

Nullable-gate substitute-signal legitimacy (independently reasoned): the full-solution `/t:Rebuild /p:TreatWarningsAsErrors=true` exits 1 solely from 2 pre-existing `CS0649` in `SVGControl` (a `ProjectReference` that fails the parallel build before `UtilitiesCS` compiles) plus 15 pre-existing `CS0618`/`CS0168` in non-Folder/Store files. Because `SVGControl` is not in this branch's diff, the branch cannot have introduced those errors. Roslyn reports all diagnostics in a single pass, so the scoped `UtilitiesCS` rebuild with `BuildProjectReferences=false` is a valid CS86xx/CS87xx signal for the cluster and not a mechanism to hide a regression. The baseline artifact (`evidence/baseline/baseline-nullable-pragma-gate.md`) documents the identical SVGControl/CS0618 pre-existing condition at `dffadd5a`.

## File Size Limit (500-line rule)

Independently verified line counts at HEAD:

- `FolderPredictor.cs` — 983 lines. Pre-existing >500 exception (974 at baseline). Not split (annotation-only scope). Documented in spec as a pre-existing policy exception.
- `FolderScorer.cs` — 664 lines. Pre-existing >500 exception (663 at baseline). Not split.
- `FolderWrapper .cs` — 532 lines. Pre-existing >500 exception (531 at baseline). Not split; filename (with literal trailing space) unchanged.
- `OutlookFolderNotificationSink.cs` — 499 lines. Under limit; not flagged.

Disposition: the three >500-line files are pre-existing exceptions the spec, user-story Non-Goals, and CLAUDE.md-consistent precedent (`#363` `ArrayExtensions.cs` 544, `#364` `PrettyPrint.cs` 677) all direct not to split within annotation-only scope. Not a new violation. PASS (no new file exceeds 500 lines).

## Unit Test Policy Compliance

No test files were added or modified (`git diff --name-only` filtered on `*.Test*`/`Fakes` returned none). The feature explicitly must not add tests around COM/VSTO/WinForms coverage-exempt classes (CLAUDE.md exemption; spec Behavior section). The existing `UtilitiesCS.Test` suite (4511 tests) passes unchanged. No temporary files, no external dependencies, no new mocks. PASS.

## Coverage Verification (mandatory per changed language)

Languages with changed files in the branch diff: **C# only.** PowerShell, Python, TypeScript have zero changed files (no `.ps1`/`.psm1`/`.py`/`.ts`/`.tsx` in the diff), so no coverage row is required for them.

Canonical C# coverage artifact path per the coverage table is `artifacts/csharp/coverage.xml`. That canonical artifact is **absent** in this worktree. A whole-assembly Cobertura report exists at `evidence/qa-gates/final-coverage.cobertura.xml` (gitignored due to ~10MB size; present locally) and was parsed independently by the reviewer.

Independently parsed figures (from the Cobertura root element):

- Repo-wide C# line coverage: 65.31% (line-rate 0.653084; 67653/103590 lines).
- Repo-wide C# branch coverage: 61.35% (branch-rate 0.613535; 15693/25578 branches).
- First-party `UtilitiesCS` package within the same report: 88.75% line / 82.55% branch (above the 85%/75% floors).
- Baseline (P0-T6): 65.30% line / 61.32% branch. Post-change: 65.31% line / 61.35% branch. Change: +0.01% line, +0.03% branch. Disposition: no regression (covered lines +32). Evidence: `evidence/qa-gates/final-coverage-delta.md`, reconciled to the Cobertura root.
- New/changed-code coverage: 96.97% (96 of 99 changed executable lines covered). The 3 uncovered changed lines are annotation-only (`?`/`!`) edits to statements that were already uncovered at baseline (Bayesian async path; two WinForms UI handlers) — no covered line became uncovered.

C# coverage verdict: **FAIL.** Two grounds require this literal verdict: (1) the canonical `artifacts/csharp/coverage.xml` is absent, which the mandatory rule treats as FAIL; (2) repo-wide C# line coverage 65.31% and branch coverage 61.35% sit below the 85%/75% repository floors.

Disposition of the C# coverage FAIL — **non-blocking for this feature.** Grounds: the sub-floor repo-wide figure is entirely pre-existing legacy VSTO/COM/WinForms debt not introduced by this annotation-only change; the CLAUDE.md testable-denominator exemption (ratified by the maintainer, tracked in `feature/csharp-coverage-uplift`) applies to this codebase; the first-party `UtilitiesCS` package that actually contains the changed Folder/Store code reads 88.75% line / 82.55% branch, above both floors; changed-line coverage is 96.97% with zero regression; and the feature adds no new executable code and no new files. Raising legacy repo-wide coverage is not achievable within annotation-only scope and is not a defect in this change. This FAIL is therefore recorded for transparency and does not count toward `blocking_count`. The absent canonical `artifacts/csharp/coverage.xml` is a procedural gap tracked as a maintainer residual; the equivalent numeric evidence is present in the feature Cobertura.

## Standing-Rules Cross-Check

- `.claude/rules/*` not edited (verified: no `.claude/rules` paths in diff). PASS.
- No project-level `<Nullable>` element introduced; no `/p:Nullable=enable` global flag used (AC2). PASS.
- No `System.Diagnostics.CodeAnalysis` post-condition attributes and no new `record`/`record struct`/`init` (AC6) — independently confirmed on the diff's added lines. PASS.
- Benchmark-baseline and CI-workflow rules: no `scripts/benchmarks/**` or `.github/workflows/**` files in the diff. Not applicable to this change (zero changed files in those areas).

## Verdict

Policy compliance: **PASS** overall. One dispositioned non-blocking C# coverage FAIL (pre-existing legacy-debt floor + absent canonical artifact). Blocking policy findings: 0.
