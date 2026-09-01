# 2026-08-27-qfc-metrics-flush-writes-empty-session-file (Plan)

- **Issue:** #646
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T20-04
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** minor-audit
- **Directive:** MINIMAL-AUDIT PLAN REQUIRED

## Requirements Source

`docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/issue.md`
is the sole requirements source. Its `## Acceptance Criteria` section (AC1-AC8, lines
116-137 at plan-authoring time) is the only acceptance-criteria source for this plan.
`spec.md`, `user-story.md`, and `research.md` do not exist in the feature folder and are
not required.

## Research Inputs

`research/research.2026-08-31T20-30.md` describes a pre-merge tree (issue #647 not yet
landed). `research/research-correction.2026-08-31T20-45.md` is authoritative where the two
disagree: issue #647 has already merged into `main`, the `MetricsFileWriter` delegate now
returns `Task<bool>`, and the writer invocation is a multi-statement block followed by an
`if (!metricsWritten)` logging branch. Every citation below was re-derived directly against
the current working tree, not carried forward from either research document.

## Hard Scope Boundaries

1. The `MetricsFileWriter` delegate signature (`Func<string, string[], string,
   CancellationToken, Task<bool>>`, declared at `QuickFiler/Controllers/
   QfcHomeController.Metrics.cs:28-34`) and the `if (!metricsWritten)` failure-logging
   branch (same file, lines 185-191) are the delivered outcome of issue #647. No task in
   this plan may alter either.
2. The only repository paths this plan writes to are:
   - `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
   - `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
   - `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/`
     (this plan file, evidence artifacts, and `issue.md` check-offs)
3. `QuickFiler/Controllers/EfcHomeController.Metrics.cs` is read-only reference. No task
   writes to it.
4. Every task that edits `QfcHomeController.Metrics.cs` re-derives its edit anchors against
   the current tree at execution time (Phase 1, before editing) rather than trusting the
   line numbers recorded in this plan or in either research document.
5. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` is 454 lines at
   plan-authoring time, against the repository's 500-line file-size cap. Phase 1 includes an
   explicit post-change line-count verification task rather than assuming the new test fits.
6. No task in this plan runs `git update-index`. `artifacts/orchestration/
   orchestrator-state.json` already carries `--skip-worktree` from the orchestrator and must
   stay outside this item's footprint; no task touches it.

## Evidence Location

Every evidence artifact resolves under `docs/features/active/
2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/<kind>/` using
`baseline/`, `regression-testing/`, `qa-gates/`, or `other/`. No task writes to any
`artifacts/...` path as an evidence location.

## Coverage Policy Note

CLAUDE.md's C# Unit Test Policy states repository-wide line coverage must remain >= 80% and
new modules/methods must reach >= 90%. `.claude/rules/general-unit-test.md` states a uniform
>= 85% line / >= 75% branch floor across all tiers. These two documents disagree on the
repository-wide floor; this plan does not resolve that conflict. Consistent with prior
practice recorded for this repository (`quickfiler-home-controller-metrics-442` and prior
coverage-reconciliation items), this plan treats the repository-wide percentage as a
recorded, non-blocking figure and treats changed-line no-regression and new-code coverage
(>= 90% for the four new guard lines) as the blocking gates, per Phase 2 P2-T7.

---

### Phase 0 — Baseline Capture

Policy reads follow the `policy-compliance-order` sequence: `CLAUDE.md` (position 1),
`.claude/rules/general-code-change.md` (position 2), `.claude/rules/general-unit-test.md`
(position 3), `.claude/rules/csharp.md` (position 4, applicable because both in-scope files
are `*.cs`).

- [x] [P0-T1] Read `CLAUDE.md` in full at the repository root. Acceptance: the read is
  recorded in the Phase 0 policy-read evidence artifact produced by P0-T5.
- [x] [P0-T2] Read `.claude/rules/general-code-change.md` in full. Acceptance: recorded in
  the P0-T5 artifact.
- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` in full. Acceptance: recorded in the
  P0-T5 artifact.
