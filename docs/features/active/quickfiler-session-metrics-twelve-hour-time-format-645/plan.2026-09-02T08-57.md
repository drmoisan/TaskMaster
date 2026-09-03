# quickfiler-session-metrics-twelve-hour-time-format-645 (Plan)

- **Issue:** #645
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T08-57
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** full-bug
- **AC Source (per `acceptance-criteria-tracking`):** `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/spec.md`, `## Acceptance Criteria` section (10 checkbox items). `issue.md`'s informal "Acceptance criteria for the resulting issue" list is superseded by spec.md and is NOT used as an AC source (per spec.md §Context and the delegation prompt).

## Working Directory and Base Commit

All commands in this plan run from the repository root of the current worktree, on branch
`bug/quickfiler-session-metrics-twelve-hour-time-format-645` (created from `origin/main`). No
absolute filesystem path is recorded anywhere in this plan or its evidence artifacts; every path
below is repository-relative.

## Scope Lock (from spec.md "Scope & Non-Goals")

In-scope files (exactly these four; no new files are created):
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (lines 48, 127)
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs` (line 96)
- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (lines 227, 243, 265, 278)
- `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` (line 53)

Forbidden paths (must never appear in any diff this plan produces): any file under
QuickFiler/Legacy/; TaskVisualization/TaskViewer.Designer.cs; any file matching .claude/\*\*,
.codex/\*\*, .agents/\*\*; config/blast-radius.json; config/orchestration-routing.json.

Forbidden content change: adding `CultureInfo.InvariantCulture` (or any other `CultureInfo`
argument) to any of the three fixed production call sites. That gap is tracked separately as
issue #742 and is explicitly out of scope here (spec.md §Scope & Non-Goals, §Assumptions).

## Plan-Level Decisions and Deviations (recorded once, applies to every task below)

1. **CSharpier format/check is scoped to the four in-scope files, not repo-wide `.`.** CLAUDE.md's
   literal C# Toolchain step 1 is `dotnet tool run csharpier format .`, but a repo-wide mutating
   format pass could silently rewrite an unrelated file that happens to be unformatted at the
   merge-base, which would violate the scope-lock boundary above (spec.md AC "No file under
   QuickFiler/Legacy/ ... is modified by this change"). Both the format and the read-only check
   commands in Phase 4 name the four in-scope paths explicitly instead of `.`.
2. **The CUT3 `vstest.console.exe <QuickFiler.Test assembly path> /EnableCodeCoverage` requirement
   is satisfied via `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test`.**
   That script wraps the same `vstest.console.exe` invocation inside `dotnet-coverage collect
   --output-format cobertura`, which is the repository's existing mechanism for turning a
   coverage-enabled MSTest run into a numeric, greppable coverage figure (Cobertura `line-rate`).
   The raw VS binary `.coverage` format produced by a bare `/EnableCodeCoverage` flag has no
   scriptable numeric-extraction path in this environment, so it cannot satisfy the Coverage
   Evidence Contract's numeric-value requirement on its own.
3. **No new regression test is authored.** Per CLAUDE.md's Bugfix Workflow and the delegation
   prompt's explicit instruction: the three existing test methods
   (`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`,
   `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine`,
   `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`) already assert the pre-fix
   12-hour literal and will fail the instant the production format string changes without a
   matching test-literal update; they are the regression tests for this defect. Phase 0 captures a
   deterministic, command-based baseline proving they PASS under the pre-fix literal (the closest
   analogue to a "fail-before" demonstration this defect admits, since the fix and the test-literal
   correction are one inseparable edit set — see spec.md §Root Cause Analysis and §Test Strategy).
4. **Every evidence artifact is written under `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/<kind>/`.**
   No `artifacts/` sub-path is used for evidence. `<kind>` is one of `baseline`, `regression-testing`,
   `qa-gates`, `other`.
5. **TRX logging is not used.** `.gitignore` does not exclude `*.trx` (verified: `.gitignore:38-40`
   covers `[Tt]est[Rr]esult*/` and `[Bb]uild[Ll]og.*`; `.gitignore:139-141` covers `*.coverage` /
   `*.coveragexml`; no `*.trx` entry exists). A committed TRX would also embed the account and
   machine name in its default filename. Evidence artifacts instead record the plain-text
   pass/fail/skipped summary line vstest prints to the console, plus the Cobertura XML's numeric
   `line-rate`, neither of which carries host-identifying data.
6. **Every command below is run unconditionally when its task executes; no `EXIT_CODE: SKIPPED`
   outcome is valid for any command-bearing task in this plan.**

---

### Phase 0 — Policy Read, Toolchain Bootstrap & Baseline Capture

- [x] [P0-T1] Read `CLAUDE.md` in full (repository root). Acceptance: the file has been read
      end-to-end in this session; no summarization substitutes for the read.
- [x] [P0-T2] Read `.claude/rules/general-code-change.md` in full. Acceptance: file read end-to-end.
- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` in full. Acceptance: file read end-to-end.
- [x] [P0-T4] Read `.claude/rules/csharp.md` in full. Acceptance: file read end-to-end.
- [x] [P0-T5] Read `.claude/rules/tonality.md` in full. Acceptance: file read end-to-end.
- [x] [P0-T6] Confirm `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/issue.md`,
      `.../spec.md`, and
      `.../research/2026-09-02T08-47-twelve-hour-time-format-research.md` have been read in full,
      then write the evidence artifact
      `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/phase0-instructions-read.<timestamp>.md`
      containing `Timestamp:`, `Policy Order:` (CLAUDE.md, general-code-change.md,
      general-unit-test.md, csharp.md, tonality.md), and an explicit list of all eight files read
      in P0-T1 through this task. Acceptance: the artifact exists and contains all required fields.
- [x] [P0-T7] Create the evidence directory structure by writing a `.gitkeep`-style placeholder is
      NOT required; instead create the four directories directly via:
      ```
      New-Item -ItemType Directory -Force -Path `
        'docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline', `
        'docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/regression-testing', `
        'docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates', `
        'docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/other' | Out-Null
      ```
      Acceptance: `Test-Path` returns `True` for all four directories.
- [x] [P0-T8] Bootstrap the pinned .NET SDK: `pwsh -File scripts/vscode/Install-RepoDotNetSdk.ps1`.
      Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `.../evidence/baseline/p0-t8-sdk-bootstrap.<timestamp>.md`. Acceptance: `EXIT_CODE: 0`.
