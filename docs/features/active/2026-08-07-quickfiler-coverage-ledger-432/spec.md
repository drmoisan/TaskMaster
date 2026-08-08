# quickfiler-coverage-ledger — Spec

- **Issue:** #432
- **Parent (optional):** epic #136 (`quickfiler-per-file-coverage`), child F1, wave 0
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08
- **Status:** Draft
- **Version:** 0.2

## Overview

Epic #136 requires that every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj`
reach at least 80% line coverage or sit on an explicitly ratified exemption ledger. Fifteen sibling
child features and the F16 capstone are blocked on three shared prerequisites that must be settled
exactly once:

1. **The denominator is undefined.** No authoritative per-file classification exists that says which
   of the 121 compiled files are `testable` and which are `ratified-exempt`. Without it, a child
   cannot state its own acceptance criteria, because it cannot tell whether a file such as
   `ItemViewer.Designer.cs` is inside its target set.
2. **The existing `[ExcludeFromCodeCoverage]` attributes are unratified.** QuickFiler carries a
   population of these attributes that has never been judged against the irreducible-remainder
   standard. Per the epic manifest, an attribute sitting on a *testable* seam is a Blocking finding.
   Until each one has a recorded disposition, children would independently and inconsistently decide
   whether to remove or keep them, and would collide on shared configuration.
3. **There is no per-file coverage measurement.** `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
   emits a Cobertura report, but nothing derives per-file line-coverage percentages from it. Fifteen
   children each building their own reporting would produce fifteen inconsistent numbers and a
   capstone (F16) that cannot close.

Aggregate assembly coverage does not satisfy issue #136, which measures success per production file.

### Authoritative inputs and settled decisions

This specification is built on the verified research artifact
`research/2026-08-07T22-15-quickfiler-coverage-ledger-research.md`. That document is ground truth
and it corrects the epic manifest in several places. Its numbered decisions D1–D17 are adopted here
as specification and are not re-opened during planning or execution.

Corrections that this feature must encode rather than inherit from the manifest:

- **The manifest's "33 currently carry `[ExcludeFromCodeCoverage]`" is refuted (D1).** The verified
  ground truth of the compiled surface is **40 attribute usages across 21 compiled files, of which
  14 are type-level and 26 are member-level; 24 compiled files are fully coverage-suppressed once
  partial-class inheritance is applied.** The manifest's 33 is a count of *files containing the
  string* across the whole `QuickFiler/` tree: 21 compiled files carrying a real attribute, plus 5
  compiled files whose only match is a comment or XML-doc mention, plus 7 files that are not
  compiled at all. The figure 33 must not be restated anywhere in this feature's deliverables as a
  target. The acceptance-criteria wording below is unchanged and is satisfied by 40 dispositions.
- **Several manifest `[X]` markers are mis-scoped or under-marked.** `QfcScanProgressBandMapper.cs`
  carries no attribute; the `QfcHighConfidencePreFilter.cs` attribute decorates a second top-level
  type (`FolderScoringService`) rather than the file's primary type; the six `QfcItemController.*`
  and the `BreadcrumbPopupUiOperations.cs` markers are member-level, not type-level; and
  `ItemViewer.Breadcrumb.cs`, `QfcDatamodel.FrameBuilding.cs`, and `QfcDatamodel.QueueProcessing.cs`
  are suppressed by inheritance but are not marked. The manifest's `[X]` markers are advisory only.
- **The manifest's 121-file count and its Feature File Assignments table are confirmed sound.**
  Every one of the 121 compiled files maps to exactly one child, with no gap, duplicate, or phantom.
  The ledger adopts the assignment table verbatim as its `owning_child` column.

## Behavior

Deliver the wave-0 enabler for epic #136. No QuickFiler production behavior changes; no file under
`QuickFiler/` is modified by this child. Three deliverables.

### 1. Per-file classification ledger

A ledger at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`, with one row for
every file listed as `<Compile Include=...>` in `QuickFiler/QuickFiler.csproj` — 121 rows. The
compiled list is derived from the csproj itself, not from a directory walk: `QuickFiler/Legacy/**`,
`QuickFiler/Notes/**`, and the orphan files enumerated in research §2.1 exist on disk but are not
compiled and are out of scope.

Each row records:

