# 2026-09-08-coverage-aggregation-double-counts-method-rows (Spec)

- **Issue:** #815
- **Parent (optional):** epic `review-residuals-2026-09-08` (wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09T00-00
- **Status:** Ready for planning
- **Version:** 1.0

> Work Mode is `full-bug`. This file is the sole authoritative acceptance-criteria source for issue
> #815. No `user-story.md` exists for this feature, and its absence is correct by design.

> Path-formatting note for later editors: repository paths written as inline code in this document
> are the change footprint. Paths that this feature does not modify are deliberately written as
> plain prose without backticks. Do not "fix" that formatting; adding backticks to an out-of-scope
> path widens the recorded blast radius.

## Context

Atomic plans in this repository quote a first-party coverage aggregate for their QA gates. Cobertura
`<package>` elements produced by dotnet-coverage carry `line-rate` and `branch-rate` but carry no
`lines-covered`, `lines-valid`, `branches-covered` or `branches-valid` attributes, so a plan author
who needs those four figures has to compute them. The repository exposes no committed, callable way
to obtain them, so plan authors write their own aggregation inline in the plan document. The
variant that has propagated selects `<line>` elements on the descendant axis, which matches both the
class-level rollup and the method-level view of the same source line, and therefore counts most rows
twice.

On issue 809's delivery the artifacts reported 79.38% first-party branch coverage and the feature
reviewer's de-duplicated recomputation over the same run returned 77.03%, with the line percentage
unaffected at 84.62% under both computations. Those two branch percentages are what the audit
records; the underlying counts are not recorded anywhere and are not reproduced here.

Subsequent measurement, recorded in this feature's research document, establishes that the
descendant-axis defect does not by itself produce a swing of that size. See Repro & Evidence.

Environment:
- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8
- Python version: not applicable
- Command/flags used: the repository coverage route (dotnet-coverage collect wrapping
  vstest.console.exe), Cobertura output consumed by an aggregation step pasted into atomic plans
- Data source or fixture: the raw Cobertura report from issue 809's final QA gate run. That report
  is not committed (see Repro & Evidence), so the fixtures for this fix are in-memory XML literals
  plus Cobertura documents committed under other feature folders.

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High because the defect corrupts a quality gate that many future plans depend on rather than one
delivered item, and the error is in the optimistic direction, so it fails to stop the cases the gate
exists to stop. It is not a Blocker: no shipped product behavior is affected, and on the one
measured occurrence both figures cleared the governing threshold, so no verdict changed.

## Repro & Evidence

Steps to Reproduce:
1. Obtain a Cobertura document produced by the repository coverage route (any committed
   cobertura.xml document under a feature folder's evidence tree serves).
2. Aggregate first-party line and branch counts using the method pinned in a current atomic plan,
   quoted verbatim below.
3. Recompute the same four counts de-duplicating by source line number before summing, which is what
   the committed helper `Get-CoberturaClassLineSummary` already does.
4. Compare the four counts. They disagree, and the pinned method reports the larger totals.

The pinned method, verified present at lines 99-119 of the atomic plan for issue 809
(docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md)
and again at lines 13-26 of
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t5-retained-cobertura-reaggregation.md:

```powershell
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath $CoberturaPath).Path)
$firstParty = @('Tags','ToDoModel','TaskVisualization','UtilitiesCS','QuickFiler','TaskTree','TaskMaster','SVGControl','VBFunctions')
$lc = 0; $lv = 0; $bc = 0; $bv = 0
foreach ($pkg in $doc.SelectNodes('/coverage/packages/package')) {
    if ($firstParty -notcontains $pkg.GetAttribute('name')) { continue }
    foreach ($ln in $pkg.SelectNodes('.//line')) {
        $lv++
        $h = $ln.GetAttribute('hits')
        if ($h -and [int]$h -gt 0) { $lc++ }
        $cc = $ln.GetAttribute('condition-coverage')
        if ($cc -and $cc -match '\((\d+)/(\d+)\)') { $bc += [int]$Matches[1]; $bv += [int]$Matches[2] }
    }
}
"LINES_COVERED=$lc LINES_VALID=$lv BRANCHES_COVERED=$bc BRANCHES_VALID=$bv"
```

The 809 plan states that the descendant-axis selection is "the selection ... and only that one", and
that the two narrower selections `classes/class/lines/line` and
`classes/class/methods/method/lines/line` "are rejected by name and must not be substituted". The
rejected narrow selections are the two halves that the correct computation unions and then
de-duplicates, so the pinned prose rejects the correct method by name.

Expected:
A coverage figure quoted in a QA gate equals the figure a de-duplicated recomputation from the same
raw Cobertura document produces. One document does not yield two answers.

Actual:
The pinned aggregation counts each source line once per view in which it appears, so the four
reported counts are close to twice the true first-party statement and branch population.

**The defect is a count defect, not a rate defect, and the issue's stated magnitude is refuted.**
This is the single most consequential correction in this specification, and it determines how the
fix must be tested. A uniform double count leaves every ratio unchanged, because numerator and
denominator scale by the same factor. The research document for this feature measured both
computations over two committed Cobertura documents, reconciling each tally exactly against the
document's own root attributes:

| Document | Lines naive vs de-duplicated | Branch percentage naive vs de-duplicated |
| --- | --- | --- |
| Raw collector output (issue 364 baseline) | 197072/273553 vs 98272/136359 | 48.4546% vs 48.4546%, a delta of exactly zero |
| Post-processed (issue 670 post-change) | 98553/114811 vs 54988/64406 | 79.7236% vs 79.3997%, a delta of 0.3239 points |

The branch-percentage movement is 0.0000 and 0.3239 points, and on both documents it is smaller than
the line-percentage movement rather than larger. Two independent records in the repository state the
same rule: .claude/agent-memory/feature-review/project_791-review-residuals.md reports that for
issue 791 "the derived percentages match to the digit under both selections", and the existing
regression test comment at `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` lines
210-211 records that "the branch RATIO is unchanged by the double count, so this must assert
branches-valid/branches-covered, never branch-rate".

A 2.35-point branch swing therefore cannot be produced by this mechanism. The provenance of the
77.03% figure is **unknown** and cannot be reconstructed, because the report that produced it is not
committed and neither the issue nor the 809 evidence tree records the recomputation method. Candidate
explanations that could not be tested include a different package set, a union keyed by
(filename, line number) across classes, and a figure taken after post-processing, which changes which
lines are in the population rather than only their multiplicity. No acceptance criterion below is
gated on reproducing either figure.

What the defect genuinely does remains worth fixing: it reports counts roughly twice the true
population, which invalidates every absolute-count assertion and every cross-artifact count
comparison built on it. Two consequences follow for the test strategy. First, any test or gate
written against a rate can pass on a fixture whose counts are demonstrably wrong, so counts
discriminate and rates do not. Second, a fixture that duplicates every row uniformly cannot
discriminate at all; the fixture must duplicate rows at differing multiplicities, which is the
field-initializer-across-constructors shape confirmed at issue 670.

Logs / Screenshots:
- [x] Evidence recorded in a committed audit artifact
- The measurement is finding F7 of
  docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/policy-audit.2026-09-08T01-35.md:
  the artifacts "compute branch coverage by summing `condition-coverage` over an all-descendant
  `.//line` selection, which counts method-level rows in addition to class-level rows. The
  reviewer's de-duplicated computation returns 77.03% where the artifacts report 79.38%. Line
  percentage is unaffected (both 84.62%) because the duplication is close to proportional on lines.
  Both figures clear the 75% floor, so no verdict changes."

**Correction to the issue text.** Issue #815 states that the raw Cobertura report for issue 809 is
committed under that item's evidence/qa-gates/ tree. It is not. Every tracked file under
docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/ is a `.md` file.
Finding F5 of the same policy audit records the reason: "the raw Cobertura document is git-ignored
under coverage/ rather than stored under evidence/qa-gates/". The 79.38% and 77.03% figures
therefore cannot be re-derived by this feature, and no acceptance criterion below requires that.
Other feature folders do carry committed Cobertura documents - for example
docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml,
which contains 7838 `<methods>` elements - so a corroborating measurement against a real report is
available even though the specific 809 report is not.

## Scope & Non-Goals

- In scope: a de-duplicated, first-party-scoped coverage aggregation exposed as committed PowerShell
  under `scripts/vscode/`, its Pester tests under `tests/scripts/vscode/`, a reported summary from
  the coverage entry point, and this feature folder's documents and evidence.
- Out of scope / non-goals: the five items enumerated under Non-Goals below.
- Explicitly excluded systems, integrations, or datasets: no C# production or test code, no build
  configuration, no CI workflow, and no governance document under .claude/ is modified.

### Write Set

Files this feature may create or modify:

- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
- `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1`
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`
- one new script file under `scripts/vscode/` and its mirrored test file under
  `tests/scripts/vscode/`, if the planner's file-placement decision requires them (see the
  file-size constraint below)
- this feature folder's own documents and evidence artifacts under
  docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/

No sibling feature in the epic `review-residuals-2026-09-08` names `scripts/vscode/` or
`tests/scripts/vscode/` as a surface, so this write set does not overlap any concurrent item.

### Non-Goals

1. **Editing CLAUDE.md.** The issue notes that CUT3 step 4 names
   `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` while the coverage route actually
   in use is dotnet-coverage collect, and that this mismatch is what made issue 809's AC6 read as
   PARTIAL on wording alone. The mismatch is real and is recorded here, but this feature operates
   under a hard constraint not to modify CLAUDE.md. The correction is to be raised as a separate
   promotion.
2. **Editing anything under .claude/skills/ or .claude/rules/.** Everything under .claude/
   other than agent-memory/ is pushed down from the separate drm-copilot governance repository
   with no templating, so a change made here is overwritten by the next push-down. The epic manifest
   docs/features/epics/review-residuals-2026-09-08/epic.md records this as Non-Goal 2 for a sibling
   feature; the same reasoning applies to any temptation to encode the corrected aggregation into a
   plan-authoring skill. The correct fix for a governance document is an upstream change in
   drm-copilot.
3. **Rewriting historical evidence or plan artifacts.** The committed plan and evidence documents
   that carry the descendant-axis snippet are the audit record of what was actually done on those
   items. Correcting them retroactively would falsify that record. This fix prevents recurrence; it
   does not restate past figures.
4. **Lowering, weakening or deleting any coverage threshold.** Epic Non-Goal 5 states: "Issue 815
   corrects the arithmetic; if the corrected figure falls below a threshold, that is a finding to
   record, not a threshold to lower." If a corrected figure falls below a threshold, record the
   finding.
5. **Editing any C# production or test file, .editorconfig, or BannedSymbols.txt.** Seven sibling
   features run against the same integration branch, and a sibling feature (issue 826) owns those
   two configuration files.

## Root Cause Analysis

The root cause is the absence of a committed, tested, callable de-duplicated first-party
aggregation, not broken arithmetic in the committed helpers.

Verified state of the committed code:

- `Get-CoberturaClassLineSummary` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
  (lines 158-256) is correct. It unions `./lines/line` with `./methods/method/lines/line`, keys the
  result by line number in a hashtable, and resolves a repeated key by taking the maximum `hits`,
  treating the line as a branch if either entry is a branch, and retaining the `condition-coverage`
  of the entry with the larger denominator.
- `Get-CoberturaPackageLineSummary` in `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1`
  accumulates that per-class summary across a package.
- `Get-CoberturaCoverageSummary` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
  (lines 101-135) sums one package summary per package into the document totals.
- That de-duplication was delivered by commit a7ac497b, "fix(coverage): dedupe Cobertura line and
  branch arithmetic (#441, #478)", together with its Pester tests.
- The string `.//line` does not appear anywhere under the scripts directory tree on this branch head (verified: zero
  occurrences).

What is missing is the exposure. `Invoke-MSTestWithCoverageMain` in
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` (lines 334-346) post-processes the report through
`ConvertTo-KoverageCoberturaXml`, calls `Assert-CoberturaLineCoverageThreshold`, and then prints
only `Done. Coverage artifact: <path>`. It never prints the covered and valid line and branch
counts, and it applies no first-party filter to any reported figure. A plan author who needs the
four counts has no supported way to obtain them, so each one writes an aggregation by hand, and the
variant that has propagated to at least two committed documents is the descendant-axis form.

**Invariant this fix establishes.** For any Cobertura document, a first-party aggregate produced by
this repository counts each (class, source line number) pair exactly once, so `lines-valid`,
`lines-covered`, `branches-valid` and `branches-covered` do not depend on whether a source line also
appears under a `<method>` element, and the same counting rule is applied by every caller because
there is exactly one implementation of it.

**Trace of one row, from the document to the reported figure.**

1. **Source rows.** In a Cobertura document produced by dotnet-coverage, class `Ns.Foo` carries line
   number 12 with `branch="True"` and `condition-coverage="50% (1/2)"` inside the class-level
   `<lines>` rollup, and the identical row appears again under
   `<methods>/<method>/<lines>`. This is not an anomaly; it is the normal shape of the format, and
   the fixture at `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` lines 212-229
   reproduces it.
2. **Selection point (no de-duplication exists here).** The pinned snippet evaluates
   `$pkg.SelectNodes('.//line')`, the descendant axis, which matches both occurrences. There is no
   key, no map, and no comparison of line numbers anywhere in the loop, so nothing downstream can
   recover the fact that the two matches are one source line.
3. **Accumulation (the error is committed here).** Each match increments `$lv`, increments `$lc`
   because `hits` is greater than zero, and adds the parsed `(1/2)` pair to `$bc` and `$bv`. The
   single source line contributes 2 valid lines, 2 covered lines, and 2 of 4 branches instead of
   1 valid line, 1 covered line, and 1 of 2 branches.
4. **Reporting (where the figure becomes a gate).** The totals are written into a QA-gate evidence
   artifact as the item's first-party coverage, compared against a threshold, and compared against a
   baseline computed the same way. The correct path exists in committed code -
   `Get-CoberturaClassLineSummary` reached through `Get-CoberturaPackageLineSummary` - but is never
   reached from a plan, because the entry point exposes no aggregate to read.

**Why neither half of the fix suffices alone.** Correcting the arithmetic in a plan document leaves
the next plan author with the same empty toolbox and the same two committed documents to copy from,
so the defect recurs. Exposing a printed number without routing it through the de-duplicating helper
would propagate the same wrong figure through a supported entry point, which is worse than the
current state because it would look authoritative. The fix must make the correct counts both correct
and obtainable by name.

## Proposed Fix

### Design summary (what changes where):

Add a de-duplicated, first-party-scoped aggregation to the coverage script family under
`scripts/vscode/`, exposed as a named PowerShell function that a plan can dot-source and call, and
make the coverage entry point report the resulting counts so a plan author reads a number instead of
pasting a snippet. The aggregation computes per-class figures through the existing
`Get-CoberturaClassLineSummary` (directly or through `Get-CoberturaPackageLineSummary`) rather than
re-deriving the de-duplication rule, so exactly one implementation of the counting rule exists.

### Boundaries and invariants to preserve:

- The de-duplication precedence rule already documented on `Get-CoberturaClassLineSummary` - maximum
  `hits`, branch if either entry is a branch, `condition-coverage` of the larger denominator - is
  retained unchanged. This fix adds an exposure; it does not re-tune that rule.
- The new aggregation is pure: it accepts a loaded document or a document string, performs no
  filesystem or network I/O beyond what its caller supplies, and mutates nothing in the source
  document. I/O stays in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
- `Assert-CoberturaLineCoverageThreshold` in
  scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 keeps its existing behavior and its
  existing threshold value.
- `ConvertTo-KoverageCoberturaXml` continues to write the document-level `line-rate`, `branch-rate`,
  `lines-covered`, `lines-valid`, `branches-covered` and `branches-valid` attributes from
  `Get-CoberturaCoverageSummary`. The new function must not produce a second, competing definition
  of those totals.

### Dependencies or blocked work:

None. This feature is in wave 0 of the epic with an empty `depends_on` list, and no sibling touches
`scripts/vscode/` or `tests/scripts/vscode/`.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

See the Write Set above. Two design inputs constrain placement:

- **Allowlist source.** `Get-KoverageProjectAllowlist` in
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` already derives the first-party allowlist
  from the tracked project files by excluding assembly names ending in `.Test`. The set it produces
  from this repository's production projects - QuickFiler, SVGControl, Tags, TaskMaster, TaskTree,
  TaskVisualization, ToDoModel, UtilitiesCS, VBFunctions - is the same set the pinned snippet
  hard-codes. The new function must default to that helper rather than restating the names, so the
  allowlist stays correct when a project is added or renamed, while still accepting an explicit
  override parameter for tests.
- **File-size headroom.** Physical line counts on this branch head:
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` 469,
  scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 413,
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` 350,
  `scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1` 65,
  scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 56. The 500-line ceiling in
  .claude/rules/general-code-change.md leaves Helpers.ps1 31 lines of headroom, and both
  PackageRate.ps1 and Threshold.ps1 carry an in-file comment recording that they were split out of
  Helpers.ps1 for exactly this reason. This is a constraint on the planner's file-placement
  decision, not a prescription of a particular file.

#### Functions/classes/CLI commands impacted:

- New: a first-party document-level aggregation function, and a pure formatting function that
  renders its result as the text the entry point prints. The formatting function is separate so the
  reported text is assertable in a Pester test without invoking the coverage run.
- Modified: `Invoke-MSTestWithCoverageMain`, to write the rendered summary before its existing
  `Done. Coverage artifact: <path>` line.
- Unmodified in behavior: `Get-CoberturaClassLineSummary`, `Get-CoberturaPackageLineSummary`,
  `Get-CoberturaCoverageSummary`, `Assert-CoberturaLineCoverageThreshold`.

#### Data flow and validation changes:

Input is a Cobertura document. Packages whose `name` is outside the allowlist are skipped. Each
retained package is reduced through the existing per-class de-duplicating summary and accumulated.
Output is the four counts plus the two derived rates. A document with no `<packages>` node is
rejected with an explicit error, matching the existing behavior of `Get-CoberturaCoverageSummary`. A
document whose retained packages contain no lines yields zero counts and a rate of `0`, matching the
zero-denominator fallback already used by the package and document summaries.

#### Error handling and logging updates:

Failures are explicit exceptions with distinct messages, consistent with
`Assert-CoberturaLineCoverageThreshold`. The reported summary uses `Write-Output`, matching the
surrounding entry-point code. No new logging framework is introduced.

#### Rollback/feature-flag considerations (if applicable):

None. The change is additive to the script family; reverting the commit restores prior behavior.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

The aggregation accepts a Cobertura document and an optional project-name allowlist, and returns an
object carrying the covered and valid line and branch counts together with the derived rates, using
the same property names and the same rounding and zero-denominator fallback as
`Get-CoberturaCoverageSummary` and `Get-CoberturaPackageLineSummary`, so a caller cannot be surprised
by a third object shape. The rendered summary text states the four counts and both percentages.

#### Required configuration keys and defaults:

None. The allowlist parameter defaults to `Get-KoverageProjectAllowlist`.

#### Backward-compatibility expectations:

No existing function signature changes. Callers of
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` gain additional output lines; the existing
`Done. Coverage artifact: <path>` line is retained so any consumer matching on it continues to work.

#### Performance constraints (latency/throughput/memory):

The aggregation is a single pass over a document already loaded in memory during a coverage run. No
constraint beyond not adding a second full traversal where the existing per-package reduction can be
reused.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - Cobertura documents produced by the repository coverage route continue to emit both a
    class-level `<lines>` rollup and method-level `<lines>` blocks. The de-duplicating helper is
    correct either way; only the size of the discrepancy depends on this.
  - The PowerShell gates are run through the drm-copilot PoshQC MCP tools (format, analyze, test),
    as recorded by prior deliveries in this repository.
- Constraints (budget, performance, compatibility):
  - 500 physical lines per file, per .claude/rules/general-code-change.md, measured as
    `(Get-Content -LiteralPath <path>).Count`.
  - Repository policy prohibits creating temporary files in tests, so fixtures must be in-memory
    here-string XML literals, matching the existing idiom in
    `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and
    `tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`.
  - Test files mirror the production layout under `tests/scripts/vscode/`. There is no
    tests/scripts/powershell/ directory in this repository (verified: the tests/scripts tree contains
    only `vscode`).
  - Measured analyzer baseline on this branch head: PoshQC analyze over `scripts/vscode` exits 1
    with 16 pre-existing PSScriptAnalyzer findings; over `tests/scripts/vscode` it returns ok. The
    same 16-finding folder baseline was already documented on 2026-08-04 by the issue 400 delivery.
    Those findings are pre-existing and out of scope, so no criterion below demands a zero count for
    `scripts/vscode`.
  - Measured format baseline: PoshQC format over both directories returns ok and leaves
    `git status --porcelain` empty.
  - Measured test baseline: PoshQC test over `tests/scripts/vscode` returns ok, with the bundled
    JUnit report recording 96 tests, zero errors and zero failures.
- External dependencies (services, libraries, releases): Pester and PSScriptAnalyzer via the
  drm-copilot PoshQC MCP tooling. No new dependency is added.

## Data / API / Config Impact

- User-facing or API changes: the coverage entry point prints additional summary lines. A new
  PowerShell function becomes available to callers that dot-source
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.
- Data or migration considerations: none. No persisted format changes.
- Logging/telemetry updates (if any): the printed summary only; no telemetry exists in these
  scripts.
- Compatibility notes (CLI flags, config schemas, versioning): no CLI flag is removed or renamed. If
  a switch is added to suppress or extend the printed summary, its default must preserve current
  behavior for existing callers apart from the added output.

## Test Strategy

- Regression tests to add or update: a named Pester test over an in-memory Cobertura fixture that
  contains duplicate method rows, asserting the de-duplicated covered and valid counts, and
  asserting in the same test that the descendant-axis computation over the same fixture produces
  different counts. Tests live under `tests/scripts/vscode/`.
- Unit tests for the fixed behavior and boundaries: allowlist defaulting and explicit override;
  a package outside the allowlist excluded from both numerator and denominator; the rendered summary
  text.
- Edge cases and negative scenarios: a document with no `<packages>` node (explicit error); a
  retained package with no classes (zero counts, rate `0`); a class carrying `<lines>` but no
  `<methods>`, and a class carrying `<methods>` but no `<lines>`; a branch row present in only one
  of the two views; a line whose class-level and method-level `condition-coverage` denominators
  differ.
- Error handling verification: the no-`<packages>` case asserts the thrown message.
- Coverage impact and targets for changed lines/modules: at or above the 90% floor that CLAUDE.md
  section UT2 sets for newly added modules and functions, measured directly over `scripts/vscode`
  (see AC10 for why the bundled PowerShell coverage artifact cannot supply this).
- Toolchain commands to run (format, lint, test): PoshQC format, then PoshQC analyze, then PoshQC
  test, over `scripts/vscode` and `tests/scripts/vscode`, restarting from format if any step changes
  a file. Type checking does not apply to PowerShell.
- Manual validation steps: run the new function and the pinned snippet over a committed Cobertura
  document from another feature folder and record both results side by side.

## Acceptance Criteria

- [x] **AC1 - A committed, callable de-duplicated first-party aggregation exists.** A PowerShell
      function defined in a file under `scripts/vscode/` accepts a Cobertura document and returns
      the first-party covered and valid line and branch counts. It resolves after dot-sourcing
      `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` alone. It obtains per-class figures by
      calling `Get-CoberturaClassLineSummary` or `Get-CoberturaPackageLineSummary` rather than
      re-deriving the de-duplication rule, and it introduces no second definition of that rule.
- [x] **AC2 - The counting rule is stated as an invariant and matched by the delivered code.** The
      function's comment-based help states the invariant from Root Cause Analysis verbatim in
      substance: each (class, source line number) pair is counted exactly once, so the four counts do
      not depend on whether a source line also appears under a `<method>` element. The four numbered
      trace steps in Root Cause Analysis describe the defect the delivered implementation removes,
      and no step of that trace remains reachable from the new code path.
- [x] **AC3 - The descendant-axis selection is absent from the delivered scripts and tests.** A
      case-sensitive search for the literal `.//line` over `scripts/vscode/` returns zero matches
      after the change, matching the verified pre-change baseline of zero. Under
      `tests/scripts/vscode/` the only permitted occurrences are inside the differential test
      required by AC6, which reproduces the defective selection deliberately in order to fail
      against it.
- [x] **AC4 - The first-party allowlist is derived, not hard-coded.** The new function's allowlist
      parameter defaults to `Get-KoverageProjectAllowlist` and the delivered code under
      `scripts/vscode/` contains no literal list of production assembly names. A test supplies an
      explicit override and asserts that a package outside the override is excluded from both the
      numerator and the denominator.
- [x] **AC5 - A named Pester test asserts de-duplicated counts over a duplicate-method-row
      fixture.** A test in `tests/scripts/vscode/` uses an in-memory here-string Cobertura fixture
      in which at least one class repeats the same line numbers in both its class-level `<lines>`
      rollup and a method-level `<lines>` block, including at least one branch row carrying
      `condition-coverage`. The fixture duplicates rows at differing multiplicities - at least one
      branch row appears in both views and at least one further branch row appears in only one of
      them, so both the line counts and the branch counts differ between the two computations - because a
      uniformly duplicated fixture cannot discriminate, for the reason recorded in Repro & Evidence.
      The
      test asserts `LinesValid`, `LinesCovered`, `BranchesValid` and `BranchesCovered` as counts.
      It asserts no rate or percentage, for the reason recorded in Repro & Evidence and in the
      existing comment at `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` lines
      210-211. The test creates no file on disk.
- [x] **AC6 - The test demonstrably fails against the pre-fix aggregation.** The demonstration
      mechanism is a differential assertion inside the same test file: a private test-scoped helper
      reproduces the pinned descendant-axis snippet quoted in Repro & Evidence, is run over the
      identical fixture, and the test asserts that its `BranchesValid` and `LinesValid` are strictly
      greater than the values the new function returns, and that the new function's values are the
      correct ones for that fixture. The assertion fails if the new function reproduces the
      descendant-axis behavior, so it cannot pass vacuously. The evidence artifact for this
      criterion records both computations' four counts for the fixture.
- [x] **AC7 - A corroborating measurement against a committed real Cobertura report is recorded.**
      Both computations are run over a Cobertura document committed under another feature folder -
      the designated document is
      docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml -
      and an evidence artifact under this feature's evidence/qa-gates/ folder records both sets of four
      counts, both derived percentages, and the document's SHA-256. The artifact confirms the
      descendant-axis `lines-valid` is strictly greater than the de-duplicated `lines-valid`. This
      criterion does not require re-deriving the 79.38% or 77.03% figures from issue 809, because
      the report that produced them is not committed.
- [x] **AC8 - The entry point reports the aggregate.** `Invoke-MSTestWithCoverageMain` in
      `scripts/vscode/Invoke-MSTestWithCoverage.ps1` writes the four counts and both derived
      percentages to output during a coverage run, and retains its existing
      `Done. Coverage artifact: <path>` line. The reported text is produced by a pure formatting
      function that takes the aggregation result and returns a string, and a Pester test asserts
      that string for a known input without invoking a coverage run.
- [x] **AC9 - Format and analyzer gates.** PoshQC format over `scripts/vscode` and
      `tests/scripts/vscode` returns ok and leaves `git status --porcelain` empty on a re-run.
      PoshQC analyze over `tests/scripts/vscode` returns ok. PoshQC analyze over `scripts/vscode`
      reports no more findings than the pre-change baseline captured in this feature's
      evidence/baseline/ folder on the same branch head, and zero findings are attributed to any file
      this feature adds or modifies. A demand for zero findings across `scripts/vscode` is
      explicitly not made, because the folder carries a pre-existing inherited baseline.
- [x] **AC10 - Test and coverage gates.** PoshQC test over `tests/scripts/vscode` returns ok with
      zero failures and zero errors, and the JUnit total exceeds the pre-change baseline total
      captured in this feature's evidence/baseline/ folder, reflecting the added tests. Coverage for the
      files this feature adds or modifies is measured by a direct Pester run whose code-coverage
      path covers `scripts/vscode`, emitting JaCoCo output stored under this feature's
      evidence/qa-gates/ folder, and reaches the 90% floor that CLAUDE.md section UT2 sets for newly
      added modules and functions. The artifact records why the bundled
      artifacts/pester/powershell-coverage.xml is not used: its packages cover only .claude and
      .codex paths and none under the scripts tree, so it cannot measure this feature's changed
      files.
- [x] **AC11 - File-size ceiling holds.** Every file under `scripts/vscode/` and
      `tests/scripts/vscode/` is at or below 500 physical lines after the change, measured with
      `(Get-Content -LiteralPath <path>).Count`, per .claude/rules/general-code-change.md.
      Before-and-after counts for every file in the Write Set are recorded in this feature's
      evidence/qa-gates/ folder.
- [x] **AC12 - No threshold is lowered.** The branch diff changes no threshold constant. In
      particular `Assert-CoberturaLineCoverageThreshold` in
      scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 retains its existing comparison
      value and its existing failure message. If any figure recomputed during this delivery falls
      below a governing threshold, it is recorded as a finding in this feature's evidence and no
      threshold is changed, per epic Non-Goal 5.
- [x] **AC13 - Scope boundary holds.** `git diff --name-only` against the merge-base lists only
      paths under `scripts/vscode/`, `tests/scripts/vscode/`, and this feature's folder. CLAUDE.md,
      any path under .claude/skills/ or .claude/rules/, .editorconfig, BannedSymbols.txt, every C#
      production and test file, and every historical plan or evidence document carrying the
      descendant-axis snippet are unmodified. The diff listing is recorded in this feature's
      evidence/qa-gates/ folder.
- [ ] **AC14 - The CLAUDE.md CUT3 wording mismatch is handed off, not fixed here.** The mismatch
      between CUT3 step 4 and the dotnet-coverage route is recorded in this feature's evidence with
      a pointer to a separate promotion or issue raised for it, and CLAUDE.md does not appear in
      this branch's diff.

## Risks & Mitigations

- Technical or operational risks:
  - **Placement risk.** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` has 31 lines of
    headroom against the 500-line ceiling, so adding the function there is likely to breach it.
    Mitigation: AC11 makes the ceiling a gate with recorded before-and-after counts, and the
    existing split precedent in PackageRate.ps1 and Threshold.ps1 shows the accepted remedy.
  - **Vacuous test risk.** A test written against a rate would pass whether or not the fix is
    correct, because uniform duplication leaves ratios unchanged. Mitigation: AC5 forbids rate
    assertions and AC6 requires a differential assertion against the reproduced defective selection.
  - **Adoption risk.** Plan authors may keep pasting the old snippet even after the function exists.
    Mitigation is limited by Non-Goal 2: the plan-authoring guidance lives under .claude/ and must
    be changed upstream in drm-copilot. This feature makes the correct route available and
    discoverable from the entry point's output; propagation into the plan-authoring skill is a
    separate upstream change and is recorded as such.
  - **Corrected figures may fall below a threshold on some future run.** Mitigation: AC12 requires
    that outcome to be recorded as a finding rather than accommodated.
- Mitigations and rollbacks: the change is additive; reverting the commit restores prior behavior.

## Rollout & Follow-up

- Release/rollout steps: merge with the epic `review-residuals-2026-09-08` integration branch. No
  runtime deployment; the change affects developer tooling only.
- Post-fix monitoring or clean-up tasks:
  - Raise the CLAUDE.md CUT3 step 4 wording correction as a separate promotion (Non-Goal 1, AC14).
  - Raise an upstream drm-copilot change so plan-authoring guidance points at the committed function
    instead of an inline snippet (Non-Goal 2).
- Links: issue #815 (https://github.com/drmoisan/TaskMaster/issues/815); epic manifest
  docs/features/epics/review-residuals-2026-09-08/epic.md; originating finding F7 in
  docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/policy-audit.2026-09-08T01-35.md.
