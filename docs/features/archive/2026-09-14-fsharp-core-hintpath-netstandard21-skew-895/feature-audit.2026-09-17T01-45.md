# Feature Audit — Issue #895 (FSharp.Core HintPath netstandard2.1 skew)

- **Issue:** #895
- **Work Mode:** `full-bug` (marker at `issue.md:13`)
- **AC source (sole, authoritative):** `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md`, section `## Acceptance Criteria`
- **Branch:** `bug/fsharp-core-hintpath-netstandard21-skew-895` at `7b865c4daaf40f396221ed10dc2a3287a8cd42d5`
- **Base:** `origin/main` at `91746d2e4776a59ee1db1856c5c490a009c4958b` after explicit fetch; merge base identical
- **Reviewer:** feature-review agent
- **Timestamp:** 2026-09-17T01-45
- **Disposition:** **PASS — 5 of 5 acceptance criteria verified. 0 blocking findings. 0 PARTIAL.**

---

## Scope and Baseline

- **Baseline (merge base):** `91746d2e`. At that commit the six FSharp.Core HintPaths split three/three
  (`QuickFiler`, `QuickFiler.Test`, `ToDoModel` on `netstandard2.1`; `UtilitiesCS`, `UtilitiesCS.Test`,
  `ToDoModel.Test` on `netstandard2.0`), recorded by the P0-T9 census (`NS21=1 NS20=0` × 3, `NS21=0 NS20=1`
  × 3) and re-derived here from the base blobs via `git diff` (each removed line carries
  `lib\netstandard2.1`).
- **Head:** `7b865c4d`, seven commits ahead. Footprint re-derived by `git diff --numstat origin/main...HEAD`:
  46 files; 7 source paths (4 `.csproj`, 3 `.cs` under `TaskMaster.Test/Bootstrap/`), 32 feature-folder
  documents, 7 agent-memory files from the preparation commit `f1c53f9b8`.
- **AC source resolution.** `issue.md:13` records `- Work Mode: full-bug`. Per the
  `acceptance-criteria-tracking` skill, `full-bug` resolves to `spec.md` only. **No `user-story.md` exists
  and none should exist**; its absence is correct for this work mode.
- **AC identification.** Grep over `spec.md` for `^- \[x\] AC` = **5** (lines 422, 430, 444, 450, 457);
  `^- \[ \] AC` = **0**. The checkbox items in `issue.md` (`Impact / Severity`, `Proposed Fix`, `Next Step`)
  are template selectors and are not acceptance criteria under `full-bug`. No criterion was added,
  reworded, renumbered or reordered by this review; the `spec.md` diff since the feature-docs commit
  `91e2e951a` consists of exactly the five `- [ ]` → `- [x]` flips (re-derived).
- **Verification basis.** **Re-derived** = measured by this reviewer on the execution worktree (git,
  grep, file hashing, `PEReader`, on-disk TRX and build logs, Cobertura root element).
  **Evidence-attested** = read from a committed evidence artifact.
- **Environment notes.** The execution worktree was clean at review start; `coverage/`, `TestResults/`
  and `artifacts/` are git-ignored and hold the raw documents this review read directly.

---

## Acceptance Criteria Inventory

| AC | `spec.md` line | Subject | Pre-fix observation required | Checked at review start |
|---|---|---|---|---|
| AC1 | 422 | Exactly six HintPaths, all `lib\netstandard2.0`; Shape A tests | Yes (flavour test failing, count passing) | `[x]` |
| AC2 | 430 | Fifteen deployed copies reference `netstandard 2.0.0.0`, none 2.1.0.0; positive control; `/t:Rebuild` with no skipped CoreCompile | Yes (three rows failing, control passing) | `[x]` |
| AC3 | 444 | Three untouched HintPaths byte-identical; three edited files differ on exactly one line each vs `origin/main` | No (containment) | `[x]` |
| AC4 | 450 | Full C# toolchain single clean pass; every test passing under Workers=0/ClassLevel; no serialisation added; evidence under `evidence/qa-gates` | No (non-regression) | `[x]` |
| AC5 | 457 | `<remarks>` on `ProbeApplicationBase` corrected; comment-only diff; no change to the line 206-207 message | Yes (`unsatisfiable` grep pre-fix) | `[x]` |

---

## Acceptance Criteria Evaluation

