# quickfiler-test-form1-live-form — Remediation Plan (Cycle 1) (Atomic Plan)

- **Issue:** #491
- **Parent:** epic `quickfiler-suite-determinism-foundation`
- **Owner:** drmoisan
- **Last Updated:** 2026-08-22T09-40
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** `full-bug` (unchanged from the primary plan). Acceptance-criteria source remains `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/spec.md` only.
- **Remediation cycle:** 1. This plan does not re-plan any task already executed and committed by the primary plan (`docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/plan.2026-08-21T18-11.md`, commits `c7557c3d`, `5cec657b`, `3f2fb8d1`). It closes the four acceptance criteria that plan left unchecked (AC1, AC8, AC9, AC10) by removing a second, pre-existing, out-of-scope dead `Form`-derived type discovered during that plan's own execution.

## Objective

Delete the dead nested class `QfcFormViewerDerived : QfcFormViewer` from
`QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` (a repository-verified zero-caller, dead
`System.Windows.Forms.Form`-derived type predating this issue), re-run the full Phase-3-equivalent
verification loop from a clean state, complete the coverage comparison and test-count parity
evidence the primary plan could not produce while the guard test was red, and check off the four
acceptance criteria this blocked.

## Finding this plan remediates

Recorded in full in
`docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/other/remediation-inputs.2026-08-22T09-40.md`.
Summary: the primary plan's guard test
`NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` is red for a reason
unrelated to `Form1` — it also, correctly, catches a second, previously-undiscovered, dead
`Form`-derived type declared inside `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`. The
orchestrator's recorded disposition is to delete that dead type as an in-scope, zero-risk root-cause
fix, not to defer it or to narrow the guard's scope. This plan implements that disposition.

## Conventions used by every task in this plan

- **Working directory.** Every command runs from the repository worktree root (the directory
  containing `TaskMaster.sln`). All paths in this plan are repository-relative.
- **Shell.** Run every C# toolchain command from a `pwsh -NoProfile` session, never raw Bash —
  MSBuild switches such as `/m` are mangled into `M:/` (MSB1008) by shells that rewrite
  POSIX-looking arguments. When nesting through `pwsh -NoProfile -Command`, wrap the entire payload
  in single quotes so `$` tokens expand in the child shell, not the parent; double any single quote
  that must appear inside the payload.
- **No shell state persists between tasks.** Each tool invocation starts a fresh shell. Every
  MSBuild or vstest task in this plan either re-resolves `$msbuild`/`$vstest` with the vswhere
  commands in P0-T3, inside the same `pwsh` session that runs the build or test, or substitutes the
  literal absolute paths recorded in the P0-T3 artifact. Likewise `$assemblies` must be re-populated
  by the enumeration command inside the same session that invokes vstest.
- **Toolchain bootstrap is not re-run.** The primary plan already installed the repo-local .NET SDK
  and restored NuGet packages in this worktree (its P0-T11/P0-T12). Those are filesystem-persistent
  states, not shell state, so this plan verifies they still exist (P0-T4) rather than re-running the
  bootstrap. If either is found missing, the executor halts and escalates rather than silently
  re-bootstrapping outside a planned task.
- **Evidence root.** `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/`.
  The only valid `kind` sub-folders used by this plan are `baseline`, `regression-testing`,
  `qa-gates`, `issue-updates`, and `other`. No evidence may be written anywhere outside this root.
  No delegation prompt supplied a non-canonical evidence path, so no
  `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record is required.
- **Timestamps.** Every evidence filename below contains the literal token `TIMESTAMP`. Replace it
  with the actual ISO-8601 capture time in the form `yyyy-MM-ddTHH-mm` at the moment the artifact is
  written.
- **Evidence schema.** Every command-step artifact contains, at minimum, the lines `Timestamp:`,
  `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- **No helper scripts under `evidence/`.** Do not create any `.ps1`, `.sh`, or `.py` file anywhere
  under the evidence tree.
- **`.claude/**` is read-only.** No task in this plan edits anything under `.claude/`.
- **Hooks are inert.** Verify every gate from durable `git` state and from recorded exit codes,
  never from a hook result.
