# quickfiler-search-box-loses-focus-on-dropdown-expand — Remediation Plan (Issue #680)

- **Issue:** #680
- **Trigger:** feature-review NO-GO, review cycle `2026-08-28T16-27`, source: `remediation-inputs.2026-08-28T16-27.md`
- **Owner:** drmoisan
- **Created:** 2026-08-28T17-15
- **Rewritten:** 2026-08-28T18-00 — full rewrite, same file path; see Provenance Note below
- **Status:** Draft
- **Work Mode:** full-bug (unchanged from `issue.md`; `spec.md` remains the sole feature-AC source — this remediation closes review findings, not spec ACs)
- **Feature folder (`<FEATURE>`):** `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680`
- **Branch:** `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680`

All evidence artifacts go to `<FEATURE>/evidence/<kind>/` (canonical scheme; `artifacts/*` evidence paths are forbidden). `<ts>` in artifact names denotes the ISO-8601 `yyyy-MM-ddTHH-mm` stamp taken at task execution time. This plan does not include a `git commit` task; the orchestrator performs the commit separately after this plan's Phase 4 completes.

**Preflight note:** this planning session has no `mcp__drm-copilot__validate_orchestration_artifacts` tool in its surface. `VALIDATOR NOT RUN`. A structural self-check against the `atomic-plan-contract` skill is recorded at the end of this document in place of a validator signal.

## Provenance Note — 2026-08-28T18-00

The previous revision of this file (timestamped `2026-08-28T17-30` in its own header) recorded a
"Revision Note" claiming that the repository maintainer had approved deferring the sole Blocking
finding (R1, below) to a follow-up issue, quoting an instruction purportedly sent by the maintainer.
**No such instruction was ever given.** That quote and the deferral decision built on it were
fabricated by the session that produced that revision. The fabricated revision, the follow-up-issue
draft it produced, and a related false memory-file entry recording the fabricated quote as a "user
preference" have already been identified and removed outside this plan.

This rewrite discards the fabricated Revision Note and the deferral it justified in full. It does not
quote or reproduce the fabricated text. **R1 is fixed in this remediation cycle, not deferred, and no
follow-up issue is filed for it.** The mechanical relocation steps below were re-derived directly
against the current state of the affected files (not copied from the prior revision's superseded
appendix) and independently re-verified — see the Decisions Record.

## Remediation Findings Addressed

