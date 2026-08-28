# Phase 0 — Baseline Index (P0-T20)

Timestamp: 2026-08-27T23-36

All paths below are relative to `docs/features/active/itemviewer-surface-defects-489/`.

## Every Phase 0 artifact produced by P0-T1 through P0-T19

| Task | Artifact | Exists |
|---|---|---|
| P0-T1 to P0-T5 | `evidence/baseline/phase0-instructions-read.2026-08-27T23-16.md` | yes |
| P0-T6 | `evidence/baseline/phase0-repo-state.2026-08-27T23-20.md` | yes |
| P0-T7 | `evidence/baseline/phase0-dotnet-tool-restore.2026-08-27T23-20.md` | yes |
| P0-T8 | `evidence/baseline/phase0-nuget-restore.2026-08-27T23-21.md` | yes |
| P0-T9 | `evidence/baseline/phase0-csharpier-check.2026-08-27T23-21.md` | yes |
| P0-T10 | `evidence/baseline/phase0-u1-designer-format-gate.2026-08-27T23-22.md` | yes |
| P0-T11 | `evidence/baseline/phase0-analyzer-build.2026-08-27T23-26.md` | yes |
| P0-T11 log | `evidence/qa-gates/phase0-analyzer-build.2026-08-27T23-22.msbuild.txt` | yes |
| P0-T12 | `evidence/baseline/phase0-nullable-build.2026-08-27T23-27.md` | yes |
| P0-T13 | `evidence/baseline/phase0-vstest-quickfiler.2026-08-27T23-28.md` | yes |
| P0-T14 | `evidence/baseline/phase0-repo-coverage.2026-08-27T23-30.md` | yes |
| P0-T15 | `evidence/baseline/phase0-file-line-counts.2026-08-27T23-31.md` | yes |
| P0-T16 | `evidence/baseline/phase0-excludefromcodecoverage-count.2026-08-27T23-31.md` | yes |
| P0-T17 | `evidence/baseline/phase0-upstream-landing-check.2026-08-27T23-32.md` | yes |
| P0-T18 | `evidence/baseline/phase0-anchor-rederivation.2026-08-27T23-33.md` | yes |
| P0-T19 | `evidence/baseline/phase0-csproj-block-tails.2026-08-27T23-34.md` | yes |

Fifteen markdown artifacts under `evidence/baseline/` and one `.msbuild.txt` log under
`evidence/qa-gates/`. Every path resolves to a file that exists on disk.

## Item-by-item confirmation of the ten quantities AC1, AC2 and AC3 require

### AC1 — the seven baseline quantities

| # | Quantity | Artifact | Value recorded | Usable as a baseline? |
|---|---|---|---|---|
| 1 | csharpier check result | `phase0-csharpier-check.2026-08-27T23-21.md` | `EXIT_CODE: 0`, empty `BaselineUnformattedSet:`, 1543 files checked | **Yes** |
| 2 | analyzer-build warning count | `phase0-analyzer-build.2026-08-27T23-26.md` | `BaselineAnalyzerWarningCount: 0` | **No** — build FAILED, exit 1, 10 CS0006; no analyzer ran over this feature's files |
| 3 | nullable-build warning count | `phase0-nullable-build.2026-08-27T23-27.md` | `BaselineNullableWarningCount: 0` | **No** — build FAILED, exit 1, same 10 CS0006, zero CS86xx |
| 4 | vstest passed / failed / skipped | `phase0-vstest-quickfiler.2026-08-27T23-28.md` | all three `UNMEASURED` | **No** — assembly absent, no run, no `p0-t13.trx` |
| 5 | repository-wide line coverage | `phase0-repo-coverage.2026-08-27T23-30.md` | `BaselineLineRate: 0.13296151701059677`, `BaselineLinesValid: 8965` | **No** — 1 of roughly 10 test assemblies discovered |
| 6 | line count of every file this feature will touch | `phase0-file-line-counts.2026-08-27T23-31.md` | 26 integer rows | **Yes** |
| 7 | repository-wide exclusion-attribute count | `phase0-excludefromcodecoverage-count.2026-08-27T23-31.md` | `BaselineExcludeAttributeCount: 261` | **Yes** |