- [x] [P0-T9] Restore the pinned CSharpier tool manifest: `dotnet tool restore` (run from
      repository root, where `dotnet-tools.json` lives). Record the same four fields in
      `.../evidence/baseline/p0-t9-tool-restore.<timestamp>.md`. Acceptance: `EXIT_CODE: 0`.
- [x] [P0-T10] Restore NuGet/`packages.config` dependencies:
      `pwsh -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU'`.
      Record the same four fields in `.../evidence/baseline/p0-t10-nuget-restore.<timestamp>.md`.
      Acceptance: `EXIT_CODE: 0`.
- [x] [P0-T11] Produce a baseline build so `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` exists
      before any source edit:
      ```
      $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
      $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
      & $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
      ```
      Record the same four fields plus `Output Summary:` (build succeeded / error count) in
      `.../evidence/baseline/p0-t11-baseline-build.<timestamp>.md`. Acceptance: `EXIT_CODE: 0` and
      `Test-Path QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` returns `True`.
- [x] [P0-T12] Capture the pre-edit literal-presence baseline with a fixed-string, case-sensitive
      search:
      ```
      Select-String -Path 'QuickFiler/Controllers/QfcHomeController.Metrics.cs' -SimpleMatch -CaseSensitive 'hh:mm'
      Select-String -Path 'QuickFiler/Controllers/EfcHomeController.Metrics.cs' -SimpleMatch -CaseSensitive 'hh:mm'
      Select-String -Path 'QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs' -SimpleMatch -CaseSensitive 'hh:mm'
      Select-String -Path 'QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs' -SimpleMatch -CaseSensitive 'hh:mm'
      ```
      `-CaseSensitive` is required because `Select-String` is case-insensitive by default, and
      `QuickFiler/Controllers/EfcHomeController.Metrics.cs:119`'s untouched
      `itemInfo.SentDate.ToString("HH:mm:ss")` call contains the substring `HH:mm`, which a
      case-insensitive search would conflate with the pre-fix `hh:mm` literal this task is
      measuring. Record the per-file line numbers and total count in
      `.../evidence/baseline/p0-t12-literal-presence.<timestamp>.md`. Acceptance: the recorded
      counts are exactly 3 (lines 46, 48, 127) for `QfcHomeController.Metrics.cs`, exactly 1 (line
      96) for `EfcHomeController.Metrics.cs`, exactly 4 (lines 227, 243, 265, 278) for
      `QfcHomeControllerMetricsTests.cs`, and exactly 0 for `EfcHomeControllerMetricsTests.cs` —
      matching the verified current-tree state cited in spec.md and research.md.
