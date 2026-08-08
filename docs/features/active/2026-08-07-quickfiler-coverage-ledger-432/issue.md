# quickfiler-coverage-ledger (Issue #432)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-coverage-ledger/ (Issue #432)
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- Epic: `quickfiler-per-file-coverage` (child F1, wave 0)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`

- Issue: #432
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/432
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Epic #136 requires that every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj`
reach at least 80% line coverage or sit on an explicitly ratified exemption ledger. Fifteen sibling
child features are blocked on three shared prerequisites that must be settled exactly once:

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

## Proposed Behavior

Deliver the wave-0 enabler for epic #136. No QuickFiler production behavior changes; no file under
`QuickFiler/` is modified by this child.

1. **Per-file classification ledger** at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`, with one row for every
   file listed as `<Compile Include=...>` in `QuickFiler/QuickFiler.csproj`. The compiled list is
   derived from the csproj itself, not from a directory walk: `QuickFiler/Legacy/**` and
   `QuickFiler/Notes/**` exist on disk but are not compiled and are out of scope. Each row records
   file path, line count, owning child feature (per the epic manifest's "Feature File Assignments"),
   classification (`testable` or `ratified-exempt`), and, for every exempt row, a rationale tested
   against the irreducible-remainder standard.

2. **Disposition of every existing `[ExcludeFromCodeCoverage]` attribute** in the compiled surface.
   Each is treated as unratified until this ledger judges it, and receives either `ratified` with a
   rationale, or `remove` naming the owning child that must remove it and cover the file.

3. **Repeatable per-file coverage report harness**: a PowerShell script that consumes the Cobertura
   output already produced by `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and emits per-file
   line-coverage percentages for the QuickFiler assembly, flagging any `testable` file below 80% and
   exiting non-zero when one is found. Pure parsing and threshold logic is separated from file and
   process I/O so the logic is unit-testable without I/O.

Only three grounds justify a `ratified-exempt` classification:

- generated WinForms `*.Designer.cs` files and generated `Properties/` files;
- interface-only declarations with no executable behavior (the type-only/interface-only clause in
  `.claude/rules/general-unit-test.md`);
- irreducible WinForms/COM wiring where no interface seam, injectable delegate, or adapter can
  isolate the logic, with the row stating specifically why.

## Acceptance Criteria (early draft)

- [ ] The ledger accounts for every compiled file in `QuickFiler.csproj` with no unassigned or
      unclassified file.
- [ ] Every `ratified-exempt` row carries a rationale meeting one of the three permitted grounds.
- [ ] Every existing `[ExcludeFromCodeCoverage]` attribute in the compiled surface has a recorded
      disposition naming the owning child.
- [ ] The harness produces a deterministic per-file line-coverage report for the QuickFiler assembly
      from Cobertura input and exits non-zero when a `testable` file is below 80%.
- [ ] The harness's pure logic is unit-tested with Pester at the mirrored `tests/` path, with no
      temporary files.
- [ ] The full PowerShell toolchain (PoshQC format, PSScriptAnalyzer, Pester) passes in final form.

## Constraints & Risks

- **No production C# change.** No file under `QuickFiler/` may be modified by this child.
- **No policy re-legislation.** `.claude/rules/**`, `CLAUDE.md`, and `.github/instructions/**` must
  not be modified. The policy reconciliation between the `CLAUDE.md` § UT2 COM/VSTO/WinForms
  exemption and the `.claude/rules/general-unit-test.md` Coverage Exclusion Policy is already
  recorded in the epic manifest; this child implements it.
- **No change to repository-wide coverage thresholds.**
- **Manifest figures require ground-truthing.** The epic manifest's counts and its `[X]` markers are
  authoring-time estimates. Preliminary inspection indicates the manifest's "33
  `[ExcludeFromCodeCoverage]`" figure counts files containing the *string* — including comment-only
  mentions and files that are not compiled — rather than attribute usages in the compiled surface.
  The ledger must be built from verified counts and must record the reconciliation against the
  manifest.
- **Cobertura schema must be confirmed, not assumed.** Element and attribute names must be read from
  the real output of the existing script rather than inferred.
- **PowerShell file-size limit.** `Invoke-MSTestWithCoverage.Helpers.ps1` is already 357 lines
  against a 500-line cap, so the new logic belongs in its own file rather than appended there.
- **Blocking risk.** Fifteen sibling children and the capstone are blocked until this child merges,
  so an incomplete or non-deterministic ledger propagates to every downstream feature.

## Test Conditions to Consider

- [ ] Pure per-file coverage computation from a Cobertura document with mixed hit and miss lines.
- [ ] Threshold evaluation: file above 80%, file exactly at 80%, file below 80%.
- [ ] Classification filtering: a `ratified-exempt` file below 80% must not trigger a failure.
- [ ] Zero-executable-line file (interface-only) must not be reported as a 0% failure.
- [ ] Malformed or empty Cobertura input produces an explicit error, not a silent pass.
- [ ] Exit-code contract: non-zero when any `testable` file is below threshold; zero otherwise.
- [ ] Determinism: identical input yields byte-identical report output and stable row ordering.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-coverage-ledger/` folder from the template
