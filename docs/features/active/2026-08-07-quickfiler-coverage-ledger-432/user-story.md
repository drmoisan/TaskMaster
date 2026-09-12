# `quickfiler-coverage-ledger` — User Story

- Issue: #432
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-08

## Story Statement

- As a **wave-1 sibling child feature (F2–F15)**, I want an authoritative statement of which of my
  assigned files are in my coverage target set, which sit on a ratified exemption, and which have no
  coverable lines at all, so that I can state my own acceptance criteria without guessing whether a
  file such as `ItemViewer.Designer.cs` belongs to me.
- As a **wave-1 sibling child feature**, I want each `[ExcludeFromCodeCoverage]` attribute on my
  assigned files to arrive with a `ratified` or `remove` disposition and a sequencing instruction,
  so that I neither leave an attribute on a testable seam nor remove one before the covering tests
  exist.
- As the **F16 capstone**, I want one consistent per-file measurement of **both line and branch**
  coverage produced by a single shared harness, so that I can close issue #136 with numeric evidence
  rather than reconciling fifteen independently built reports.
- As a **sibling that creates a production file mid-wave** (F2, F3, F7, F9, F11), I want the ledger
  to carry the classification rules and not just the rows, so that I can classify and append a row
  for a file that did not exist when the ledger was written, without re-running F1.
- As the **maintainer reviewing a pull request**, I want every retained `[ExcludeFromCodeCoverage]`
  attribute and every `ratified-exempt` classification to carry an auditable rationale drawn from a
  closed set of grounds, so that I can judge each exemption in review instead of accepting it on
  assertion.
- As the **maintainer**, I want the ledger's file list to be verified for completeness against
  `QuickFiler/QuickFiler.csproj` by an automated test, so that a compiled file with no ledger row
  fails at the next toolchain run rather than silently invalidating fifteen downstream features —
  while a sibling that adds a compiled file *and* its ledger row in the same change passes.

## Problem / Why

Epic #136 requires that every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj`
reach at least 80% line coverage and 75% branch coverage, or sit on an explicitly ratified exemption
ledger, or be recorded as having no coverable lines at all. Fifteen sibling child features are
blocked on three shared prerequisites that must be settled exactly once:

1. **The denominator is undefined.** No authoritative per-file classification exists that says which
   compiled files are `testable`, which are `ratified-exempt`, and which are
   `interface-only / not-measured`. Without it, a child cannot state its own acceptance criteria,
   because it cannot tell whether a file such as `ItemViewer.Designer.cs` is inside its target set.
   The denominator is also **dynamic**: it is the set of `<Compile Include=...>` entries in
   `QuickFiler/QuickFiler.csproj` at evaluation time, and 121 is the count at authoring time rather
   than a frozen list, because F2, F3, F7, F9, and F11 create production files during execution.
2. **The existing `[ExcludeFromCodeCoverage]` attributes are unratified.** QuickFiler carries a
   population of these attributes that has never been judged against the irreducible-remainder
   standard. Per the epic manifest, an attribute sitting on a *testable* seam is a Blocking finding.
   Until each one has a recorded disposition, children would independently and inconsistently decide
   whether to remove or keep them, and would collide on shared configuration.
3. **There is no per-file coverage measurement.** `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
   emits a Cobertura report, but nothing derives per-file line- or branch-coverage percentages from
   it. Fifteen children each building their own reporting would produce fifteen inconsistent numbers
   and a capstone (F16) that cannot close. The epic manifest makes the per-file line figure
   (>= 80%) and the per-file branch figure (>= 75%) two independent gates and requires the shared
   harness to emit both; F8 measured `EfcHomeController.Timing.cs` at 100% line and 66.67% branch,
   passing one gate and failing the other.

Aggregate assembly coverage does not satisfy issue #136, which measures success per production file.

The verified research artifact `research/2026-08-07T22-15-quickfiler-coverage-ledger-research.md`
sharpens the problem in two ways that these stories depend on. First, the epic manifest's figure of
33 `[ExcludeFromCodeCoverage]` files is a count of files containing the *string* across the whole
`QuickFiler/` tree; the compiled surface actually carries **40 attribute usages across 21 files, 14
type-level and 26 member-level, with 24 files fully coverage-suppressed once partial-class
inheritance is applied**. A child working from the manifest alone would inherit a wrong inventory.
Second, the existing coverage tooling's `<class>` `line-rate` attribute and its `.//lines/line`
descendant axis are both double-counted, so a child that computed its own per-file number the
obvious way would report a figure that neither its siblings nor the capstone could reproduce.

