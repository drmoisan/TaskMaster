# P0-T5 — Toolchain and coverage baseline adopted by tree identity (cycle 1)

Timestamp: 2026-08-28T03-42
Task: [P0-T5]
Command: git status --porcelain && git diff --name-only 7ad2bd17..HEAD -- . ':(exclude)docs/**'
EXIT_CODE: 0

## Why no gate is re-executed here

The plan adopts the feature's own final-QC measurements as this remediation's baseline rather than
re-running them, on the ground of **tree identity**. Three facts establish that, and all three were
verified rather than assumed:

1. **The tree is clean at REM_BASE.** P0-T2 ran `git status --porcelain` and it printed zero lines.
   Nothing is uncommitted, so the files on disk are exactly the files at the recorded commit.
2. **The feature's final-QC gates were executed and committed at this same code tree.** They are
   recorded in `FEATURE/evidence/qa-gates/p11-t14-final-commit.2026-08-28T02-35.md` (the Phase 11
   commit record) and `p11-t15-clean-tree.2026-08-28T02-35.md` (the end-of-loop clean-tree lock,
   `EXIT_CODE: 0`, zero porcelain lines over all nineteen C# project directories plus `scripts/` and
   `coverage/`).
3. **No code changed between those gates and REM_BASE.** `git diff --name-only ac4a996a..HEAD`
   restricted to everything outside `docs/` is **empty**, and so is
   `git diff --name-only 7ad2bd17..HEAD` under the same restriction. Every commit between the Phase 11
   gate commits and REM_BASE is documentation or evidence only. The compiled inputs — every `.cs`,
   `.csproj`, `.props`, `.targets` and `.config` in the solution — are byte-identical to the tree
   those gates measured.

A rebuild or a re-run over a byte-identical input tree can only reproduce the recorded figures, up to
the instrumentation jitter documented below. Re-executing would consume roughly twenty minutes of
solution rebuilds and test runs to restate numbers already on record, so the recorded figures are
adopted, each with a citation to the artifact that produced it.

## Adopted values

### Analyzer

BaselineAnalyzerWarningCount: 5

Source: `FEATURE/evidence/qa-gates/p11-t4-analyzer-build.2026-08-28T02-18.md`
(`EXIT_CODE: 0`, `FinalAnalyzerWarningCount: 5`, `Build succeeded.` with `5 Warning(s)` and
`0 Error(s)`, over 50 `(Rebuild target)` entries).
Command that produced it:
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:normal /fl ...`

All five warnings are the pre-existing non-Roslyn `System.Reactive` `packages.config` advisory, not
analyzer diagnostics. The source artifact notes the warning text appears 10 times in the log because
MSBuild prints each warning once inline and once in the end-of-build summary; the **deduplicated**
count is 5, and 5 is the figure adopted. P1-T2, P2-T2 and P4-T3 all compare their deduplicated counts
against this 5.

### Nullable

Baseline nullable build EXIT_CODE: 0

Source: `FEATURE/evidence/qa-gates/p11-t6-nullable-build.2026-08-28T02-20.md` (`EXIT_CODE: 0`).
Command that produced it:
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
— `/t:Rebuild`, spaced platform, and **no** `/p:Nullable=enable`, which is the command shape P4-T5
must reproduce.

### Scoped test gate — `QuickFiler.Test`

- BaselinePassed: 1121
- BaselineFailed: 0
- BaselineSkipped: 0

Source: `FEATURE/evidence/qa-gates/p11-t7-vstest-quickfiler.2026-08-28T02-22.md`
(`EXIT_CODE: 0`, `FinalPassed: 1121`, `FinalFailed: 0`, `FinalSkipped: 0`).
Command that produced it:
`& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p11-t7.trx" /ResultsDirectory:...`

This remediation adds exactly one test method and changes no other test, so the P4-T6 gate expects
`FinalPassed: 1122` — 1121 plus one — with failed and skipped both still 0.

### Repository-wide coverage

- BaselineLineRate: 0.851567
- BaselineBranchRate: 0.792213
- BaselineLinesCovered: 54416
- BaselineLinesValid: 63901
- BaselineRepoPassed: 6741
- BaselineRepoFailed: 0
- BaselineRepoSkipped: 0

Source: `FEATURE/evidence/qa-gates/p11-t8-repo-coverage.2026-08-28T02-28.md`
(`FinalLineRate: 0.851567`, `FinalBranchRate: 0.792213`, `FinalLinesValid: 63901`,
lines-covered 54416 in its run-comparison table, `FinalRepoPassed: 6741`, `FinalRepoFailed: 0`,
`FinalRepoSkipped: 0`).
Command that produced it:
`pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml`

**Document shape.** These figures are in the **Koverage-post-processed** shape the canonical runner
emits on a passing run: repo-relative backslash `<class filename>` attributes, first-party packages
only, nine packages. The raw-shape figures from an earlier baseline — `line-rate 0.7051419519922018`
at `lines-valid 82070` — are **not** a comparable basis and must not be compared against these. P4-T7
compares like for like, in the post-processed shape only.

**Instrumentation jitter, as measured at the source.** The source artifact ran the command twice over
the identical tree and recorded:

| Run | line-rate | branch-rate | lines-covered | lines-valid |
|---|---|---|---:|---:|
| 1 | 0.851599 | 0.792151 | 54418 | 63901 |
| 2 | 0.851567 | 0.792213 | 54416 | 63901 |

The denominator is **identical** across both runs at 63901; the numerator moves by **2 covered
lines**, which is **0.000032** of line rate. That is `dotnet-coverage`'s own instrumentation
non-determinism. The adopted `BaselineLineRate: 0.851567` is the **lower** of the two observed runs,
so the P4-T7 floor is the conservative one. The +/-0.000032 band is what P4-T7's single-re-execution
clause is scoped to.

## Acceptance

| P0-T5 condition | Result |
|---|---|
| The artifact exists | **Yes** — this file |
| Every listed numeric value present, no placeholders | **Yes** — 5; 0; 1121 / 0 / 0; 0.851567 / 0.792213 / 54416 / 63901; 6741 / 0 / 0 |
| Each value carries a citation to its source artifact | **Yes** — four cited artifacts, each named with the field it supplies |
| Tree-identity justification stated | **Yes** — clean porcelain at REM_BASE, gates committed at the same code tree, empty non-`docs/` diff between them |

Output Summary: The feature's own final-QC measurements are adopted as this remediation's baseline by
tree identity — the tree is clean at REM_BASE, the Phase 11 gates were executed and committed at the
same code tree, and the diff between them restricted to everything outside `docs/` is empty, so the
compiled inputs are byte-identical. Adopted values: `BaselineAnalyzerWarningCount: 5` (all five the
pre-existing `System.Reactive` `packages.config` advisory, deduplicated from 10 log occurrences);
nullable build `EXIT_CODE: 0`; `QuickFiler.Test` `BaselinePassed: 1121`, `BaselineFailed: 0`,
`BaselineSkipped: 0`; repository-wide `BaselineLineRate: 0.851567`, `BaselineBranchRate: 0.792213`,
`BaselineLinesCovered: 54416`, `BaselineLinesValid: 63901`, `BaselineRepoPassed: 6741`,
`BaselineRepoFailed: 0`, `BaselineRepoSkipped: 0`, all in the Koverage-post-processed shape with a
measured run-to-run jitter of +/-2 covered lines (+/-0.000032 line rate) on a stable 63901
denominator. The raw-shape 0.7051 at 82070 is explicitly not a comparable basis. `EXIT_CODE: 0`.