- [x] [P0-T4] Read `.claude/rules/csharp.md` in full. Acceptance: recorded in the P0-T5
  artifact.
- [x] [P0-T5] Write the Phase 0 policy-read evidence artifact to
  `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/
  evidence/baseline/phase0-instructions-read.md` with `Timestamp:`, `Policy Order:` (the
  four files in the order listed above), and an explicit list of files read. Acceptance:
  the file exists and contains all three required fields.
- [x] [P0-T6] Run `git fetch origin`, then reconcile the current branch onto the
  `origin/main` tip (fast-forward merge if a clean fast-forward is possible, otherwise a
  merge of `origin/main` into the current branch). Record the pre- and post-reconciliation
  `git rev-parse HEAD` values. Acceptance: `git merge-base --is-ancestor origin/main HEAD`
  exits `0`. Evidence:
  `evidence/baseline/branch-reconciliation.2026-08-31T20-04.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T7] Run `dotnet tool run csharpier check .` from the repository root. Record the
  printed summary line verbatim (CSharpier's check-mode success output, or the list of
  files needing formatting on failure). Acceptance: `EXIT_CODE` and `Output Summary` are
  both recorded, whatever the exit code is (this step establishes the pre-existing
  formatting state; it is not gated pass/fail). Evidence:
  `evidence/baseline/csharpier-check.2026-08-31T20-04.md`.
- [x] [P0-T8] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug
  "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` from the
  repository root. Record `EXIT_CODE` and the printed `Build succeeded`/`Build FAILED`
  summary line with warning/error counts. Acceptance: `EXIT_CODE` recorded (this is a
  baseline capture, not a gate). Evidence:
  `evidence/baseline/msbuild-analyzer-rebuild.2026-08-31T20-04.md`.
- [x] [P0-T9] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug
  "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` from the repository root. Record
  `EXIT_CODE` and the printed build summary line. Acceptance: `EXIT_CODE` recorded.
  Evidence: `evidence/baseline/msbuild-nullable-rebuild.2026-08-31T20-04.md`.
- [x] [P0-T10] Resolve `vstest.console.exe` via `vswhere.exe` (not on PATH in this
  environment): `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual
  Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find
  'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1`. Then
  run `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` — the
  exact assembly path and flag CUT3 names, with no other flags added. Record `EXIT_CODE`
  and the printed `Passed!`/`Failed!` summary line with Passed/Failed/Total counts.
  Acceptance: `EXIT_CODE` and the printed summary line are both recorded. Evidence:
  `evidence/baseline/vstest-coverage-run.2026-08-31T20-04.md`.
- [x] [P0-T11] Locate the `.coverage` file created by P0-T10 (`Get-ChildItem -Path
  TestResults -Filter *.coverage -Recurse | Sort-Object LastWriteTime -Descending |
  Select-Object -First 1`), then run `dotnet-coverage merge -f cobertura -o
  docs\features\active\2026-08-27-qfc-metrics-flush-writes-empty-session-file-646\evidence\
  baseline\baseline-coverage.cobertura.xml <located-.coverage-path>`. Record the resulting
  XML's root `<coverage>` element `line-rate` and `branch-rate` attribute values verbatim
  as the baseline coverage headline. Acceptance: the `.cobertura.xml` artifact exists and
  its root `line-rate` value is a numeric string, not a placeholder. Evidence:
  `evidence/baseline/coverage-cobertura-baseline.2026-08-31T20-04.md` referencing the
  `.cobertura.xml` file.

### Phase 1 — Constrained Implementation

- [x] [P1-T1] Re-derive the two edit anchors against the current tree (post-P0-T6
  reconciliation): search `QuickFiler/Controllers/QfcHomeController.Metrics.cs` for the
  literal `var lines = strOutput.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();`
  (Anchor A) and the literal `bool metricsWritten = await MetricsFileWriter(` (Anchor B),
  and record their current line numbers. Do not anchor on the absolute line numbers
  recorded in this plan or in either research artifact. Acceptance: both literals are found
  exactly once each in the file, and their line numbers are recorded. Evidence:
  `evidence/other/anchor-rederivation.2026-08-31T20-04.md`.
