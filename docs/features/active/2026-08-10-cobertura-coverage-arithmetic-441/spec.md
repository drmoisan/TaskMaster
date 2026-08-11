# 2026-08-10-cobertura-coverage-arithmetic-441 (Spec)

- Work Mode: full-bug
- **Issue:** #441 (also closes #478)
- **Epic:** build-ci-coverage-gate-fidelity (wave 0)
- **Parent (optional):** `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md`
- **Integration Branch:** `epic/build-ci-coverage-gate-fidelity-integration`
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T18-24
- **Status:** Prepared (plan preflight-cleared; execution deferred to epic-orchestrator)
- **Version:** 1.1 (AC-15 and AC-16 amended — see § Acceptance Criteria)
- **Complexity Band:** C3 (`cross_module_contract_change`)
- **AC Source:** This document. Under work mode `full-bug`,
  `.claude/skills/acceptance-criteria-tracking/SKILL.md` resolves `spec.md` as the sole
  authoritative acceptance-criteria source. `user-story.md` in this folder is narrative context
  only and deliberately contains no checkbox items; `issue.md` § Acceptance Criteria is a pointer.

## Context

- **Summary of the bug and its impact.** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
  post-processes the Cobertura report emitted by `dotnet-coverage` into the shape the Koverage VS
  Code extension consumes. In doing so it recomputes and overwrites the report's root coverage
  attributes. The recomputation selects `<line>` elements over the XPath **descendant** axis, and
  the documents this pipeline actually processes carry every source line twice — once under
  `<class>/<methods>/<method>/<lines>` and once again in the class-level `<class>/<lines>` rollup.
  Every line is therefore counted twice (#441). A second, dependent symptom appears on merged
  classes: `Merge-CoberturaClassesByFilename` unions the class-level `<lines>` of same-filename
  classes correctly but leaves `<methods>` un-merged, so the recomputed per-file `line-rate` blends
  the correct union with only the primary class's method-level lines and matches neither
  denominator (#478).
- **Observed environment(s).** Windows 11, PowerShell 7+. The report is produced by the local VS
  Code task `test: MSTest with Coverage (Koverage)` (`.vscode/tasks.json:189-209`) invoking
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`. GitHub Actions
  CI (`.github/workflows/ci.yml:118-160`) uploads raw `.coverage` binaries and never produces or
  reads a Cobertura document, so CI is not affected by either defect or by this fix.
- **Customer impact and severity.** The consumer is the repository's own quality-gate machinery.
  Severity **High** for both issues. Every reported repository-wide and per-assembly line-coverage
  figure is computed over an inflated denominator, and the inflation is not uniform across
  assemblies because line duplication is not uniform across classes. Epic #136 gates each of its
  fifteen children on a per-file `line-rate` produced by the defective path. Every committed
  coverage baseline in the repository consumes the wrong figure.
- **First observed date and version(s) impacted.** The defective expression is present in the
  current `main` (`a682c7a2`) and in every committed Cobertura evidence artifact produced by
  `ConvertTo-KoverageCoberturaXml`. Formally identified 2026-08-10 by the research cited below;
  a prior evidence artifact
  (`.../424/evidence/qa-gates/coverage-delta.2026-08-07T00-48.md:65`) had already observed the
  symptom on 2026-08-07 but misattributed it to `dotnet-coverage` denominator instability.

Authoritative research:
`docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/research/2026-08-10T14-20-cobertura-arithmetic-research.md`.

## Repro & Evidence

**Steps to reproduce (no test run required; the input is a committed, fixed document).**

1. From the worktree root, dot-source `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.
2. Load the committed **raw** generator output
   `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`
   as `[xml]`. It is raw `dotnet-coverage` output (absolute filenames, no `<sources>` element), so
   its own root attributes are the generator's ground truth.
3. Read the document's own root attributes, then call
   `Get-CoberturaCoverageSummary -XmlDocument $doc` and compare.

**Expected vs actual.**

| Quantity | Document's own root attribute (ground truth) | `Get-CoberturaCoverageSummary` today |
| --- | --- | --- |
| `lines-valid` / `LinesValid` | **79957** | **161086** (verified by element count) |
| `lines-covered` / `LinesCovered` | **56124** | inflated (exact value to be recorded pre-change) |
| `branches-valid` / `BranchesValid` | **23109** | inflated (exact value to be recorded pre-change) |
| `branches-covered` / `BranchesCovered` | **13472** | inflated (exact value to be recorded pre-change) |

**Per-file evidence (confirmed by independent re-measurement).** In
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
the merged class for `QuickFiler\Controllers\QfcHomeController.Iteration.cs` (element at `:22612`
through `:22739`):

| Quantity | Value |
| --- | --- |
| Class-level union (correct) | 45 / 56 = **0.803571** |
| Method-level lines of the primary class | 24 / 24 |
| Emitted `line-rate` attribute (blended, defective) | **0.8625** = 69 / 80 |

Class-level enumeration: 20 lines with `hits=1`, 11 with `hits=0`, 1 with `hits=1`, then 12, 7 and
5 with `hits=1` — total 56, covered 45. Method-level: `Iterate ()` 12 lines, `Iterate2 ()` 7 lines,
`SwapStopWatch ()` 5 lines — 24, all covered. 45 + 24 = 69; 56 + 24 = 80; 69/80 = 0.8625. The
issue's arithmetic table is confirmed exactly.

**Repository-wide evidence for the same committed sample.** Element counts (ripgrep, indentation
anchored, class level = 12 spaces, method level = 16 spaces): class-level `<line>` = 62345,
method-level `<line>` = 48504, sum = 110849 = the emitted `lines-valid` attribute. Class-level
`hits="0"` = 9332, so class-level covered = 53013.

| Metric | Emitted (defective) | Corrected (class-level) | Delta |
| --- | --- | --- | --- |
| `lines-valid` | 110849 | **62345** | -48504 |
| `lines-covered` | 94937 | **53013** | -41924 |
| `line-rate` | 0.856453 | **0.850317** | **-0.61 pp** |

**Logs / snippets.** The single defective expression:

```powershell
# scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-132
foreach ($cls in $pkg.SelectNodes('.//class')) {
    foreach ($line in $cls.SelectNodes('.//lines/line')) {   # :122 -- DEFECT (descendant axis)
        $totalLines++
        if ([int]$line.hits -gt 0) { $coveredLines++ }
        if ($line.branch -eq 'True' -and ...) {              # :128-131 -- branch accumulator
            $coveredBranches += [int]$Matches[1]
            $totalBranches   += [int]$Matches[2]
        }
    }
}
```

**Frequency / determinism.** Fully deterministic and data-dependent on document shape. The defect
manifests on every document in which any `<class>` carries a non-empty `<methods>` subtree — which
is every real `dotnet-coverage` output (534 of 534 classes in the committed sample). It does **not**
manifest on documents whose classes have no `<methods>` element or an empty `<methods />`, which is
precisely the shape of every existing test fixture (see § Test Strategy).

## Scope & Non-Goals

**In scope.**

- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — line- and branch-counting arithmetic
  only (`Get-CoberturaCoverageSummary`, plus the `$classSummaryXml` delegation inside
  `Merge-CoberturaClassesByFilename`), and one new pure helper.
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` — six new `It` blocks plus
  direct unit tests for the new helper's precedence branches. No existing block is modified.
- Evidence capture of pre-change and post-change figures under
  `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/`.

**Out of scope / non-goals (explicit).**

1. **No coverage threshold may be re-tuned.** Threshold reconciliation is owned by child feature
   #494, which runs after this one (wave 2). Recorded handoff to #494, stated as fact and nothing
   more: against the uniform >= 85% line floor in `.claude/rules/general-unit-test.md`, the
   corrected repository-wide line rate for the #424 committed sample is **85.0317%** — a margin of
   **0.03 percentage points**. This feature proposes no threshold change and makes no threshold
   edit.
2. **No `[ExcludeFromCodeCoverage]` nested-lambda work (#457).** That is a separate dependent child
   (wave 1). It changes *which* lines enter the denominator; this feature changes only *how* they
   are counted.
3. **No edits to `CLAUDE.md` or anything under `.claude/rules/`.** Sibling features #512 and #494
   own those documents.
4. **No edit to `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.** A repository-wide grep confirms
   it contains no line-axis selection; its only involvement is the call at `:340`. Its missing
   `\.claude\` discovery exclusion (`:296-302`) is a real but separate defect and is recorded below
   as follow-up candidate 3, not fixed here.
5. **`Invoke-MSTestWithCoverage.Helpers.ps1:219` (`./lines/line`) must NOT be changed.** It is the
   child axis and is already correct; it builds the merged union. Editing it would destroy the
   correct half of #478 and leave both defects in place.
6. **Do not merge or strip the `<methods>` subtrees**, and do not recompute package-level rates.
   See § Proposed Fix, rejected alternatives.

**Explicitly excluded systems.** GitHub Actions CI (produces no Cobertura document); the
feature-review coverage hook `.claude/hooks/validate-feature-review-coverage.ps1` (reads JaCoCo
`artifacts/csharp/coverage.xml`, a different format from a different producer — do not conflate);
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (mocks
`ConvertTo-KoverageCoberturaXml` outright and asserts only invocation count).

## Root Cause Analysis

**Confirmed root cause: exactly one defective selection exists in the repository.**

| Site | Expression | Verdict |
| --- | --- | --- |
| `Invoke-MSTestWithCoverage.Helpers.ps1:122` | `$cls.SelectNodes('.//lines/line')` | **The one and only defective selection.** Descendant axis; matches the class-level rollup *and* every method-level copy. |
| `Invoke-MSTestWithCoverage.Helpers.ps1:219` | `$classNode.SelectNodes('./lines/line')` | **ALREADY CORRECT.** Child axis. This is the merge union builder and MUST NOT be changed. |
| `Invoke-MSTestWithCoverage.Helpers.ps1:270-273` | `Get-CoberturaCoverageSummary -XmlDocument $classSummaryXml` | **Indirect.** `Merge-CoberturaClassesByFilename` does not itself select over the descendant axis; it inherits the `:122` defect through this delegation, which is what makes #478 a consequence of #441 rather than an independent defect. |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | — | **No line-axis selection at all. No change required.** |

Sites deliberately excluded from the change: `:121` (`.//class` — equivalent to
`./classes/class` given Cobertura's fixed nesting; optional tightening, not required), `:254`/`:258`
(`./conditions`, child axis, unrelated), `:324` (`//class[@filename]`, filename rewrite only), and
the three child-axis XPaths in the existing test file (`:86`, `:87`, `:126`).

**Signals supporting the diagnosis.**

- Structural: the emitted document is indented by `XmlTextWriter` at
  `Helpers.ps1:349-354`, giving a fixed two-space-per-level layout, so indentation-anchored counts
  are a reliable proxy for the XPath axis. Class-level `<line>` sits at 12 spaces, method-level at
  16. The two populations partition the `<line>` set exactly (62345 + 48504 = 110849); no third
  nesting depth exists.
- Arithmetic: 110849 equals the emitted `lines-valid` attribute exactly. Class-level uncovered
  (9332) plus method-level uncovered (6580) equals `lines-valid` - `lines-covered`
  (110849 - 94937 = 15912). The accounting closes.
- Generator parity: on the **raw** baseline document the class-level count (79957) equals its own
  `lines-valid` exactly, and class-level minus class-level-uncovered (79957 - 23833) equals its own
  `lines-covered` (56124) exactly, while the both-axes count is 161086.
- Branch parity: on the raw document, method-level branch lines alone (9627, each with denominator
  >= 2) would contribute at least 19254, so a both-axes total would be >= 41638 against the actual
  `branches-valid="23109"`. The both-axes hypothesis for branches is decisively falsified.

**Affected components.** One production file
(`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, 357 lines), two functions within it
(`Get-CoberturaCoverageSummary`, `Merge-CoberturaClassesByFilename`), and their downstream
consumers of the six rewritten root attributes.

## Proposed Fix

### Design summary (what changes where)

Introduce **one new pure helper** that reduces a single `<class>` element to a deduplicated per-line
map, and call it from both existing paths.

```
Get-CoberturaClassLineSummary -ClassNode <System.Xml.XmlElement> -> [pscustomobject]
    LineMap         : hashtable   # [int] line number -> { Node; Hits; Covered; Total }
    TotalLines      : int
    CoveredLines    : int
    TotalBranches   : int
    CoveredBranches : int
```

Construction rule:

1. Enumerate `./lines/line` (the class-level rollup) **then** `./methods/method/lines/line`.
2. Key by `[int]$node.number`. On a repeat key: `hits = max(existing, candidate)`; `branch = True`
   if either entry is `True`; the `condition-coverage` value is taken from the entry with the
   larger `Total`, tie-broken by the larger `Covered` — reusing the existing pure helper
   `Get-CoberturaLineConditionCoverageParts` (`Helpers.ps1:146-165`), which is the same precedence
   already implemented for the merge at `Helpers.ps1:240-245`.
3. `TotalLines` = distinct key count; `CoveredLines` = keys whose `hits > 0`; `TotalBranches` /
   `CoveredBranches` = sums of the retained `condition-coverage` fractions over distinct keys.

Then:

- **`Get-CoberturaCoverageSummary` (`:98-144`)** — delete the descendant-axis selection at `:122`
  and replace the inner loop body (`:122-132`) with one call to the helper per class, accumulating
  the four returned totals.
- **`Merge-CoberturaClassesByFilename` (`:167-292`)** — keep the union builder at `:217-268`
  **exactly as it is**, and replace `:270-273` with a direct call to the new helper on
  `$mergedClassNode`, then set the attributes at `:275-276` from its result.

The union formulation is preferred over a bare child-axis switch because it cannot silently drop a
method-only line if the class-level rollup is ever absent, and because it implements the
`max(hits)` dedup the issue's expected behavior states literally. On all observed data it collapses
to the class-level set and therefore reproduces the generator's own arithmetic.

**Rejected alternatives (recorded, with reasons).**

| Alternative | Verdict | Reason |
| --- | --- | --- |
| Bare child-axis switch (`.//lines/line` -> `./lines/line` at `:122`, nothing else) | Rejected as primary | Smallest possible diff and identical on all observed data, but silently drops any method-only line and does not implement the stated `max(hits)` dedup. Its behavior is pinned as a guard by fixture F5. |
| **Merge the `<methods>` subtrees** of same-filename classes | **Rejected** | Sibling classes sharing a filename are compiler-generated partners (`Foo` and `Foo.<>c`, async state machines) that routinely both declare `name=".ctor" signature="()"`. Appending sibling `<method>` elements produces duplicate `(name, signature)` pairs, breaking any consumer that keys methods that way — including the per-method `line-rate` technique this repository uses for coverage-delta work. Deduplicating by `(name, signature)` would be worse: it discards genuinely distinct methods. Also out of scope under `CLAUDE.md` Bugfix Workflow step 2 (minimal targeted fix, no opportunistic refactors). Recorded as follow-up candidate 2. |
| **Strip `<methods>`** from merged classes | **Rejected outright** | Destroys per-method `line-rate` data that coverage-delta work in this repository actively relies on; per-`<method>` figures are the only rollup-method-insensitive numbers available. |
| **Recompute package-level rates** | **Rejected — out of scope** | `ConvertTo-KoverageCoberturaXml` writes only root attributes (`:342-347`) and merged-class attributes (`:275-281`), so every `<package line-rate=...>` is stale after package filtering and class merging. This is a genuine but separate latent defect; touching it widens the diff without serving #441 or #478. Recorded as follow-up candidate 1. |

### Boundaries and invariants to preserve

- **Not one `<line>` or `<method>` element changes.** The fix alters exactly six root attributes and
  two attributes (`line-rate`, `branch-rate`) on merged class elements. Koverage gutters and VS
  Code coverage decoration are driven by class-level `<lines>` `hits` values, which are untouched.
- `Helpers.ps1:219` and the whole union builder `:217-268` remain byte-identical.
- The existing `condition-coverage` precedence rule (`:240-245`) is reused, not re-derived.
- `Get-CoberturaCoverageSummary` keeps its `[xml]$XmlDocument` signature and its
  `throw 'Cobertura XML does not contain a <packages> node.'` guard (`:111-114`); it remains the
  document-level entry point used by `ConvertTo-KoverageCoberturaXml` at `:341`.
- Unmerged classes retain the generator's own `line-rate` / `branch-rate` attributes; the
  post-processor has never rewritten them and must not start.
- `Helpers.ps1` must remain under the 500-line file ceiling
  (`.claude/rules/general-code-change.md`). It is 357 lines; the estimated net addition is 50-70
  lines.

### Dependencies or blocked work

- **Depends on:** nothing. This is epic wave 0.
- **Blocks:** #457 (wave 1), which cannot be verified against a still-doubled denominator, and
  transitively #494 (wave 2).
- **Coordination note (outside this feature's control):** twenty-one unmerged branches from epic
  #136 gate on per-file line rates computed by the defective code. None touches any file this
  feature modifies, so there is no merge conflict, but their committed coverage evidence will not
  reproduce against the corrected arithmetic.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

| File | Change | Est. scope |
| --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | Add `Get-CoberturaClassLineSummary`; rewrite the inner loop of `Get-CoberturaCoverageSummary` (`:116-134`) to call it; replace `:270-273` with a direct helper call. | ~50-70 lines net, 1 production file |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | Add F1-F6 plus direct precedence-branch unit tests for the new helper. No existing block modified. | 1 test file |
| `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/` | Pre-change and post-change A/B figures. | evidence only |

Within the `.claude/rules/powershell.md:37-41` change budget (up to 2 production PowerShell files
plus corresponding tests).

#### Functions/classes/CLI commands impacted

- **New:** `Get-CoberturaClassLineSummary` (pure; takes an `[System.Xml.XmlElement]`, performs no
  I/O, mutates nothing in the source document).
- **Modified:** `Get-CoberturaCoverageSummary` (inner loop only), `Merge-CoberturaClassesByFilename`
  (`:270-273` only).
- **Unmodified:** `Get-KoverageProjectAllowlist`, `ConvertTo-KoverageRelativePath`,
  `Get-CoberturaLineConditionCoverageParts`, `ConvertTo-KoverageCoberturaXml`.
- **CLI surface:** unchanged. No parameter is added, removed, or renamed on any exported function.

#### Data flow and validation changes

Input document -> package filtering -> filename normalization -> `Merge-CoberturaClassesByFilename`
(union builder unchanged; per-class rate now from the new helper) -> `<sources>` insertion ->
`Get-CoberturaCoverageSummary` (now dedup-based) -> six root attributes rewritten -> indented
serialization. Only the two summary computations change; the traversal order and every mutation of
the DOM outside those two attribute sets stay as they are.

New helper preconditions: `-ClassNode` is mandatory and typed `[System.Xml.XmlElement]`. A class
with no `<lines>` element, no `<methods>` element, or both absent is valid input and yields
`TotalLines = 0`. `$mergedClassNode` is an orphan clone (`$primaryNode.CloneNode($true)` at `:200`)
that is not re-parented until `:283`; child-axis XPath on an orphan element works normally, so no
`ImportNode` is required.

#### Error handling and logging updates

None. The module has no logging surface and the single existing `throw` guard is preserved
verbatim. The new helper introduces no new failure mode: a malformed or absent
`condition-coverage` attribute already falls through `Get-CoberturaLineConditionCoverageParts` to
`{ Covered = 0; Total = 0 }`. Do not add silent catch-alls (`.claude/rules/powershell.md`,
Prohibited Behaviors).

#### Rollback/feature-flag considerations

Not applicable and not wanted. No flag, no opt-out, no dual-path. The defective arithmetic has no
legitimate consumer, and a flag would leave two denominators in the repository — the exact
condition #478 describes. Rollback is `git revert` of a single commit touching two files.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- **Input:** a Cobertura 1.x document, either raw `dotnet-coverage` output (absolute `filename`
  attributes, no `<sources>`) or an already post-processed document. Both shapes are accepted, as
  today.
- **Output:** the same document with six root attributes and two per-merged-class attributes
  rewritten. All values remain **strings**, formatted exactly as today: rates via
  `[string]([math]::Round($covered / $total, 6))` and counts via `[string]$int`, with `'0'` when the
  denominator is zero. Do not change the rounding, the string coercion, or the zero-denominator
  fallback — existing assertions such as `line-rate | Should -Be '1'` depend on them.

#### Required configuration keys and defaults

None. No new parameter, environment variable, settings key, or config file entry. `.vscode/settings.json`
(`koverage.coverageFileNames`, `koverage.coverageFilePaths`) is unchanged.

#### Backward-compatibility expectations

- The XML schema of the emitted document is unchanged: same elements, same attributes, same order,
  same indentation.
- `scripts/temp-extract-coverage.ps1:13` (per-class `line-rate`) begins reading correct values for
  merged classes; values for unmerged classes are unchanged. `:47` (package-level `line-rate`)
  reads the same stale values as before — a pre-existing separate defect, not a regression.
- Historical committed coverage evidence becomes non-comparable with post-fix figures. This is the
  intended, unavoidable consequence of correcting the arithmetic and is why the epic requires a
  re-captured baseline in the same change.

#### Performance constraints (latency/throughput/memory)

No regression permitted, and a modest improvement is expected. The helper visits each `<line>`
node at most once per class and allocates one hashtable entry per distinct line number, versus
today's two full passes over an inflated node set. Replacing `:270-273` additionally removes one
synthetic `[xml]` document construction and one deep `ImportNode` per merged filename group. The
Pester suite must remain fast enough for frequent runs
(`.claude/rules/general-unit-test.md`, Fast Execution); this is enforced by the fixture-size
constraint in § Test Strategy.

## Assumptions, Constraints, Dependencies

**Assumptions.**

1. `dotnet-coverage`'s Cobertura writer defines `lines-valid` / `lines-covered` / `branches-valid` /
   `branches-covered` from the class-level rollup only. Verified by exact reconciliation against
   the committed raw document (79957 / 56124 / 23109 / 13472).
2. Method-level line numbers are a subset of the class-level rollup. Verified on three
   spot-checked classes and consistent with assumption 1. **It has not been exhaustively proven
   across all 534 classes** — that requires script execution. The chosen union-with-dedup design is
   correct whether or not the subset property holds universally, which is why it is preferred over
   a bare child-axis switch.
3. The two committed sample documents remain present at their cited paths for the duration of this
   feature. They are used as one-time evidence inputs only, never as in-suite fixtures.

**Constraints.**

- PowerShell 7+ only; toolchain is PoshQC format -> PSScriptAnalyzer analyze -> Pester
  (`.claude/rules/powershell.md:13-20`), run via the MCP server functions, not VS Code task
  wrappers. Type checking is not applicable to PowerShell.
- **`CLAUDE.md`'s `/p:Nullable=enable` type-check command is a known defect (#522) and is not a
  gate for this change.** It produces roughly 200-414 spurious errors on a clean `main`. This
  feature touches no C# file.
- Change budget: <= 2 production PowerShell files plus corresponding tests.
- File size ceiling: 500 lines per file.
- No temporary files anywhere in the test suite (`.claude/rules/general-unit-test.md`).
- Evidence must be written under `<FEATURE>/evidence/<kind>/` per
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. No `artifacts/` evidence path is
  permitted.

**External dependencies.** None added. Pester 5.x and PSScriptAnalyzer are already in use.

## Data / API / Config Impact

- **User-facing or API changes.** None. No exported function signature changes; one internal helper
  is added.
- **Data or migration considerations.** No migration. Previously generated coverage documents are
  not rewritten; they simply carry the old (wrong) root attributes and should be treated as
  non-comparable with post-fix output.
- **Logging/telemetry updates.** None.
- **Compatibility notes.** No CLI flag, config schema, or version change. The emitted Cobertura
  document remains schema-identical.

## Test Strategy

### Regression tests to add

Six new inline-fixture Pester `It` blocks in the **existing** file
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, matching its established style:
single-quoted here-strings (`@'` ... `'@`) declared inside the `It` block, cast via
`[xml]$resultXml = ConvertTo-KoverageCoberturaXml ...`, with `-ProjectNames` supplied explicitly
whenever the package name is not a real repository assembly (see the existing comment at `:29-30`).
**No existing block is modified.**

| ID | Target | Fixture shape | Assertions (post-fix) | Pre-fix result |
| --- | --- | --- | --- | --- |
| **F1** | #441 lines | One package, one class. `<methods>` with one `<method>` carrying lines 10 (`hits=1`), 11 (`hits=0`), 12 (`hits=1`); class-level `<lines>` carrying the identical three. | `lines-valid` = `'3'`, `lines-covered` = `'2'`, `line-rate` = `'0.666667'` | 6 / 4 / `'0.666667'` |
| **F2** | #441 branches | As F1 plus line 12 `branch="True" condition-coverage="50% (1/2)"` on **both** axes, with a `<conditions>` child. | `branches-valid` = `'2'`, `branches-covered` = `'1'` | 4 / 2 |
| **F3** | #478 merge | Two classes, same `filename`. Primary `Ns.Foo`: `<methods>` with lines 56,57,58 (`hits=1`) and class-level `<lines>` 56,57,58. Sibling `Ns.Foo.<>c`: `<methods>` with lines 12,13 (`hits=0`) and class-level `<lines>` 12,13. | merged class `line-rate` = `'0.6'` (3/5); merged class-level `<lines>` has exactly 5 `line` children, numbers `12,13,56,57,58` ascending | `'0.75'` (6/8) |
| **F4** | `max(hits)` dedup | One class; line 5 appears in `.ctor ()` with `hits=1` **and** in `.ctor (int)` with `hits=0`; class-level `<lines>` has line 5 `hits=1`. | `lines-valid` = `'1'`, `lines-covered` = `'1'` | 3 / 2 |
| **F5** | rollup-absent guard | One class with `<methods>` carrying lines 20 (`hits=1`) and 21 (`hits=0`) and **no class-level `<lines>` element at all**. | `lines-valid` = `'2'`, `lines-covered` = `'1'` — the lines must NOT be dropped | 2 / 1 (unchanged today; guards against a regression a naive child-axis switch would introduce) |
| **F6** | structure preservation | Reuse the F3 document. | merged class still has a `<methods>` element with exactly the primary's one `<method>` child; no `<line>` element's `hits` attribute differs from the input | passes today; locks the "do not merge or strip `<methods>`" decision |

F3 is a deliberate miniature of the independently confirmed
`QfcHomeController.Iteration.cs` case: `0.8625` (69/80) -> `0.803571` (45/56) at repository scale
becomes `0.75` (6/8) -> `0.6` (3/5) at fixture scale.

**Branch-rate trap — mandatory fixture-design constraint.** For the `QfcHomeController` class the
branch *ratio* is unchanged by the double count: class-level totals are 8/12 = 0.666667 and
both-axes totals are 12/18 = 0.666667, while the *counts* are inflated by 50%. **A regression test
that asserts only on `branch-rate` passes against the defective code.** Branch fixtures MUST assert
on `branches-valid` and `branches-covered`. F2's post-fix expectation of 2/1 against a pre-fix 4/2
satisfies this; an assertion on `branch-rate` alone would not.

### Unit tests for the fixed behavior and boundaries

If `Get-CoberturaClassLineSummary` is exposed (it is dot-sourced with the rest of the file and
therefore will be), add one direct unit test per branch of its precedence rule: candidate `Total`
greater than existing; `Total` equal and `Covered` greater; neither (existing value retained). This
is where scenario completeness (`.claude/rules/general-unit-test.md`) is cheapest to achieve, and
it exercises the rule without routing through the whole document pipeline.

### Edge cases and negative scenarios

- Class with no `<methods>` element (existing fixture shape) — result must be unchanged.
- Class with an empty `<methods />` (existing fixture shape) — result must be unchanged.
- Class with `<methods>` but no class-level `<lines>` (F5) — method lines must be retained.
- Same line number on both axes with differing `hits` (F4) — `max` wins.
- Same line number on both axes where only one carries `branch="True"` — the merged entry is a
  branch line.
- A line whose `condition-coverage` attribute is absent or unparseable — contributes 0/0, no throw.
- Zero total lines in a document — `line-rate` remains `'0'`, no division by zero.

### Error handling and logging verification

Assert that `Get-CoberturaCoverageSummary` still throws
`'Cobertura XML does not contain a <packages> node.'` for a document without `//packages`. No new
logging exists to verify.

### Do NOT use the committed sample as an in-suite fixture

`.../424/evidence/qa-gates/coverage-final.cobertura.xml` is **186,913 lines** carrying 110,849
`<line>`, 6,330 `<method>` and 534 `<class>` elements. Rejected as a Pester fixture for three
reasons: (a) an `[xml]` cast materializes a full DOM — hundreds of megabytes of managed objects and
multiple seconds of parse time per `It` block, violating the fast-execution and
single-behavior requirements; (b) path fragility — it lives under `docs/features/active/2026-08-06-.../`
and moves out of `active/` when #424 completes, silently breaking the test; (c) a 534-class document
cannot express a targeted assertion, so a failure would not identify the faulty unit. It is a
one-time **evidence** input only, per § Evidence and Baseline Re-capture.

### Why the existing suite cannot detect either defect

Read in full: all eight existing `It` blocks. **Every existing fixture uses either no `<methods>`
element or an empty `<methods />`** (`:17-22` no element; `:61`, `:68`, `:105`, `:112`, `:146`,
`:157` empty). On such documents `.//lines/line` and `./lines/line` select the identical node set,
so the defective and correct implementations are indistinguishable. This includes the block at
`:97-133`, whose `lines-valid | Should -Be '3'` assertion looks like an arithmetic guard but is
computed over an empty `<methods />` and therefore never exercised the defect. **The existing suite
is structurally incapable of detecting #441 or #478.** That is the reason the fix must ship with
F1-F6, all of which populate `<methods>` with real `<line>` children.

**Zero existing tests are expected to break.** Per-block analysis: the two path-normalization
blocks assert no arithmetic; the two merge blocks assert on a union of `{10,11,12}` that is
identical under both axes; the `.Test`-exclusion block has a single production class with two
class-level lines and an empty `<methods />`; the three `Get-KoverageProjectAllowlist` blocks do not
touch the arithmetic at all. Any existing-test failure is therefore a genuine regression signal, not
expected churn.

### Hard test constraints

- No temporary files on disk, anywhere, for any purpose.
- No mocks for the arithmetic paths — exercise the real code. Mocking is confined to the existing
  `Get-ChildItem`/`Get-Content` allowlist-fallback block, which is untouched.
- Pester 5.x, `Describe`/`Context`/`It`, one behavior per `It`.
- Deterministic: no clock, no randomness, no filesystem, no network, no working-directory
  assumption. Results must be identical in Terminal and in the VS Code Test Explorer.

### Toolchain commands to run

PowerShell-only work; run in order and restart from step 1 if any step fails or changes files:

1. Format — `mcp__drm-copilot__run_poshqc_format`
2. Analyze — `mcp__drm-copilot__run_poshqc_analyze`
3. Type check — **not applicable to PowerShell**; skip to testing.
4. Test — `mcp__drm-copilot__run_poshqc_test` using
   `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`.

The C# toolchain is not a gate for this change. In particular `CLAUDE.md`'s
`/p:Nullable=enable` command is defective (#522) and must not be invoked as a gate here.

### Manual validation steps

The A/B evidence run in the next section is the manual validation. It is deterministic and requires
no test execution.

## Evidence and Baseline Re-capture

Two figures are required — **pre-change and post-change, recorded numerically**.

### PRIMARY method: deterministic A/B over a fixed committed input (no test run)

This change alters post-processing arithmetic only. Re-running the MSTest suite to obtain the two
figures would confound the fix's effect with documented `dotnet-coverage` denominator
nondeterminism. The correct experiment holds the input fixed.

Input (fixed, committed, raw generator output — carries the ground truth in its own root
attributes):
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`

Procedure: dot-source `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, cast the file to
`[xml]`, print the document's own root attributes, then run
`Get-CoberturaCoverageSummary -XmlDocument $doc`. Execute once against unmodified `Helpers.ps1`
(pre-change) and once after the fix (post-change).

| | `LinesValid` | `LinesCovered` | `BranchesValid` | `BranchesCovered` |
| --- | --- | --- | --- | --- |
| Input document's own root attributes (ground truth) | 79957 | 56124 | 23109 | 13472 |
| **Required AFTER the fix** | **79957** | **56124** | **23109** | **13472** |
| Expected BEFORE the fix | **161086** | record numerically; must exceed 56124 | record numerically; must exceed 23109 | record numerically; must exceed 13472 |

`LinesValid = 161086` pre-fix is established by element count. The pre-fix covered and branch
figures were not obtainable without execution and must be recorded from the actual pre-change run.

A **second, package-filtered A/B** is run the same way by reprocessing
`.../424/evidence/qa-gates/coverage-final.cobertura.xml` through `ConvertTo-KoverageCoberturaXml`.
Required corrected root figures: `lines-valid` = 62345, `lines-covered` = 53013, `line-rate`
~= 0.850317 (pre-change: 110849 / 94937 / 0.856453).

### Evidence locations (canonical, non-overridable)

- Pre-change figures: `<FEATURE>/evidence/baseline/` where `<FEATURE>` is
  `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441`.
- Post-change figures and toolchain results: `<FEATURE>/evidence/qa-gates/`.
- Fail-before regression artifacts (F1-F6 failing against unmodified `Helpers.ps1`):
  `<FEATURE>/evidence/regression-testing/`.

Every artifact carries `Timestamp: <yyyy-MM-ddTHH-mm>`, `Command: <exact command>`, and
`EXIT_CODE: <int>`; baseline artifacts additionally carry `Output Summary:`. **No `artifacts/`
evidence path is permitted** — `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/` and
`artifacts/evidence/` are forbidden and are blocked by the `enforce-evidence-locations.ps1`
PreToolUse hook.

### SECONDARY method (only if the plan demands a live repository-wide figure)

A live re-capture via `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
is possible but is not the acceptance evidence, for three recorded reasons: the script has no
`\.claude\` discovery exclusion (`:296-302`), so a mandatory pre-run assertion that no discovered
`*.Test.dll` path contains `\.claude\` is required; the 9-assembly single-process run is
known-unstable (`Test host process crashed`, tracked as #511) and its documented recovery — a
per-assembly `/InIsolation` loop — yields nine separate `.coverage` files that do not reconstitute a
single repository-wide Cobertura document without a merge step; and residual `pwsh`/testhost
processes from a prior timed-out run cause deterministic-looking hangs and anomalous rates. Prefer
the primary method.

## Risks & Mitigations

| Risk | Likelihood | Mitigation |
| --- | --- | --- |
| An implementer "fixes" `:219` (the correct child-axis union builder) instead of `:122` | Medium — the GitHub issue text points at the wrong lines | The site table in § Root Cause Analysis names `:219` as correct and off-limits; F3 and F6 fail if the union is broken; an acceptance criterion requires `:219` to remain byte-identical. |
| A branch regression test asserts only on `branch-rate` and passes against the defective code | Medium — the ratio is genuinely unchanged for the confirmed class | F2 asserts on `branches-valid` and `branches-covered`; an acceptance criterion states the requirement explicitly. |
| A naive child-axis switch silently drops method-only lines | Low on observed data, non-zero in general | The union-with-dedup design cannot drop them; F5 pins the behavior. |
| Merging `<methods>` is attempted as an "obvious" completion of #478 | Medium | Recorded as an explicit non-goal with the duplicate `(name, signature)` rationale; F6 fails if `<methods>` is merged or stripped. |
| The corrected repository-wide figure lands 0.03 pp above the 85% floor and someone lowers the threshold to create margin | Medium | Explicit non-goal; an acceptance criterion requires zero diff to threshold-bearing files. The margin is a recorded handoff to #494, which owns the decision. |
| Historical coverage evidence becomes non-comparable, confusing epic #136 reviews | High (certain) | Stated in § Data / API / Config Impact and in the epic's coordination notes. The re-captured baseline in this change is the new reference point. |
| A live suite re-capture is attempted and confounds the fix with `dotnet-coverage` nondeterminism or the #511 crash | Medium | The primary evidence method holds the input fixed and requires no test run. |

## Rollout & Follow-up

**Release/rollout steps.** Merge to `epic/build-ci-coverage-gate-fidelity-integration` as epic wave
0. There is no runtime deployment, no flag, and no migration. Rollback is a single-commit revert.
Downstream: #457 (wave 1) may proceed once this lands; #494 (wave 2) follows #457.

**Post-fix clean-up owned by this feature.** None beyond the evidence artifacts.

**Follow-up issue candidates — NOT to be fixed in this feature.** Each was surfaced by the research
and must be filed through the MCP promotion lifecycle rather than left as prose:

1. **Package-level rates are never recomputed.** `ConvertTo-KoverageCoberturaXml` writes only root
   attributes (`:342-347`) and merged-class attributes (`:275-281`), so after package filtering and
   class merging every `<package line-rate=...>` / `<package branch-rate=...>` is stale. Consumed by
   `scripts/temp-extract-coverage.ps1:47`.
2. **A merged class retains only the primary class's `<methods>`.** The emitted document's methods
   therefore do not account for all of its class-level lines. Deferred here because merging carries
   a duplicate `(name, signature)` hazard on compiler-generated sibling classes.
3. **`scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302` lacks a `\.claude\` discovery
   exclusion.** It filters only `\bin\<Configuration>\`, `\obj\` and `\ref\`, so running with
   `-SearchRoot .` from the main checkout descends into `.claude\worktrees\agent-*\**` and picks up
   stale sibling-worktree assemblies, producing bogus `AssemblyInitialize` signature failures.
4. **A stored agent memory records an incorrect generalization.**
   `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:34-36`
   asserts that repo-wide root attributes are already deduped and need no adjustment. That is true
   only of raw `dotnet-coverage` output and false for any post-processed
   `ConvertTo-KoverageCoberturaXml` artifact. Correct it once this fix lands.

**Links.**

- Issues: #441 (primary), #478 (also closed).
- Epic: `docs/features/epics/build-ci-coverage-gate-fidelity/epic.md` (wave 0).
- Research: `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/research/2026-08-10T14-20-cobertura-arithmetic-research.md`.
- Feature issue record: `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/issue.md`.
- Narrative context: `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/user-story.md`.
- Cross-check of the correct recipe:
  `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-delta.2026-08-07T00-48.md:10`.

## Acceptance Criteria

**Amendment 2026-08-10T18-24 (AC-15 and AC-16 only; preparation-time, before any execution).** Two criteria were unsatisfiable as originally written and were corrected so that execution cannot check off a false statement. AC-15 had required *zero* PSScriptAnalyzer findings on both changed files; `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` carries one pre-existing `PSUseSingularNouns` finding on `Get-CoberturaLineConditionCoverageParts`, and clearing it would require renaming an exported function that § Implementation strategy lists as Unmodified and § Technical specifications forbids changing. AC-15 is now a no-new-findings gate against a recorded Phase 0 baseline. AC-16 had enumerated only `{baseline, qa-gates, regression-testing}`; the plan legitimately also writes to `issue-updates/` and `other/`, both canonical under `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, so the enumeration was completed. No other criterion text is altered, no threshold is affected, and no criterion is weakened in substance. A further preparation-time correction on 2026-08-10T21-40 scoped AC-16's schema-field requirement to command-step artifacts, because the plan legitimately writes narrative artifacts (for example the AC status summary) that record no command and therefore carry no `EXIT_CODE:`; requiring that field of every artifact made the criterion unsatisfiable and would have forced a false check-off. No threshold is affected and the criterion is not weakened in substance.

- [ ] **AC-1 (headline: generator parity).** With the fix applied, dot-sourcing
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and running
  `Get-CoberturaCoverageSummary` over
  `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`
  returns exactly `LinesValid = 79957`, `LinesCovered = 56124`, `BranchesValid = 23109`,
  `BranchesCovered = 13472` — reproducing that document's own root attributes.
- [ ] **AC-2 (pre-change figure).** The pre-change run of the same procedure against unmodified
  `Helpers.ps1` is recorded numerically and shows `LinesValid = 161086`, with `LinesCovered`,
  `BranchesValid` and `BranchesCovered` recorded as concrete integers, each strictly greater than
  its AC-1 post-change counterpart.
- [ ] **AC-3 (package-filtered A/B).** Reprocessing
  `.../424/evidence/qa-gates/coverage-final.cobertura.xml` through `ConvertTo-KoverageCoberturaXml`
  yields root `lines-valid = 62345`, `lines-covered = 53013`, and `line-rate` = 0.850317 (post-fix),
  recorded against the pre-fix values 110849 / 94937 / 0.856453.
- [ ] **AC-4 (per-file merged rate).** The merged-class `line-rate` defect is corrected from
  0.8625 (69/80) to 0.803571 (45/56) for the `QfcHomeController.Iteration.cs` case, demonstrated in
  the suite by fixture F3, whose merged class asserts `line-rate` = `'0.6'` (3/5) where the
  unmodified code produces `'0.75'` (6/8), and whose merged class-level `<lines>` contains exactly
  five `line` children numbered 12, 13, 56, 57, 58 in ascending order.
- [ ] **AC-5 (branch counts deduplicated).** Fixture F2 asserts `branches-valid` = `'2'` and
  `branches-covered` = `'1'` where the unmodified code produces 4 and 2. No branch regression
  assertion in the suite relies on `branch-rate` alone.
- [ ] **AC-6 (helper contract).** A new pure function `Get-CoberturaClassLineSummary` exists in
  `Helpers.ps1`, takes a mandatory `-ClassNode [System.Xml.XmlElement]`, enumerates `./lines/line`
  then `./methods/method/lines/line`, keys by `[int]$node.number`, resolves repeats by `max(hits)`
  / `branch=True` if either / `condition-coverage` from the larger `Total` (tie-broken by larger
  `Covered`) via `Get-CoberturaLineConditionCoverageParts`, and returns `LineMap`, `TotalLines`,
  `CoveredLines`, `TotalBranches`, `CoveredBranches`.
- [ ] **AC-7 (defect removed at its one site).** The expression
  `$cls.SelectNodes('.//lines/line')` no longer appears anywhere in
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, and `Get-CoberturaCoverageSummary`
  accumulates its four totals from `Get-CoberturaClassLineSummary`.
- [ ] **AC-8 (correct site untouched).** The union builder at `Helpers.ps1:217-268`, including the
  child-axis selection `$classNode.SelectNodes('./lines/line')`, is byte-identical to `main` in the
  diff.
- [ ] **AC-9 (delegation replaced).** The `$classSummaryXml` synthetic-document block formerly at
  `Helpers.ps1:270-273` is removed, and the merged class's `line-rate` / `branch-rate` attributes
  are set from a direct `Get-CoberturaClassLineSummary` call on `$mergedClassNode`.
- [ ] **AC-10 (structure preserved).** Fixture F6 passes: the merged class still carries a
  `<methods>` element containing exactly the primary class's one `<method>` child, and no `<line>`
  element's `hits` attribute differs from the input. The `<methods>` subtrees are neither merged nor
  stripped.
- [ ] **AC-11 (six fixtures present and passing).** Fixtures F1-F6 exist as six new `It` blocks in
  `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`, use inline single-quoted
  here-strings matching the file's existing style, create no file on disk, use no mock in any
  arithmetic path, and all pass.
- [ ] **AC-12 (fail-before evidence).** F1, F2, F3 and F4 are demonstrated to fail against
  unmodified `Helpers.ps1` with the pre-fix values stated in § Test Strategy, and the failing run is
  recorded under
  `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/regression-testing/`.
- [ ] **AC-13 (helper precedence branches covered).** Direct unit tests cover all three branches of
  the `condition-coverage` precedence rule: candidate `Total` greater; `Total` equal and `Covered`
  greater; neither (existing value retained).
- [ ] **AC-14 (zero existing tests broken).** All eight pre-existing `It` blocks in
  `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` pass unmodified, and the diff
  shows no edit to any of them — including the `lines-valid | Should -Be '3'` assertion.
- [ ] **AC-15 (toolchain green).** A single clean pass of PoshQC format
  (`mcp__drm-copilot__run_poshqc_format`, no files changed), PSScriptAnalyzer
  (`mcp__drm-copilot__run_poshqc_analyze` plus a per-file `Invoke-ScriptAnalyzer` breakdown, **no
  finding on either changed file that is absent from the recorded Phase 0 baseline**; the single
  pre-existing `PSUseSingularNouns` finding on `Get-CoberturaLineConditionCoverageParts` is
  baselined and out of scope, since clearing it requires renaming an exported function this spec
  marks Unmodified), and Pester (direct `Invoke-Pester` run, `FailedCount` = 0) is recorded under
  `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/qa-gates/`.
- [ ] **AC-16 (canonical evidence locations).** Every evidence artifact for this feature lives under
  `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/{baseline,qa-gates,regression-testing,issue-updates,other}/`,
  every command-step artifact carries `Timestamp`, `Command` and `EXIT_CODE` fields (baseline
  artifacts additionally carry `Output Summary`), every narrative artifact that records no command
  carries `Timestamp` and is individually enumerated in the final sweep, and no evidence artifact is
  written under any `artifacts/` path.
- [ ] **AC-17 (no threshold re-tuned).** The diff contains no change to `CLAUDE.md`, to any file
  under `.claude/rules/`, to `coverage.config`, or to any other file that states a coverage
  threshold. The 85.0317%-versus-85% observation is recorded in evidence as a handoff to #494 and
  nowhere acted upon.
- [ ] **AC-18 (scope boundary held).** The diff touches exactly two source files —
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and
  `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` — plus evidence and feature
  documents. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is unchanged, including its missing
  `\.claude\` discovery exclusion.
- [ ] **AC-19 (file ceiling).** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` remains under
  500 lines (357 before the change).
- [ ] **AC-20 (follow-ups filed).** The four follow-up candidates in § Rollout & Follow-up are filed
  as GitHub issues through the promotion lifecycle, with their issue numbers recorded in this
  feature's evidence, and none of them is fixed in this change.
