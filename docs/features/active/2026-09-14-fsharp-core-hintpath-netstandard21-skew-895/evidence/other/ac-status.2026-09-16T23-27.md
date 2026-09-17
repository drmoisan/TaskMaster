# Acceptance-Criteria Status and Follow-Up Record (Issue #895)

Timestamp: 2026-09-17T01-31
Task: [P5-T6]
WORKTREE-LEAF: agent-a8bc4dc5978785885

No `AC1: NOT MET` through `AC5: NOT MET` line was appended by `[P5-T1]` to `[P5-T5]`: every
criterion took the checked branch on its named evidence.

EXIT_CODE: 0
ExpectedExitCode: 0

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

The `Checked off (delivered)` figure is counted mechanically over `spec.md` lines 422 to 465: five
lines in that range begin `- [x] AC` and none begins `- [ ] AC`. The five criteria sit at lines 422
(AC1), 430 (AC2), 444 (AC3), 450 (AC4) and 457 (AC5), and each of those five lines was verified to
begin with the six characters `- [x] `.

Work mode is `full-bug`, so `spec.md` is the sole acceptance-criteria source and `user-story.md` is
correctly absent from the feature folder. Its absence was verified on disk during `[P0-T2]`.

UNMET: NONE

## Evidence per criterion

- **AC1** — pre-fix `evidence/regression-testing/expect-fail-shape-a.2026-09-16T23-27.md`
  (`EveryFSharpCoreHintPath_SelectsNetstandard20 OUTCOME=Failed` naming `QuickFiler.csproj`,
  `QuickFiler.Test.csproj` and `ToDoModel.csproj` and naming none of the three already-correct
  files, with `SolutionHasExactlySixFSharpCoreHintPaths OUTCOME=Passed`); post-fix
  `evidence/regression-testing/pass-after-shape-a.2026-09-16T23-27.md`
  (`COUNTERS_TOTAL=2 EXECUTED=2 PASSED=2 FAILED=0`).
- **AC2** — `evidence/regression-testing/expect-fail-build.2026-09-16T23-27.md` and
  `evidence/regression-testing/pass-after-build.2026-09-16T23-27.md`, both recording
  `SKIPPED_CORECOMPILE=0` with a per-project compiler-invocation count of 1 for all fifteen
  projects; `evidence/regression-testing/expect-fail-shape-b.2026-09-16T23-27.md`
  (`[QuickFiler]`, `[QuickFiler.Test]` and `[ToDoModel]` failed, positive control passed);
  `evidence/regression-testing/pass-after-shape-b.2026-09-16T23-27.md`
  (`PASSED=16 FAILED=0`).
- **AC3** — `evidence/other/scope-boundary-diff.2026-09-16T23-27.md`: two byte-identical three-line
  numstat outputs against `origin/main` after a fetch, no line for the three untouched files, and
  `OUT-OF-WRITE-SET: NONE`.
- **AC4** — `evidence/qa-gates/loop-closure.2026-09-16T23-27.md` (`LOOP: CLEAN PASS`);
  `evidence/qa-gates/test-final.2026-09-16T23-27.md` (`COUNTERS_FAILED=0` on the measured run,
  `RUNSETTINGS-UNCHANGED: NONE`);
  `evidence/regression-testing/pass-after-bootstrap-namespace.2026-09-16T23-27.md`
  (`NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed`);
  `evidence/other/post-format-sweep.2026-09-16T23-27.md` (`DONOTPARALLELIZE` 0, 0 and 2). The
  criterion's "every test passing" clause is discharged by `COUNTERS_FAILED=0` over all 7311
  results, not by a baseline-subset argument.
- **AC5** — the `AC5 Comment-Only Diff:` heading in
  `evidence/other/scope-boundary-diff.2026-09-16T23-27.md` (`UNSATISFIABLE_COUNT=1`,
  `DISPLAY_NAME_TESTS_COUNT=1`, `BECAUSE_206_COUNT=1`, `NON_COMMENT_CHANGED_LINES=0`), against the
  pre-fix `UNSATISFIABLE_COUNT=2` recorded in
  `evidence/baseline/tree-baseline.2026-09-16T23-27.md`.