- [x] [P1-T2] [expect-fail] Add a new MSTest regression test method,
  `WriteMetricsAsync_WithAllNullOrWhitespaceDiagnostics_DoesNotInvokeWriter`, to
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, inserted immediately after
  `WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter` inside the `#region Issue
  #442 — metrics flush tests` block. Model it on that test: call
  `BuildLooseMetricsController(new[] { "   ", null, "\t" })` (default `withMyDocuments:
  true`, so the pre-existing MyDocuments guard does not itself cause the early return), set
  `controller.MetricsFileWriter` to a lambda that sets a `bool invoked = true` and returns
  `Task.FromResult(true)`, call `await controller.WriteMetricsAsync("metrics.csv")`, then
  assert `invoked.Should().BeFalse(...)`. Acceptance: the method exists verbatim as
  described in the file.
- [x] [P1-T3] Check off AC3 in `issue.md` (`A new MSTest regression test in
  QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs stubs GetMoveDiagnostics to
  return an array whose every element is null or whitespace and asserts that the injected
  MetricsFileWriter delegate is invoked zero times.`), backed by P1-T2. Change only `- [ ]`
  to `- [x]` for that line.
- [x] [P1-T4] [expect-fail] Rebuild the test project: `msbuild
  QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug
  /p:Platform=AnyCPU`. Use `/p:Platform=AnyCPU` (no space) for this project-level build,
  not the solution-level `"/p:Platform=Any CPU"` alias: `QuickFiler.Test.csproj`'s
  `PropertyGroup` conditions key on the literal `Debug|AnyCPU` string, and a `Platform`
  value of `Any CPU` (with a space) matches no `PropertyGroup`, which leaves `OutputPath`
  unset and fails the build with `The BaseOutputPath/OutputPath property is not set for
  project 'QuickFiler.Test.csproj'`. Then, using the vswhere-resolved `$vstest` from
  P0-T10 (or re-resolve it if the session state is gone), run `& $vstest
  QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
  /TestCaseFilter:"FullyQualifiedName~WriteMetricsAsync_WithAllNullOrWhitespaceDiagnostics_DoesNotInvokeWriter"`.
  Acceptance: the run reports a non-zero `EXIT_CODE` and the printed summary line shows
  `Failed:     1` — the new test fails against the unguarded implementation, because the
  guard does not exist yet. Evidence:
  `evidence/regression-testing/fail-before-new-test.2026-08-31T20-04.md`.
- [x] [P1-T5] Apply the fix: insert
  ```
  if (lines.Length == 0)
  {
      return;
  }
  ```
  immediately after the statement that computes the filtered array (Anchor A, as re-derived
  in P1-T1) and before the three-line explanatory comment about `CancellationToken.None`
  that immediately precedes the writer invocation statement (Anchor B), so that comment
  stays adjacent to the writer statement it explains, in `QuickFiler/Controllers/
  QfcHomeController.Metrics.cs`. Acceptance: the exact four-line block appears in the file
  immediately after Anchor A and immediately before that comment block.
- [x] [P1-T6] Verify the production diff is scoped to exactly the new guard: run `git diff
  origin/main -- QuickFiler/Controllers/QfcHomeController.Metrics.cs`. Confirm zero removed
  (`-`) lines appear in the diff, confirm the only added (`+`) lines are the four guard
  lines from P1-T5, and confirm the diff contains no hunk touching the `MetricsFileWriter`
  property declaration (the `Task<bool>` lines at 28-34) or the `if (!metricsWritten)` block
  (lines 185-191). Acceptance: all three conditions hold. Evidence:
  `evidence/other/production-diff-scope.2026-08-31T20-04.md`.
- [x] [P1-T7] Check off AC6 in `issue.md` (`The MetricsFileWriter delegate signature and the
  writer's failure-handling branch are unchanged by this item. Both are owned by issue #647
  and are out of scope here.`), backed by P1-T6.
