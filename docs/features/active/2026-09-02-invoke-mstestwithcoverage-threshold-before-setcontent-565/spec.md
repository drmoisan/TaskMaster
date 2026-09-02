# invoke-mstestwithcoverage-threshold-before-setcontent (Spec)

- **Issue:** #565
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T08-59
- **Status:** Draft
- **Version:** 0.1

## Write Set
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`

## Context
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` asserts the coverage threshold before it writes the
post-processed Cobertura document to disk. When the assertion fails, the script throws and the
post-processed document is discarded, leaving the raw un-post-processed document at the output path.

Environment:
- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — PowerShell 7+ coverage/test-runner scripts
- Command/flags used: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Only the failure path is affected; a passing run writes the correct document. But the failure path is
exactly when someone reads the artifact to diagnose the shortfall, and what they find is a document
with different numbers than the one that produced the failure message. It also means a failed gate
leaves behind an artifact that, if fed to any downstream consumer, reports the pre-#441 inflated
denominator.


## Repro & Evidence
Steps to Reproduce:
1. Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` against a test suite whose measured line
   coverage is below the configured 80% threshold.
2. Observe that `Assert-CoberturaLineCoverageThreshold` (in Invoke-MSTestWithCoverage.Helpers.ps1)
   throws before the post-processed XML is persisted.
3. Inspect the coverage output path named by `-CoverageOutput` after the throw.

Expected:
The artifact on disk should be the same post-processed Cobertura document that the threshold
assertion judged, in both the passing and failing case.

Actual:
At `scripts/vscode/Invoke-MSTestWithCoverage.ps1:341-343` the threshold assertion runs ahead of the
`Set-Content` that persists the post-processed XML. On a failing run, the artifact left on disk is
the raw `dotnet-coverage` output — absolute paths, third-party packages included, unmerged duplicate
classes, and the double-counted line totals that #441 corrected.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citation above.


## Scope & Non-Goals
- In scope:
  - Reorder two statements in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: move the
    `Set-Content` call (currently line 343) above the `Assert-CoberturaLineCoverageThreshold` call
    (currently line 341), inside `Invoke-MSTestWithCoverageMain`.
  - Add one new Pester regression test to the existing `Describe 'Invoke-MSTestWithCoverageMain'`
    block in `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, asserting that the
    post-processed Cobertura document is persisted before the threshold assertion can throw.
- Out of scope / non-goals:
  - Do NOT change the 80% threshold value inside `Assert-CoberturaLineCoverageThreshold`
    (scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, line 487). That threshold-value
    contradiction is tracked separately by issue #563 and is explicitly excluded from this fix.
  - Do not change `Assert-CoberturaLineCoverageThreshold`'s parsing, throw conditions, or message
    text.
  - Do not add a `try`/`finally` wrapper or any other indirection; the fix is a pure statement
    reorder (see Proposed Fix).