- **Re-derive every line number.** The line range cited for `QfcFormViewerDerived` in the
  remediation-inputs artifact (lines 243-250) has already drifted from the working tree (the class
  currently spans lines 243-252, followed by a blank line at 253). Phase 0 re-derives the exact
  current range from the working tree, and the executor uses the re-derived value, not any number
  cited in this plan or in the remediation-inputs artifact.
- **CSharpier output wins.** If `dotnet tool run csharpier format .` adjusts blank-line spacing
  around the deleted block, keep the formatter's output; do not hand-tune spacing.

## Literal quoted here (so a later search for it is recognized as an assertion, not a false gap)

- `QfcFormViewerDerived` — the dead nested class this plan deletes from
  `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`. The literal remains present in tracked
  evidence and plan prose after deletion (this file, the remediation-inputs artifact, and the
  primary plan's `ac-status-summary` artifact), so its removal from the one `.cs` file it is deleted
  from does not make it absent from the tracked tree.
- `NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` — the existing guard
  test whose green result this plan's Phase 2 proves.

---

### Phase 0 — Context and Current-State Recap (Remediation Cycle 1)

This phase is deliberately light: it records current repository state and re-derives facts that
have drifted, rather than re-running the primary plan's full toolchain baseline. Policy reading
order for this cycle: `CLAUDE.md`, then `.claude/rules/general-code-change.md`, then
`.claude/rules/general-unit-test.md`, then `.claude/rules/quality-tiers.md`, then
`.claude/rules/plan-acceptance-gates.md`, then the feature documents. All five rule files are
read-only.

- [x] [P0-T1] Read, in this order and without editing any of them: `CLAUDE.md`,
      `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`,
      `.claude/rules/quality-tiers.md`, `.claude/rules/plan-acceptance-gates.md`, then
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/other/remediation-inputs.2026-08-22T09-40.md`,
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/plan.2026-08-21T18-11.md`,
      and `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/spec.md`. Write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/other/remediation-phase0-instructions-read.TIMESTAMP.md`
      with `Timestamp:`, `Policy Order:`, and an explicit bulleted list of every file read.
      Acceptance: the file exists and all three required field labels are present, and the list
      contains all eight cited files.
- [x] [P0-T2] Run `git rev-parse --abbrev-ref HEAD`, `git rev-parse HEAD`,
      `git status --porcelain`, and the three concrete ancestry checks
      `git merge-base --is-ancestor c7557c3d HEAD`, `git merge-base --is-ancestor 5cec657b HEAD`,
      and `git merge-base --is-ancestor 3f2fb8d1 HEAD` (record the exit code of each; `0` means the
      commit is an ancestor of the current `HEAD`). Write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/baseline/remediation-phase0-branch-state.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the observed
      branch name, the observed HEAD sha, the porcelain output, and whether each of the three shas
      is an ancestor of `HEAD`. Acceptance: the artifact records the observed branch and HEAD sha as
      observed values (not asserted equal to any pinned value), states whether the observed branch
      equals `bug/quickfiler-test-form1-live-form-491-exec`, and records `is-ancestor` exit code `0`
      for all three commits. `.claude/agent-memory/` is tracked and may be dirty; the porcelain
      output is recorded as observed, not asserted empty.
- [x] [P0-T3] Resolve the absolute paths of `MSBuild.exe` and `vstest.console.exe` with `& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe` and `& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -requires Microsoft.VisualStudio.PackageGroup.TestTools.Core -find Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe`, and write both resolved paths to
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/baseline/remediation-phase0-tool-resolution.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: both resolved
      paths are non-empty and both files exist on disk. Every later MSBuild and vstest task in this
      plan uses these absolute paths, re-resolved or substituted literally in its own session.
- [x] [P0-T4] Verify the toolchain bootstrap from the primary plan is still present by running
      `pwsh -NoProfile -Command '(Test-Path -LiteralPath ".dotnet-sdk"); (Test-Path -LiteralPath "packages")'`
      and write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/baseline/remediation-phase0-toolchain-prereqs.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: both reported
      values are `True`. If either is `False`, the executor halts and escalates rather than
      re-running the primary plan's bootstrap tasks outside a planned task in this cycle.
- [x] [P0-T5] Re-derive the exact current line range of the `QfcFormViewerDerived` nested class by
      running `pwsh -NoProfile -Command '$lines = Get-Content -LiteralPath "QuickFiler.Test/Controllers/QfcHomeControllerTests.cs"; $hit = 0..($lines.Count - 1) | Where-Object { $lines[$_] -like "*class QfcFormViewerDerived*" } | Select-Object -First 1; $start = [Math]::Max(0, $hit - 1); $end = [Math]::Min($lines.Count - 1, $hit + 12); $start..$end | ForEach-Object { "{0}: {1}" -f ($_ + 1), $lines[$_] }'`
      and write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/baseline/remediation-phase0-line-derivation.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: the artifact
      records the observed line number of the `public class QfcFormViewerDerived : QfcFormViewer`
      declaration and the observed line number of that class's closing `}`, read directly from the
      printed, numbered context window, and states in one sentence that the executor uses these
      observed numbers and does not trust any number cited in this plan or in the
      remediation-inputs artifact. The printed window's twelve-line lookahead is sufficient to show
      the closing brace because the class body is four members; if the closing brace is not visible
      in the printed window, the command is re-run with a larger lookahead before this task is
      checked off.
- [x] [P0-T6] Confirm the class is still dead code by running
      `pwsh -NoProfile -Command 'Select-String -Path "**/*.cs" -SimpleMatch "QfcFormViewerDerived" | ForEach-Object { "{0}:{1}: {2}" -f $_.Path, $_.LineNumber, $_.Line.Trim() }'`
      from the repository root and write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/baseline/remediation-phase0-deadcode-confirmation.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: every reported
      match is inside `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, the match count is
      exactly 2 (the class declaration and its constructor name), and the artifact states that no
      `new QfcFormViewerDerived(` construction and no reference from any other file was found. A
      match count other than 2, or a match outside this one file, halts this plan: it would mean the
      type is not the zero-caller dead code the orchestrator's disposition assumed, and the
      remediation scope would need to be re-evaluated before deletion.