- [x] [P1-T8] Check off AC2 in `issue.md` (`The guard is an early return placed between the
  statement that computes the filtered diagnostic-line array and the statement that awaits
  MetricsFileWriter, and is textually equivalent to the guard already present in
  QuickFiler/Controllers/EfcHomeController.Metrics.cs`), backed by P1-T5 and by the
  `if (dataLines.Length == 0) { return; }` guard already present at
  `QuickFiler/Controllers/EfcHomeController.Metrics.cs:72-75`.
- [x] [P1-T9] Rebuild the test project again (`msbuild
  QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug
  /p:Platform=AnyCPU`, per the same `/p:Platform=AnyCPU` (no space) requirement documented
  in P1-T4), then re-run the same scoped command as P1-T4 (same `/TestCaseFilter`) against
  the fixed implementation. Acceptance: `EXIT_CODE 0` and the printed summary line shows
  `Passed:     1`. Evidence:
  `evidence/regression-testing/pass-after-new-test.2026-08-31T20-04.md`.
- [x] [P1-T10] Check off AC1 in `issue.md` (`WriteMetricsAsync ... returns without invoking
  MetricsFileWriter when the null-and-whitespace filter leaves the filtered
  diagnostic-line array empty.`), backed by P1-T9.
- [x] [P1-T11] Check off AC4 in `issue.md` (`The new regression test fails against the
  unguarded implementation and passes after the guard is added, with fail-before evidence
  recorded ...`), backed by P1-T4 and P1-T9.
- [x] [P1-T12] Run the two existing tests `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce`
  and `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` via the vswhere-resolved
  `$vstest` with `/Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation
  /TestCaseFilter:"FullyQualifiedName~WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce|FullyQualifiedName~WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting"`.
  Acceptance: `EXIT_CODE 0` and the printed summary line shows `Passed:     2`. Evidence:
  `evidence/regression-testing/existing-tests-pass.2026-08-31T20-04.md`.
- [x] [P1-T13] Verify the test-file diff contains zero removed lines: run `git diff
  origin/main -- QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`. Confirm no
  `-` line appears in the diff (only additions), which together with P1-T12 confirms the
  two pre-existing tests were not modified. Evidence:
  `evidence/other/test-file-diff-scope.2026-08-31T20-04.md`.
- [x] [P1-T14] Check off AC5 in `issue.md` (`The existing tests
  WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce and
  WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting still pass and are not
  modified.`), backed by P1-T12 and P1-T13.
- [x] [P1-T15] Verify the post-change line count of the test file: run `(Get-Content
  'QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs').Count`. Acceptance: the
  result is less than or equal to `500`. Evidence:
  `evidence/other/test-file-line-count.2026-08-31T20-04.md`.

### Phase 2 — Final QC Loop

Run P2-T1 through P2-T5 in order, unconditionally. If any of P2-T1 through P2-T5 reports a
non-zero `EXIT_CODE`, or if P2-T1 rewrites any tracked file, restart the loop from P2-T1.
`EXIT_CODE: SKIPPED` is not a valid recorded outcome for any task in this phase.

- [x] [P2-T1] Run `git status --porcelain` and record the set of modified paths and their
  diff line-counts (the tree already carries the Phase 1 edits to the two owned files).
  Then run `dotnet tool run csharpier format .` from the repository root. Then run `git
  status --porcelain` again. Acceptance: `EXIT_CODE 0` is recorded, and the task records
  whether the second `git status --porcelain` shows any additional changed-line count for
  the two owned files, or any newly-modified path, beyond the pre-format snapshot (a
  difference means the formatter rewrote content; no difference means the tree was already
  compliant). Evidence: `evidence/qa-gates/csharpier-format.2026-08-31T20-04.md`.
- [x] [P2-T2] Run `dotnet tool run csharpier check .`. Acceptance: `EXIT_CODE 0`. Evidence:
  `evidence/qa-gates/csharpier-check-final.2026-08-31T20-04.md`.