| AC | Verdict | Basis |
|---|---|---|
| AC1 | **PASS** | Re-derived (census, code) + evidence-attested (expect-fail / pass-after) + TRX read directly |
| AC2 | **PASS** | Re-derived (hashes, PEReader, build logs) + evidence-attested (expect-fail / pass-after) |
| AC3 | **PASS** | Re-derived (anchored numstat, two-dot and three-dot) |
| AC4 | **PASS** | Re-derived (build logs, Cobertura root, runsettings diff, DoNotParallelize census) + evidence-attested |
| AC5 | **PASS** | Re-derived (`-U0` diff, non-comment line count, token counts) |

**5 PASS / 0 PARTIAL / 0 FAIL / 0 UNVERIFIED.**

### AC1 — Six HintPaths, all netstandard2.0; Shape A tests — **PASS**

- **Post-fix state, re-derived.** Grep over every `*.csproj` under the worktree root (excluding `.claude`,
  `packages`, `bin`, `obj`, `.git`) for `FSharp.Core.dll` returns exactly **six** HintPath lines
  (`QuickFiler:52`, `QuickFiler.Test:259`, `ToDoModel:42`, `ToDoModel.Test:96`, `UtilitiesCS:70`,
  `UtilitiesCS.Test:599`), every one ending `lib\netstandard2.0\FSharp.Core.dll`.
- **Tests exist as named, re-derived.** `FSharpCoreHintPathAlignmentTests.cs`: `SolutionHasExactlySixFSharpCoreHintPaths`
  (:154) and `EveryFSharpCoreHintPath_SelectsNetstandard20` (:178). The walk (:83-138) skips dot-prefixed
  directories (which covers `.git` and `.claude`) plus `packages`, `bin`, `obj`, `node_modules` (:29-35),
  enumerates from the repository root found by `FindRepositoryRoot` (:47), and asserts count == 6 (:166) and
  suffix `\lib\netstandard2.0\FSharp.Core.dll` ordinal-ignore-case (:189-195, :203).
- **Pre-fix observation, evidence-attested and corroborated on disk.**
  `evidence/regression-testing/expect-fail-shape-a.2026-09-16T23-27.md`: `COUNTERS_TOTAL=2 EXECUTED=2 PASSED=1 FAILED=1`,
  flavour test `Failed`, count test `Passed`, `EXIT_CODE: 1` / `ExpectedExitCode: 1`. This reviewer read
  `TestResults/p1-t6/p1-t6.trx` directly: the `<Message>` names `QuickFiler.Test\QuickFiler.Test.csproj`,
  `QuickFiler\QuickFiler.csproj` and `ToDoModel\ToDoModel.csproj`, each with the `lib\netstandard2.1` value,
  and names none of the three correct files.
- **Pass-after, evidence-attested.** `pass-after-shape-a`: `PASSED=2 FAILED=0`, `EXIT_CODE: 0`. Also present
  and passing in the P4-T4 namespace run and the P4-T9 full suite.
- **Exclusion-list note.** The implementation's dot-prefix rule is a superset of the criterion's literal
  `.git, .claude` list; no project file lives under any dot-prefixed directory, and the count assertion would
  expose an over-exclusion. Graded PASS; recorded as a Low code-review note.

### AC2 — Fifteen deployed copies reference netstandard 2.0.0.0; positive control; real rebuild — **PASS**