### Phase 1 — Removal of the Dead `QfcFormViewerDerived` Nested Class

- [x] [P1-T1] Delete the contiguous line range from the re-derived opening line (the line containing
      `public class QfcFormViewerDerived : QfcFormViewer`) through the re-derived closing `}` of
      that class, inclusive, as recorded in the P0-T5 artifact, from
      `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`. Delete the whole nested class block;
      do not delete only the constructor or only a subset of members. Do not hand-adjust
      surrounding blank-line spacing — CSharpier normalizes it in Phase 2. Acceptance:
      `pwsh -NoProfile -Command '(Select-String -LiteralPath "QuickFiler.Test/Controllers/QfcHomeControllerTests.cs" -SimpleMatch "QfcFormViewerDerived" | Measure-Object).Count'`
      returns `0`.
- [x] [P1-T2] Verify the edit touched no other line by running
      `git diff --numstat -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` and
      `git diff -U0 -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, and write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase1-diff-scope.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording both outputs.
      Acceptance: the diff contains exactly one hunk, the added-line count is exactly 0, the
      deleted-line count equals the P0-T5-derived line count (closing-line minus opening-line plus
      1), and no other file appears in `git status --porcelain -- QuickFiler.Test`. This is the
      pre-formatting scope check; Phase 2's CSharpier pass is verified separately and is not
      expected to touch this file's substantive content, only blank-line spacing if any.

### Phase 2 — Verification Loop, Rerun From a Clean State

Run P2-T1 through P2-T6 in order as one uninterrupted toolchain pass. If any step fails, or if any
step modifies a file, restart from P2-T1. Do not leave this loop while any step is failing.

- [x] [P2-T1] Run `dotnet tool run csharpier format .` and then `dotnet tool run csharpier check .`,
      writing
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase2-csharpier.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the number of
      files reformatted by the format command and the check command's exit code. Acceptance: the
      check command reports `EXIT_CODE: 0`.
- [x] [P2-T2] Audit the edited file's size by running
      `pwsh -NoProfile -Command '(Get-Content -LiteralPath "QuickFiler.Test/Controllers/QfcHomeControllerTests.cs" | Measure-Object -Line).Lines'`
      and record the result in
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase2-file-size-audit.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: the recorded
      line count is 500 or fewer. This audit runs after the formatting pass because CSharpier can
      change line counts.
- [x] [P2-T3] Rebuild with
      `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fileLogger "/fileLoggerParameters:LogFile=coverage\msbuild\remediation-phase2-analyzers.log;Verbosity=normal"`
      (using the `$msbuild` path re-resolved or substituted per the Conventions section) and write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase2-msbuild-analyzers.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the error count,
      the count of log lines matching `CoreCompile:`, and the count of log lines matching
      `Skipping target "CoreCompile"`. Acceptance: `EXIT_CODE: 0`, error count 0, `CoreCompile:`
      count at least 1, and `Skipping target "CoreCompile"` count exactly 0.
- [x] [P2-T4] Rebuild with
      `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /fileLogger "/fileLoggerParameters:LogFile=coverage\msbuild\remediation-phase2-nullable.log;Verbosity=normal"`
      and write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase2-msbuild-nullable.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the error count,
      the `CoreCompile:` count, and the `Skipping target "CoreCompile"` count. Acceptance:
      `EXIT_CODE: 0`, error count 0, `CoreCompile:` count at least 1, `Skipping target
      "CoreCompile"` count exactly 0, and the artifact's `Command:` line contains no
      `Nullable=enable` property.
- [x] [P2-T5] Run the full suite inside ONE `pwsh -NoProfile -Command` payload that first enumerates
      the test assemblies with
      `$all = @(Get-ChildItem -Path . -Recurse -Filter *.Test.dll -File | ForEach-Object { Resolve-Path -Relative $_.FullName } | Where-Object { $_ -like "*\bin\Debug\*" -and $_ -notlike "*\obj\*" -and $_ -notlike "*\ref\*" }); $assemblies = @($all | Where-Object { $_ -notlike "*\.claude\*" })`,
      then either re-resolves `$vstest` with the P0-T3 vswhere command or substitutes the literal
      absolute vstest path recorded in the P0-T3 artifact, then invokes
      `& $vstest @assemblies /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`,
      teeing output to `coverage/logs/remediation-phase2-vstest.log`. A bare `@assemblies` in a
      fresh session is forbidden: the enumeration and the vstest call must execute in the same
      payload. Write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase2-vstest.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the total,
      passed, failed, and skipped counts and the number of assemblies actually passed on the
      command line. Acceptance: `EXIT_CODE: 0`, the assembly count is at least 1, and the failed
      count is exactly 0. If the failed count is exactly 1 and the single failure is the
      pre-existing, unrelated, load-flaky
      `UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`
      test recorded in the primary plan's `phase0-vstest-baseline` evidence (not a test this plan
      touches), this task may be re-run at most once; the acceptance comparison is then made against
      the re-run's counts, and the artifact states that a re-run occurred and why. A second
      occurrence of that same failure, or any failure other than that named test, is not covered by
      this allowance and must be resolved (per the Phase-2-loop restart rule) before this task is
      checked off.
- [x] [P2-T6] Prove the guard is green by name by running
      `& $vstest .\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~NoLiveFormInTestAssemblyTests"`,
      teeing output to `coverage/logs/remediation-phase2-guard-green.log`, and write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase2-guard-green.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the total,
      passed, and failed counts and the fully-qualified name of the test that ran. Acceptance:
      `EXIT_CODE: 0`, total is 1, passed is 1, failed is 0, and the recorded fully-qualified name
      ends in `NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType`. This is
      the evidence AC1 is checked off against.