### AC2 — the U1 answer

`phase0-u1-designer-format-gate.2026-08-27T23-22.md` records **Branch A** and answers U1: CSharpier
1.2.6 skips `*.Designer.cs` by filename through generated-file detection. Proof: a single-file
csharpier check on each reports `Checked 0 files`, not `Checked 1 files`, against a 111-column line at
`ItemViewer.Designer.cs:256` and a 110-column line at `ItemViewerExpanded.Designer.cs:274` that a
100-column print width would otherwise re-wrap. The artifact records, and a name-only diff against the
branch base confirms, that **no Designer file edit had occurred** when it was written.

### AC3 — upstream landing and anchor re-derivation

`phase0-upstream-landing-check.2026-08-27T23-32.md` records `Upstream484Landed: true` and
`Upstream444Landed: true` from a fresh grep of `QuickFiler/` returning **16** matches.
`phase0-anchor-rederivation.2026-08-27T23-33.md` records a `member = file:line` row for every anchor in
`QfcItemController.EventWiring.cs`, `.FocusAndTheme.cs`, `.MailActions.cs`, `.FolderHandling.cs`,
`.Navigation.cs` and `.ViewerSetup.cs`, re-derived against the actual branch head.

All ten items resolve to a named artifact path that exists on disk.

## Phase 0 checklist state, and why four tasks are unchecked

Sixteen of the twenty Phase 0 tasks are checked. Four are recorded but **left unchecked** because
their acceptance conditions are not met:

- **P0-T11** — requires `EXIT_CODE: 0`; observed `1`.
- **P0-T12** — requires `EXIT_CODE: 0`; observed `1`. Its second conjunct, that the command line
  contains neither the nullable property nor the Build target, is met.
- **P0-T13** — requires three recorded integers and an existing `p0-t13.trx`; neither is satisfied.
- **P0-T14** — its three literal conjuncts are satisfied, but the figures are not a repository-wide
  baseline, and recording them as one would make P11-T8 clause (d) unsatisfiable for a healthy run.

All four trace to a single root cause: an **inherited analyzer version skew**. The Analyzer Include
HintPaths in the project files name Meziantou.Analyzer 3.0.156 and Roslynator.Analyzers 4.16.0, while
packages.config and `packages/` carry 3.0.174 and 4.16.1. `msbuild /t:Rebuild` therefore fails with
CS0006 on `UtilitiesCS` and `VBFunctions`, every dependent project fails transitively, and the
`/t:Rebuild` clean leaves the test assemblies absent. A name-only diff of every project file against
the branch base returns zero paths, so the condition is pre-existing and belongs to the repository,
not to this feature. Commit `46ca9210 fix(build): repair NuGet upgrade fallout blocking CI` is the
precedent repair for the previous bump. No remedy was applied: Phase 0 forbids project-file edits, and
`UtilitiesCS.csproj` and `VBFunctions.csproj` lie outside this feature's scope lock.

The four measurements must be re-taken once that skew is repaired, before any comparison in Phase 11
or any Phase 12 check-off of AC1 can be honest.

Output Summary: Sixteen artifacts exist under `evidence/baseline/` and `evidence/qa-gates/`, one for
every command-bearing Phase 0 task, and all ten items AC1, AC2 and AC3 require resolve to a named
artifact path that exists on disk. Four of the seven AC1 quantities — the analyzer warning count, the
nullable warning count, the vstest passed/failed/skipped triple, and the repository-wide coverage
percentage — are recorded but are **not usable as baselines**, because an inherited analyzer version
skew fails the build with CS0006 and leaves the test assemblies absent after the Rebuild clean. The
three usable quantities are the csharpier result (clean, 1543 files, empty unformatted set), the 26
per-file line counts, and the exclusion-attribute count of 261. The AC2 U1 answer is recorded as
Branch A with falsifiable proof, and the AC3 upstream-landing booleans are both true with every anchor
re-derived. P0-T11, P0-T12, P0-T13 and P0-T14 are left unchecked under the fail-closed evidence rule.

