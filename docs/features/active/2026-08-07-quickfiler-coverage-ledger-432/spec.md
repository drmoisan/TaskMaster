# quickfiler-coverage-ledger — Spec

- **Issue:** #432
- **Parent (optional):** epic #136 (`quickfiler-per-file-coverage`), child F1, wave 0
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08
- **Status:** Draft
- **Version:** 0.3 (amended for the epic manifest commit `c8d9e4c8`; see "Binding epic-manifest
  directives incorporated by amendment")

## Overview

Epic #136 requires that every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj`
reach at least 80% line coverage and 75% branch coverage, or sit on an explicitly ratified exemption
ledger, or be recorded as having no coverable lines at all. Fifteen sibling child features and the
F16 capstone are blocked on three shared prerequisites that must be settled exactly once:

1. **The denominator is undefined.** No authoritative per-file classification exists that says which
   compiled files are `testable`, which are `ratified-exempt`, and which are
   `interface-only / not-measured`. Without it, a child cannot state its own acceptance criteria,
   because it cannot tell whether a file such as `ItemViewer.Designer.cs` is inside its target set.
2. **The existing `[ExcludeFromCodeCoverage]` attributes are unratified.** QuickFiler carries a
   population of these attributes that has never been judged against the irreducible-remainder
   standard. Per the epic manifest, an attribute sitting on a *testable* seam is a Blocking finding.
   Until each one has a recorded disposition, children would independently and inconsistently decide
   whether to remove or keep them, and would collide on shared configuration.
3. **There is no per-file coverage measurement.** `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
   emits a Cobertura report, but nothing derives per-file line- or branch-coverage percentages from
   it. Fifteen children each building their own reporting would produce fifteen inconsistent numbers
   and a capstone (F16) that cannot close.

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

### Binding epic-manifest directives incorporated by amendment

After this specification was first written, the epic manifest was updated (commit `c8d9e4c8`) with
requirements that are binding on F1's deliverables. Three sibling children — F4, F7, and F8 —
independently converged on gaps in F1's original brief. The manifest sections
`## Coverage-Target Reconciliation (authoritative for this epic)`,
`## Directives for F1's Ledger and Harness`, `## Latent Defect Promotion`,
`## Mid-Wave File Creation and the Ledger Denominator`, and
`## Cross-Child Constraints Discovered During Preparation` are authoritative and are implemented
below. In summary:

1. **A third classification bucket, `interface-only / not-measured`.** Files with zero coverable
   lines are not `ratified-exempt`; they are a distinct category with no denominator. Implemented in
   "Behavior §1" and "Data & State".
2. **Two harness correctness requirements** — aggregate per file rather than per class, and decide
   the denominator on `<line>` child count rather than `line-rate`. Both were already required by
   this specification via research decision D4 and the row-state matrix; the manifest is now the
   epic-level source for them.
3. **The coverage denominator is dynamic**, derived from `<Compile Include=...>` at evaluation time.
   121 is the authoring-time snapshot, not a frozen list.
4. **Per-file branch coverage is an independent gate at >= 75%** and the harness must report it
   alongside the line figure.
5. **Latent defects are promoted to GitHub issues**, not left as feature-folder prose.
6. **Cross-child constraints** on `QuickFiler/QuickFiler.csproj` and on `InternalsVisibleTo` apply to
   every child; F1's position against both is stated in "Constraints & Risks".

### Coverage targets reconciled at epic level

The manifest's `## Coverage-Target Reconciliation` table is authoritative for this epic and is
adopted here without alteration:

| Scope | Target |
| --- | --- |
| Per production file, line | >= 80% |
| Per production file, branch | >= 75% |
| Files newly created by this epic, line | >= 90% |
| Changed lines | No regression |
| Repository-wide | Retain or improve against the measured baseline |

The per-file line figure and the per-file branch figure are **independent gates**. F8 measured
`EfcHomeController.Timing.cs` at 100% line and 66.67% branch — passing one gate and failing the
other. Both figures must therefore be reported per file, and the harness must emit both.

The repository-wide row records a reconciliation, not a waiver. Issue #424's evidence at the merge
base recorded a **repository baseline line rate of 70.19%**, which is below the absolute
repository-wide floors in `CLAUDE.md` (80%) and `.claude/rules/general-unit-test.md` (85%). Those
floors remain the standing repository aspiration and are untouched by this feature; the per-child
gate for this epic is retain-or-improve against the measured baseline, because gating every child on
a pre-existing shortfall would make the epic unexecutable. This resolves which number gates which
scope. It changes no threshold, and this feature still adjudicates nothing: the reconciliation is
recorded in the manifest and F1 implements it.

Note that the 70.19% baseline figure and the 85.65% repository-wide `line-rate` quoted in research
§7.4 are different measurements: the latter is the figure emitted by `ConvertTo-KoverageCoberturaXml`
over a **double-counted** denominator (research §4.2, issue #441) and is indicative only. Neither is
changed by this feature.

## Behavior

Deliver the wave-0 enabler for epic #136. No QuickFiler production behavior changes; no file under
`QuickFiler/` is modified by this child. Three deliverables.

### 1. Per-file classification ledger

A ledger at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`, with one row for
every file listed as `<Compile Include=...>` in `QuickFiler/QuickFiler.csproj`. The compiled list is
derived from the csproj itself, not from a directory walk: `QuickFiler/Legacy/**`,
`QuickFiler/Notes/**`, and the orphan files enumerated in research §2.1 exist on disk but are not
compiled and are out of scope.

**The denominator is dynamic.** Per the manifest's `## Mid-Wave File Creation and the Ledger
Denominator`, the coverage denominator is the set of `<Compile Include=...>` entries in
`QuickFiler/QuickFiler.csproj` **at evaluation time**, never a frozen list. **121 is the
authoring-time snapshot** — the count at repo HEAD `74be1964` — and not a target. Several siblings
create production files during execution, after this ledger exists: F2 splits `QfcQueue.cs` (610
lines), F3 adds two K1 seam files, F7 adds `QfcHomeController.Properties.cs` (a mandatory split:
the file is at 487 lines against a 500-line limit and the minimum seam set projects it to ~502), and
F9 and F11 split `EfcFormController.cs`, `EfcItemController.cs`, and `QfcCollectionController.cs`.
The property this feature verifies is therefore **completeness**, not the literal count: a compiled
file that lacks a ledger row is a failure; a changed count with every compiled file carrying a row
is not.

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
- **Classification** — exactly one of `testable`, `ratified-exempt`, or `interface-only`, rendered in
  the Markdown ledger as `interface-only / not-measured`.
- **Rationale** — required for every row that is not `testable`, citing one of exactly three
  permitted grounds and no other. The ground determines the classification:

| Ground | Meaning | Classification |
| --- | --- | --- |
| `generated-designer` | Generated WinForms `*.Designer.cs` files and generated `Properties/` files | `ratified-exempt` |
| `irreducible-host-wiring` | Irreducible WinForms/COM wiring where no interface seam, injectable delegate, or adapter can isolate the logic, with the row stating specifically why | `ratified-exempt` |
| `interface-only` | A declaration-only file with **no executable behavior** — no coverable lines, no executable IL (the type-only/interface-only clause in `.claude/rules/general-unit-test.md`) | `interface-only` |

**The `interface-only` ground no longer yields `ratified-exempt`.** Per the manifest's
`### A third ledger bucket: interface-only / not-measured`, a file with zero coverable lines must not
be classified `ratified-exempt`, because that classification asserts untestable production logic that
was argued away against the irreducible-remainder standard. A declaration-only file has no such
logic and no denominator at all. The two remaining `ratified-exempt` grounds are the ones that cover
genuinely irreducible production code. Acceptance criterion 2 is unaffected: the permitted-ground set
is still exactly three, and every `ratified-exempt` row still cites one of them.

Binding sub-rules for the third bucket:

- A third-bucket file receives **no `[ExcludeFromCodeCoverage]` attribute**. Verified against
  research §3.2 and §3.3: none of the 21 attribute-carrying compiled files is a third-bucket
  candidate, so this rule creates no new `remove` disposition.
- It is reported **`N/A`, never `0%`**, for both the line and the branch figure, and never counts as
  a failure.
- **Shape-assertion tests written purely to manufacture coverage for such a file are prohibited.**
  A test that asserts only that a type exists, that an interface declares a member, or that an enum
  has a given member count, written for no purpose other than to move a coverage number, is a
  Blocking finding. There is no number to move: the file has no denominator.

Verified members of this bucket named by the manifest, found by F4 and F7:
`Helper Classes\IConversationResolver.cs`, `Interfaces\IEmailMoveMonitor.cs`,
`Helper Classes\QfEnums.cs`, `Helper Classes\cInfoMail.cs` (231 lines of entirely commented-out dead
code whose only live content is eight `using` directives), and F7's two interface files. F7 evidenced
both of its interface files three ways, using `MailItemActionsAdapter` as a positive control to prove
the folder was instrumented. Research §4.7 independently derives the full candidate population from
the sample
report: 23 interface-only declarations, `QfEnums.cs`, `cInfoMail.cs`, and
`Properties\AssemblyInfo.cs` (assembly attributes only) are all absent from the report because no
instrumentable code was emitted. `Properties\Resources.Designer.cs` is **not** in this bucket: its
absence is caused by `[DebuggerNonUserCodeAttribute]` on generated code (research §4.7), so its
ground is `generated-designer` and its classification remains `ratified-exempt`. The exact
third-bucket membership is settled at ledger-authoring time by applying the rule above to each
compiled file.

The distinction the harness must draw, stated directly by the manifest: **a declaration-only file
reports `line-rate="0"` because it has no lines, not because it is uncovered.** Keying on `line-rate`
mis-reports every third-bucket file as a 0% failure — exactly the false alarm the third bucket exists
to prevent. The harness therefore decides the denominator on `<line>` child count, never on
`line-rate`; see "Harness correctness requirements" below.

#### Classification rules travel with the ledger

Per the manifest's `## Mid-Wave File Creation and the Ledger Denominator` rule 2, **the ledger
carries rules, not just rows.** The ledger Markdown must state the classification rules — the three
permitted grounds, their mapping to the three classifications, and the third-bucket sub-rules above —
in a dedicated section, so that a file which did not exist at authoring time can be classified by its
creating child without re-running F1. The remaining rules of that manifest section are directed at
siblings and are recorded verbatim in the same ledger section:

- **The creating child appends its own row**, in the same change that adds the `<Compile Include>`
  entry to `QuickFiler/QuickFiler.csproj`. Like the csproj, the ledger is an additive shared file;
  fan-in conflicts on it are expected and are resolved by keeping both sides.
- **A newly created file defaults to `testable` with a >= 90% line target**, because a file extracted
  from existing code is new production code and takes the `CLAUDE.md` § UT2 new-module target.
  Claiming `ratified-exempt` for a newly created file requires a rationale meeting one of the two
  `ratified-exempt` grounds.
- **F16 re-derives and reconciles.** The capstone recomputes the denominator from the csproj and
  fails if any compiled file lacks a ledger row.

The ledger carries an explicit **"Reconciliation with epic manifest"** section (D1) recording the
verified attribute figures above, explaining that the manifest's 33 decomposes without residual as
21 + 5 + 7 files containing the string, stating that the manifest's `[X]` markers are advisory, and
citing the research document.

The ledger also records that the double-counted repository-wide `lines-valid` defect in
`Get-CoberturaCoverageSummary` is a known pre-existing condition that is **out of scope for this
feature and is tracked separately at issue #441** (D5). This feature must not change that behavior;
changing the repository-wide figure would perturb every existing gate and every committed evidence
baseline. The statement exists so a reviewer does not read the omission as an oversight.

**Latent defect promotion (manifest `## Latent Defect Promotion`).** The manifest requires latent
defects surfaced during preparation to be promoted to GitHub issues through the MCP promotion
lifecycle rather than left as feature-folder prose, because prose is lost when the folder moves to
`completed/`. Siblings F3, F7, and F8 produced issues #442–#447 and #451 this way. F1's research
surfaced one such defect — the existing coverage scripts double-count `<line>` nodes via the
`.//lines/line` descendant axis, inflating `lines-valid` and every derived rate (research §4.2). It
is tracked at **issue #441** and is out of scope for #432.

*Deviation recorded explicitly:* issue #441 was created directly with `gh issue create` rather than
through the MCP promotion lifecycle. The directive's purpose is met — a persistent GitHub issue
exists outside the feature folder and survives archival — so a second issue was **not** created,
which would have produced a duplicate. The deviation is in mechanism only and is recorded here and
in the ledger's notes so a reviewer does not read it as a missing promotion.

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
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, emits per-file **line and branch** coverage
percentages for the `QuickFiler` assembly, and exits non-zero when any `testable` file is below its
line threshold (80%) or below the branch threshold (75%).

#### Per-file branch coverage

**The harness must compute and report per-file branch coverage alongside line coverage.** This
follows from F1's own charter: the manifest's `### Why F1 is a real dependency, not stylistic
ordering` argues that fifteen children independently building per-file coverage reporting would
produce fifteen inconsistent numbers. If the shared harness reported only the line figure, every
child would compute branch coverage ad hoc and that fragmentation would return through the branch
gate instead. The manifest states the same requirement directly: "Children must report both, and
F1's harness must emit both."

The data is available in the real output. Research §4.3 confirms that `<line>` elements carry a
`branch` attribute with the literal values `"True"` / `"False"` (capitalised, matched literally at
`Invoke-MSTestWithCoverage.Helpers.ps1:128` and `:236`) and an optional `condition-coverage`
attribute in the form `"50% (1/2)"`.

Exact derivation, which mirrors the line figure's treatment:

- Take the file's `<line>` elements after the same union-by-`filename` and dedupe-by-line-number
  treatment the line figure receives. On a line-number collision the richer `condition-coverage` is
  kept — the larger denominator, and on equal denominators the larger numerator — mirroring
  `Merge-CoberturaClassesByFilename` (`Invoke-MSTestWithCoverage.Helpers.ps1:240`–`:261`).
- Restrict to elements with `branch="True"`.
- Parse each `condition-coverage` value's fraction `(covered/total)` with the regex
  `\(([0-9]+)/([0-9]+)\)`, matching `Get-CoberturaLineConditionCoverageParts`
  (`Invoke-MSTestWithCoverage.Helpers.ps1:146`–`:165`).
- Branch rate = `sum(covered) / sum(total)` across those deduplicated branch lines. **It is not the
  average of the per-line percentages**, which would weight a two-condition line equally with an
  eight-condition line and would not reproduce the `branch-rate` semantics of the document.
- A `branch="True"` element with no `condition-coverage` attribute contributes nothing to either
  sum; the harness must not infer a fraction.

Gate semantics:

- The line gate and the branch gate are **two independent gates**. A `testable` file below either
  threshold is a failure, and the report names which gate failed.
- The branch threshold is 75.0% and, like the line threshold, is compared against the **unrounded**
  ratio (`-lt 0.75`) and displayed rounded to one decimal place under `InvariantCulture`. Exactly
  75.0% passes.
- A file with lines but **no branch points** reports `n/a` for branch, never `0%`, and never
  contributes a branch failure. A branch rate is only defined where branch points exist.
- A third-bucket (`interface-only`) file reports `N/A` for both figures and never fails either gate.
- A `ratified-exempt` file reports both figures for information only and fails neither gate.

#### Harness correctness requirements

The manifest's `### Two harness correctness requirements` states two epic-level requirements. Both
are silent-wrong-answer bugs rather than crashes, and both are already required by this
specification — the first by research decision D4 and the union-by-`filename` rule below, the second
by the `NO EXECUTABLE LINES` row of the row-state matrix in "Data & State". They are recorded here
once, with the manifest cited as the epic-level source, rather than restated a second time:

1. **Aggregate per file, not per class** — one source file can produce multiple Cobertura `<class>`
   elements sharing one `filename` (a type plus its compiler-generated `<>c` closure class); union
   them taking **max hits per line**. See the `<line>`-node rule immediately below and research §4.5.
2. **Decide the denominator on `<line>` child count, never `line-rate`** — see the `<line>`-node rule
   below, the `NO EXECUTABLE LINES` row of the row-state matrix, and research §4.3.

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
  failure; absent + `interface-only` is the expected state for a third-bucket file and is not a
  failure; absent + `testable` **is** a failure with a distinct `NO DATA` message; present with zero
  `<line>` nodes is `n/a` and is never reported as 0%; present in the report but not in the ledger
  **is** a failure.
- **Exactly 80.0% passes (D9).** Implemented as `-lt 0.80` against the unrounded ratio. The
  displayed value is rounded to one decimal place using `InvariantCulture`. Comparing the rounded
  display value is prohibited: 7999/10000 displays as 80.0 but must fail. The same rule applies to
  the branch gate at `-lt 0.75`.
- **Per-row line targets (manifest, mid-wave rule 4).** A row may carry a `line_target_percent` that
  overrides the ledger-wide `threshold_percent` for that file. Files created during the epic carry
  `90.0`. When the field is absent or `null` the ledger-wide threshold applies. The branch threshold
  is ledger-wide and has no per-row override, because the manifest states no per-file branch
  variation.
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
| Line threshold | `-ThresholdPercent <double>` | `80.0` (also carried as `threshold_percent` in the ledger JSON) |
| Branch threshold | `-BranchThresholdPercent <double>` | `75.0` (also carried as `branch_threshold_percent` in the ledger JSON) |
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
  `threshold_percent`, `branch_threshold_percent`, `new_file_line_target_percent`, and `files[]`.
  The thresholds live in data rather than in code.

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
  `max(hits)` on collision and the richer `condition-coverage` retained for branch lines.
- `Get-CoberturaConditionCoverageParts` — pure: a `condition-coverage` attribute value → the
  `(covered, total)` pair. This deliberately duplicates the three-line regex in
  `Get-CoberturaLineConditionCoverageParts` (`Invoke-MSTestWithCoverage.Helpers.ps1:146`–`:165`)
  rather than dot-sourcing it, because the pure-logic file must stay dot-sourceable with no
  cross-file load-order dependency, and because that file is a read-only dependency this feature
  must not modify.
- `Get-CoberturaPerFileCoverage` — pure: `[xml]` document + package name → per-file row objects
  carrying both the line figures (covered/total) and the branch figures (covered/total).
- `Test-PerFileCoverageThreshold` — pure: rows + classification + line threshold + branch threshold →
  verdict object naming, per failing row, which gate failed.
- `Format-PerFileCoverageReport` — pure: rows → deterministic report text with both a line-coverage
  and a branch-coverage column.
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
| `0` | Every `testable` file is at or above both its line threshold and the branch threshold. |
| `1` | At least one `testable` file is below its line threshold **or** below the branch threshold, or a `NO DATA` row, or an `UNLEDGERED` row. |
| `2` | Input error: file missing, XML not well-formed, no `<packages>` node, no `QuickFiler` package, ledger JSON missing or malformed, unrecognised `schema_version`. |

Distinguishing `2` from `1` matters because a child must not mistake a broken input for a coverage
regression. No existing repository script uses more than `0`/`1`, so all three codes are asserted in
Pester. The pure layer raises malformed input with a bare `throw` and a specific literal message,
matching `Invoke-MSTestWithCoverage.Helpers.ps1:113`; the entry script catches and maps to exit `2`.

**Validation rules**

- `classification` ∈ `{testable, ratified-exempt, interface-only}`.
- `exempt_ground` is a closed enum: `generated-designer`, `interface-only`,
  `irreducible-host-wiring`, and it determines the classification:
  - `generated-designer` or `irreducible-host-wiring` ⇔ `classification: ratified-exempt`;
  - `interface-only` ⇔ `classification: interface-only`;
  - `exempt_ground: null` ⇔ `classification: testable`.
  Any other pairing is invalid. Every non-`testable` row must carry a non-empty `rationale`.
- No row with `classification: interface-only` may carry a non-empty `attribute_dispositions` array,
  because a declaration-only file receives no `[ExcludeFromCodeCoverage]` attribute.
- `line_target_percent` is `null` or a number in `[0, 100]`. When non-null it overrides
  `threshold_percent` for that row and is only permitted on a `testable` row.
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
  "branch_threshold_percent": 75.0,
  "new_file_line_target_percent": 90.0,
  "files": [
    {
      "path": "QuickFiler\\Controllers\\QfcQueue.cs",
      "lines": 610,
      "owning_child": "F2",
      "classification": "testable",
      "line_target_percent": null,
      "exempt_ground": null,
      "rationale": null,
      "attribute_dispositions": []
    },
    {
      "path": "QuickFiler\\Helper Classes\\IConversationResolver.cs",
      "lines": 33,
      "owning_child": "F4",
      "classification": "interface-only",
      "line_target_percent": null,
      "exempt_ground": "interface-only",
      "rationale": "Interface declaration only; no executable IL, therefore no coverage denominator. Not measured, reported N/A. No [ExcludeFromCodeCoverage] attribute is applied and no shape-assertion test may be written for it.",
      "attribute_dispositions": []
    },
    {
      "path": "QuickFiler\\Viewers\\ItemViewer.Designer.cs",
      "lines": 6224,
      "owning_child": "F14",
      "classification": "ratified-exempt",
      "line_target_percent": null,
      "exempt_ground": "generated-designer",
      "rationale": "WinForms Designer-generated code; exempt under CLAUDE.md UT2 (b).",
      "attribute_dispositions": [
        { "kind": "inherited", "from": "QuickFiler\\Viewers\\ItemViewer.cs:20" }
      ]
    }
  ]
}
```

A row appended by a sibling for a file it creates mid-wave carries
`"classification": "testable"` and `"line_target_percent": 90.0`, per the manifest's mid-wave rule 4.

A non-inherited disposition carries
`{ kind, site: "<path>:<line>", target: "<declaration>", disposition, owning_child, rationale }`.

**Invariants**

1. The JSON is authoritative; the Markdown ledger is the human view. Regenerating the Markdown from
   the JSON at authoring time is acceptable, but the gate must not depend on the Markdown at run
   time.
2. Bijection: every `<Compile Include=...>` path in `QuickFiler/QuickFiler.csproj` has exactly one
   `files[]` entry, and every `files[]` entry corresponds to a `<Compile Include=...>` path. This is
   evaluated against the csproj **at evaluation time**. `files[].length == 121` holds at authoring
   time and is recorded as a snapshot, not asserted as a constant: a sibling that adds a
   `<Compile Include>` entry and its ledger row in the same change must pass. The failure condition
   is a compiled file that lacks a ledger row, or a ledger row with no compiled file — never a
   changed count on its own.
3. The Markdown table's data-row count equals the JSON `files` length.
4. Non-inherited dispositions total 40: 14 of `kind: type` and 26 of `kind: member`. This inventory
   is fixed at authoring time and describes the pre-existing attribute population; a file created
   mid-wave carries no attribute and therefore no disposition.
5. `files[]` is ordinal-sorted by `path` at authoring time. Ordering is not a post-authoring
   invariant, because siblings append rows and fan-in resolution keeps both sides; the harness sorts
   ordinally at run time, so report determinism does not depend on ledger order.

**Harness row-state table (D7)** — the complete case matrix. Both the line column and the branch
column follow it:

| Case | Classification | Row status | Line column | Branch column | Exit contribution |
| --- | --- | --- | --- | --- | --- |
| Absent from report | `interface-only` | `NOT MEASURED (no executable lines)` | `n/a` | `n/a` | none |
| Absent from report | `ratified-exempt` | `EXEMPT (not measured)` | `n/a` | `n/a` | none |
| Absent from report | `testable` | `NO DATA` (distinct message naming the file) | `n/a` | `n/a` | **failure** |
| Present, zero `<line>` nodes | any | `NO EXECUTABLE LINES` | `n/a` (never 0%) | `n/a` (never 0%) | none |
| Present with lines, zero `branch="True"` lines | `testable` | percentage, line gate only | percentage | `n/a` (never 0%) | failure iff line below |
| Present with lines | `ratified-exempt` | percentage, informational | percentage | percentage | none |
| Present with lines | `testable` | percentage, compared to both thresholds | percentage | percentage | failure iff **either** gate is below |
| Present with lines | `interface-only` | `RECLASSIFY` (distinct message) | percentage, informational | percentage, informational | none |
| Present but not in ledger | — | `UNLEDGERED` (distinct message) | percentage | percentage | **failure** |

The absent-plus-`testable` case is the one a naive implementation would silently pass, and it is
exactly what a stale build produces after a child removes a type-level attribute. Treating it as a
failure is the required default.

The `RECLASSIFY` case is the converse drift signal: a row classified `interface-only` that appears in
the report with executable lines was misclassified, or the file gained executable code after the
ledger was written. It is surfaced as a distinct, named row status so it is visible to a reviewer and
to the F16 reconciliation, but it contributes **no** exit failure, because the manifest states that a
third-bucket file "never counts as a failure". The ledger's own Pester suite and F16's re-derivation
are the mechanisms that act on it.

Third-bucket expectation: research §4.7 verified that all 23 interface-only declarations, `QfEnums.cs`,
`cInfoMail.cs`, and `AssemblyInfo.cs` are absent from the sample report, so `NOT MEASURED` is the
expected state for a third-bucket row and `RECLASSIFY` should not occur in practice.

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
  `line-rate` / `lines-valid` computation (D5, issue #441). The epic-level reconciliation of which
  number gates which scope is recorded in the manifest and restated under "Coverage targets
  reconciled at epic level"; this feature implements it and does not adjudicate it.
- **Cross-child constraints — F1's position (manifest
  `## Cross-Child Constraints Discovered During Preparation`).** The manifest records two constraints
  that apply to every child:
  1. `QuickFiler/QuickFiler.csproj` is an unavoidable shared file for any child that adds a
     production `.cs` file, because the project is legacy non-SDK and uses no globbing.
  2. `QuickFiler.Test` has no `InternalsVisibleTo` grant from `UtilitiesCS`, so a `UtilitiesCS`
     internal is unreachable from a QuickFiler test and the resolution is a local seam in the child's
     own assignment.

  **F1 adds no C# and no compiled file, so it edits neither `QuickFiler/QuickFiler.csproj` nor
  `UtilitiesCS/Properties/AssemblyInfo.cs`.** This is stated explicitly so a reviewer does not look
  for csproj handling in F1's diff. F1's obligation is documentary: the ledger's rules section
  instructs a creating child to append its ledger row in the same change that adds the
  `<Compile Include>` entry, and that guidance must be consistent with the manifest's rules for that
  file — minimal adjacent hunks, no property or reference changes, no reordering of unrelated
  entries, and **CRLF preserved** (a git-bash `sed -i` strips it and produces a whole-file diff that
  is guaranteed to conflict; use the Edit tool or `perl -0777` with explicit `\r\n`).
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
  that staleness into a test failure at the next toolchain run rather than a silent divergence. The
  assertion must be written as a completeness check, not a count check, so that the expected mid-wave
  file creation by F2, F3, F7, F9, and F11 passes when the creating child appends its ledger row in
  the same change, and fails only when a compiled file has no row.
- **PoshQC scan coverage is unverifiable from the repository (D17).** No repo-local scan
  configuration exists, and `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` — referenced
  by `.claude/rules/powershell.md` — is absent from this worktree. The chosen directories
  (`scripts/vscode/`, `tests/scripts/vscode/`) already contain formatted, analyzer-clean, Pester-run
  files, so they inherit a demonstrably working configuration. Phase 0 of the plan must still
  confirm empirically that `run_poshqc_format`, `run_poshqc_analyze`, and `run_poshqc_test` pick up
  the new files before the harness is treated as gated.

## Implementation Strategy

**Scope of change**

- Add `scripts/vscode/Get-PerFileCoverage.Helpers.ps1` (pure logic, target 250–380 lines — raised
  from 200–300 by the branch-coverage derivation and the `condition-coverage` parser; the 500-line
  cap still applies and the file must be split if the target is exceeded).
- Add `scripts/vscode/Get-PerFileCoverage.ps1` (entry point, target 100–160 lines).
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
  every ledger row has exactly one csproj path — with every row carrying a non-empty `owning_child`
  and a `classification` in `{testable, ratified-exempt, interface-only}`, and the Markdown ledger's
  data-row count equal to `files.Count`.

  *Note on the number 121:* 121 is the count of compiled files at authoring time (repo HEAD
  `74be1964`), and the ledger records it as a snapshot. Per the manifest's `## Mid-Wave File Creation
  and the Ledger Denominator`, the denominator is the set of `<Compile Include=...>` entries at
  evaluation time, so the property this test verifies is **completeness against the csproj**, not the
  literal count. The bijection assertion reads the csproj at run time and fails only when a compiled
  file lacks a ledger row or a ledger row has no compiled file. A sibling that adds a production file
  and its ledger row in the same change passes; a sibling that adds the file without the row fails.
  F16 re-derives the denominator from the csproj and performs the same reconciliation at the epic
  level.

- [ ] Every `ratified-exempt` row carries a rationale meeting one of the three permitted grounds.

  *Verification:* the same Pester suite asserts that every non-`testable` row has an `exempt_ground`
  drawn from the closed enum `{generated-designer, interface-only, irreducible-host-wiring}` and a
  non-empty `rationale`, and that no row invents a fourth ground. Rows classified `testable` must
  carry `exempt_ground -eq $null`. Each `irreducible-host-wiring` rationale is reviewed in the PR
  against the irreducible-remainder standard and must name the specific host dependency that no
  interface seam, injectable delegate, or adapter can isolate.

  *Note on the three grounds:* the permitted-ground set is still exactly three and this criterion is
  unchanged, but per the manifest's `### A third ledger bucket: interface-only / not-measured` the
  grounds no longer map one-to-one onto `ratified-exempt`. The suite additionally asserts the
  ground-to-classification mapping: `generated-designer` and `irreducible-host-wiring` imply
  `classification -eq 'ratified-exempt'`; `interface-only` implies
  `classification -eq 'interface-only'`; and no row with `classification -eq 'interface-only'` carries
  a non-empty `attribute_dispositions` array, since a declaration-only file receives no
  `[ExcludeFromCodeCoverage]` attribute. Every `ratified-exempt` row therefore still cites one of the
  three permitted grounds, drawn from the two that denote irreducible production logic.

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

  *Note on branch coverage:* per the manifest's `## Coverage-Target Reconciliation`, the per-file
  branch figure at >= 75% is an **additional independent gate** alongside the 80% line gate named in
  this criterion, and both figures appear in the report. The same suite therefore also asserts that
  every row carries a branch column; that the branch rate is derived as `sum(covered)/sum(total)`
  across the file's deduplicated `<line branch="True">` elements parsed from `condition-coverage`,
  not as an average of per-line percentages; that exactly 75.0% passes and 74.99% fails against the
  unrounded ratio; that a file with lines but no branch points reports `n/a` for branch rather than
  0%; and that a `testable` file at or above 80% line but below 75% branch exits `1` with a message
  naming the branch gate — the shape F8 measured on `EfcHomeController.Timing.cs` (100% line,
  66.67% branch).

  *Note on the third bucket:* a row classified `interface-only` reports `N/A` for both figures, never
  `0%`, and never contributes a failure, and no shape-assertion test may be written to manufacture
  coverage for such a file.

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

Additional conditions derived from the epic manifest amendment (commit `c8d9e4c8`):

- [ ] A row classified `interface-only` and absent from the report reports `N/A` for both the line
      and the branch column, is not a failure, and is never rendered as `0%`.
- [ ] A `<class>` whose `line-rate` is `"0"` but which has zero `<line>` children is reported as
      `NO EXECUTABLE LINES`, proving the denominator is decided on `<line>` child count.
- [ ] The ground-to-classification mapping is enforced: `interface-only` never yields
      `ratified-exempt`, and an `interface-only` row carries no attribute disposition.
- [ ] Branch coverage is computed as `sum(covered)/sum(total)` over deduplicated
      `<line branch="True">` `condition-coverage` fractions, not as an average of per-line
      percentages; a fixture with a 1/2 line and a 6/8 line yields 70.0%, not 62.5%.
- [ ] A `testable` file at 100% line and 66.67% branch fails the branch gate and exits `1`, with the
      diagnostic naming the branch gate.
- [ ] Exactly 75.0% branch passes; 74.99% branch fails against the unrounded ratio.
- [ ] A `testable` file with lines but no `branch="True"` elements reports `n/a` for branch and does
      not fail the branch gate.
- [ ] A `branch="True"` element with no `condition-coverage` attribute contributes to neither branch
      sum.
- [ ] A `line_target_percent` of `90.0` on a row is applied instead of the ledger-wide
      `threshold_percent`: the file fails at 85% and passes at 92%.
- [ ] The csproj-vs-ledger completeness assertion passes when a fixture csproj gains a
      `<Compile Include>` entry **and** the ledger gains the matching row, and fails when the entry
      is added without the row.