- [x] [P2-T7] Record the clean consecutive pass by writing
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase2-clean-pass.TIMESTAMP.md`
      listing, from one uninterrupted iteration, the exit codes of P2-T1, P2-T3, P2-T4, P2-T5, and
      P2-T6, plus the output of
      `git status --porcelain -- QuickFiler.Test docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491`
      taken immediately after P2-T1. Acceptance: all five recorded exit codes are 0, and the scoped
      porcelain output contains no path that was modified by P2-T1 after P2-T1 ran. If the loop was
      restarted, only the exit codes from the final uninterrupted iteration are recorded, and the
      number of restarts is stated.

### Phase 3 — Coverage Comparison and Test-Count Parity

- [x] [P3-T1] Capture the post-change coverage figure with
      `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\2026-08-07-quickfiler-test-form1-live-form-491\evidence\qa-gates\coverage-postchange-remediation.cobertura.xml`,
      and write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase3-coverage-capture.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: the Cobertura
      file exists at that path, its root element carries non-empty `lines-covered`, `lines-valid`,
      and `line-rate` attributes, and the capture artifact records `EXIT_CODE: 0`. If
      `Assert-CoberturaLineCoverageThreshold` throws (the documented pre-existing HALT condition:
      the assertion in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` throwing after the
      raw, unfiltered Cobertura file has already been written to the same path), the executor
      records the observed numeric line-rate verbatim in this artifact, discards the raw file
      written at that path, and HALTS and escalates rather than checking this task off. The
      executor must not lower any threshold, edit `coverage.config`, exclude any production file
      from measurement, or substitute a hand-computed figure.