- **File path** — the repo-relative path with backslash separators, exactly as the Cobertura
  `filename` attribute expresses it (for example `QuickFiler\Controllers\QfcQueue.cs`). The path is
  the row key. The leaf filename is never the key: `IQfcFormController.cs` exists as two distinct
  compiled files, `Controllers\IQfcFormController.cs` (43 lines) and `Interfaces\IQfcFormController.cs`
  (25 lines).
- **Line count** — physical line count, counting a final line that lacks a trailing newline (D14).
  The ledger header states this counting method so the capstone can reproduce it. Three designer
  files differ by exactly one line from the manifest under this method
  (`BayesianPerformanceViewer.Designer.cs` 499, `EfcViewer.Designer.cs` 4277,
  `QfcFormViewer.Designer.cs` 258); the ledger records the verified value.
- **Owning child feature** — taken verbatim from the epic manifest's "Feature File Assignments".
- **Classification** — exactly one of `testable` or `ratified-exempt`.
- **Rationale** — required for every `ratified-exempt` row, meeting one of exactly three permitted
  grounds and no other:
  - generated WinForms `*.Designer.cs` files and generated `Properties/` files;
  - interface-only declarations with no executable behavior (the type-only/interface-only clause in
    `.claude/rules/general-unit-test.md`);
  - irreducible WinForms/COM wiring where no interface seam, injectable delegate, or adapter can
    isolate the logic, with the row stating specifically why.

The ledger carries an explicit **"Reconciliation with epic manifest"** section (D1) recording the
verified attribute figures above, explaining that the manifest's 33 decomposes without residual as
21 + 5 + 7 files containing the string, stating that the manifest's `[X]` markers are advisory, and
citing the research document.

The ledger also records that the double-counted repository-wide `lines-valid` defect in
`Get-CoberturaCoverageSummary` is a known pre-existing condition that is **out of scope for this
feature and is tracked separately at issue #441** (D5). This feature must not change that behavior;
changing the repository-wide figure would perturb every existing gate and every committed evidence
baseline. The statement exists so a reviewer does not read the omission as an oversight.

### 2. Disposition of every `[ExcludeFromCodeCoverage]` attribute in the compiled surface

Every attribute usage is treated as unratified until this ledger judges it. **The disposition unit
is the attribute usage, not the file (D2)**, because the two have materially different consequences:
removing a member-level attribute returns one member's lines to the denominator, whereas removing a
type-level attribute returns an entire type — and every partial file of that type — at once.

Each disposition records `kind` ∈ `{type, member, inherited}`, the site as `<path>:<line>`, the
decorated declaration, the disposition (`ratified` with a rationale, or `remove` naming the owning
child that must remove it and cover the file), and the owning child.

- `kind: type` — 14 usages across 14 files.
- `kind: member` — 26 usages across 7 files.
- `kind: inherited` — informational rows recording that a file emits no coverage data because
  another partial of its type carries a type-level attribute. Inherited rows are **not** counted
  toward the 40 and are not dispositions.

Every `remove` disposition must carry sequencing instructions to the owning sibling (D16):

- **Write the tests first and remove the attribute last.** Removal expands the denominator
  immediately and the numerator only as tests land, so removing first registers as a coverage
  regression both for the `QuickFiler` package and repository-wide.
- **A type-level removal is a per-type decision, not a per-file decision.** It affects every partial
  of that type simultaneously — up to seven files for `ItemViewer`, which would return
  `ItemViewer.Designer.cs` (6,224 lines) to the denominator alongside the hand-written partials —
  and must be planned and executed as a single unit.

### 3. Repeatable per-file coverage harness

A PowerShell harness that consumes the Cobertura output already produced by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, emits per-file line-coverage percentages for the
`QuickFiler` assembly, and exits non-zero when any `testable` file is below 80%.

Behavioral requirements, each derived from verified schema facts:

- **Recompute rates from `<line>` nodes (D4).** Per-file rates are computed by unioning `<line>`
  nodes by `filename` and deduplicating by line number, taking `max(hits)` on collision. The harness
  **must never read the `<class>` `line-rate` attribute** — for merged classes that value is
  recomputed through a double-counting path that weights the primary class's lines twice — and
  **must never use the `.//lines/line` descendant axis**, which matches both the method-level
  `<lines>` and the class-level `<lines>` rollup and therefore counts every line twice. Research
  §4.2 and §4.3 prove both defects numerically.