- [x] [P2-T3] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug
  "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Acceptance: `EXIT_CODE 0` and the printed summary line reads `Build succeeded.`. Evidence:
  `evidence/qa-gates/msbuild-analyzer-rebuild.2026-08-31T20-04.md`.
- [x] [P2-T4] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug
  "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`. Acceptance: `EXIT_CODE 0` and the
  printed summary line reads `Build succeeded.`. Evidence:
  `evidence/qa-gates/msbuild-nullable-rebuild.2026-08-31T20-04.md`.
- [x] [P2-T5] Resolve `vstest.console.exe` via `vswhere.exe` (same resolution as P0-T10),
  then run `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage` —
  the exact assembly path and flag CUT3 names, with no other flags added. Acceptance:
  `EXIT_CODE 0` and the printed summary line shows `Failed:     0` with a `Total:` count
  greater than or equal to the P0-T10 baseline total. Evidence:
  `evidence/qa-gates/vstest-coverage-run.2026-08-31T20-04.md`.
- [x] [P2-T6] Locate the `.coverage` file created by P2-T5 (same discovery method as
  P0-T11), then run `dotnet-coverage merge -f cobertura -o
  docs\features\active\2026-08-27-qfc-metrics-flush-writes-empty-session-file-646\evidence\
  qa-gates\final-coverage.cobertura.xml <located-.coverage-path>`. Record the resulting
  XML's root `<coverage>` element `line-rate` and `branch-rate` attribute values verbatim as
  the final coverage headline. Acceptance: the `.cobertura.xml` artifact exists and its root
  `line-rate` value is a numeric string, not a placeholder. Evidence:
  `evidence/qa-gates/coverage-cobertura-final.2026-08-31T20-04.md`.
- [x] [P2-T7] Coverage delta verification: compare the baseline `line-rate` (P0-T11) to the
  final `line-rate` (P2-T6) and record both values plus the difference; confirm the final
  value is not lower than the baseline value. Separately, in the final Cobertura XML,
  locate the `<class filename="...QfcHomeController.Metrics.cs">` element and the `<line
  number="..." hits="...">` entries for the four guard lines added in P1-T5 (using the
  post-fix line numbers recorded in P1-T6), and confirm each of the four lines has `hits`
  greater than `0`. Acceptance: both conditions hold (no repository-wide regression, and
  100% coverage on the new guard, satisfying the CLAUDE.md >= 90% new-code floor). Evidence:
  `evidence/qa-gates/coverage-delta-verification.2026-08-31T20-04.md`.
- [x] [P2-T8] Verify the total change footprint: run `git status --porcelain` and `git diff
  origin/main --name-status`. Confirm every path listed by either command begins with one
  of `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, `QuickFiler.Test/Controllers/
  QfcHomeControllerMetricsTests.cs`, or `docs/features/active/
  2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/`. Acceptance: no listed path
  falls outside that set. Evidence: `evidence/qa-gates/footprint-scope.2026-08-31T20-04.md`.
- [x] [P2-T9] Check off AC7 in `issue.md` (`No repository file outside
  QuickFiler/Controllers/QfcHomeController.Metrics.cs,
  QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs, and this feature folder is
  modified.`), backed by P2-T8.
- [x] [P2-T10] Check off AC8 in `issue.md` (`The C# toolchain passes in order in a single
  final pass: csharpier format then csharpier check, the analyzer msbuild rebuild, the
  nullable msbuild rebuild, and vstest.console.exe with coverage enabled.`), backed by
  P2-T1 through P2-T5 having completed with no restart of the loop.
- [x] [P2-T11] Final reconciliation: read `issue.md` and confirm all eight items under
  `## Acceptance Criteria` (AC1 through AC8) are `- [x]`. Acceptance: all eight are checked;
  if any is not, this task fails and the gap must be documented rather than the checkbox
  force-checked.

---

## Self-Review

SELF-REVIEW: RE-DERIVED THIS PASS