- [x] [P3-T2] Extract the post-change numbers with
      `pwsh -NoProfile -Command 'Select-Xml -Path "docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/coverage-postchange-remediation.cobertura.xml" -XPath "/coverage" | ForEach-Object { $_.Node.GetAttribute("lines-covered"); $_.Node.GetAttribute("lines-valid"); $_.Node.GetAttribute("line-rate") }'`
      and write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase3-coverage-postchange.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance: `Output
      Summary:` records the numeric post-change line-coverage percentage to four decimal places plus
      the integer `lines-covered` and `lines-valid` values, all as actual numbers.
- [ ] [P3-T3] Write the comparison artifact
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase3-coverage-comparison.TIMESTAMP.md`
      recording, side by side, the baseline figures already captured by the primary plan
      (`85.5788%`, `lines-covered=53402`, `lines-valid=62401`, per
      `evidence/baseline/phase0-coverage-baseline.2026-08-22T13-13.md` — cited directly, not
      re-captured) against the P3-T2 post-change figures, and the arithmetic difference of each
      pair. Acceptance: the post-change percentage is greater than or equal to the baseline
      percentage; both figures were produced by `Invoke-MSTestWithCoverage.ps1` and are therefore
      both Koverage-filtered first-party figures; and the artifact states explicitly that no raw
      `dotnet-coverage collect` figure was substituted on either side.
- [x] [P3-T4] Write the test-count parity artifact
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/remediation-phase3-test-count-parity.TIMESTAMP.md`
      recording the baseline total, passed, failed, and skipped counts already captured by the
      primary plan (`Total 6437, Passed 6436, Failed 1, Skipped 0`, per
      `evidence/baseline/phase0-vstest-baseline.2026-08-22T13-13.md` — cited directly, not
      re-captured) alongside the P2-T5 post-change counts. Acceptance: the post-change failed count
      is 0 and the post-change total equals the baseline total plus exactly 1 (the guard test added
      by the primary plan). The primary plan's own baseline failure
      (`UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`)
      is a pre-existing, load-flaky, unrelated condition; its absence from the post-change failed
      count is expected and does not indicate a dropped test.

### Phase 4 — Acceptance-Criteria Check-off and Status Update

- [x] [P4-T1] Check off acceptance criterion 1 in `spec.md` (no `System.Windows.Forms.Form`-derived
      type is compiled into the `QuickFiler.Test` assembly, proven by a named MSTest guard test) by
      changing its `- [ ]` to `- [x]`, citing the P2-T6 artifact as evidence. Acceptance: exactly one
      checkbox changes state in this task, its criterion text is unmodified, and the cited artifact
      records the named guard test with passed count 1 and failed count 0.
- [x] [P4-T2] Check off acceptance criterion 8 in `spec.md` (the vstest run with coverage,
      isolation, and the `LiveOutlook` category filter completes with zero failing tests), citing
      the P2-T5 artifact. Acceptance: exactly one checkbox changes state, its criterion text is
      unmodified, and the cited artifact records a failed count of 0.