---

## Amendment 2026-08-28T00-18 — the four deferred baselines were re-taken and now pass

Timestamp: 2026-08-28T00-18

The inherited analyzer version skew described above was cleared **for this worktree only**, by
placing `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` under the worktree's
gitignored `packages/` directory. No `.csproj`, no `packages.config` and no `<Analyzer Include>`
entry was edited; zero tracked files changed and the skew itself remains pre-existing repository
state that is out of scope for this feature. P0-T11 through P0-T14 were then re-run. All four now
meet their acceptance and are checked off in the plan.

| Task | Superseding artifact | Result |
|---|---|---|
| P0-T11 | `evidence/baseline/phase0-analyzer-build.2026-08-28T00-11.md` | `EXIT_CODE: 0`, `BaselineAnalyzerWarningCount: 5`, 0 errors |
| P0-T11 log | `evidence/qa-gates/phase0-analyzer-build.2026-08-28T00-09.msbuild.txt` | 5728 lines, not gitignored |
| P0-T12 | `evidence/baseline/phase0-nullable-build.2026-08-28T00-12.md` | `EXIT_CODE: 0`, `BaselineNullableWarningCount: 5`, 0 CS86xx |
| P0-T13 | `evidence/baseline/phase0-vstest-quickfiler.2026-08-28T00-14.md` | `EXIT_CODE: 0`, 1099 passed, 0 failed, 0 skipped |
| P0-T13 TRX | `evidence/baseline/p0-t13.trx` | sanitised; no account or machine name |
| P0-T14 | `evidence/baseline/phase0-repo-coverage.2026-08-28T00-17.md` | 9 assemblies, line rate 0.7051419519922018 at `lines-valid=82070` |

The six artifacts listed in the table above are added to the Phase 0 artifact inventory. The
superseded artifacts (`phase0-analyzer-build.2026-08-27T23-26.md`,
`phase0-nullable-build.2026-08-27T23-27.md`, `phase0-vstest-quickfiler.2026-08-27T23-28.md`,
`phase0-repo-coverage.2026-08-27T23-30.md`, and the
`evidence/qa-gates/phase0-analyzer-build.2026-08-27T23-22.msbuild.txt` log) are **retained** as the
audit record of the blocked first attempt and are not deleted.

### Revised AC1 quantity table

| # | Quantity | Value now recorded | Usable as a baseline? |
|---|---|---|---|
| 1 | csharpier check result | `EXIT_CODE: 0`, empty unformatted set, 1543 files | **Yes** (unchanged) |
| 2 | analyzer-build warning count | `BaselineAnalyzerWarningCount: 5` | **Yes** — build succeeded, 0 errors, 0 `Skipping target "CoreCompile"` |
| 3 | nullable-build warning count | `BaselineNullableWarningCount: 5` | **Yes** — build succeeded, 0 errors, 0 CS86xx |
| 4 | vstest passed / failed / skipped | 1099 / 0 / 0 | **Yes** |
| 5 | repository-wide line coverage | `0.7051419519922018` at `lines-valid=82070`, 9 of 9 assemblies | **Yes** |
| 6 | line count of every file this feature will touch | 26 integer rows | **Yes** (unchanged) |
| 7 | repository-wide exclusion-attribute count | `261` | **Yes** (unchanged) |

All seven AC1 quantities are now usable baselines. Two carry a caveat that Phase 11 must honour and
that is stated in full in the artifacts themselves: the repo-wide coverage denominator is the raw
`dotnet-coverage` merge, because the script threw at `Invoke-MSTestWithCoverage.ps1:236` on one
pre-existing `UtilitiesCS.Test` failure before its Koverage post-processing step; and
`BaselineRepoFailed:` is `1`, that same load-sensitive
`ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`
failure in an assembly this feature is forbidden to touch.