- `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/issue.md` —
  lines 12 (`Work Mode: minor-audit`), 114 (`## Acceptance Criteria` heading), 116-137
  (AC1-AC8 verbatim text).
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs` — lines 28-34 (`MetricsFileWriter`
  delegate, `Task<bool>` return type), 107 (method signature), 131-134 (pre-existing
  MyDocuments guard), 162-169 (`GetMoveDiagnostics` call), 171-174 (Anchor A and its
  preceding comment), 176-184 (Anchor B and its preceding comment), 185-191 (`if
  (!metricsWritten)` failure branch), 192 (method close).
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs` — lines 59-81 (`QuickFileMetrics_WRITE`
  four-arg overload), 72-75 (`if (dataLines.Length == 0) { return; }` guard).
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` — file length (454 lines,
  confirmed by reading to EOF at line 455/blank), lines 72-135 (`BuildLooseMetricsController`,
  confirming lambdas return `Task.FromResult(true)` post-#647), 300-323 (`MetricsWrite`
  capture record), 330-347 (`WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce`),
  404-425 (`WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting`), 432-450
  (`WriteMetricsAsync_WithoutMyDocumentsFolder_DoesNotInvokeWriter`, the test template).
- `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/research/
  research-correction.2026-08-31T20-45.md` — full document, confirming it supersedes
  `research.2026-08-31T20-30.md` on delegate signature, writer-call text, and line numbers.
- `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/` (glob) —
  confirmed no `spec.md`, `user-story.md`, or `research.md` at feature-folder root, and no
  pre-existing `evidence/` directory.
- `.claude/rules/csharp.md` — lines 12-19 (toolchain commands, including the `/t:Rebuild`
  requirement for both msbuild gates).
- `.claude/rules/plan-acceptance-gates.md` — lines 27-50 (G1-G9 rule table), 139-177
  (write-mode register: CSharpier is not a register member; the six entries are
  `black-write`, `ruff-fix`, `prettier-write`, `poshqc-format`,
  `run_poshqc_analyze_autofix`, `poshqc-suite`).
- `dotnet-tools.json` (repository root) — confirmed the only pinned local tool is
  `csharpier` 1.2.6; `dotnet-coverage` is not a local manifest tool and is assumed
  available as a global tool per `scripts/vscode/Invoke-MSTestWithCoverage.ps1`'s own
  precondition check.
- `QuickFiler.Test/QuickFiler.Test.csproj` — line 454 (`ProjectReference` to
  `QuickFiler\QuickFiler.csproj`), confirming a scoped rebuild of the test project pulls in
  the changed production file via project-reference propagation.
- `QuickFiler.Test/QuickFiler.Test.csproj` — re-derived this pass (revision round 2): line
  12 (`<Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>`), line 32
  (`<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">`), line
  36 (`<OutputPath>bin\Debug\</OutputPath>`, set only inside that conditioned
  `PropertyGroup`). Confirms a project-level `msbuild` invocation with `"/p:Platform=Any
  CPU"` (the space-containing solution alias) matches no `PropertyGroup` on this
  legacy-style project, leaves `OutputPath` unset, and fails the build; `/p:Platform=AnyCPU`
  (no space) is required at the project level instead.