## Personas & Scenarios

- **Persona: a wave-1 sibling child feature (F2–F15), acting through its planner and executor.**
  - *Who:* one of fourteen parallel child features, each owning a disjoint set of compiled
    QuickFiler files ranging from 2 to 15 files.
  - *What they care about:* knowing precisely which of their files must reach 80% line coverage and
    75% branch coverage, and being able to prove they did.
  - *Constraints:* they cannot modify files owned by a sibling, cannot change repository-wide
    thresholds, and cannot re-legislate the CLAUDE.md / rules coverage-exemption reconciliation.
  - *Goals and frustrations:* a child that mis-reads the denominator either over-invests in
    generated designer code or under-delivers by omitting a file the capstone will later demand.
  - *Context:* fourteen children run in two parallel batches, so any per-child divergence in method
    surfaces only at fan-in, when it is most expensive to fix.

- **Scenario: F10 plans its coverage work for the `QfcItemController` partial family.**
  1. *Trigger:* F1 has merged; the epic orchestrator releases wave 1 and F10 begins research.
  2. F10 reads `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` and finds its
     eleven assigned files, each classified `testable`, `ratified-exempt`, or
     `interface-only / not-measured` with an owning child of `F10`.
  3. F10 finds 18 member-level `[ExcludeFromCodeCoverage]` dispositions across six of its files —
     not the six file-level exemptions the manifest's `[X]` markers implied. Each disposition is
     `ratified` with a rationale or `remove` with the instruction to write the covering tests first
     and remove the attribute last.
  4. *Decision point:* F10 sets its acceptance criteria from the `testable` rows only, and schedules
     each `remove` as a tests-then-removal pair rather than a standalone edit.
  5. *Obstacle avoided:* had F10 removed an attribute before writing tests, the removed member's
     lines would have entered the denominator immediately and the numerator only later, registering
     as a coverage regression for both the `QuickFiler` package and the repository.
  6. *Outcome:* F10 runs `scripts/vscode/Get-PerFileCoverage.ps1` against its own Cobertura output
     and commits the per-file report — carrying both a line and a branch percentage for each file —
     under `<FEATURE>/evidence/qa-gates/` as numeric evidence for each of its eleven files.

- **Persona: the F16 capstone.**
  - *Who:* the wave-2 verification gate that closes epic #136.
  - *What they care about:* a single reproducible number per file, comparable across all fourteen
    children and against the ledger.
  - *Constraints:* adds no production files; must close each acceptance criterion of issue #136 with
    numeric evidence and must confirm repository-wide coverage is retained or improved.
  - *Frustration avoided:* reconciling fourteen differently-computed percentages, some derived from
    the double-counted `line-rate` attribute, would make the epic uncloseable on evidence.

- **Scenario: F16 verifies the epic.**
  1. *Trigger:* all fourteen wave-1 children have merged to the integration branch.
  2. F16 runs the C# toolchain, producing one Cobertura report for the integrated branch.
  3. F16 runs the shared harness once against that report and the ledger JSON.
  4. *Obstacle:* a file that a child removed an attribute from but did not cover appears as a
     `NO DATA` or below-threshold row rather than being silently skipped, and the harness exits `1`.
  5. *Outcome:* either a clean exit `0` with a deterministic per-file report that F16 attaches to
     issue #136, or a specific, named list of the files that still block closure.

- **Persona: the maintainer reviewing the pull request.**
  - *Who:* the repository owner, reviewing F1's diff and later each child's diff.
  - *What they care about:* that no exemption is granted by assertion, and that the exemption
    population cannot grow quietly.
  - *Constraints:* review happens in a pull request, so the evidence must be readable in a diff
    rather than requiring a local run.
  - *Goals:* be able to challenge any single `ratified-exempt` row on its stated ground.