- [x] [P4-T3] Check off acceptance criterion 9 in `spec.md` (no pre-existing `QuickFiler.Test` test
      regresses; test-count and pass-count parity apart from the one new guard test), citing the
      P3-T4 artifact. Acceptance: exactly one checkbox changes state, its criterion text is
      unmodified, and the cited artifact shows a post-change total equal to the baseline total plus
      1 with a post-change failed count of 0.
- [ ] [P4-T4] Check off acceptance criterion 10 in `spec.md` (post-change line coverage is greater
      than or equal to the baseline, both recorded as actual numbers), citing the P3-T3 artifact.
      Acceptance: exactly one checkbox changes state, its criterion text is unmodified, and the
      cited artifact records two numeric percentages with the post-change value greater than or
      equal to the baseline value.
- [ ] [P4-T5] Update
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/other/ac-status-summary.2026-08-22T13-13.md`
      in place: change `Checked off (delivered): 7` to `Checked off (delivered): 11`, change
      `Remaining (unchecked): 4` to `Remaining (unchecked): 0`, replace the four-item "Items
      remaining" list with a single line stating all 11 acceptance criteria are checked off as of
      this remediation cycle citing this plan's path, and append a new line
      `Remediation cycle 1 resolution:` summarizing that the root cause (the dead
      `QfcFormViewerDerived` nested class) was deleted and the four previously-blocked criteria are
      now met, citing the P2-T6, P2-T5, and P3-T3 artifacts. Acceptance: `Total AC items: 11` is
      unchanged, `Checked off (delivered): 11` and `Remaining (unchecked): 0` are both present, and
      no other line of the pre-existing narrative (the root-cause paragraph, the `issue.md` note) is
      deleted — this task appends and edits counts, it does not remove the historical record of what
      was found and why.
- [ ] [P4-T6] Update the `- **Status:**` and `- **Last Updated:**` header fields of
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/spec.md` to state that all
      11 acceptance criteria are now met following remediation cycle 1, changing no criterion text
      and adding no new criterion. Acceptance: exactly two header lines change, and the count of
      checkbox items under the `## Acceptance Criteria` heading is still 11, with all 11 now `- [x]`.

### Phase 5 — Commit

- [x] [P5-T1] Stage exactly the owned paths with
      `git add -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491`
      (no `-A` flag; no other pathspec) followed by
      `git commit -m "fix(quickfiler-test): remove dead QfcFormViewerDerived nested class blocking the live-form guard (#491)"`.
      Before staging, confirm no derived coverage settings file survives under the evidence tree by
      running
      `pwsh -NoProfile -Command '(Get-ChildItem -Path "docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence" -Recurse -Filter *.effective-coverage.config -File | Measure-Object).Count'`
      and recording the result in the commit artifact; that count must be 0. Write
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/other/remediation-phase5-commit.TIMESTAMP.md`
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording the new commit
      sha from `git rev-parse HEAD`. Acceptance: the derived-settings count is 0;
      `git status --porcelain -- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491`
      produces empty output after the commit; any residual porcelain entry outside that pathspec is
      recorded verbatim in the artifact and must lie under `.claude/agent-memory/`; and the recorded
      new commit sha differs from each of `c7557c3d`, `5cec657b`, and `3f2fb8d1`.
- [x] [P5-T2] Verify scope lock over this cycle's own commit by running
      `git show --name-only --format= HEAD` and recording the full introduced-path list in the
      P5-T1 artifact. Acceptance: every path introduced by the P5-T1 commit is either
      `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` or a path under
      `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/`; no path under
      `.claude/`, under `docs/features/potential/`, or under any other project directory appears in
      that commit; and none of `c7557c3d`, `5cec657b`, or `3f2fb8d1` is amended, reordered, or
      dropped by this commit (`git log --oneline c7557c3d~1..HEAD` (no pathspec) still lists all
      three shas — `3f2fb8d1`, `5cec657b`, `c7557c3d`, newest first — followed by this cycle's new
      commit at the top; a pathspec-scoped log is not used for this check because two of the three
      primary-plan commits (`5cec657b`, `3f2fb8d1`) touched only the plan document under
      `docs/features/active/...`, not any path under `QuickFiler.Test`, and would be silently
      dropped by a `QuickFiler.Test` pathspec filter).
