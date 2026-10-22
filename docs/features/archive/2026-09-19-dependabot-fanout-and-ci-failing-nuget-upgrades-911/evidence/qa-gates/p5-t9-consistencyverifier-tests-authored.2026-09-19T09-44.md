# P5-T9 — ConsistencyVerifier module-level suite authored

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = "<execution-worktree-root>\tests\scripts\dependencies\ConsistencyVerifier.Tests.ps1"; line count, It count, block-and-test names matching AC\d, temporary-file idiom count'
```

EXIT_CODE: 0

## Output Summary

```
LINES=272
IT=11
NAMES_WITH_ACDIGIT=0
TEMPFILE=0
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| File at most 500 lines | <= 500 | 272 |
| `It` blocks | at least 11 | 11 |
| `Describe`, `Context` or `It` names matching `AC\d` | exactly 0 | 0 |
| Creates no temporary file | 0 idioms | 0 |

The bound is 11 rather than 9 because a bound of 9 is satisfied by a suite that omits the
fifth surface entirely. The eleven are a clean case and a broken case for each of the five
surfaces, plus the guarded-unmanifested-import case.

| Surface | Clean case | Broken case |
|---|---|---|
| Disagreement | no disagreement, examined > 0, analyzer examined = 1 | stale analyzer item reported with found and expected versions |
| Orphaned `<HintPath>` | no orphan, examined > 0 | orphan named by package folder |
| Reference completeness | no missing reference, examined = 1 | missing reference named by asset file name, `HasHintPath` false |
| Absent from manifest | nothing absent, examined > 0 | element named by package folder |
| Missing Roslyn segment | no finding with examined item count 3 | two records aggregated and counted, offered segments named |

The missing-Roslyn-segment cases assert the **aggregation and the count** over records
supplied as if returned by `AnalyzerItemRepair.psm1`. The derivation itself is owned by
`tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` and is deliberately not asserted
here, so the class has exactly one test owner per concern.

The clean case for the aggregator supplies `-ExaminedItemCount 3` with an empty record set.
That is what makes the clean case meaningful for this surface: the records only exist when
a segment is missing, so an empty record set alone would be indistinguishable from an
aggregation over nothing.

## The altcover class

The eleventh case reproduces `QuickFiler.Test/QuickFiler.Test.csproj` lines 8 and 514 in
memory: two `Exists()`-guarded `<Import>` elements naming
`..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props` and `AltCover.targets`,
with no matching manifest entry and no `<Error>` guard. The case asserts that the verifier

- reports exactly 2 instances of the absent-from-manifest class,
- names them in the report by package folder, and
- still returns a success result, because the class is non-fatal.

No exception is hard-coded for altcover or any other package identifier: the instances are
reported by the general rule. The import itself is tracked as a separate issue.

## Smoke run

A run of this suite against the implemented verifier reported
`Passed=11 Failed=0 Total=11`. The measured, filtered run required by P5-T10 follows.