## Record, Not Fix:

AfterInstall_DeedleTypeInitializerSucceeds passes with or without the #879 installer now that
QuickFiler.Test deploys the netstandard2.0 flavour; its discriminating power has moved to the
display-name tests, whose negative control is unaffected. Recorded, not fixed.

## Follow-Ups (not opened by this plan):

1. **Latent defect 1 — `ToDoModel.Test/packages.config` omission.** That file carries no
   `FSharp.Core` and no `Deedle` entry, although `ToDoModel.Test/ToDoModel.Test.csproj` lines 92-96
   carry HintPaths for both. The HintPaths resolve today only because five sibling projects restore
   the same package folders. `scripts/vscode/Sync-PackageReferences.ps1` skips a HintPath whose
   package id is absent from the project's own `packages.config`, so the next `FSharp.Core` or
   `Deedle` version bump would leave `ToDoModel.Test` pointing at a folder that no longer exists
   (`CS0006`). Not addressed here; recommended as its own issue.
2. **Latent defect 2 — TFM preference order in `scripts/vscode/Sync-PackageReferences.ps1`.** Lines
   13-19 rank `netstandard2.1` ahead of `netstandard2.0` in the fallback search. For a `net481`
   project that order is inverted, and the script is the only tool in the repository that can write
   a `netstandard2.1` HintPath into a project file. It leaves the six HintPaths alone now that they
   all resolve, so there is no in-scope trigger; a future bump that removed the `netstandard2.0`
   folder could reintroduce the skew. Shape A asserts the flavour rather than mere agreement, so any
   such reintroduction fails at test time. Reordering the list needs accompanying Pester coverage
   under the existing gate, so it is recommended as a separate issue.
3. **Analyzer-package skew handled at `[P0-T3]` — issue #898.** Fifteen first-party project files
   carry `<Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\...">` while every
   `packages.config` pins `3.0.235`, so a `packages.config` restore materialises no `3.0.203`
   folder and every build would fail with `CS0006`. `[P0-T3]` materialised the `3.0.203` package
   into the git-ignored `packages/` directory so the builds in this run could proceed. No project
   file was edited for it and no `<Analyzer Include>` line was touched, per this plan's Non-Goals.
   The skew itself remains open as issue #898.

## Spec Observed Corrections:

Reproduced from `[P0-T1]`.

1. Already applied upstream; recorded as confirmed rather than outstanding. An earlier revision of
   the plan recorded that `spec.md` denied the existence of any `879` folder under `docs/features`.
   `spec.md` has since been corrected: its `### Dependencies or blocked work:` bullet at lines
   257-267 now states that issue #879's active feature folder
   `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/`
   still exists on disk with a fully checked plan and has not yet been archived, and that the
   comment-only edit targets `TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`,
   ordinary shared repository test code outside #879's own feature folder. Re-derived on the
   assigned worktree at `[P0-T1]`: the folder exists, its `plan.2026-09-13T18-22.md` carries zero
   unchecked task lines (the one line matching the task prefix is prose describing the pattern, not
   a task), and its `evidence/other/preflight-clearance.2026-09-14T01-55.md` records the completed
   run. Nothing in this plan wrote into #879's canonical feature-folder state.
2. AC5 (`spec.md` line 463) says a grep of the unfixed file for `unsatisfiable` returns the stale
   sentence at lines 366-367. Re-derived: the token occurred at line 281 (the
   `NegativeControl_WithoutInstall_Netstandard21Throws` summary, which stays true after the fix and
   is retained) and at line 366 (the stale sentence). The pre-fix count was therefore 2, recorded by
   `[P0-T9]`, and the post-fix count is 1, gated by `[P3-T2]` and confirmed by `[P4-T15]`.

## Acceptance

- The artifact exists: yes.
- `Checked off (delivered):` equals the count of `spec.md` lines in the range 422-465 beginning
  `- [x] `: yes, both are 5.
- `Total AC items: 5`: yes.
- The `Record, Not Fix:`, `Follow-Ups (not opened by this plan):` and `Spec Observed Corrections:`
  sections exist: yes.
