# Evidence Correction — Coverage Artifact Substitution (Raw Cobertura to JaCoCo Projection)

Timestamp: 2026-09-01T16-41

Branch: `bug/qfc-metrics-flush-writes-empty-session-file-646`
HEAD at substitution: `e23a9afc`

## Why

Two committed evidence artifacts were raw `dotnet-coverage` Cobertura reports totalling
52,131,269 bytes and 892,256 lines. Raw Cobertura must not be committed as evidence in this
repository. The precedent is commit `d0955dc4` ("docs(#503): replace raw cobertura coverage
evidence with jacoco summaries"), which removed approximately 20 MB of exactly this artifact
class and replaced it with compact package-level JaCoCo files.

Each raw report was replaced by a package-level JaCoCo projection that preserves every figure
the plan's gates relied on.

## Command

Command: `pwsh -NoProfile -File Convert-CoberturaToJacoco.ps1 -InputPath docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/baseline/baseline-coverage.cobertura.xml -OutputPath docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/baseline/baseline-coverage.jacoco.xml`
EXIT_CODE: 0

Command: `pwsh -NoProfile -File Convert-CoberturaToJacoco.ps1 -InputPath docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/qa-gates/final-coverage.cobertura.xml -OutputPath docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/evidence/qa-gates/final-coverage.jacoco.xml`
EXIT_CODE: 0

`Convert-CoberturaToJacoco.ps1` was written to the session scratchpad outside the repository
and is not committed, per the repository rule that no helper script is written under
`evidence/`. Its path is account-derived and is therefore not reproduced here. The conversion
is fully specified by the method below and is reproducible from it.

Output Summary: Both conversions succeeded with EXIT_CODE 0 and both reconciled exactly to
their source Cobertura root counters — baseline `lines-covered=48426 lines-valid=142226`,
final `lines-covered=48436 lines-valid=142240`. Each projection carries all 15 packages. The
two raw `.cobertura.xml` files were deleted only after both reconciliations passed. Total
committed evidence for these two artifacts falls from 52,131,269 bytes to 4,718 bytes with no
loss of any figure a plan gate relied on.

## Method

The source files are approximately 26 MB each and were streamed with `System.Xml.XmlReader`;
neither was loaded into a DOM.

1. Track the current `package` name. The `package` element places `name` after `line-rate`,
   so a search for the literal text `package name=` returns zero matches in these files and
   would falsely suggest the reports contain no packages. `XmlReader.GetAttribute` reads
   attributes by name irrespective of their order, so the ordering is immaterial here.
2. Within each `class` element, collect `line` elements into a map keyed by the `number`
   attribute. Cobertura repeats `line` elements across the `method` blocks and the
   class-level `lines` block, so deduplication by line number within the class is required or
   the totals do not reconcile. Where a line number recurs, the maximum `hits` is kept.
3. On closing each `class`, fold the deduplicated entries into the package counters:
   `hits > 0` counts as covered, `hits == 0` counts as missed.
4. Branch counters are derived from the `(covered/total)` pair inside each
   `condition-coverage` attribute.

### Branch Counters Are Zero, and That Is Faithful

Every `BRANCH` counter in both projections reads `missed="0" covered="0"`. This is a true
projection of the source, not a parsing miss. Verified on the source before deletion:

- `grep -c 'condition-coverage' baseline-coverage.cobertura.xml` returned `0` (exit 1).
- `grep -o 'branch="True"' baseline-coverage.cobertura.xml` returned no output.
- The Cobertura root carries `branch-rate="1"` with no `branches-covered` and no
  `branches-valid` attribute, as already recorded in
  `evidence/baseline/coverage-cobertura-baseline.2026-08-31T20-04.md`.

The run emitted no branch data at all, so no branch figure exists to project. The counters are
retained at zero rather than omitted so the JaCoCo shape stays uniform across packages.

## Reconciliation — Mandatory Gate, Passed

The summed `LINE` counters across all packages in each projection must reproduce the source
Cobertura root element's `lines-covered` and `lines-valid` attributes exactly. Both did. No
number was adjusted to make this match, and neither source file was deleted until its
reconciliation passed.

| Report | Measure | Cobertura root | Derived from projection | Match |
|---|---|---|---|---|
| Baseline | `lines-covered` | 48426 | 48426 | Exact |
| Baseline | `lines-valid` | 142226 | 142226 | Exact |
| Final | `lines-covered` | 48436 | 48436 | Exact |
| Final | `lines-valid` | 142240 | 142240 | Exact |

As a secondary confirmation, the ratio derived from each projection reproduces the source root
`line-rate` to ten decimal places: baseline `0.3404862683` against a root `line-rate` of
`0.3404862683334974`, and final `0.3405230596` against `0.3405230596175478`.

## Files Replaced

| File | Bytes | Lines |
|---|---|---|
| `evidence/baseline/baseline-coverage.cobertura.xml` (deleted) | 26,064,187 | 446,104 |
| `evidence/qa-gates/final-coverage.cobertura.xml` (deleted) | 26,067,082 | 446,152 |
| **Source total** | **52,131,269** | **892,256** |
| `evidence/baseline/baseline-coverage.jacoco.xml` (added) | 2,359 | 62 |
| `evidence/qa-gates/final-coverage.jacoco.xml` (added) | 2,359 | 62 |
| **Replacement total** | **4,718** | **124** |

Line counts for the two Cobertura files are the true line counts. Neither file ended with a
newline — the final byte of each was `>` — so `wc -l` reported one fewer line for each
(446,103 and 446,151) than the file actually contains.

## Sequence — The Gates Ran Against the Raw Reports and Were Not Skipped

An auditor reading this feature folder will find plan tasks that name a `.cobertura.xml` file
which is no longer present. That is a deliberate substitution performed after those tasks
completed, not a missing gate. The order of events was:

1. **P0-T11** produced `baseline-coverage.cobertura.xml` and read its root attributes,
   recording `line-rate="0.3404862683334974"`, `lines-covered="48426"`,
   `lines-valid="142226"`. Its acceptance condition — that the artifact exists, parses, and
   reports a numeric `line-rate` rather than a placeholder — was evaluated against the raw
   file at that time and was satisfied. Recorded in
   `evidence/baseline/coverage-cobertura-baseline.2026-08-31T20-04.md`.
2. **P2-T6** produced `final-coverage.cobertura.xml` by the identical
   `dotnet-coverage merge -f cobertura` invocation and read its root attributes, recording
   `line-rate="0.3405230596175478"`, `lines-covered="48436"`, `lines-valid="142240"`. Its
   acceptance condition was evaluated against the raw file at that time and was satisfied.
   Recorded in `evidence/qa-gates/coverage-cobertura-final.2026-08-31T20-04.md`.
3. **P2-T7** read both raw files, compared the two `line-rate` values, and read the per-line
   `hits` entries for the four new guard lines out of the raw final report. Both of its
   acceptance conditions were evaluated against the raw files at that time. Recorded in
   `evidence/qa-gates/coverage-delta-verification.2026-08-31T20-04.md`.
4. **Only then** — after all three tasks had completed and their conditions had been satisfied
   against the raw reports — were the two raw files converted to the projections described
   above and deleted.

Every figure those three tasks recorded remains verifiable. The package-level `LINE` totals in
the projections reconcile exactly to the root counters the tasks quoted, so the headline
figures can be re-derived from the committed evidence. The one class of detail the projection
does not retain is per-line and per-class granularity, which affects only Part 2 of the P2-T7
artifact; the per-line `hits` values that Part 2 depends on are quoted verbatim in that
artifact, together with the corroborating `EfcHomeController.Metrics.cs` precedent read from
the same report.

The conversion is lossless with respect to the LINE counters and lossy only with respect to
per-class and per-line detail that no gate's acceptance condition reads from the file after
the fact.

## Incidental Benefit — Host Path Removal

The JaCoCo projection carries no `filename` attribute. Dropping it also removes the vendored
build-machine source paths that the Cobertura files carried on their `class` elements, which
originated on third-party build agents rather than in this repository.

## Denominator Scope Statement

This statement applies to every coverage figure in this feature folder and is repeated here so
it travels with the artifact record.

The `line-rate` of approximately 34.05% recorded at both baseline and final is measured on a
**single-assembly unfiltered denominator (QuickFiler.Test run; includes vendored and test
assemblies)**. It is neither a repository-wide figure nor the repository's policy coverage
figure.

Both reports contain 15 packages. Eight are vendored third-party assemblies — `Deedle`,
`FluentAssertions`, `FSharp.Core`, `log4net`, `Microsoft.IO.RecyclableMemoryStream`,
`Mono.Reflection`, `System.Interactive`, `System.Linq.Async` — and a ninth is the
`QuickFiler.Test` test assembly itself. Only six are first-party production packages.
Separately, only `QuickFiler.Test.dll` was executed, so assemblies from the rest of the
solution sit in the denominator with no test driving them. The repository's policy denominator
(`coverage.config`, and the `CLAUDE.md` UT2 testable-denominator rule) is nine first-party
packages with no `*.Test` assembly.

Two consequences:

- **The no-regression comparison remains valid.** Baseline and final were produced by the
  identical `dotnet-coverage merge -f cobertura` invocation over the identical assembly set,
  so the delta is apples-to-apples and a regression in first-party code would still move the
  figure down.
- **The absolute magnitude must not be quoted as a policy figure.** It evidences neither
  compliance with nor breach of any coverage floor. Establishing a true policy figure would
  require a full-suite coverage pass, which is out of scope for a four-line guard and on which
  none of this plan's gates depend.

### First-Party Subset of This Same Run

Derived from the two projections by excluding the eight vendored packages and
`QuickFiler.Test`, leaving `QuickFiler`, `UtilitiesCS`, `ToDoModel`, `TaskVisualization`,
`Tags`, and `SVGControl`:

| Measure | Baseline | Final | Delta |
|---|---|---|---|
| first-party `lines-covered` | 14537 | 14540 | +3 |
| first-party `lines-valid` | 62118 | 62121 | +3 |
| first-party line coverage | 23.4022% | 23.4059% | +0.0037 pp |

This is a first-party subset of a single-assembly run, not a repository-wide policy figure:
only `QuickFiler.Test` was executed, and three of the six first-party packages (`ToDoModel`,
`TaskVisualization`, `Tags`) report zero covered lines for exactly that reason. It is recorded
so the delta can be read against first-party code alone, and it shows no regression.

## Effect on the Recorded Change Footprint

`evidence/qa-gates/footprint-scope.2026-08-31T20-04.md` recorded 29 diff paths at HEAD
`ba134b57`. This correction pass changes that count: it deletes two paths, adds two
projections, adds this record, and modifies five existing artifacts plus `issue.md`. Every
path involved remains inside the third AC7-allowed prefix,
`docs/features/active/2026-08-27-qfc-metrics-flush-writes-empty-session-file-646/`, so the AC7
boundary is unchanged. No production or test source file was touched by this pass.
