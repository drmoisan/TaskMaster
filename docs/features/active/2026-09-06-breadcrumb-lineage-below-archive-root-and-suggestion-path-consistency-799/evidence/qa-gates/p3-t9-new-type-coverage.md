# [P3-T9] Coverage of the two new production types

Timestamp: 2026-09-07T08-10

Command: the [P3-T9] separator-anchored `class` `filename` selection and de-duplicated per-line aggregation over artifacts\csharp\coverage.xml

EXIT_CODE: 0

ExpectedExitCode: 0

## Aggregation output (verbatim, printed by the block)

```
FILE=ArchiveStemProjection.cs CLASS_ELEMENTS=1 LINES_COVERED=12 LINES_VALID=12 BRANCHES_COVERED=16 BRANCHES_VALID=16
FILE=ArchiveChainProjection.cs CLASS_ELEMENTS=1 LINES_COVERED=29 LINES_VALID=29 BRANCHES_COVERED=32 BRANCHES_VALID=32
```

One `FILE=` line was printed per new type, which is this task's first acceptance condition.

## Derived rates

| File | CLASS_ELEMENTS | Lines covered / valid | Line rate | Branches covered / valid | Branch rate | 90 percent floor |
|---|---|---|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` | 1 | 12 / 12 | 100.00 | 16 / 16 | 100.00 | met |
| `UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` | 1 | 29 / 29 | 100.00 | 32 / 32 | 100.00 | met |

UNCOVERED-LINES-ArchiveStemProjection: none
UNCOVERED-LINES-ArchiveChainProjection: none

## Acceptance conditions

- One `FILE=` line per new type: satisfied, two lines printed.
- `CLASS_ELEMENTS` greater than zero on both: satisfied, 1 on each. A zero would have meant the separator-anchored
  selection matched nothing and the rates below it were computed over an empty set; that did not happen and the
  count is recorded so the outcome is visible rather than silent.
- `LINES_COVERED` divided by `LINES_VALID` at or above 0.90 for each file: satisfied, 1.00 on both.

## Policy context

The repository unit-test policy requires new modules, classes and methods to target at least 90 percent coverage.
Both new production types reach 100 percent line and 100 percent branch coverage, measured on the same
de-duplicated per-line map [P3-T7] uses, over the same Cobertura document [P3-T5] produced.

The coverage comes from the two dedicated test classes [P1-T6] and [P1-T7] created —
`UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` with 11 `[TestMethod]` attributes and
`UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` with 7 — plus the incidental exercise both
types receive through the four converted `ToDisplayStem` call sites and the provider trim. Every branch of both
types is reached: the null guards, the exact-equality zero-length-stem case, the Archive2 false-prefix case, the
trailing-separator normalisation, the empty and whitespace root cases, and, in the chain projection, the
root-not-found, leaf-is-root, empty-chain and single-element-chain arms.

Output Summary: Both new production types are fully covered. `ArchiveStemProjection.cs` reports 12 of 12 lines and
16 of 16 branches; `ArchiveChainProjection.cs` reports 29 of 29 lines and 32 of 32 branches. `CLASS_ELEMENTS` is 1
on each, so neither selection was empty. Both clear the 90 percent new-code floor at 100 percent, and neither file
has an uncovered line.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