- **Select the package explicitly (D12).** The harness selects `<package name="QuickFiler">` by
  name and fails with an input error if it is absent. It must not scan all packages: other
  first-party packages are present in the same document and contain same-named files.
- **Classification comes from a machine-readable sidecar (D3).** The authoritative classification
  input is `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.json`. The Markdown
  ledger is the human view of the same data. The two must not diverge. Parsing the Markdown table
  is prohibited: it would couple the gate for fifteen children to prose formatting.
- **Absent-file semantics are explicit (D7).** Absence from the report means *no instrumentable
  code was emitted*, which is genuinely different from *instrumented but never executed*. The full
  case table is in "Data & State" below. The load-bearing cases: absent + `ratified-exempt` is not a
  failure; absent + `testable` **is** a failure with a distinct `NO DATA` message; present with zero
  `<line>` nodes is `n/a` and is never reported as 0%; present in the report but not in the ledger
  **is** a failure.
- **Exactly 80.0% passes (D9).** Implemented as `-lt 0.80` against the unrounded ratio. The
  displayed value is rounded to one decimal place using `InvariantCulture`. Comparing the rounded
  display value is prohibited: 7999/10000 displays as 80.0 but must fail.
- **Deterministic output (D10).** Rows are ordered by an ordinal sort on the repo-relative path.
  Percentages are rendered with the `'0.0'` format string under `InvariantCulture`. The comparable
  report body contains no timestamp, host name, or path outside the repository.
- **Path matching is a string operation (D11).** A `<Compile Include="Controllers\Foo.cs" />` entry
  maps to the Cobertura `filename` `QuickFiler\Controllers\Foo.cs` by prefixing `QuickFiler\`. Both
  sides are normalised to backslash and compared `OrdinalIgnoreCase`. Forward-slash input is
  accepted by normalisation, since a report captured under a non-default `-PathSeparator` uses
  forward slashes. Paths containing spaces (for example `QuickFiler\Helper Classes\...`) must be
  handled without quoting assumptions.
- **Pure logic is separated from I/O.** Parsing, rate computation, threshold evaluation, and report
  formatting are pure functions returning objects; all file reads, report writes, and the exit code
  live in the entry script.

## Inputs / Outputs

**Inputs**

| Input | Form | Default |
| --- | --- | --- |
| Cobertura report | `-CoberturaPath <string>` (file path) | mandatory |
| Classification ledger | `-LedgerJsonPath <string>` | `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.json` |
| Package name | `-PackageName <string>` | `QuickFiler` |
| Threshold | `-ThresholdPercent <double>` | `80.0` (also carried as `threshold_percent` in the ledger JSON) |
| Report destination | `-OutputPath <string>` | none; stdout only when omitted |
| Ad-hoc exemption override | `-ExemptFile <string[]>` | empty; diagnostic use only, never the primary mechanism |

Every environment-derived default is an overridable parameter, so tests never depend on ambient
state.

**Outputs**

- The rendered per-file report on stdout via `Write-Output`, and additionally to `-OutputPath` when
  supplied. Children capture the report as coverage evidence under `<FEATURE>/evidence/qa-gates/`
  per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- Per-row diagnostics for failing rows on the error stream via `Write-Error`, so failures remain
  visible when stdout is redirected to an evidence file.
- Exit code per the contract below.

**Config keys and defaults**

- `coverage-ledger.json` carries `schema_version`, `generated_from`, `source_commit`, `package`,
  `threshold_percent`, and `files[]`. The threshold lives in data rather than in code.

**Versioning and backward compatibility**

- `schema_version: 1`. The harness rejects an unrecognised `schema_version` as an input error rather
  than attempting a best-effort read.
- No existing script's behavior, output, or exit code changes. `Invoke-MSTestWithCoverage.ps1` and
  `Invoke-MSTestWithCoverage.Helpers.ps1` are read-only dependencies of this feature.

## API / CLI Surface

**Files (D6)**

| Role | Path |
| --- | --- |
| Pure logic — function definitions only, no top-level side effects, dot-sourceable | `scripts/vscode/Get-PerFileCoverage.Helpers.ps1` |
| Entry point — all file I/O, report rendering, exit code | `scripts/vscode/Get-PerFileCoverage.ps1` |
| Pester tests for the pure logic | `tests/scripts/vscode/Get-PerFileCoverage.Helpers.Tests.ps1` |
| Pester tests for ledger completeness and consistency | `tests/scripts/vscode/QuickFilerCoverageLedger.Tests.ps1` |

**Functions** — advanced functions with `[CmdletBinding()]`, explicit `[OutputType(...)]`, comment-based
help, approved verbs, and `[Parameter(Mandatory = ...)]` with validation attributes, matching the
conventions already established in `Invoke-MSTestWithCoverage.Helpers.ps1`:

- `Merge-CoberturaFileLines` — pure: `<line>` nodes for one filename → deduplicated line set,
  `max(hits)` on collision.
- `Get-CoberturaPerFileCoverage` — pure: `[xml]` document + package name → per-file row objects.
- `Test-PerFileCoverageThreshold` — pure: rows + classification + threshold → verdict object.
- `Format-PerFileCoverageReport` — pure: rows → deterministic report text.
- `Invoke-PerFileCoverageGate` — entry function in `Get-PerFileCoverage.ps1`.

The entry script uses the dot-source-safe guard already established at
`Invoke-MSTestWithCoverage.ps1:346`–`348`, so a test can dot-source it without executing it. The
harness invokes no external executable, so no wrapper seam is required; its only I/O is reading the
Cobertura and ledger files and optionally writing the report.

**Example invocation**

```powershell
pwsh -File scripts/vscode/Get-PerFileCoverage.ps1 `
    -CoberturaPath 'docs/features/active/<feature>/evidence/qa-gates/coverage-final.cobertura.xml' `
    -OutputPath   'docs/features/active/<feature>/evidence/qa-gates/per-file-coverage.txt'