- Explicitly excluded systems, integrations, or datasets:
  - .claude/**, .codex/**, .agents/**, config/blast-radius.json,
    config/orchestration-routing.json — these are published from an upstream repository and must
    not be edited as part of this fix.

## Root Cause Analysis
Statement ordering defect only, not a logic change. Found during the `build-ci-coverage-gate-fidelity`
epic fan-in review; identified independently by two review passes. Note: issue #563 (threshold VALUE
contradiction) is a separate, deliberately excluded concern — this fix must not change the threshold
value, only the statement order.


## Proposed Fix

### Design summary (what changes where):
Pure two-statement reorder inside `Invoke-MSTestWithCoverageMain`
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, lines 341 and 343): swap the `Set-Content` call
(line 343) to execute immediately after `$processedXmlContent` is computed (line 340) and before
the `Assert-CoberturaLineCoverageThreshold` call (currently line 341). No function signature or
parameter changes anywhere in the change.

### Boundaries and invariants to preserve:
- `Assert-CoberturaLineCoverageThreshold`'s own 80% threshold logic and message text
  (scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, line 487) are unchanged.
- The dot-source of Invoke-MSTestWithCoverage.Helpers.ps1 at line 261 continues to precede both
  calls (unaffected by the reorder; it already sits far above lines 341/343).
- `Assert-CoberturaLineCoverageThreshold` remains a pure read-and-throw function over its own
  local `[xml]$coverageDocument` copy — it has no side effect that `Set-Content` could observe or
  that could observe `Set-Content`'s effect, so reordering is behaviorally safe.
- The file's bottom auto-invocation guard (`if ($MyInvocation.InvocationName -ne '.')`,
  lines 347-349) is unaffected.

### Dependencies or blocked work:
None. The fix is self-contained to the one file and its regression test; it has no dependency on
issue #563 (threshold-value contradiction), which is deliberately excluded from this fix.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (production) — swap lines 341 and 343.
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (test) — add one new `It` inside the
  existing `Describe 'Invoke-MSTestWithCoverageMain'` block.

#### Functions/classes/CLI commands impacted:
- `Invoke-MSTestWithCoverageMain` only. `Assert-CoberturaLineCoverageThreshold` is not modified.

#### Data flow and validation changes:
None. `$processedXmlContent` is already fully computed by line 340 under both call orders; no data
computed, transformed, or validated differently.

#### Error handling and logging updates:
None. `Assert-CoberturaLineCoverageThreshold` still throws under the same conditions with the same
message; only the point in the statement sequence at which that throw can occur (relative to
`Set-Content`) changes.

#### Rollback/feature-flag considerations (if applicable):
None required. The change is a two-line swap with no new configuration surface; rollback is a
direct revert of the diff.

### Technical specifications (interfaces/contracts):
No interface or contract changes. `Invoke-MSTestWithCoverageMain`'s parameters, return behavior,
and external call sites (see research, section 5: exactly one production call site each for
`Invoke-MSTestWithCoverageMain` and `Assert-CoberturaLineCoverageThreshold`) are unaffected.

#### Inputs/outputs and formats:
Unchanged. Output artifact format (post-processed Cobertura XML at `-CoverageOutput`) is
unaffected; only the ordering of when it is written relative to the threshold check changes.

#### Required configuration keys and defaults:
None added or changed.

#### Backward-compatibility expectations:
Fully backward compatible. Passing runs (coverage at or above 80%) are unaffected because both
statements already execute in that case today; only the failing-run artifact content changes (the
post-processed document is now what is left on disk instead of the raw `dotnet-coverage` output).

#### Performance constraints (latency/throughput/memory):
None. The reorder does not add, remove, or change any I/O or computation — it only changes the
sequence of two already-existing statements.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- Constraints (budget, performance, compatibility):
- External dependencies (services, libraries, releases):

## Data / API / Config Impact
- User-facing or API changes:
- Data or migration considerations:
- Logging/telemetry updates (if any):
- Compatibility notes (CLI flags, config schemas, versioning):

## Test Strategy
Seeded from issue:

- [x] Move the `Set-Content` above the `Assert-CoberturaLineCoverageThreshold` call so the judged
      document is persisted before the threshold is evaluated.
- [ ] Add a Pester test under `tests/scripts/vscode/` asserting that a sub-threshold run still leaves
      the post-processed document on disk (not the raw `dotnet-coverage` output).

- Regression tests to add or update:
  - One new `It` inside the existing `Describe 'Invoke-MSTestWithCoverageMain'` block in
    `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, following the file's existing
    `BeforeEach` mocking conventions exactly:
    - Override `Mock ConvertTo-KoverageCoberturaXml { '<coverage line-rate="0.5" />' }` (fixture
      well below the 80% threshold, avoiding boundary/rounding ambiguity).
    - Leave `Assert-CoberturaLineCoverageThreshold` unmocked (real), so it genuinely throws.
    - Call `Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir` inside
      `{ ... } | Should -Throw`.
    - Assert `Should -Invoke Set-Content -Times 1 -Exactly` (filtered on `$Path`, not
      `$LiteralPath` — this call site uses `-Path`, unlike the other `Set-Content` call at line 219
      of the same file).
  - This is a **bugfix regression test** per the repository's Bugfix Workflow: it is tagged
    `[expect-fail]` against the pre-fix statement order (fails, because
    `Assert-CoberturaLineCoverageThreshold` throws before `Set-Content` is ever reached) and must
    pass after the fix (because `Set-Content` runs first, unconditionally, before the threshold
    outcome is evaluated).
- Unit tests (pytest) for the fixed behavior and boundaries: n/a — PowerShell/Pester repo, no
  Python involved in this change.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - Sub-threshold run (this fix's regression test, fixture `line-rate="0.5"`).
  - Existing boundary fixtures (`0.799999`, `0.8`, `0.800001` in
    Invoke-MSTestWithCoverage.Helpers.Tests.ps1, lines 495-497) already cover
    `Assert-CoberturaLineCoverageThreshold`'s own boundary behavior in isolation and require no
    change.
  - At/above-threshold run: already covered by the existing `It` at lines 400-406 of
    `Invoke-MSTest.RunSettings.Tests.ps1` and remains unaffected by the reorder.
- Error handling and logging verification:
  - The new test confirms `Assert-CoberturaLineCoverageThreshold` still throws on a sub-threshold
    run (via `{ ... } | Should -Throw`); only the timing of the throw relative to `Set-Content`
    changes.
- Coverage impact and targets for changed lines/modules: the changed lines (341/343 swap) are
  already fully exercised by the existing `Describe 'Invoke-MSTestWithCoverageMain'` block plus the
  new test; no new uncovered lines are introduced.
- Toolchain commands to run (format → lint → type-check → test): this repository's PowerShell
  toolchain has no type-check stage.
  1. PoshQC format (PowerShell formatting check/apply).
  2. PSScriptAnalyzer (linting/static analysis).
  3. Pester (`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, and the full PowerShell
     suite for regression safety).
- Manual validation steps (if required): none required beyond the automated toolchain; the
  regression test deterministically reproduces the sub-threshold-run scenario in-memory (no temp
  files, matching repo policy).


## Acceptance Criteria
- [ ] The new Pester test in `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, inside the
      `Describe 'Invoke-MSTestWithCoverageMain'` block, fails against the pre-fix statement order
      (`Assert-CoberturaLineCoverageThreshold` at line 341 ahead of `Set-Content` at line 343) and
      passes after the fix (statement order swapped).
- [ ] `Set-Content` is invoked before `Assert-CoberturaLineCoverageThreshold` can throw on a
      sub-threshold run, verified by `Should -Invoke Set-Content -Times 1 -Exactly` asserted inside
      a `{ ... } | Should -Throw` block, using the `ConvertTo-KoverageCoberturaXml` mock returning
      `'<coverage line-rate="0.5" />'`.
- [ ] The coverage threshold value (80%) is unchanged: no diff touches
      `Assert-CoberturaLineCoverageThreshold`'s threshold literal (line 487) or its throw message
      text in scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1.
- [ ] No production file other than `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is changed (in
      particular, scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 and
      scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 are untouched).
- [ ] PoshQC format, PSScriptAnalyzer, and Pester all pass cleanly on the changed files
      (`scripts/vscode/Invoke-MSTestWithCoverage.ps1` and
      `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`), with no format or lint
      auto-fixes needed and no regression in the existing `Describe 'Invoke-MSTestWithCoverageMain'`
      cases (lines 345-414) or the boundary tests in Invoke-MSTestWithCoverage.Helpers.Tests.ps1.
- [ ] Repro steps from `## Repro & Evidence` now produce the expected behavior: after the fix, the
      artifact left on disk at `-CoverageOutput` on a sub-threshold run is the same post-processed
      Cobertura document that the threshold assertion judged, not the raw `dotnet-coverage` output.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