- [x] [P0-T13] Capture the read-only CSharpier baseline scoped to the four in-scope files:
      ```
      dotnet tool run csharpier check `
        QuickFiler/Controllers/QfcHomeController.Metrics.cs `
        QuickFiler/Controllers/EfcHomeController.Metrics.cs `
        QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs `
        QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs
      ```
      Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (exit code and, if
      non-zero, the reported unformatted-file list verbatim) in
      `.../evidence/baseline/p0-t13-csharpier-baseline.<timestamp>.md`. This is an observation
      task; a non-zero exit code here is recorded as the pre-existing drift state and does not
      block the plan (Phase 4's format task normalizes it).
- [x] [P0-T14] Run the deterministic pre-edit regression baseline naming the two affected test
      classes, proving both currently PASS under the pre-fix 12-hour literal:
      ```
      $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
      $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
      & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
        /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
        /TestCaseFilter:"FullyQualifiedName~QfcHomeControllerMetricsTests|FullyQualifiedName~EfcHomeControllerMetricsTests"
      ```
      Record `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (the vstest console
      pass/fail/skipped/total summary line, verbatim) in
      `.../evidence/baseline/p0-t14-scoped-regression-baseline.<timestamp>.md`. Acceptance:
      `EXIT_CODE: 0` and the summary line reports 0 failed. This is the fail-before-alternative
      evidence described in Plan-Level Decision 3: it demonstrates the sites' current PASS state
      under the literal this plan is about to change.
- [x] [P0-T15] Run the full-assembly, coverage-enabled baseline:
      ```
      pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test `
        -Configuration Debug `
        -CoverageOutput 'docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml'
      ```
      Record `Timestamp:`, `Command:`, `EXIT_CODE:` (from the `pwsh` process; the script `throw`s
      on a non-zero inner exit rather than propagating it, so treat any non-zero `pwsh` exit as a
      hard stop for this task), and `Output Summary:` containing the numeric `line-rate` read from
      the produced Cobertura XML's root `<coverage>` element (expressed as a percentage) and the
      vstest pass/fail/skipped/total summary line from the script's console output, in
      `.../evidence/baseline/p0-t15-coverage-baseline.<timestamp>.md`. If `Assert-CoberturaLineCoverageThreshold`
      throws below the repository's 80% floor before the post-processed Cobertura file is written
      back (the raw, un-post-processed output from `dotnet-coverage collect` already exists at that
      path when the exception is thrown), record the thrown percentage verbatim as the baseline
      figure and continue — that is a pre-existing
      repository-wide condition, not a regression introduced by this change, and is not gated by
      this plan (this plan's own three changed lines are already covered by existing passing
      tests, per spec.md §Test Strategy).
- [x] [P0-T16] Fetch and record the merge-base anchor used by every later scope-boundary diff gate:
      ```
      git fetch origin main
      git merge-base HEAD origin/main
      ```
      Record the resulting SHA in `.../evidence/baseline/p0-t16-merge-base.<timestamp>.md` under a
      field `MergeBaseSha:`. Acceptance: the command exits 0 and prints exactly one 40-character
      SHA. Every later task in this plan that runs a ref-anchored diff (for example
      `git diff <merge-base>...HEAD`) recomputes `git merge-base HEAD origin/main` independently
      rather than reading this file, so this task is a traceability record, not a shared-state
      dependency.
- [x] [P0-T17] Commit the Phase 0 planning and baseline evidence:
      ```
      git add docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/
      git status --porcelain
      git commit -m "docs(645): Phase 0 baseline evidence for twelve-hour time-format fix" `
        -m "Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>" `
        -m "Claude-Session: https://claude.ai/code/session_01LTjXvNFHVh7Fo7kYGgWsx2"
      ```
      Acceptance: `git commit` exits 0 and a subsequent `git status --porcelain` scoped to
      `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/` prints
      nothing.

---

### Phase 1 — Production Format-String Fix

- [x] [P1-T1] In `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, change line 48 from
      `dataLineBeg = $"{now:MM/dd/yyyy},{now:hh:mm},";` to
      `dataLineBeg = $"{now:MM/dd/yyyy},{now:HH:mm},";` (the interpolated `dataLineBeg` assignment
      inside `QuickFileMetrics_WRITE(string filename)`). Acceptance: `(Get-Content
      'QuickFiler/Controllers/QfcHomeController.Metrics.cs')[47]` equals exactly
      `            dataLineBeg = $"{now:MM/dd/yyyy},{now:HH:mm},";` and every other line in the
      file is byte-identical to its pre-edit content.
- [x] [P1-T2] In `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, change line 127 from
      `curTimeText = now.ToString("hh:mm");` to `curTimeText = now.ToString("HH:mm");` (inside
      `WriteMetricsAsync`). Acceptance: `(Get-Content
      'QuickFiler/Controllers/QfcHomeController.Metrics.cs')[126]` equals exactly
      `            curTimeText = now.ToString("HH:mm");` and no other line in the file changed
      relative to its state after P1-T1.
- [x] [P1-T3] In `QuickFiler/Controllers/EfcHomeController.Metrics.cs`, change line 96 from
      `var curTimeText = currentDateTime.ToString("hh:mm");` to
      `var curTimeText = currentDateTime.ToString("HH:mm");` (inside
      `BuildQuickFileMetricLines`). Acceptance: `(Get-Content
      'QuickFiler/Controllers/EfcHomeController.Metrics.cs')[95]` equals exactly
      `            var curTimeText = currentDateTime.ToString("HH:mm");` and no other line in the
      file changed.

---

### Phase 2 — Test Literal & Doc-Comment Updates

- [x] [P2-T1] In `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, change line 243
      (inside `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`) from
      `expectedLocal.ToString("MM/dd/yyyy") + "," + expectedLocal.ToString("hh:mm") + ",";` to
      `expectedLocal.ToString("MM/dd/yyyy") + "," + expectedLocal.ToString("HH:mm") + ",";`.
      Acceptance: `(Get-Content
      'QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs')[242]` equals exactly
      `                expectedLocal.ToString("MM/dd/yyyy") + "," + expectedLocal.ToString("HH:mm") + ",";`
      and no other line in the file changed.
- [x] [P2-T2] In the same file, change line 278 (inside
      `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine`) with the identical substitution as
      P2-T1. Acceptance: `(Get-Content
      'QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs')[277]` equals exactly
      `                expectedLocal.ToString("MM/dd/yyyy") + "," + expectedLocal.ToString("HH:mm") + ",";`
      and no other line in the file changed relative to its state after P2-T1.
- [x] [P2-T3] In the same file, change line 227 (the XML doc comment on
      `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`) from
      `/// ("MM/dd/yyyy","hh:mm") and the OlEndTime passed to GetMoveDiagnostics must reflect the`
      to `/// ("MM/dd/yyyy","HH:mm") and the OlEndTime passed to GetMoveDiagnostics must reflect the`.
      Acceptance: `(Get-Content
      'QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs')[226]` equals exactly
      `        /// ("MM/dd/yyyy","HH:mm") and the OlEndTime passed to GetMoveDiagnostics must reflect the`
      and no other line changed. This edit is comment-accuracy only and does not affect test
      correctness (spec.md §Scope & Non-Goals).
- [x] [P2-T4] In the same file, change line 265 (the XML doc comment on
      `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine`) from
      `/// the injected <see cref="TimeProvider"/>. The dataLineBeg ("MM/dd/yyyy","hh:mm") and the`
      to
      `/// the injected <see cref="TimeProvider"/>. The dataLineBeg ("MM/dd/yyyy","HH:mm") and the`.
      Acceptance: `(Get-Content
      'QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs')[264]` equals exactly
      `        /// the injected <see cref="TimeProvider"/>. The dataLineBeg ("MM/dd/yyyy","HH:mm") and the`
      and no other line changed.
- [x] [P2-T5] In `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`, change line 53
      (the asserted literal in `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`)
      from
      `"07/04/2026,01:05,Quarterly Update,SingleSorted,120,2.00,Recipient,Sender,Email,Archive/Target,06/30/2026,09:45:10"`
      to
      `"07/04/2026,13:05,Quarterly Update,SingleSorted,120,2.00,Recipient,Sender,Email,Archive/Target,06/30/2026,09:45:10"`
      (only the time-of-day field changes; every other field, including the already-24-hour
      `SentDate` field `09:45:10`, is unchanged). Acceptance: `(Get-Content
      'QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs')[52]` equals exactly
      `                    "07/04/2026,13:05,Quarterly Update,SingleSorted,120,2.00,Recipient,Sender,Email,Archive/Target,06/30/2026,09:45:10"`
      and no other line in the file changed.

---

### Phase 3 — Scope-Boundary & Regression Verification

- [x] [P3-T1] Re-run the fixed-string search from P0-T12 against the same four files, now with
      `-CaseSensitive` (see P0-T12 for why the flag is required):
      ```
      Select-String -Path 'QuickFiler/Controllers/QfcHomeController.Metrics.cs' -SimpleMatch -CaseSensitive 'hh:mm'
      Select-String -Path 'QuickFiler/Controllers/EfcHomeController.Metrics.cs' -SimpleMatch -CaseSensitive 'hh:mm'
      Select-String -Path 'QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs' -SimpleMatch -CaseSensitive 'hh:mm'
      Select-String -Path 'QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs' -SimpleMatch -CaseSensitive 'hh:mm'
      ```
      Record results in `.../evidence/regression-testing/p3-t1-zero-hit-verification.<timestamp>.md`.
      Acceptance: `EfcHomeController.Metrics.cs`, `QfcHomeControllerMetricsTests.cs`, and
      `EfcHomeControllerMetricsTests.cs` each return zero matches. `QfcHomeController.Metrics.cs`
      returns exactly one match, at line 46 (`//var curTimeText = DateTime.Now.ToString("hh:mm");`),
      the commented-out dead-code line spec.md excludes from scope and this plan never edits. Down
      from the P0-T12 baseline of 3, 1, 4, 0 (total 8), 7 of the 8 occurrences are eliminated; the
      one remaining occurrence at line 46 is the expected, correct outcome, not a defect.
- [x] [P3-T2] Verify none of the three fixed production lines gained a `CultureInfo` argument:
      ```
      (Get-Content 'QuickFiler/Controllers/QfcHomeController.Metrics.cs')[47]
      (Get-Content 'QuickFiler/Controllers/QfcHomeController.Metrics.cs')[126]
      (Get-Content 'QuickFiler/Controllers/EfcHomeController.Metrics.cs')[95]
      ```
      Record the three literal lines in
      `.../evidence/regression-testing/p3-t2-no-cultureinfo-added.<timestamp>.md`. Acceptance: all
      three lines match the exact post-edit text asserted in P1-T1, P1-T2, and P1-T3 respectively,
      and none of the three contains the substring `CultureInfo`.
- [x] [P3-T3] Run the scoped two-class regression run post-edit:
      ```
      $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
      $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
      & $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
      $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
      & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
        /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
        /TestCaseFilter:"FullyQualifiedName~QfcHomeControllerMetricsTests|FullyQualifiedName~EfcHomeControllerMetricsTests"
      ```
      Record `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (vstest pass/fail/skipped/
      total summary line, verbatim) in
      `.../evidence/regression-testing/p3-t3-scoped-regression-postedit.<timestamp>.md`.
      Acceptance: `EXIT_CODE: 0` and 0 failed, confirming the two clock-seam test methods and the
      EFC fixed-clock test method pass under the corrected `HH:mm` rendering.
- [x] [P3-T4] Scope-boundary diff check, anchored to the merge-base:
      ```
      $mergeBase = git merge-base HEAD origin/main
      git diff --name-only $mergeBase..HEAD
      git status --porcelain
      ```
      Record both outputs in
      `.../evidence/regression-testing/p3-t4-scope-boundary-diff.<timestamp>.md`. Acceptance: the
      `git diff --name-only` output, unioned with any untracked paths shown by `git status
      --porcelain`, contains only: the four in-scope files listed in the Scope Lock section above,
      plus paths under `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/`
      (this plan file and its evidence), plus the single pre-existing untracked path
      `docs/features/potential/promoted/2026-09-02-quickfiler-date-time-format-missing-invariant-culture.md`
      (a queued sibling-issue-#742 promotion record that predates this plan, is unrelated to and
      untouched by every task here, and must not be added or committed by any task in this plan).
      No path under QuickFiler/Legacy/, no TaskVisualization/TaskViewer.Designer.cs, no path
      matching .claude/\*\*, .codex/\*\*, .agents/\*\*, and neither config/blast-radius.json nor
      config/orchestration-routing.json appears in either output.
- [ ] [P3-T5] Commit the Phase 1 and Phase 2 source edits:
      ```
      git add `
        QuickFiler/Controllers/QfcHomeController.Metrics.cs `
        QuickFiler/Controllers/EfcHomeController.Metrics.cs `
        QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs `
        QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs `
        docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/
      git status --porcelain
      git commit -m "fix(645): render QuickFiler/EFC session-metrics time-of-day as 24-hour HH:mm" `
        -m "Changes the three hh:mm format-string literals to HH:mm and updates the three dependent test literals, per issue #645 / spec.md." `
        -m "Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>" `
        -m "Claude-Session: https://claude.ai/code/session_01LTjXvNFHVh7Fo7kYGgWsx2"
      ```
      Acceptance: `git commit` exits 0, and `git diff --name-only HEAD~1..HEAD` lists exactly the
      four source files plus any Phase 3 evidence paths added in this same commit.

---

### Phase 4 — Final QA Loop

Restart rule for this phase: if any of P4-T1 through P4-T5 changes a tracked file's bytes (beyond
what its own task intends) or reports a non-zero exit code, stop, remediate, and restart the loop
from P4-T1. Do not proceed to P4-T6 until P4-T1 through P4-T5 have all completed cleanly in the
same pass. This restart rule does not apply to a coverage-threshold exception from
`Assert-CoberturaLineCoverageThreshold` in P4-T5, which is handled per P4-T5's own acceptance text.

- [ ] [P4-T1] Scoped CSharpier format pass. Capture SHA-256 hashes of the four in-scope files
      immediately before and immediately after running:
      ```
      $files = 'QuickFiler/Controllers/QfcHomeController.Metrics.cs', `
               'QuickFiler/Controllers/EfcHomeController.Metrics.cs', `
               'QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs', `
               'QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs'
      $before = $files | Get-FileHash -Algorithm SHA256
      dotnet tool run csharpier format @files
      $after = $files | Get-FileHash -Algorithm SHA256
      ```
      Record all eight hashes (four before, four after) and the console line CSharpier prints
      (`Formatted N files` — noted explicitly as a PROCESSED count, not a rewritten count) in
      `.../evidence/qa-gates/p4-t1-csharpier-format.<timestamp>.md`, plus a `RewrittenCount:` field
      computed as the number of files whose before/after hash differ. Acceptance: `EXIT_CODE: 0`
      for the `csharpier format` invocation, regardless of `RewrittenCount` value.
- [ ] [P4-T2] Scoped CSharpier read-only verification:
      ```
      dotnet tool run csharpier check `
        QuickFiler/Controllers/QfcHomeController.Metrics.cs `
        QuickFiler/Controllers/EfcHomeController.Metrics.cs `
        QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs `
        QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs
      ```
      Record the four evidence fields in
      `.../evidence/qa-gates/p4-t2-csharpier-check.<timestamp>.md`. Acceptance: `EXIT_CODE: 0`.
- [ ] [P4-T3] Analyzer rebuild:
      ```
      $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
      $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
      $beforeTime = (Get-Item QuickFiler.Test\bin\Debug\QuickFiler.Test.dll).LastWriteTimeUtc
      & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" `
        /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
      $afterTime = (Get-Item QuickFiler.Test\bin\Debug\QuickFiler.Test.dll).LastWriteTimeUtc
      ```
      Record the four evidence fields plus `AssemblyRebuilt: <bool>` (`$afterTime -gt $beforeTime`)
      in `.../evidence/qa-gates/p4-t3-analyzer-rebuild.<timestamp>.md`. Acceptance: `EXIT_CODE: 0`
      and `AssemblyRebuilt: True`.
- [ ] [P4-T4] Nullable rebuild:
      ```
      $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
      $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
      $beforeTime = (Get-Item QuickFiler.Test\bin\Debug\QuickFiler.Test.dll).LastWriteTimeUtc
      & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" `
        /p:TreatWarningsAsErrors=true
      $afterTime = (Get-Item QuickFiler.Test\bin\Debug\QuickFiler.Test.dll).LastWriteTimeUtc
      ```
      Record the four evidence fields plus `AssemblyRebuilt: <bool>` in
      `.../evidence/qa-gates/p4-t4-nullable-rebuild.<timestamp>.md`. Acceptance: `EXIT_CODE: 0`
      and `AssemblyRebuilt: True`. Do NOT add `/p:Nullable=enable` (CLAUDE.md C#1 explicitly
      prohibits it as a solution-wide opt-in the CI command omits).
- [ ] [P4-T5] Full-assembly, coverage-enabled final run:
      ```
      pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test `
        -Configuration Debug `
        -CoverageOutput 'docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml'
      ```
      Record `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (numeric `line-rate`
      from the produced Cobertura XML, expressed as a percentage, plus the vstest
      pass/fail/skipped/total summary line) in
      `.../evidence/qa-gates/p4-t5-coverage-final.<timestamp>.md`. Acceptance: `EXIT_CODE: 0` and
      0 failed tests in the summary line. This satisfies the spec.md AC requiring the full
      `QuickFiler.Test` assembly to be green under a coverage-enabled run (Plan-Level Decision 2
      records why this script, not a bare `/EnableCodeCoverage` flag, is the command used). If
      `Assert-CoberturaLineCoverageThreshold` throws below the repository's 80% floor, apply the
      same handling as P0-T15: record the thrown percentage verbatim as the final-run figure in
      `Output Summary:`, treat this task as complete rather than triggering the Phase 4 restart
      rule, and carry the recorded percentage into P4-T6's `FinalLineRate:` field. This is the same
      pre-existing, repository-wide condition P0-T15 addresses, which this plan's three-line
      format-string change neither introduces nor can remediate.
- [ ] [P4-T6] Coverage delta/threshold verification: read the numeric `line-rate` recorded in
      P0-T15 (baseline) and P4-T5 (final) and record both figures side by side, plus the signed
      delta, in `.../evidence/qa-gates/p4-t6-coverage-delta.<timestamp>.md` with fields
      `BaselineLineRate:`, `FinalLineRate:`, `Delta:`. Acceptance: `FinalLineRate` is not lower
      than `BaselineLineRate` by more than 0.0 percentage points (i.e., `Delta >= 0`), consistent
      with spec.md §Test Strategy's statement that every changed line is already exercised by an
      existing, passing test and no coverage regression is expected.
- [ ] [P4-T7] Record the single-pass completion declaration for the toolchain loop: write
      `.../evidence/qa-gates/p4-t7-toolchain-single-pass.<timestamp>.md` stating that P4-T1
      through P4-T6 all recorded `EXIT_CODE: 0` (or the passing threshold for P4-T5 and P4-T6 as
      described in their acceptance text) without any task in this phase triggering the restart
      rule stated at the top of Phase 4. Acceptance: the
      artifact names all six preceding tasks by ID and states, for each, its recorded `EXIT_CODE`
      or pass/fail outcome, with zero restarts recorded.
- [ ] [P4-T8] Commit the Phase 4 QA-gate evidence:
      ```
      git add docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/
      git status --porcelain
      git commit -m "docs(645): Phase 4 final-QA-loop evidence for twelve-hour time-format fix" `
        -m "Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>" `
        -m "Claude-Session: https://claude.ai/code/session_01LTjXvNFHVh7Fo7kYGgWsx2"
      ```
      Acceptance: `git commit` exits 0 and a subsequent `git status --porcelain` scoped to
      `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/`
      prints nothing.

---

### Phase 5 — PR Notes, Acceptance Criteria Check-off & Final Wrap-up

- [ ] [P5-T1] Draft the PR body content and write it to
      `.../evidence/other/pr-body-draft.<timestamp>.md`. The draft MUST include, verbatim, a
      sentence stating that this change alters the emitted session-metrics CSV's time-of-day
      column content (per spec.md §Data / API / Config Impact and §Risks & Mitigations), for
      example: "This change alters the emitted session-metrics CSV: the time-of-day column now
      renders on a 24-hour clock (`HH:mm`) instead of the previous ambiguous 12-hour rendering
      (`hh:mm`, no AM/PM designator)." Acceptance: the artifact exists and contains a sentence
      matching this content requirement; the orchestrator's subsequent `pr-author` invocation must
      reuse this language in the actual PR description.
- [ ] [P5-T2] Check off AC1 in `spec.md`: change the line
      `` - [ ] `QuickFiler/Controllers/QfcHomeController.Metrics.cs:48` renders the time-of-day field ``
      to
      `` - [x] `QuickFiler/Controllers/QfcHomeController.Metrics.cs:48` renders the time-of-day field ``
      (checkbox marker only; no other character on the line changes). Acceptance: P1-T1's
      post-edit verification passed and the checkbox now reads `[x]`.
- [ ] [P5-T3] Check off AC2 in `spec.md`: change the line
      `` - [ ] `QuickFiler/Controllers/QfcHomeController.Metrics.cs:127` renders `curTimeText` using ``
      to
      `` - [x] `QuickFiler/Controllers/QfcHomeController.Metrics.cs:127` renders `curTimeText` using ``.
      Acceptance: P1-T2's post-edit verification passed and the checkbox now reads `[x]`.
- [ ] [P5-T4] Check off AC3 in `spec.md`: change the line
      `` - [ ] `QuickFiler/Controllers/EfcHomeController.Metrics.cs:96` renders `curTimeText` using ``
      to
      `` - [x] `QuickFiler/Controllers/EfcHomeController.Metrics.cs:96` renders `curTimeText` using ``.
      Acceptance: P1-T3's post-edit verification passed and the checkbox now reads `[x]`.
- [ ] [P5-T5] Check off AC4 in `spec.md`: change the line
      `` - [ ] `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (`expectedDataLineBeg` at ``
      to
      `` - [x] `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` (`expectedDataLineBeg` at ``.
      Acceptance: P2-T1 and P2-T2's post-edit verifications passed and P3-T3 recorded 0 failed for
      both test methods; the checkbox now reads `[x]`.
- [ ] [P5-T6] Check off AC5 in `spec.md`: change the line
      `` - [ ] `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` (line 53) asserts the ``
      to
      `` - [x] `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` (line 53) asserts the ``.
      Acceptance: P2-T5's post-edit verification passed and P3-T3 recorded 0 failed for
      `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`; the checkbox now reads
      `[x]`.
- [ ] [P5-T7] Check off AC6 in `spec.md`: change the line
      `- [ ] No file under QuickFiler/Legacy/, no TaskVisualization/TaskViewer.Designer.cs, and no`
      to
      `- [x] No file under QuickFiler/Legacy/, no TaskVisualization/TaskViewer.Designer.cs, and no`.
      Acceptance: P3-T4's scope-boundary diff check passed with none of the forbidden paths
      present; the checkbox now reads `[x]`.
- [ ] [P5-T8] Check off AC7 in `spec.md`: change the line
      `` - [ ] None of the three fixed call sites gain a `CultureInfo.InvariantCulture` argument (or any ``
      to
      `` - [x] None of the three fixed call sites gain a `CultureInfo.InvariantCulture` argument (or any ``.
      Acceptance: P3-T2's verification passed (none of the three lines contains `CultureInfo`); the
      checkbox now reads `[x]`.
- [ ] [P5-T9] Check off AC8 in `spec.md`: change the line
      `` - [ ] The full `QuickFiler.Test` assembly is green after the changes above (`vstest.console.exe` ``
      to
      `` - [x] The full `QuickFiler.Test` assembly is green after the changes above (`vstest.console.exe` ``.
      Acceptance: P4-T5 recorded `EXIT_CODE: 0` and 0 failed tests; the checkbox now reads `[x]`.
- [ ] [P5-T10] Check off AC9 in `spec.md`: change the line
      `- [ ] Full toolchain pass completed in order (CSharpier format/check, analyzer rebuild, nullable`
      to
      `- [x] Full toolchain pass completed in order (CSharpier format/check, analyzer rebuild, nullable`.
      Acceptance: P4-T7's single-pass completion declaration recorded zero restarts across P4-T1
      through P4-T6; the checkbox now reads `[x]`.
- [ ] [P5-T11] Check off AC10 in `spec.md`: change the line
      `- [ ] The PR description explicitly states that this change alters the emitted session-metrics`
      to
      `- [x] The PR description explicitly states that this change alters the emitted session-metrics`.
      Acceptance: P5-T1's PR body draft contains the required sentence; the checkbox now reads
      `[x]`.
- [ ] [P5-T12] AC reconciliation: read the `## Acceptance Criteria` section of `spec.md` in full
      and confirm all 10 items now read `- [x]`, and that no character other than the checkbox
      marker changed on any of the 10 lines relative to the pre-Phase-5 text quoted in this plan's
      Phase 5 tasks above. Record the result in
      `.../evidence/other/p5-t12-ac-reconciliation.<timestamp>.md`. Acceptance: all 10 checkboxes
      read `[x]` and a line-by-line diff against the quoted pre-edit text shows only the marker
      changed on each of the 10 lines.
- [ ] [P5-T13] Commit the `spec.md` AC check-offs and the PR draft evidence:
      ```
      git add `
        docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/spec.md `
        docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/
      git status --porcelain
      git commit -m "docs(645): check off spec.md acceptance criteria and record PR-body draft" `
        -m "Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>" `
        -m "Claude-Session: https://claude.ai/code/session_01LTjXvNFHVh7Fo7kYGgWsx2"
      ```
      Acceptance: `git commit` exits 0.
- [ ] [P5-T14] Final clean-tree verification:
      ```
      git status --porcelain
      ```
      Acceptance: the command's only output, if any, is the single pre-existing untracked line for
      `docs/features/potential/promoted/2026-09-02-quickfiler-date-time-format-missing-invariant-culture.md`
      (the queued sibling-issue-#742 promotion record described in P3-T4, not touched by this
      plan); this confirms every change this plan produced — the four source edits, the plan file
      itself, and every evidence artifact — has been committed.

---

## Planner Adversarial Self-Review

`SELF-REVIEW: RE-DERIVED THIS PASS`

**Revision round (this pass).** This round applied two preflight-directed deltas from round-3
preflight: (1) the `-CaseSensitive` additions to every `Select-String` invocation in P0-T12 and
P3-T1, and (2) the untracked-file allowance for the queued sibling-issue-#742 promotion record in
P3-T4 and P5-T14. Per the Planner Adversarial Self-Review rule, the citations these edits touched
were re-derived directly against the current tree in this pass, not carried forward from the
round-1 delta bucket below (which documents a different, earlier round's edits) or from the
original-authoring-pass citation list:

1. `QuickFiler/Controllers/EfcHomeController.Metrics.cs:119` — re-read directly in this pass;
   confirmed the live text is still exactly
   `+ $"{itemInfo.SentDate.ToString("HH:mm:ss")}"`, untouched by this plan (Phase 1 only edits
   `EfcHomeController.Metrics.cs:96`). This confirms why `-CaseSensitive` is required on the P0-T12
   and P3-T1 `Select-String` calls: a case-insensitive fixed-string search for `hh:mm` would also
   match the `HH:mm` substring inside this untouched, already-correct site's `HH:mm:ss` literal,
   inflating the match count against a file that contains zero instances of the pre-fix lowercase
   literal.
2. PowerShell's `Select-String` cmdlet is case-insensitive by default when no `-CaseSensitive`
   switch is supplied. This is documented PowerShell cmdlet behavior and requires no repository file
   citation; it is the reason the round-2 delta added `-CaseSensitive` to every invocation in P0-T12
   and P3-T1 rather than relying on the cmdlet's default matching behavior.
3. `P0-T12` (lines 121-138) and `P3-T1` (lines 286-300) — both re-read directly in this pass. Each
   task contains exactly four `Select-String` invocations, and all eight (four in P0-T12, four in
   P3-T1) carry the `-SimpleMatch -CaseSensitive 'hh:mm'` argument pair; none of the eight omits
   `-CaseSensitive`. This confirms the round-2 delta was applied uniformly to every invocation in
   both tasks, with no site left on the case-insensitive default.
4. Scope-boundary untracked-file state — re-derived in this pass by direct filesystem check (`Glob`)
   rather than a `git status --porcelain` invocation, because no shell/command-execution tool was
   available to this planning session in this pass. Confirmed the single path
   `docs/features/potential/promoted/2026-09-02-quickfiler-date-time-format-missing-invariant-culture.md`
   exists on disk at the location P3-T4 and P5-T14 name, and confirmed the feature folder
   `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/` currently contains
   only `issue.md`, `spec.md`, `research/2026-09-02T08-47-twelve-hour-time-format-research.md`, and
   this plan file — no unexpected additional path and no pre-existing `evidence/` tree, consistent
   with Phase 0 not yet having executed. This is a best-effort substitute for a `git status
   --porcelain` run and does not by itself distinguish tracked from untracked state; the executor's
   own P3-T4/P5-T14 `git status --porcelain` invocations remain the authoritative, command-based
   confirmation at execution time. No new extraneous path was observed beyond the one already named
   in P3-T4/P5-T14.

**Prior round (first revision pass).** This round applied three preflight-directed deltas: (1) P3-T1's
acceptance clause, (2) P4-T5's acceptance clause plus the Phase 4 restart-rule preamble plus
P4-T7's acceptance text, and (3) P0-T15's exception-timing wording. Per the Planner Adversarial
Self-Review rule, the citations these edits touched were re-derived directly against the current
tree in this pass, not carried forward from the prior round's citation list below (items 1-25),
and the sibling lines/tasks in the same region were re-checked alongside each:

1. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:46` — re-read in this pass; confirmed the
   live text is exactly `            //var curTimeText = DateTime.Now.ToString("hh:mm");`, unchanged
   by Phase 1 (which only edits lines 48 and 127), so it remains the one expected non-zero match
   P3-T1's revised acceptance clause names.
2. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` and
   `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` and
   `QuickFiler/Controllers/EfcHomeController.Metrics.cs` — re-derived via the P3-T1 arithmetic
   check in this pass: the P0-T12 baseline counts are 3 for `QfcHomeController.Metrics.cs` (lines
   46, 48, 127), 1 for `EfcHomeController.Metrics.cs` (line 96), 4 for
   `QfcHomeControllerMetricsTests.cs` (lines 227, 243, 265, 278), and 0 for
   `EfcHomeControllerMetricsTests.cs` — total 8. Phase 1/2 edits the three live sites (48, 127,
   Efc:96) and the four test-literal/doc-comment sites (227, 243, 265, 278) — 7 sites — and never
   edits the commented line 46. 7 of the P0-T12 baseline's 8 total occurrences are therefore
   eliminated and line 46 is the sole survivor, matching the revised P3-T1 acceptance text.
3. Phase 3 sibling tasks `P3-T2` (lines 296-305) and `P3-T3` (lines 306-320) — re-read in this pass;
   neither references the total-elimination-count language that P3-T1 revised, so neither required
   a corresponding edit.
4. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — re-read in full in this pass. Confirmed
   `Invoke-DotnetCoverageCollection` (lines 172-243) writes the raw Cobertura output via
   `dotnet-coverage collect --output <OutputPath>` before the caller resumes; back in
   `Invoke-MSTestWithCoverageMain` (lines 326-343), `Get-Content $resolvedOutputPath` (line 339)
   reads that already-written raw file, `Assert-CoberturaLineCoverageThreshold` (line 341) runs
   next and throws below the 80% floor, and `Set-Content` (line 343, the post-processed rewrite)
   never executes when it throws. This confirms both the P0-T15 wording correction (the raw file
   exists at the exception point; only the post-processed rewrite is skipped) and that P4-T5, which
   invokes this same script against the same `QuickFiler.Test` search root, is subject to the
   identical throw-before-rewrite mechanism, supporting the P4-T5 carve-out added this round.
5. `P0-T14` (lines 147-161) and `P0-T16` (lines 181-191) — re-read as P0-T15's immediate siblings in
   this pass; neither references the Cobertura-write-timing language P0-T15 revised, so neither
   required a corresponding edit.
6. `P4-T1` through `P4-T4` (lines 363-413) — re-read as the Phase 4 preamble's and P4-T5's sibling
   tasks in this pass; none of the four names `Assert-CoberturaLineCoverageThreshold` or a coverage
   threshold, so the new preamble carve-out sentence (scoped explicitly to P4-T5) does not affect
   their own restart-rule exposure.
7. `P4-T6` (lines 433-439) — re-read in this pass; its acceptance text (`Delta >= 0`) remains
   satisfiable when both `BaselineLineRate` and `FinalLineRate` are the identical thrown-exception
   percentage carried over from P0-T15 and P4-T5 respectively, so no edit to P4-T6 itself was
   required by this round's delta.

**Original authoring pass.** The citations below were re-derived directly against the
current tree in that pass (not carried forward from spec.md, issue.md, or research.md prose) and
remain valid for the plan regions this revision round did not touch:

1. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:46` — re-read; confirmed commented-out
   dead code `//var curTimeText = DateTime.Now.ToString("hh:mm");`, excluded from the fix scope.
2. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:48` — re-read; confirmed live text
   `dataLineBeg = $"{now:MM/dd/yyyy},{now:hh:mm},";`.
3. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:127` — re-read; confirmed live text
   `curTimeText = now.ToString("hh:mm");`.
4. `QuickFiler/Controllers/EfcHomeController.Metrics.cs:96` — re-read; confirmed live text
   `var curTimeText = currentDateTime.ToString("hh:mm");`.
5. `QuickFiler/Controllers/EfcHomeController.Metrics.cs:118-119` — re-read (sibling region);
   confirmed `SentDate` already renders `"MM/dd/yyyy"` / `"HH:mm:ss"` with no `CultureInfo`
   argument, supporting the "no CultureInfo addition" scope boundary.
6. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:227` — re-read; confirmed doc
   comment text containing `("MM/dd/yyyy","hh:mm")`.
7. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:242-243` — re-read; confirmed live
   assertion-construction text ending `expectedLocal.ToString("hh:mm") + ",";`.
8. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:265` — re-read; confirmed doc
   comment text containing `("MM/dd/yyyy","hh:mm")`.
9. `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:277-278` — re-read; confirmed
   identical live assertion-construction text as line 243.
10. `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs:25` — re-read (sibling); confirmed
    `MetricsNow = new DateTime(2026, 7, 4, 13, 5, 0)`.
11. `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs:53` — re-read; confirmed asserted
    literal contains `01:05` at the exact position claimed.
12. `QuickFiler.Test/QuickFiler.Test.csproj` — re-derived via grep; confirmed both
    `EfcHomeControllerMetricsTests.cs` and `QfcHomeControllerMetricsTests.cs` are registered
    `<Compile Include>` entries.
13. `TaskMaster.sln` — re-derived via grep; confirmed `QuickFiler.Test` is a registered project.
14. `QuickFiler.Test/QuickFiler.Test.csproj` — re-derived via grep; confirmed `<AssemblyName>`
    is `QuickFiler.Test`, `<TargetFrameworkVersion>` is `v4.8.1`, and the Debug|AnyCPU
    `<OutputPath>` is `bin\Debug\`.
15. `.gitignore:1-50` — re-read in full; confirmed `[Tt]est[Rr]esult*/` at line 39 and
    `[Bb]uild[Ll]og.*` at line 40 (bracketed-class forms), and no literal `TestResults` line.
16. `.gitignore:139-145` — re-derived via grep; confirmed `*.coverage` (140), `*.coveragexml`
    (141), and `coverage/*` (144) are ignored, and no `*.trx` entry exists anywhere in the file.
17. `scripts/vscode/Invoke-Restore.ps1` — re-read; confirmed parameters `-SolutionPath`
    (default `TaskMaster.sln`), `-Configuration` (default `Debug`), `-Platform` (default
    `Any CPU`), and confirmed it resolves `MSBuild.exe` via `vswhere.exe`.
18. `scripts/vscode/Install-RepoDotNetSdk.ps1` — re-read; confirmed it exists with no required
    parameters (defaults `-Version 8.0.205 -Architecture x64`).
19. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — re-read in full; confirmed `-SearchRoot`
    joins to `$repoRoot`, filters `*.Test.dll` under `\bin\$Configuration\`, wraps
    `vstest.console.exe` inside `dotnet-coverage collect --output-format cobertura`, appends
    `/TestCaseFilter:TestCategory!=LiveOutlook` internally, and throws inside
    `Assert-CoberturaLineCoverageThreshold` before the Koverage post-processing step when the
    measured line coverage is below the repository's 80% floor.
20. `coverage.config` — re-derived via glob; confirmed it exists at repository root (required by
    `Invoke-MSTestWithCoverage.ps1`).
21. `scripts/vscode/TaskMaster.cli.runsettings` — re-derived via glob; confirmed it exists.
22. `dotnet-tools.json` — re-derived via glob; confirmed it exists at repository root (pins
    CSharpier).
23. `.claude/rules/csharp.md` and the other four Phase-0-cited rule files — re-derived via glob
    listing of `.claude/rules/*.md`; confirmed all five exist.
24. `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/` — re-derived
    via glob; confirmed only `issue.md`, `spec.md`, `research/2026-09-02T08-47-twelve-hour-time-format-research.md`,
    and this plan file exist prior to this plan's Phase 0 evidence-directory-creation task (no
    pre-existing `evidence/` tree).
25. `spec.md:241-265` (`## Acceptance Criteria` block) — re-read verbatim in full; the 10
    check-off task quotations in Phase 5 above are copied character-for-character from this
    re-read, not from any earlier summary.

## Planner Internal Review Record

`PLANNER-INTERNAL-REVIEW: PASS`

`CITATION-TO-TREE: PASS`

`CITATION: QuickFiler/Controllers/QfcHomeController.Metrics.cs | line 48 (dataLineBeg interpolation, pre-edit "hh:mm")`
`CITATION: QuickFiler/Controllers/QfcHomeController.Metrics.cs | line 127 (curTimeText assignment, pre-edit "hh:mm")`
`CITATION: QuickFiler/Controllers/QfcHomeController.Metrics.cs | line 46 (commented dead code, excluded from scope)`
`CITATION: QuickFiler/Controllers/EfcHomeController.Metrics.cs | line 96 (curTimeText assignment, pre-edit "hh:mm")`
`CITATION: QuickFiler/Controllers/EfcHomeController.Metrics.cs | lines 118-119 (SentDate sibling convention, no CultureInfo)`
`CITATION: QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | line 227 (doc comment)`
`CITATION: QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | lines 242-243 (WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps)`
`CITATION: QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | line 265 (doc comment)`
`CITATION: QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | lines 277-278 (QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine)`
`CITATION: QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs | line 25 (MetricsNow fixture)`
`CITATION: QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs | line 53 (asserted literal)`
`CITATION: QuickFiler.Test/QuickFiler.Test.csproj | Compile Include entries for both test files`
`CITATION: TaskMaster.sln | QuickFiler.Test project registration`
`CITATION: .gitignore | lines 38-40, 139-145 (TestResults/build-log/coverage ignore rules, no *.trx entry)`
`CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | full file (coverage command mechanics)`
`CITATION: docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/spec.md | lines 241-265 (## Acceptance Criteria block, 10 items)`

`AC-TRACEABILITY: PASS`

`AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10`

`AC-MAPPING: AC1 | IMPLEMENTATION: P1-T1 | TESTS: P3-T3 | EVIDENCE: p3-t3-scoped-regression-postedit`
`AC-MAPPING: AC2 | IMPLEMENTATION: P1-T2 | TESTS: P3-T3 | EVIDENCE: p3-t3-scoped-regression-postedit`
`AC-MAPPING: AC3 | IMPLEMENTATION: P1-T3 | TESTS: P3-T3 | EVIDENCE: p3-t3-scoped-regression-postedit`
`AC-MAPPING: AC4 | IMPLEMENTATION: P2-T1,P2-T2 | TESTS: P3-T3 | EVIDENCE: p3-t3-scoped-regression-postedit`
`AC-MAPPING: AC5 | IMPLEMENTATION: P2-T5 | TESTS: P3-T3 | EVIDENCE: p3-t3-scoped-regression-postedit`
`AC-MAPPING: AC6 | IMPLEMENTATION: P3-T4 (verification-only, no code change) | TESTS: P3-T4 | EVIDENCE: p3-t4-scope-boundary-diff`
`AC-MAPPING: AC7 | IMPLEMENTATION: P3-T2 (verification-only, no code change) | TESTS: P3-T2 | EVIDENCE: p3-t2-no-cultureinfo-added`
`AC-MAPPING: AC8 | IMPLEMENTATION: P4-T5 | TESTS: P4-T5 | EVIDENCE: p4-t5-coverage-final`
`AC-MAPPING: AC9 | IMPLEMENTATION: P4-T1,P4-T2,P4-T3,P4-T4,P4-T5,P4-T6 | TESTS: P4-T7 | EVIDENCE: p4-t7-toolchain-single-pass`
`AC-MAPPING: AC10 | IMPLEMENTATION: P5-T1 | TESTS: P5-T12 | EVIDENCE: pr-body-draft`

`SCOPE-BOUNDARY: PASS`

`UNRESOLVED-GAPS: NONE`

`DIRECTIVE: PREFLIGHT VALIDATION ONLY`