- **Rebuild non-vacuity, re-derived.** Over `coverage/logs/p1-t5-build.txt` (expect-fail build) and
  `coverage/logs/p4-t1-build.txt` (pass-after build): `Skipping target "CoreCompile"` = **0** in each;
  `^\s+0 Error(s)$` = 1 in each; for every one of the fifteen enumerated projects exactly one `csc.exe`
  command line carries `/out:obj\Debug\<name>.dll` (plus one `BuildResponseFile =` echo, which is the
  source of the executor's `CSC_OUT=2`). Both were `/t:Rebuild` whole-solution builds.
- **Tests exist as named, re-derived.** `FSharpCoreDeployedIdentityTests.cs`: `[DataTestMethod]` at :94 with
  fifteen `[DataRow]`s (:95-152) naming exactly the fifteen directories in the criterion, method
  `DeployedFSharpCore_ReferencesNetstandard20` (:153); positive control
  `Detector_OnPackageNetstandard21Binary_Reports21` (:209). `ReadAssemblyReferences` (:44-72) uses
  `PEReader` / `MetadataReader.AssemblyReferences`, so no assembly is loaded. Assertions: the single
  `netstandard` reference equals 2.0.0.0 (:166-178) and no reference has version 2.1.0.0 (:180-192).
- **Pre-fix observation, evidence-attested.** `expect-fail-shape-b`: `COUNTERS_TOTAL=16 EXECUTED=16 PASSED=13 FAILED=3`;
  failed rows `[QuickFiler]`, `[QuickFiler.Test]`, `[ToDoModel]` (matched by ordinal `Contains`), each
  message containing `2.1.0.0`; `Detector_OnPackageNetstandard21Binary_Reports21 OUTCOME=Passed`;
  `Additional Failing Rows: NONE` (recorded as an observed build-order outcome, as the criterion allows).
- **Pass-after, evidence-attested.** `pass-after-shape-b`: `PASSED=16 FAILED=0`, all fifteen rows and the
  control passed.
- **Independent post-state confirmation, re-derived (check-only, no test run).** SHA-256 of
  `<dir>/bin/Debug/FSharp.Core.dll` for all fifteen directories equals the package's
  `lib/netstandard2.0/FSharp.Core.dll` hash (`9C29FE3DB01726CF…`); none equals the `lib/netstandard2.1`
  hash (`3882D154D637EFBA…`). Direct `PEReader` reads: the netstandard2.0 package binary and the deployed
  `QuickFiler.Test` and `TaskMaster` copies reference `netstandard=2.0.0.0`; the netstandard2.1 package
  binary references `netstandard=2.1.0.0`, so the control's premise holds. `SVGControl`, `SVGControl.Test`
  and `VBFunctions` hold no copy, matching the research.
- **Plan-literal note (not an AC clause).** The plan's per-project `CSC_OUT=1` literal is unsatisfiable at
  this MSBuild verbosity (two lines carry the token per compilation); the criterion text itself requires only
  "no Skipping target CoreCompile line", which is met. See policy audit W-2.

### AC3 — Three untouched HintPaths byte-identical; three edited files differ on exactly one line — **PASS**

- **Re-derived after `git fetch origin main`.** `git diff --numstat origin/main...HEAD -- <six files>`
  prints exactly three lines: `1 1 QuickFiler.Test/QuickFiler.Test.csproj`, `1 1 QuickFiler/QuickFiler.csproj`,
  `1 1 ToDoModel/ToDoModel.csproj`; no line for `UtilitiesCS/UtilitiesCS.csproj`,
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj` or `ToDoModel.Test/ToDoModel.Test.csproj`. The two-dot form
  (`origin/main..HEAD`) reports the identical 46-file set, as expected when the merge base equals
  `origin/main`. The `-U0` hunks are `@@ -259 +259 @@`, `@@ -52 +52 @@`, `@@ -42 +42 @@`, each removing a
  `lib\netstandard2.1` HintPath and adding the `lib\netstandard2.0` one.
- **Anchor discipline.** Anchored on `origin/main` after an explicit fetch, never local `main`; the
  execution worktree's `origin/main` and merge base both resolve to `91746d2e`.
- **Corroborating evidence.** `evidence/other/scope-boundary-diff.2026-09-16T23-27.md` records the same three
  lines in both diff forms, `OUT-OF-WRITE-SET: NONE`, and an empty porcelain span.

### AC4 — Full C# toolchain single clean pass; every test passing; no serialisation added — **PASS**

- **Order and commands, evidence-attested.** `format-final` (format then check), `analyzer-final`,
  `nullable-final`, `test-final`, in that order, with the CLAUDE.md commands verbatim (`/t:Rebuild`, no
  `/p:Nullable=enable`, `dotnet tool run csharpier`). `loop-closure`: one iteration, five
  `EXPECTATION-MET: YES`, `LOOP: CLEAN PASS` (anchored counts re-derived: 5 and 1).
- **CSharpier, evidence-attested.** `check` exit 0, `Checked 1641 files in 5450ms.` = baseline 1639 + 2;
  `format` rewrote nothing (SHA-256 of the three `.cs` files identical before and after).
- **Analyzer and nullable rebuilds, re-derived.** `coverage/logs/p4-t7-analyzer.txt` and
  `coverage/logs/p4-t8-nullable.txt`: `Skipping target "CoreCompile"` = 0 and `^\s+0 Error(s)$` = 1 in
  each; 36 `out:obj` lines per log (2 × 18 projects). Both exit 0 with 0 warnings.
- **Tests with coverage, re-derived and evidence-attested.** `test-final`: `COUNTERS_TOTAL=7311 EXECUTED=7311 PASSED=7311 FAILED=0`,
  `NEW_TEST_RESULT_COUNT=18` / `NEW_TEST_PASSED_COUNT=18`, `Cobertura Document State: POSTPROCESSED`,
  `NEWLY-FAILING: NONE`, single run. The root element of `coverage/coverage.cobertura.xml` on disk reads
  `line-rate="0.858678" branch-rate="0.800317" lines-covered="56343" lines-valid="65616"`, matching the
  extract exactly. The pre-existing `NetstandardBindChildDomainTests` class and its negative control passed
  in the P4-T4 namespace run (29/29) and inside the full suite.
- **Runsettings and serialisation, re-derived.** `git diff origin/main...HEAD -- scripts/vscode/TaskMaster.cli.runsettings`
  is empty. `DoNotParallelize` occurrences: 0 in each new file; 2 in `NetstandardBindChildDomainTests.cs`
  (line 22 comment, line 30 attribute), identical at base, placed by #879. No retry, sleep or tolerance
  exists in either new file.
- **Evidence location.** All four gate artifacts plus `coverage-delta` and `loop-closure` are under
  `evidence/qa-gates/` with `Command:`, `EXIT_CODE:` and output summaries.

### AC5 — `<remarks>` corrected; comment-only diff; line 206-207 message unchanged — **PASS**

- **Pre-fix observation, evidence-attested.** `evidence/baseline/tree-baseline.2026-09-16T23-27.md`
  (P0-T9 census): `UNSATISFIABLE_COUNT=2` on the unfixed file (line 281 summary and the stale sentence at
  line 366). The spec's own citation of "lines 366-367" undercounts by the retained line-281 occurrence;
  recorded as an observed correction in the plan and not a defect in the change.
- **Post-fix, re-derived.** `unsatisfiable` occurrences now **1**; `display-name tests` present (the new
  block, :371); the string `"the flavour of FSharp.Core deployed beside Deedle is what determines "` is at
  line 206, unchanged (not in the diff). The new block (:363-375) states that every deployed copy references
  `netstandard 2.0.0.0`, retains the directory as the historically failing root, and names the display-name
  tests as the carriers of discriminating power.
- **Comment-only, re-derived.** `git diff -U0 origin/main...HEAD -- TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`:
  one hunk `@@ -364,7 +364,11 @@`; 18 changed lines (11 added, 7 removed); lines whose content after the
  `+`/`-` marker does not begin with `///` = **0**. No test method, assertion, attribute or constant changed;
  `[TestMethod]` count 9 and `[DoNotParallelize]` count 2 are unchanged.
- **Plan-literal note (not an AC clause).** The plan's `CHANGED_LINES=22` / numstat `13 9` / deletions 9
  literals counted the unchanged `/// <remarks>` and `/// </remarks>` boundary lines; git renders them as
  context, giving 18 / `11 7` / 7. The criterion text requires a diff "showing changes only inside XML
  documentation comment lines", which is exactly what is measured. See policy audit W-1.

---

## Acceptance Criteria Check-off

All five criteria were already checked off in `spec.md` by the executor (P5-T1 to P5-T5) before this
review. This review independently evaluated each as **PASS** and confirms the existing `[x]` marks at
`spec.md` lines 422, 430, 444, 450 and 457. **No criterion was newly checked off by this review, and none
was unchecked.** No criterion text was modified.

Executor check-off basis was spot-checked against the named evidence for each criterion
(`ac-status.2026-09-16T23-27.md` per-criterion table) and found consistent with the artifacts on disk.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

---

## Summary

**PASS — 5 of 5 acceptance criteria verified; 0 blocking findings; 0 PARTIAL; ready to merge.**

- The defect is removed at its source: all six HintPaths select the netstandard2.0 flavour, and every
  deployed `FSharp.Core.dll` in the fifteen output directories is the netstandard2.0 binary (hash-confirmed
  by this reviewer independently of the tests).
- Both regression shapes were observed failing on the unfixed tree in exactly the predicted members
  (three offenders; three rows) with a passing positive control, and passing after the fix; the pre-fix
  TRX was read directly.
- The full toolchain passed in one clean iteration; build non-vacuity and the coverage root figures were
  re-derived from the git-ignored logs and Cobertura document, not taken from the extracts.
- Two plan-literal deviations (items (a) and (b)) are prediction errors in the plan, correctly recorded by
  the executor and now ratified by independent re-measurement; neither affects any acceptance criterion's
  text. Item (c) is sound test design.
- Owed before closing #895: file the two recorded latent defects (`ToDoModel.Test/packages.config`
  omission; `Sync-PackageReferences.ps1` TFM order) as GitHub issues, and note the ratified plan-literal
  deviations in the PR body or merge record.

No `remediation-inputs` artifact is produced.
