# P2-T8 — Coverage Delta Verification

Timestamp: 2026-09-01T14-41

## Copied numeric fields

Read from
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t15-coverage.md`
and
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/qa-gates/p2-t7-coverage.md`.

BaselineLineCoveragePercent: 85.3761
PostLineCoveragePercent: 85.373
BaselineLinesCovered: 54966
PostLinesCovered: 54964
BaselineLinesValid: 64381
PostLinesValid: 64381

## Derived fields

LinesValidDifference: 0
LinesValidDifferencePercent: 0
LinesCoveredDifference: -2
LinesCoveredDifferencePercent: 0.00363861296073937
DenominatorComparability: within-tolerance
LargerDenominatorRun: neither — the two runs carried the identical `lines-valid` figure of 64381, so
the difference is 0 and no run carried the larger denominator.

## Descriptive fields

ChangedCodeCoverage: NOT-MEASURED-BY-DESIGN

The changed lines are not measurable in this document, by design of the pipeline rather than by any
choice this plan makes. The three citations:

1. The coverage allowlist is built by skipping every project whose assembly name ends with `.Test` —
   `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:40-42`, intent stated at `:21-24`.
2. Every package outside that allowlist is removed from the document at
   `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:417-421`.
3. The root `line-rate`, `lines-covered` and `lines-valid` attributes are recomputed from what
   remains at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:441-445`.

On a successful run there is therefore no class element for
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` to read at all, and a change confined to a test
file moves neither `lines-covered` nor `lines-valid`. This was confirmed directly on both documents: a
fixed-string search for `QuickFiler.Test` returns 0 matches in each.

A class element for that file survives only when the raw pre-post-processing document is left on disk
because the write at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:343` never runs, and two distinct
throw sites produce that outcome: `:235-236`, whose guard at `:235` reaches the throw statement at
`:236` whenever any test beneath the search root failed, and `:341`, where
`Assert-CoberturaLineCoverageThreshold` evaluates the 80 percent floor. Both precede the write, so
both leave the raw document, and neither is recoverable from the document itself. An acceptance
condition that read a per-file figure from that element would therefore be satisfiable only on a run
that failed for one of those two reasons, so no such condition is stated.

CoverageDocumentState: post-processed

Both coverage artifacts record `EXIT_CODE: 0`, so the `post-processed` value applies. `CoverageDocumentState:`
is derived from the recorded exit code rather than from which throw condition fired, because the two
conditions are not distinguishable from the document itself. Exactly one of the two values applies,
because P2-T7 requires its recorded exit code to equal P0-T15's and halts when it cannot make them
equal.

## Acceptance, in six parts

**First — all fourteen fields are present.** The six copied numeric fields, the six derived fields,
and the two descriptive fields are all recorded above.

**Second — `CoverageDocumentState:` is `post-processed`.** Both coverage artifacts record
`EXIT_CODE: 0`.

**Third — the numerator.** The direction check does **not** hold: `PostLinesCovered:` is 54964 and
`BaselineLinesCovered:` is 54966, so 54964 is not greater than or equal to 54966. The tolerance branch
therefore applies. `LinesCoveredDifference:` is -2 and `LinesCoveredDifferencePercent:` is
0.00363861296073937, that is under four thousandths of one percent of the baseline numerator. The
baseline run carried the larger `lines-covered` figure.

The difference is not attributable to this change. The document carries no package for any assembly
whose name ends with `.Test`, so the changed file contributes nothing to `lines-covered` in either
document; the two figures are recomputed at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:441-445` from the surviving first-party packages
only. The surviving packages' line counts come from a `dotnet-coverage` cross-assembly merge over
every runtime-loaded module, and that merge is order- and parallelism-sensitive. Both runs executed
the same 6925 tests and both reported `Test Run Successful.`, so no test changed its pass state
between them. `branches-covered` moved in the same direction and by a similar magnitude, 13106 to
13103, which is consistent with a merge-ordering effect rather than with a source change confined to
one test method.

**Fourth — the denominator.** `LinesValidDifference:` is 0, `LinesValidDifferencePercent:` is 0, and
`LargerDenominatorRun:` records that neither run carried the larger figure because the two are
identical at 64381. `DenominatorComparability:` is `within-tolerance`, because 0 is at most 5. The
`beyond-tolerance` clause does not apply. The `raw-pre-post-processing` clause does not apply either,
because `CoverageDocumentState:` is `post-processed`.

For context on why a tolerance rather than an equality is used: the repository's recorded
measurements at `.claude/agent-memory/atomic-executor/project_dotnet_coverage_denominator_nondeterminism.md`
show `lines-valid` swings of 46 percent and 22 percent of their baselines on unchanged trees. This run
pair happened to reproduce the denominator exactly, which is a stronger result than the tolerance
required, but the tolerance remains the stated gate because that reproduction is not guaranteed.

**Fifth — `ChangedCodeCoverage: NOT-MEASURED-BY-DESIGN`** is recorded above with all three citations.

**Sixth — execution evidence for the changed lines.** The execution evidence is
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/qa-gates/p2-t5-scoped-run.md`,
which records `Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread` passing. Every
changed line falls into one of four categories, and the anchored diff recorded in
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/regression-testing/p1-t7-ac4-behavior-preserved.md`
shows no changed line outside them:

- lines inside `Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread`, which that run
  records as passing;
- the two removed `using` directives, `using System.Reflection;` and `using UtilitiesCS;`, which carry
  no executable statement;
- the added `const` field declaration `private const int GateTimeoutMs = 60000;` and the blank line
  after it, which carry no executable statement;
- XML documentation comment lines attached to that method, which carry no executable statement.