- `QuickFiler/QuickFiler.csproj` — re-derived this pass (revision round 2): line 7
  (`<Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>`), line 20
  (`<PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">`).
  Confirms the referenced production project (built transitively via the `ProjectReference`
  at `QuickFiler.Test.csproj:454` during P1-T4/P1-T9) also keys its `PropertyGroup` on the
  literal `Debug|AnyCPU` string, so `/p:Platform=AnyCPU` (no space) propagates correctly to
  it as a global property during the project-level rebuild.
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs` — re-derived this pass (revision
  round 2): lines 170-192 re-read in full against the current tree. Confirms line 174 is
  Anchor A (`var lines = strOutput.Where(...)`), lines 176-178 are the three-line
  `CancellationToken.None` explanatory comment, and line 179 is Anchor B (`bool
  metricsWritten = await MetricsFileWriter(`). The guard insertion point in P1-T5 is
  tightened to "immediately after Anchor A and before the comment block" (matching
  `research-correction.2026-08-31T20-45.md` lines 58-66) rather than the looser "between
  Anchor A and Anchor B", which was ambiguous about whether the comment stays attached to
  the writer statement it explains.
- `docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/research/
  research-correction.2026-08-31T20-45.md` — re-derived this pass (revision round 2): lines
  58-66 (Anchor A/B definitions and "The guard belongs between Anchor A and that comment
  block" sentence at line 66).
- `.claude/agent-memory/atomic-planner/reference_vstest_scoped_run_command.md` — the
  vswhere-resolution and `/TestCaseFilter` command form used in P0-T10, P1-T4, P1-T9,
  P1-T12, and P2-T5.
- `.claude/agent-memory/atomic-planner/feedback_ac_checkoff_one_per_task.md` and
  `.claude/skills/acceptance-criteria-tracking/SKILL.md` — the one-AC-per-task check-off
  protocol applied throughout Phase 1 and Phase 2.

## Planner Internal Review Record

PLANNER-INTERNAL-REVIEW: PASS

CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS

CITATION: docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/issue.md | lines 12, 114, 116-137
CITATION: QuickFiler/Controllers/QfcHomeController.Metrics.cs | lines 28-34, 107, 131-134, 162-192
CITATION: QuickFiler/Controllers/EfcHomeController.Metrics.cs | lines 59-81
CITATION: QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | lines 72-135, 300-323, 330-347, 404-425, 432-450, file length 454
CITATION: docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/research/research-correction.2026-08-31T20-45.md | full document
CITATION: .claude/rules/csharp.md | lines 12-19
CITATION: .claude/rules/plan-acceptance-gates.md | lines 27-50, 139-177
CITATION: dotnet-tools.json | lines 1-13
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | line 454, lines 12, 32, 36
CITATION: QuickFiler/QuickFiler.csproj | lines 7, 20

AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8

AC-MAPPING: AC1 | IMPLEMENTATION: P1-T5 | TESTS: P1-T9 | EVIDENCE: evidence/regression-testing/pass-after-new-test.2026-08-31T20-04.md
AC-MAPPING: AC2 | IMPLEMENTATION: P1-T5 | TESTS: P1-T6 | EVIDENCE: evidence/other/production-diff-scope.2026-08-31T20-04.md
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T2 | TESTS: P1-T2 | EVIDENCE: QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs (new test method)
AC-MAPPING: AC4 | IMPLEMENTATION: P1-T5 | TESTS: P1-T4, P1-T9 | EVIDENCE: evidence/regression-testing/fail-before-new-test.2026-08-31T20-04.md, evidence/regression-testing/pass-after-new-test.2026-08-31T20-04.md
AC-MAPPING: AC5 | IMPLEMENTATION: N/A (no-change requirement) | TESTS: P1-T12 | EVIDENCE: evidence/regression-testing/existing-tests-pass.2026-08-31T20-04.md, evidence/other/test-file-diff-scope.2026-08-31T20-04.md
AC-MAPPING: AC6 | IMPLEMENTATION: N/A (no-change requirement) | TESTS: P1-T6 | EVIDENCE: evidence/other/production-diff-scope.2026-08-31T20-04.md
AC-MAPPING: AC7 | IMPLEMENTATION: N/A (scope-boundary requirement) | TESTS: P2-T8 | EVIDENCE: evidence/qa-gates/footprint-scope.2026-08-31T20-04.md
AC-MAPPING: AC8 | IMPLEMENTATION: N/A (toolchain requirement) | TESTS: P2-T1, P2-T2, P2-T3, P2-T4, P2-T5 | EVIDENCE: evidence/qa-gates/csharpier-format.2026-08-31T20-04.md, evidence/qa-gates/csharpier-check-final.2026-08-31T20-04.md, evidence/qa-gates/msbuild-analyzer-rebuild.2026-08-31T20-04.md, evidence/qa-gates/msbuild-nullable-rebuild.2026-08-31T20-04.md, evidence/qa-gates/vstest-coverage-run.2026-08-31T20-04.md

UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