```

**Exit-code contract (D8)**

| Code | Meaning |
| --- | --- |
| `0` | Every `testable` file is at or above the threshold. |
| `1` | At least one `testable` file is below the threshold, or a `NO DATA` row, or an `UNLEDGERED` row. |
| `2` | Input error: file missing, XML not well-formed, no `<packages>` node, no `QuickFiler` package, ledger JSON missing or malformed, unrecognised `schema_version`. |

Distinguishing `2` from `1` matters because a child must not mistake a broken input for a coverage
regression. No existing repository script uses more than `0`/`1`, so all three codes are asserted in
Pester. The pure layer raises malformed input with a bare `throw` and a specific literal message,
matching `Invoke-MSTestWithCoverage.Helpers.ps1:113`; the entry script catches and maps to exit `2`.

**Validation rules**

- `classification` ∈ `{testable, ratified-exempt}`.
- `exempt_ground` is a closed enum: `generated-designer`, `interface-only`,
  `irreducible-host-wiring`. A `ratified-exempt` row must carry a non-null `exempt_ground` and a
  non-empty `rationale`. A `testable` row must carry `exempt_ground: null`.
- `attribute_dispositions[].kind` ∈ `{type, member, inherited}`;
  `attribute_dispositions[].disposition` ∈ `{ratified, remove}` for `type` and `member` kinds.

## Data & State

**Ledger JSON shape** — one entry per compiled file, keyed by the repo-relative backslash path:

```json
{
  "schema_version": 1,
  "generated_from": "QuickFiler/QuickFiler.csproj",
  "source_commit": "74be1964",
  "package": "QuickFiler",
  "threshold_percent": 80.0,
  "files": [
    {
      "path": "QuickFiler\\Controllers\\QfcQueue.cs",
      "lines": 610,
      "owning_child": "F2",
      "classification": "testable",
      "exempt_ground": null,
      "rationale": null,
      "attribute_dispositions": []
    },
    {
      "path": "QuickFiler\\Viewers\\ItemViewer.Designer.cs",
      "lines": 6224,
      "owning_child": "F14",
      "classification": "ratified-exempt",
      "exempt_ground": "generated-designer",
      "rationale": "WinForms Designer-generated code; exempt under CLAUDE.md UT2 (b).",
      "attribute_dispositions": [
        { "kind": "inherited", "from": "QuickFiler\\Viewers\\ItemViewer.cs:20" }
      ]
    }
  ]
}
```

A non-inherited disposition carries
`{ kind, site: "<path>:<line>", target: "<declaration>", disposition, owning_child, rationale }`.

**Invariants**

1. The JSON is authoritative; the Markdown ledger is the human view. Regenerating the Markdown from
   the JSON at authoring time is acceptable, but the gate must not depend on the Markdown at run
   time.
2. Bijection: every `<Compile Include=...>` path in `QuickFiler/QuickFiler.csproj` has exactly one
   `files[]` entry, and every `files[]` entry corresponds to a `<Compile Include=...>` path.
   `files[].length == 121`.
3. The Markdown table's data-row count equals the JSON `files` length.
4. Non-inherited dispositions total 40: 14 of `kind: type` and 26 of `kind: member`.
5. `files[]` is ordinal-sorted by `path` at authoring time.

**Harness row-state table (D7)** — the complete case matrix:

| Case | Classification | Row status | Coverage column | Exit contribution |
| --- | --- | --- | --- | --- |
| Absent from report | `ratified-exempt` | `EXEMPT (not measured)` | `n/a` | none |
| Absent from report | `testable` | `NO DATA` (distinct message naming the file) | `n/a` | **failure** |
| Present, zero `<line>` nodes | either | `NO EXECUTABLE LINES` | `n/a` (never 0%) | none |
| Present with lines | `ratified-exempt` | percentage, informational | percentage | none |
| Present with lines | `testable` | percentage, compared to threshold | percentage | failure iff below |
| Present but not in ledger | — | `UNLEDGERED` (distinct message) | percentage | **failure** |

The absent-plus-`testable` case is the one a naive implementation would silently pass, and it is
exactly what a stale build produces after a child removes a type-level attribute. Treating it as a
failure is the required default.

**Migration or backfill** — none. No existing artifact is rewritten and no state is persisted
outside the two ledger files and the optional report file.

## Constraints & Risks

Hard constraints. Each is a Blocking finding if violated.

- **No production C# change.** No file under `QuickFiler/` may be modified by this child.
- **No policy re-legislation.** `.claude/rules/**`, `CLAUDE.md`, and `.github/instructions/**` must
  not be modified. The policy reconciliation between the `CLAUDE.md` § UT2 COM/VSTO/WinForms
  exemption and the `.claude/rules/general-unit-test.md` Coverage Exclusion Policy is already
  recorded in the epic manifest; this child implements it and does not re-legislate it. Research
  §7.2 confirms the two rules do not conflict on mechanism: QuickFiler's exclusions are entirely
  attribute-based and no `coverage.config` entry names a QuickFiler path, so the only open question
  is whether a given file qualifies — which is the irreducible-remainder test the manifest settles.
- **No change to repository-wide coverage thresholds**, and no change to the existing repository-wide
  `line-rate` / `lines-valid` computation (D5, issue #441).
- **PowerShell toolchain order.** PoshQC format, then PSScriptAnalyzer, then Pester, in that order,
  restarting from the first stage whenever any stage fails or changes a file.
- **Test location and hygiene.** Pester tests live at the mirrored `tests/scripts/...` path.
  Colocation in the script tree is prohibited. No temporary files may be created in tests. Tests
  must be deterministic and must construct in-memory XML fixtures — modelled on the real document
  shape including the method-level `<lines>` nesting — rather than reading files from disk.
- **Separation of concerns.** Pure parsing and threshold logic is separated from file and process
  I/O per `.claude/rules/general-code-change.md`. The pure-logic file must contain only function
  definitions plus `Set-StrictMode`, with no top-level side effects, so a test can dot-source it.
- **File size.** No file may exceed 500 lines. `Invoke-MSTestWithCoverage.Helpers.ps1` is already
  357 lines, so appending the new logic there is not viable; the new logic goes in its own file.
- **Change budget.** Two new production PowerShell files plus the deletion of one dead script, and
  two test files, sit inside the per-batch cap of three production and three test files in
  `.claude/rules/powershell.md`.

Risks:

- **Blocking risk.** Fifteen sibling children and the capstone are blocked until this child merges,
  so an incomplete or non-deterministic ledger propagates to every downstream feature.
- **Ledger staleness from in-flight work (D15).** Features #400 and #424 produce no textual conflict
  with this child, which touches no file under `QuickFiler/`; both features' production files are
  already present in the compiled surface at `74be1964`. The residual risk is semantic: if either
  branch later adds, removes, or renames a compiled file, or adds or removes an attribute, the
  ledger's file list and attribute inventory go stale and every wave-1 child inherits a stale
  denominator. The csproj-vs-ledger completeness assertion in this feature's own Pester suite turns
  that staleness into a test failure at the next toolchain run rather than a silent divergence.
- **PoshQC scan coverage is unverifiable from the repository (D17).** No repo-local scan
  configuration exists, and `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` — referenced
  by `.claude/rules/powershell.md` — is absent from this worktree. The chosen directories
  (`scripts/vscode/`, `tests/scripts/vscode/`) already contain formatted, analyzer-clean, Pester-run
  files, so they inherit a demonstrably working configuration. Phase 0 of the plan must still
  confirm empirically that `run_poshqc_format`, `run_poshqc_analyze`, and `run_poshqc_test` pick up
  the new files before the harness is treated as gated.

## Implementation Strategy

**Scope of change**

- Add `scripts/vscode/Get-PerFileCoverage.Helpers.ps1` (pure logic, target 200–300 lines).
- Add `scripts/vscode/Get-PerFileCoverage.ps1` (entry point, target 100–150 lines).
- Add `tests/scripts/vscode/Get-PerFileCoverage.Helpers.Tests.ps1`.
- Add `tests/scripts/vscode/QuickFilerCoverageLedger.Tests.ps1`.
- Add `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.json`.
- Add `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.
- Delete `scripts/temp-extract-coverage.ps1` (D13). It is dead, untested, PSScriptAnalyzer-dirty,
  hard-codes another feature's evidence path, and reads the unreliable `line-rate` attribute. Its
  continued existence invites a sibling child to use it instead of the new harness. A repository
  search confirms no live code or task references it; the only references are in research and
  archived planning documents. The alternative considered and rejected was leaving it in place with
  a deprecation comment, which preserves the risk of accidental use for no benefit.

**New functions** — the five listed under "API / CLI Surface".

**Dependency changes** — none. The harness uses only built-in PowerShell XML and JSON handling and
invokes no external executable.

**Logging** — status output via `Write-Output` from the entry script only; diagnostics via
`Write-Error`. `Write-Host` is prohibited. The pure-logic file emits no console output.

**Rollout** — no feature flag. The harness is additive and no existing gate consumes it until a
sibling child opts in. The fallback path if the harness is unavailable is the pre-existing aggregate
coverage figure, which is explicitly insufficient for epic #136 and therefore not a substitute.

## Acceptance Criteria

- [ ] The ledger accounts for all 121 compiled files with no unassigned or unclassified file.

  *Verification:* `tests/scripts/vscode/QuickFilerCoverageLedger.Tests.ps1` asserts a bijection
  between the `<Compile Include=...>` entries in `QuickFiler/QuickFiler.csproj` and the
  `files[]` entries in `coverage-ledger.json` — every csproj path has exactly one ledger row and
  every ledger row has exactly one csproj path — with `files.Count -eq 121`, every row carrying a
  non-empty `owning_child` and a `classification` in `{testable, ratified-exempt}`, and the Markdown
  ledger's data-row count equal to `files.Count`.

- [ ] Every `ratified-exempt` row carries a rationale meeting one of the three permitted grounds.

  *Verification:* the same Pester suite asserts that every row with
  `classification -eq 'ratified-exempt'` has an `exempt_ground` drawn from the closed enum
  `{generated-designer, interface-only, irreducible-host-wiring}` and a non-empty `rationale`, and
  that no row invents a fourth ground. Rows classified `testable` must carry `exempt_ground -eq
  $null`. Each `irreducible-host-wiring` rationale is reviewed in the PR against the
  irreducible-remainder standard and must name the specific host dependency that no interface seam,
  injectable delegate, or adapter can isolate.

- [ ] Every existing `[ExcludeFromCodeCoverage]` attribute in the compiled surface has a recorded
      disposition naming the owning child.

  *Verification:* the same Pester suite asserts that the ledger's non-inherited
  `attribute_dispositions` total exactly 40, comprising 14 of `kind -eq 'type'` and 26 of
  `kind -eq 'member'`, spread across exactly 21 distinct file paths; that each carries a `site` of
  the form `<path>:<line>`, a `disposition` in `{ratified, remove}`, a non-empty `owning_child`, and
  a non-empty `rationale`; and that every `remove` disposition carries the tests-first,
  attribute-removal-last sequencing instruction. `kind -eq 'inherited'` rows are excluded from the
  count of 40 and are asserted to reference an existing type-level site.

- [ ] The harness produces a deterministic per-file line-coverage report for the QuickFiler assembly
      from Cobertura input and exits non-zero when a `testable` file is below 80%.

  *Verification:* `tests/scripts/vscode/Get-PerFileCoverage.Helpers.Tests.ps1` calls
  `Format-PerFileCoverageReport` twice on identical rows and asserts byte-identical strings, and
  asserts ordinal row ordering and `InvariantCulture` percentage rendering. Threshold behavior is
  asserted at four points — above 80%, exactly 80.0% (pass), 79.99% displaying as 80.0 (fail), and
  clearly below (fail) — against the unrounded ratio. Exit codes `0`, `1`, and `2` are each asserted
  against the entry function, including the `NO DATA`, `UNLEDGERED`, and missing-`QuickFiler`-package
  paths. A fixture with both method-level and class-level `<lines>` blocks asserts that the computed
  rate is not double-counted and that the `<class>` `line-rate` attribute is not consulted.

- [ ] The harness's pure logic is unit-tested with Pester at the mirrored `tests/` path; no temporary
      files are used in tests.

  *Verification:* the test files exist at `tests/scripts/vscode/Get-PerFileCoverage.Helpers.Tests.ps1`
  and `tests/scripts/vscode/QuickFilerCoverageLedger.Tests.ps1`, dot-source the production scripts
  from `$PSScriptRoot`-relative paths, and build every Cobertura and ledger fixture as an in-memory
  here-string cast to `[xml]` or parsed with `ConvertFrom-Json`. A reviewer confirms no test creates,
  writes, or deletes a filesystem path, and that no test depends on network access, machine PATH, or
  the current working directory.

- [ ] The full PowerShell toolchain passes in final form.

  *Verification:* PoshQC format, PSScriptAnalyzer, and Pester are run in that order, restarting from
  the first stage after any failure or file change, with the final consecutive clean pass recorded as
  evidence under
  `docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/evidence/qa-gates/`. Phase 0 of the
  plan additionally confirms empirically (D17) that the PoshQC MCP commands discover the four new
  file paths.

## Definition of Done

The six acceptance criteria above are the authoritative completion gate. This checklist is the
delivery hygiene list and does not add criteria.

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (ledger Markdown and JSON committed; epic manifest reconciliation cited)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (PoshQC format → PSScriptAnalyzer → Pester)

## Seeded Test Conditions (from potential)
- [ ] Pure per-file coverage computation from a Cobertura document with mixed hit and miss lines.
- [ ] Threshold evaluation: file above 80%, file exactly at 80%, file below 80%.
- [ ] Classification filtering: a `ratified-exempt` file below 80% must not trigger a failure.
- [ ] Zero-executable-line file (interface-only) must not be reported as a 0% failure.
- [ ] Malformed or empty Cobertura input produces an explicit error, not a silent pass.
- [ ] Exit-code contract: non-zero when any `testable` file is below threshold; zero otherwise.
- [ ] Determinism: identical input yields byte-identical report output and stable row ordering.

Additional conditions derived from the research artifact:

- [ ] A file at 79.99% displays as 80.0 but fails, proving the comparison uses the unrounded ratio.
- [ ] A fixture carrying both method-level and class-level `<lines>` yields the distinct-line rate,
      not the double-counted rate.
- [ ] Two `<class>` nodes sharing one `filename` are unioned and deduplicated by line number with
      `max(hits)`, so unmerged raw `dotnet-coverage` output is handled correctly.
- [ ] A file absent from the report and classified `testable` produces a `NO DATA` failure.
- [ ] A file present in the report but absent from the ledger produces an `UNLEDGERED` failure.
- [ ] A document without a `<package name="QuickFiler">` node produces exit code `2`, not `1`.
- [ ] A `filename` containing a space (for example `QuickFiler\Helper Classes\EfcViewerQueue.cs`) is
      matched correctly.
- [ ] A report using forward-slash separators is matched correctly after normalisation.
- [ ] The ledger JSON and the Markdown ledger agree on row count and classification.