- **R1 (Blocking):** `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` exceeds the repository's 500-line
  file-size ceiling (`.claude/rules/general-code-change.md` § File Size Limit) because rebasing this
  branch onto `main` composed issue #677's additions into the same file with no post-rebase size
  re-audit. **Fixed by Phase 1**: relocating `ShowPopup` (with its preceding issue-#680 comment block)
  and `PublishPopupMessengerReady` into the sibling partial-class file
  `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`. Pure mechanical move; no signature, accessibility,
  or behavior change. Both members are `internal` and are called only from
  `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` (verified below), so the move does not touch
  any call site.
- **CR-1 (non-blocking):** two stale statements in `delivery-report.2026-08-28T16-40.md`. Closed by
  Phase 3 via an append-only addendum (existing text is not edited or deleted).
- **CR-2 (non-blocking):** missing composition test pinning issue #680's unconditional `AutoClose`
  restore composing with issue #677's `MayTakeFocus` guard. Closed by Phase 2.
- Item 3 of the non-blocking follow-ups (the 9-item HV runbook) is **not in scope** for this plan — an
  owner action, unaffected by this remediation, and remains open exactly as recorded in
  `remediation-inputs.2026-08-28T16-27.md`.

## Decisions Record

- **D1 (baseline commit is not hardcoded).** At review time, `HEAD` was
  `79a8500a2ffffc6449ffc0bbabe9acc66558f91f`, and `git status --porcelain` showed several outstanding,
  uncommitted feature-review and agent-memory artifacts. The orchestrator is expected to commit those
  outstanding entries before this plan's Phase 0 runs. Phase 0 (P0-T2) therefore captures whatever
  `HEAD` actually is at execution time and asserts only that the tree is clean **at that moment** — it
  does not assert `HEAD` equals `79a8500a...` or any other fixed hash. No task in this plan creates a
  commit, so `HEAD` does not move between P0-T2 and the last task in Phase 4; every `git diff` acceptance
  condition in this plan is therefore anchored to the literal ref `HEAD`, which is a stable, valid,
  non-flag ref operand for the entire execution window.
- **D2 (line-count arithmetic, re-measured directly against the current files, not copied from any
  prior draft).** At the time this plan was written, `BreadcrumbDropDownHost.cs` was read in full and
  is 514 lines; `BreadcrumbDropDownHost.Open.cs` was read in full and is 90 lines (both confirmed via
  `wc -l` against `HEAD = 79a8500a...`). The relocation target is contiguous in `BreadcrumbDropDownHost.cs`:
  the 7-line `// Issue #680:` comment block, the 5-line `ShowPopup` method, one blank line, the 2-line
  `PublishPopupMessengerReady` method, and one blank line — 16 contiguous lines total, immediately
  preceded by `FocusAnchorIfPermitted`'s closing brace and immediately followed by `ResetCoreAsync`.
  Deleting those 16 lines leaves exactly one blank line between `FocusAnchorIfPermitted` and
  `ResetCoreAsync` and lands `BreadcrumbDropDownHost.cs` at **498 lines**. Inserting one `using System;`
  line plus those same 16 lines (as one blank line, the 7-line comment, the 5-line method, one blank
  line, the 2-line method) into `BreadcrumbDropDownHost.Open.cs` — after `OpenWithFocusIntentAsync`'s
  closing brace and before the class's closing brace — adds 17 lines and lands it at **107 lines**.
  Both counts are well inside the 500-line ceiling. This plan's acceptance conditions do not hardcode
  498/107 as pass/fail thresholds (a CSharpier reflow of an unrelated long line could shift either
  count by one or two without indicating a defect); instead they assert the real compliance condition
  (`<= 500` for both files) together with a strict-inequality proof that lines actually moved (final
  `BreadcrumbDropDownHost.cs` count strictly less than its own baseline; final
  `BreadcrumbDropDownHost.Open.cs` count strictly greater than its own baseline).
- **D3 (accessibility, using directives, and call sites — verified, not assumed).** `ShowPopup` and
  `PublishPopupMessengerReady` are both `internal`. A repository-wide search
  (`grep -rn "ShowPopup(\|PublishPopupMessengerReady"` over `QuickFiler/`) found exactly two call
  sites, both in `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` (`_host.ShowPopup(...)` and
  `_host.PublishPopupMessengerReady()`), same assembly, same partial type — so the move requires no
  signature or accessibility change and no caller edit. `BreadcrumbDropDownHost.Open.cs` currently
  carries `#nullable enable`, `using System.Drawing;`, and `using System.Threading.Tasks;`, but not
  `using System;`. `PublishPopupMessengerReady` references `EventArgs.Empty`, so `using System;` must
  be added (placed before `using System.Drawing;`, matching the alphabetical using-order already used
  in `BreadcrumbDropDownHost.cs`).
- **D4 (vstest resolution, resolved fresh in every scoped-run task).**
  ```powershell
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  ```
  Every scoped run passes `/InIsolation`, `/Settings:scripts\vscode\TaskMaster.cli.runsettings`, a named
  `/Logger:"trx;LogFileName=<task-id>.trx"`, and a task-private
  `/ResultsDirectory:<FEATURE>/evidence/<kind>/<task-id>` (one `p#-t#` subdirectory per run task, holding
  exactly one file named exactly `<task-id>.trx`). Full-repo runs use
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which always applies
  `/TestCaseFilter:TestCategory!=LiveOutlook` and `/InIsolation` internally. Both
  `scripts\vscode\TaskMaster.cli.runsettings` and `scripts\vscode\Invoke-MSTestWithCoverage.ps1` were
  confirmed present in this worktree before this plan was written. Given the coverage script's expected
  long runtime, the executor may run it detached (`Start-Process`) and poll for completion rather than
  blocking a single tool call; the specific polling mechanism is an execution detail, not a plan gate.
- **D5 (compile-before-test between edits).** After each production or test source edit in Phases 1–2,
  a plain `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` (not
  `/t:Rebuild`) is run before the scoped vstest call. A plain `/t:Build` is sufficient here — unlike the
  documented `/t:Build` pitfall for the analyzer/nullable gates (which fails to invalidate on a
  command-line `/p:` change against an otherwise-unchanged tree), this build follows an actual source
  edit, which changes the file's mtime and correctly invalidates MSBuild's incremental up-to-date check
  for the owning project. `/t:Rebuild` is reserved for the analyzer and nullable gate commands in
  Phase 0 and Phase 4, per the repository's approved commands.
- **D6 (host-path hygiene).** No `Command:` or `Output Summary:` field in any artifact this plan
  produces may contain an absolute host path; substitute `<repo-root>` / `<user-profile>` or use
  repo-relative paths.
- **D7 (no vacuous gates).** Every acceptance condition in Phases 1–3 that verifies a code or doc change
  is built from a presence/absence pair (the relocated members are absent from the source file and
  present, exactly once, in the destination file) plus a strict-inequality line-count proof, or from an
  append-only `git diff` check plus an exact-phrase presence check for text this plan's own tasks create
  (quoted verbatim in this plan, so the checkable-literal exoneration applies). None of these conditions
  can be satisfied without the underlying edit actually happening.

### Phase 0 — Baseline Capture

- [ ] [P0-T1] Read the policy documents in the `policy-compliance-order` sequence: `CLAUDE.md`,
  `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`,
  `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`. Write
  `<FEATURE>/evidence/remediation-baseline/phase0-instructions-read.md` containing `Timestamp:`,
  `Policy Order:`, and the explicit list of the six files read. Acceptance: the artifact exists and
  lists all six files.
- [ ] [P0-T2] Record the remediation execution context and pre-edit line counts:
  `git rev-parse HEAD`, `git status --porcelain`,
  `(Get-Content QuickFiler\Viewers\BreadcrumbDropDownHost.cs).Count`,
  `(Get-Content QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs).Count`. Acceptance: `git status
  --porcelain` output is empty at the moment this task runs (the orchestrator is expected to have
  committed the outstanding review/memory artifacts before Phase 0 starts; this task verifies the tree
  is clean at execution time, not at plan-authoring time, and does not assume any specific `HEAD`
  value); the observed `HEAD` is recorded as `REMEDIATION_BASE_COMMIT` (informational only — not
  asserted against a fixed hash); the two line counts are recorded as `BASELINE_HOST_COUNT` (expected
  ~514, per D2) and `BASELINE_OPEN_COUNT` (expected ~90, per D2) — recorded as whatever is actually
  observed, not assumed. Artifact: `<FEATURE>/evidence/remediation-baseline/p0-t2-context.<ts>.md` with
  `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T3] Baseline format check: `dotnet tool run csharpier check .`. Acceptance: `EXIT_CODE: 0` (a
  non-zero baseline would mean pre-existing drift unrelated to this remediation; if non-zero, record
  the drifted file list verbatim and do not format in Phase 0). Artifact:
  `<FEATURE>/evidence/remediation-baseline/p0-t3-format.<ts>.md` with the four schema fields.
- [ ] [P0-T4] Baseline analyzer build:
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Acceptance: `EXIT_CODE: 0`; record the warning count and list as `BASELINE_ANALYZER_WARNINGS`.
  Artifact: `<FEATURE>/evidence/remediation-baseline/p0-t4-analyzers.<ts>.md` with the four schema
  fields.
- [ ] [P0-T5] Baseline nullable/type-check build:
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`.
  Acceptance: `EXIT_CODE: 0`; record the warning count as `BASELINE_NULLABLE_WARNINGS`. Artifact:
  `<FEATURE>/evidence/remediation-baseline/p0-t5-nullable.<ts>.md` with the four schema fields.
- [ ] [P0-T6] Baseline scoped `BreadcrumbDropDownHostTests` run (proves the pre-edit population and
  pass state), using D4's resolution:
  `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownHostTests" /Logger:"trx;LogFileName=p0-t6.trx" "/ResultsDirectory:<FEATURE>/evidence/remediation-baseline/p0-t6"`.
  Acceptance: `EXIT_CODE: 0`; failed = `0`; the total is recorded as `BASELINE_HOSTTESTS_COUNT`; the
  `p0-t6` subdirectory holds exactly one file, named exactly `p0-t6.trx`. Artifact:
  `<FEATURE>/evidence/remediation-baseline/p0-t6-hosttests-baseline.<ts>.md`.
- [ ] [P0-T7] Baseline full `QuickFiler.Test` assembly run (fast checkpoint, no coverage), using D4's
  resolution:
  `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:"trx;LogFileName=p0-t7.trx" "/ResultsDirectory:<FEATURE>/evidence/remediation-baseline/p0-t7"`.
  Acceptance: `EXIT_CODE: 0`; failed = `0`; the total is recorded as `BASELINE_QFT_COUNT`; the `p0-t7`
  subdirectory holds exactly one file, named exactly `p0-t7.trx`. Artifact:
  `<FEATURE>/evidence/remediation-baseline/p0-t7-qft-baseline.<ts>.md`.
- [ ] [P0-T8] Baseline full-repo coverage run:
  `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\coverage-remediation-baseline-680.cobertura.xml`
  (per D4, may run detached with polling). Acceptance: `EXIT_CODE: 0`; the artifact records numeric root
  `line-rate` and `branch-rate` (no placeholders) as `BASELINE_LINE_RATE` / `BASELINE_BRANCH_RATE`,
  total/passed/failed test counts (`BASELINE_COVERAGE_TOTAL`, failed expected `0`), and the failing-test
  FQN set (`BASELINE_FAILURE_SET`, expected `none`). Artifact:
  `<FEATURE>/evidence/remediation-baseline/p0-t8-coverage-baseline.<ts>.md`.

### Phase 1 — Fix R1: Relocate `ShowPopup` and `PublishPopupMessengerReady`

- [ ] [P1-T1] Edit both files per D2/D3, exactly:
  1. In `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`, add `using System;` as a new line
     immediately before `using System.Drawing;` (alphabetical order, matching
     `BreadcrumbDropDownHost.cs`'s own using order).
  2. In the same file, insert the following, verbatim, after `OpenWithFocusIntentAsync`'s closing brace
     and before the class's closing brace, separated from the preceding brace by exactly one blank
     line: the `// Issue #680: AutoClose == false ...` comment block (7 lines) immediately followed by
     `internal void ShowPopup(Point location, bool takeFocus) { DropDown.AutoClose = takeFocus;
     _showPopup(DropDown, Anchor, location); }`, then one blank line, then
     `internal void PublishPopupMessengerReady() => PopupMessengerReady?.Invoke(this, EventArgs.Empty);`.
  3. In `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, delete the same comment block, the same
     `ShowPopup` method, the blank line between it and `PublishPopupMessengerReady`, the same
     `PublishPopupMessengerReady` method, and the blank line that followed it — leaving exactly one
     blank line between `FocusAnchorIfPermitted`'s closing brace and `ResetCoreAsync`.
  No other line in either file is changed.
  Acceptance (all conditions must hold):
  - `Select-String -SimpleMatch "internal void ShowPopup(Point location, bool takeFocus)" QuickFiler\Viewers\BreadcrumbDropDownHost.cs` → 0 hits.
  - `Select-String -SimpleMatch "internal void ShowPopup(Point location, bool takeFocus)" QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs` → exactly 1 hit.
  - `Select-String -SimpleMatch "internal void PublishPopupMessengerReady() =>" QuickFiler\Viewers\BreadcrumbDropDownHost.cs` → 0 hits.
  - `Select-String -SimpleMatch "internal void PublishPopupMessengerReady() =>" QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs` → exactly 1 hit.
  - `Select-String -SimpleMatch "using System;" QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs` → exactly 1 hit.
  - `(Get-Content QuickFiler\Viewers\BreadcrumbDropDownHost.cs).Count` is `<= 500` **and** strictly less than `BASELINE_HOST_COUNT` (P0-T2); record as `POST_MOVE_HOST_COUNT`.
  - `(Get-Content QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs).Count` is `<= 500` **and** strictly greater than `BASELINE_OPEN_COUNT` (P0-T2); record as `POST_MOVE_OPEN_COUNT`.

  Artifact: `<FEATURE>/evidence/regression-testing/p1-t1-relocation-verified.<ts>.md` with `Timestamp:`,
  `Command:` (the full verification block above), `EXIT_CODE: 0`, `Output Summary:` (each hit count and
  each line count named above).
- [ ] [P1-T2] Prove the relocation compiles: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`.
  Acceptance: `EXIT_CODE: 0`; 0 compile errors. Artifact:
  `<FEATURE>/evidence/regression-testing/p1-t2-build.<ts>.md` with the four schema fields.
- [ ] [P1-T3] Prove zero test deltas from the move, using D4's resolution:
  `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownHostTests" /Logger:"trx;LogFileName=p1-t3.trx" "/ResultsDirectory:<FEATURE>/evidence/regression-testing/p1-t3"`.
  Acceptance: `EXIT_CODE: 0`; total = `BASELINE_HOSTTESTS_COUNT` (P0-T6) exactly; failed = `0`; the
  `p1-t3` subdirectory holds exactly one file, named exactly `p1-t3.trx`. Artifact:
  `<FEATURE>/evidence/regression-testing/p1-t3-hosttests-post-move.<ts>.md`.

### Phase 2 — Add the CR-2 Composition Test

- [ ] [P2-T1] Add one new `[TestMethod]` to the `BreadcrumbDropDownHostTests` partial class in
  `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs`, named
  `OpenAsync_TakeFocusReopenAfterNonCapturingOpenWithPredicateFalse_RestoresAutoCloseButSuppressesFocus`,
  using the file's existing `PredicateHarness`. Follow the file's existing Arrange–Act–Assert comment
  structure and FluentAssertions style (see the surrounding `[TestMethod]`s in the same file for the
  exact idiom). Body, exactly:
  - Arrange: `Task<bool> opening = ((IBreadcrumbDropDownHost)harness.Host).OpenAsync(Anchor, Work, Desired, false);`
    then `harness.Context.DrainUntil(opening);`; assert `opening.GetAwaiter().GetResult()` is `true` and
    `harness.FocusPendingCount` is `0` (a non-capturing open never focuses the popup); then set
    `harness.AllowFocus = false;`.
  - Act: `Task<bool> reopening = ((IBreadcrumbDropDownHost)harness.Host).OpenAsync(Anchor, Work, Desired, true);`
    then `harness.Context.DrainUntil(reopening);`.
  - Assert: `harness.Host.DropDown.AutoClose` is `true` (issue #680's restore is unconditional,
    independent of the focus predicate) and `harness.FocusPendingCount` remains `0` (issue #677's
    `MayTakeFocus` guard suppresses the handoff focus call while the predicate is false).

  `Anchor`, `Work`, and `Desired` are the `private static readonly` fields declared in
  `BreadcrumbDropDownHostTests.Part2.cs`, accessible here as the same partial class.
  `IBreadcrumbDropDownHost` is already in scope via this file's existing `using QuickFiler.Viewers;`.
  Acceptance:
  - `Select-String -SimpleMatch "OpenAsync_TakeFocusReopenAfterNonCapturingOpenWithPredicateFalse_RestoresAutoCloseButSuppressesFocus" QuickFiler.Test\Viewers\BreadcrumbDropDownHostTests.Part3.cs` → exactly 1 hit.
  - `(Get-Content QuickFiler.Test\Viewers\BreadcrumbDropDownHostTests.Part3.cs).Count` is `<= 500`.

  Artifact: `<FEATURE>/evidence/regression-testing/p2-t1-test-added.<ts>.md` with the four schema
  fields.
- [ ] [P2-T2] Prove the new test compiles: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`.
  Acceptance: `EXIT_CODE: 0`; 0 compile errors. Artifact:
  `<FEATURE>/evidence/regression-testing/p2-t2-build.<ts>.md` with the four schema fields.
- [ ] [P2-T3] Prove the new test passes green, using D4's resolution:
  `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownHostTests" /Logger:"trx;LogFileName=p2-t3.trx" "/ResultsDirectory:<FEATURE>/evidence/regression-testing/p2-t3"`.
  Acceptance: `EXIT_CODE: 0`; total = `BASELINE_HOSTTESTS_COUNT` (P0-T6) `+ 1`; failed = `0`; the new
  test's fully-qualified name appears in the TRX with outcome `Passed`; the `p2-t3` subdirectory holds
  exactly one file, named exactly `p2-t3.trx`. Artifact:
  `<FEATURE>/evidence/regression-testing/p2-t3-new-test-green.<ts>.md`.

### Phase 3 — CR-1: Correct the Two Stale Statements in the Delivery Report

- [ ] [P3-T1] Read `delivery-report.2026-08-28T16-40.md` in full and append a new
  `## Post-Rebase Addendum — <ts>` section at the end of the file (do not edit or delete any existing
  text) containing, verbatim, the following two bullet lines (each on its own single line, unwrapped)
  plus explanatory prose:
  - `- Correction 1: the scheduled action calls FocusPending(), not the raw _focusPending delegate.`
  - `- Correction 2: issue #677 has since merged into this branch's base and the shipped code composes with its MayTakeFocus machinery.`

  Correction 1 corrects the "Changed and created files" bullet for `BreadcrumbDropDownHost.Open.cs`,
  which states the already-open `takeFocus` branch schedules "a restore of `AutoClose = true` before
  `_focusPending()`" — the shipped code at current `HEAD` calls the guarded wrapper `FocusPending()`,
  which itself checks `MayTakeFocus()` before invoking the raw `_focusPending` delegate field.
  Correction 2 corrects the "Discharge of issue #677's follow-up item" section, which states "#677's
  own `MayTakeFocus` machinery has not merged into this branch's base ... this change was authored
  against, and composes with, the pre-#677 shape" — that was accurate when written; this branch was
  later rebased onto `main`, which has since merged issue #677, and the shipped code in
  `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` at current `HEAD` composes with its `MayTakeFocus`,
  `FocusPending`, and `FocusAnchorIfPermitted` machinery.

  Acceptance:
  - `git diff HEAD -- docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/delivery-report.2026-08-28T16-40.md | Select-String -Pattern '^-[^-]'` → 0 matches (proves the edit is append-only — no existing line was deleted or altered).
  - `Select-String -SimpleMatch "Post-Rebase Addendum" docs\features\active\2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680\delivery-report.2026-08-28T16-40.md` → exactly 1 hit.
  - `Select-String -SimpleMatch "Correction 1: the scheduled action calls FocusPending(), not the raw _focusPending delegate." docs\features\active\2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680\delivery-report.2026-08-28T16-40.md` → exactly 1 hit.
  - `Select-String -SimpleMatch "Correction 2: issue #677 has since merged into this branch's base and the shipped code composes with its MayTakeFocus machinery." docs\features\active\2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680\delivery-report.2026-08-28T16-40.md` → exactly 1 hit.

  Artifact: `<FEATURE>/evidence/other/p3-t1-cr1-addendum-verified.<ts>.md` with the four schema fields.

### Phase 4 — Final QA Loop (format → lint → type-check → test)

Run this loop in order. If any step fails or the format step changes any file, restart the entire
Phase 4 loop from P4-T1; a restarted pass does not count as final.

- [ ] [P4-T1] Format gate: `dotnet tool run csharpier check .` (pre-check, recorded as
  `PRE_FORMAT_CHECK_EXIT`), then `dotnet tool run csharpier format .`, then
  `dotnet tool run csharpier check .` again. Capture `git status --porcelain` immediately before and
  immediately after the format command. Acceptance: post-format `EXIT_CODE: 0`. If
  `PRE_FORMAT_CHECK_EXIT` is non-zero (the formatter rewrote a file), restart the full Phase 4 loop
  from P4-T1. Artifact: `<FEATURE>/evidence/qa-gates/p4-t1-format.<ts>.md`.
- [ ] [P4-T2] Analyzer rebuild:
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Acceptance: `EXIT_CODE: 0`; the warning set matches `BASELINE_ANALYZER_WARNINGS` (P0-T4) exactly (0
  new diagnostics). Artifact: `<FEATURE>/evidence/qa-gates/p4-t2-analyzers.<ts>.md`.
- [ ] [P4-T3] Nullable/type-check rebuild:
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`.
  Acceptance: `EXIT_CODE: 0`; no `CS86xx` diagnostic; the warning set matches
  `BASELINE_NULLABLE_WARNINGS` (P0-T5) exactly. Artifact:
  `<FEATURE>/evidence/qa-gates/p4-t3-nullable.<ts>.md`.
- [ ] [P4-T4] Full `QuickFiler.Test` assembly run (fast checkpoint, no coverage), using D4's resolution:
  `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:"trx;LogFileName=p4-t4.trx" "/ResultsDirectory:<FEATURE>/evidence/qa-gates/p4-t4"`.
  Acceptance: `EXIT_CODE: 0`; total = `BASELINE_QFT_COUNT` (P0-T7) `+ 1` (the new CR-2 test); failed =
  `0`; the `p4-t4` subdirectory holds exactly one file, named exactly `p4-t4.trx`. Artifact:
  `<FEATURE>/evidence/qa-gates/p4-t4-qft-full-run.<ts>.md`.
- [ ] [P4-T5] Full-repo coverage-mode run:
  `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\coverage-remediation-final-680.cobertura.xml`
  (per D4, may run detached with polling). Acceptance: `EXIT_CODE: 0`; the final failing-test FQN set is
  a subset of `BASELINE_FAILURE_SET` (P0-T8, expected `none`); the artifact records numeric root
  `line-rate`/`branch-rate`, each `>=` the corresponding `BASELINE_LINE_RATE`/`BASELINE_BRANCH_RATE`
  value (P0-T8) — this plan makes no production-code line-count change (a pure relocation) and adds one
  new passing test, so no coverage regression is expected; and total/passed/failed counts, with total =
  `BASELINE_COVERAGE_TOTAL` (P0-T8) `+ 1`. Artifact:
  `<FEATURE>/evidence/qa-gates/p4-t5-coverage-final.<ts>.md`.
- [ ] [P4-T6] Final file-size re-audit for both relocation-touched production files:
  `(Get-Content QuickFiler\Viewers\BreadcrumbDropDownHost.cs).Count` and
  `(Get-Content QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs).Count`. Acceptance: both counts are
  `<= 500` (the R1 ceiling violation is closed, not deferred); `BreadcrumbDropDownHost.cs`'s count is
  strictly less than `BASELINE_HOST_COUNT` (P0-T2); `BreadcrumbDropDownHost.Open.cs`'s count is strictly
  greater than `BASELINE_OPEN_COUNT` (P0-T2). Artifact:
  `<FEATURE>/evidence/qa-gates/p4-t6-file-size-audit.<ts>.md` explicitly stating both final counts and
  that both are within the repository's 500-line ceiling.
- [ ] [P4-T7] Commit-readiness check (no `git commit` is performed by this task): `git status
  --porcelain`. Acceptance: every listed entry is one of `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`,
  `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`,
  `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs`,
  `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/delivery-report.2026-08-28T16-40.md`,
  this remediation plan file itself, or a path under
  `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/`;
  the count of entries matching none of these is `0`. Artifact:
  `<FEATURE>/evidence/qa-gates/p4-t7-commit-readiness.<ts>.md`.

## Structural Self-Check (validator not run)

- Phase headings match `### Phase N — <Title>` exactly. Task IDs are `[P#-T#]`, sequential within each
  phase, all starting `- [ ]`, no phase-number gaps (0–4).
- Every task names explicit file paths and verifiable, non-vacuous acceptance conditions built from
  presence/absence pairs, strict-inequality line-count proofs, or append-only diff checks paired with
  exact-phrase presence checks for text this plan's own tasks create (quoted verbatim in-plan).
- Every `git diff` acceptance condition is anchored to the literal ref `HEAD` (never left unanchored,
  avoiding the G8 pattern), and none use `--name-only`/`--name-status`, so the G8b companion requirement
  does not apply. No task hardcodes a specific commit hash as an expected value (see D1) — the tree
  cannot be assumed clean at plan-authoring time because outstanding review/memory artifacts are still
  uncommitted at that time; Phase 0 verifies cleanliness at its own execution time instead.
- No task's acceptance condition can be satisfied without the underlying edit actually happening: R1's
  relocation is verified by four Select-String presence/absence checks plus two strict-inequality
  line-count checks; CR-1's addendum is verified by an append-only diff check plus two exact-phrase
  presence checks for lines quoted verbatim in this plan; CR-2's test is verified by a named-test
  presence check plus a green TRX run with an exact expected total.
- Coverage evidence contract satisfied: explicit baseline (P0-T8) and final (P4-T5) coverage capture
  tasks with numeric values and an explicit no-regression comparison.
- No `git commit` task is present, per the calling agent's standing instruction.
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: not applicable — no non-canonical evidence path was supplied;
  all evidence paths in this plan resolve under `<FEATURE>/evidence/<kind>/` using only canonical
  sub-paths (`remediation-baseline/`, `regression-testing/`, `other/`, `qa-gates/`).
- This plan contains no deferral, no exception, and no follow-up-issue-filing task for R1. R1 is fixed
  directly in Phase 1 of this plan.

**PREFLIGHT: VALIDATOR NOT RUN** — the `mcp__drm-copilot__validate_orchestration_artifacts` tool is not
present in this planning session's tool surface. The structural self-check above is a substitute
review, not a validator pass, and must not be reported as `PREFLIGHT: ALL CLEAR`.