- **Scenario: the maintainer audits an exemption.**
  1. *Trigger:* a child's pull request retains an `[ExcludeFromCodeCoverage]` attribute.
  2. The maintainer opens `coverage-ledger.md`, locates the row by its repo-relative path, and reads
     the `exempt_ground` and rationale.
  3. *Decision point:* the ground must be one of `generated-designer`, `interface-only`, or
     `irreducible-host-wiring`. For `irreducible-host-wiring`, the rationale must name the specific
     host dependency that no interface seam, injectable delegate, or adapter can isolate. A file
     citing `interface-only` is not `ratified-exempt` at all — it belongs to the
     `interface-only / not-measured` bucket, carries no `[ExcludeFromCodeCoverage]` attribute, and
     must not be accompanied by shape-assertion tests written to manufacture coverage for it.
  4. *Outcome:* the maintainer either accepts the exemption on its stated ground or rejects it as a
     Blocking finding, in both cases from the diff alone.

## Acceptance Criteria

These are the same six criteria recorded in `spec.md`, which carries a verification note beneath
each one. This document introduces no additional or conflicting criteria.

- [ ] The ledger accounts for all 121 compiled files with no unassigned or unclassified file.
- [ ] Every `ratified-exempt` row carries a rationale meeting one of the three permitted grounds.
- [ ] Every existing `[ExcludeFromCodeCoverage]` attribute in the compiled surface has a recorded
      disposition naming the owning child.
- [ ] The harness produces a deterministic per-file line-coverage report for the QuickFiler assembly
      from Cobertura input and exits non-zero when a `testable` file is below 80%.
- [ ] The harness's pure logic is unit-tested with Pester at the mirrored `tests/` path; no temporary
      files are used in tests.
- [ ] The full PowerShell toolchain passes in final form.

## Non-Goals

- **No coverage work on any QuickFiler file.** This feature classifies and measures; it writes no
  C# test and raises no file's coverage. Raising coverage is the work of F2–F15.
- **No production C# change.** No file under `QuickFiler/` is modified.
- **No policy re-legislation.** `.claude/rules/**`, `CLAUDE.md`, and `.github/instructions/**` are
  not modified. The reconciliation between the CLAUDE.md § UT2 COM/VSTO/WinForms exemption and the
  `.claude/rules/general-unit-test.md` Coverage Exclusion Policy is already recorded in the epic
  manifest; this feature implements it.
- **No change to repository-wide coverage thresholds**, and no adjudication of which figure applies
  at which scope. The epic manifest's `## Coverage-Target Reconciliation` settles that once for the
  epic: per production file, line >= 80% and branch >= 75%; files newly created by the epic, line
  >= 90%; changed lines, no regression; repository-wide, retain or improve against the measured
  baseline of 70.19% recorded in issue #424's evidence at the merge base. The absolute
  repository-wide floors in `CLAUDE.md` § UT2 (80%) and `.claude/rules/general-unit-test.md` (85%)
  remain the standing repository aspiration and are untouched. This feature implements that
  reconciliation; it does not re-legislate it.
- **No fix to the double-counted repository-wide `lines-valid` figure.** That defect in
  `Get-CoberturaCoverageSummary` is real and verified, but correcting it would perturb every existing
  gate and every committed evidence baseline. It is out of scope here and tracked at issue #441. The
  new harness simply does not reproduce the defect in its own per-file computation. Issue #441 was
  created directly with `gh issue create` rather than through the MCP promotion lifecycle the
  manifest's `## Latent Defect Promotion` directs; the persistent issue exists, so no duplicate was
  raised. The deviation is recorded in `spec.md` and in the ledger's notes.
- **No edit to `QuickFiler/QuickFiler.csproj` or `UtilitiesCS/Properties/AssemblyInfo.cs`.** The
  manifest records both as cross-child constraints. This feature adds no C# and no compiled file, so
  it touches neither; its obligation is limited to recording the rules siblings must follow when they
  do.
- **No coverage of files that are not compiled.** `QuickFiler/Legacy/**`, `QuickFiler/Notes/**`, and
  the orphan viewer files that are absent from the csproj `<Compile>` list are outside the
  denominator. The seven attribute-carrying non-compiled files are recorded in the ledger's
  reconciliation note only, not as ledger rows.
- **No behavior change to `Invoke-MSTestWithCoverage.ps1`** or to any existing coverage
  configuration (`coverage.config`, `TaskMaster.runsettings`). The new harness is a read-only
  consumer of their output.
