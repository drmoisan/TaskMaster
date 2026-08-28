# [P15-T8] Coverage comparison, baseline to post-change

Timestamp: 2026-08-26T16-50

Command:

```
head -c 400 docs/.../evidence/baseline/coverage-baseline.cobertura.xml     # P0-T14 root element
head -c 400 docs/.../evidence/qa-gates/coverage-final.cobertura.xml        # P15-T5 root element
grep -o '<package line-rate="[^"]*" branch-rate="[^"]*"[^>]*name="QuickFiler"' <both files>
grep -o '<class ... name="...QfcCollectionController..." filename="..."' <final file>
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Six numeric values, no placeholder.

| Metric | Baseline (P0-T14) | Post-change (P15-T5) | Delta |
|---|---|---|---|
| **Repository `line-rate`** | **84.7703%** | **84.9435%** | **+0.1732 pp** |
| **Repository `branch-rate`** | **78.6876%** | **78.9377%** | **+0.2501 pp** |

Both rates rose. Both figures are read from the root `<coverage>` element of the respective Cobertura
documents.

### Source values, verbatim

Baseline — `evidence/baseline/coverage-baseline.cobertura.xml`:

```
line-rate="0.847703" branch-rate="0.786876" lines-covered="53763" lines-valid="63422" branches-covered="12675" branches-valid="16108"
```

Post-change — `evidence/qa-gates/coverage-final.cobertura.xml`:

```
line-rate="0.849435" branch-rate="0.789377" lines-covered="54143" lines-valid="63740" branches-covered="12840" branches-valid="16266"
```

### Absolute counts

| Count | Baseline | Post-change | Delta |
|---|---|---|---|
| lines covered | 53,763 | 54,143 | +380 |
| lines valid (denominator) | 63,422 | 63,740 | +318 |
| branches covered | 12,675 | 12,840 | +165 |
| branches valid (denominator) | 16,108 | 16,266 | +158 |

## Changed-line coverage

Changed-line coverage: 0 changed production lines lie in the coverage denominator. `QuickFiler/Controllers/QfcCollectionController.cs` carries [ExcludeFromCodeCoverage] at :21, and QuickFiler/Interfaces/IQfcCollectionController.cs gains only XML documentation with no executable line. Changed-code coverage is therefore undefined rather than unmeasured.

### Verification of that statement

Two independent checks confirm it rather than assert it:

1. **The controller is absent from the coverage document as a measured class.** Searching
   `coverage-final.cobertura.xml` for a `<class …>` element whose `name` contains
   `QfcCollectionController` returns **zero** matches. The identifier does appear twelve times in the
   file, but every occurrence is inside a `<method>` signature belonging to a *different* class, where
   `QuickFiler.Interfaces.IQfcCollectionController` is a parameter type. A type carrying
   `[ExcludeFromCodeCoverage]` contributes no `<class>` element, hence no `<line>` element, hence
   nothing to either the numerator or the denominator.
2. **The interface change is documentation only.** The whole diff at
   `QuickFiler/Interfaces/IQfcCollectionController.cs` across this feature is nineteen added lines,
   of which thirteen are the XML doc block for `MoveEmailsAsync` and the rest are blank separators.
   Zero lines removed. The member declaration itself is byte-identical to the base commit. XML doc
   comments emit no IL and are never instrumented, so the file gained no coverable line. This is
   recorded verbatim in `evidence/qa-gates/p14-t13-scope-creep-audit.2026-08-26T16-40.md`.

Those two files are the only production files this feature changed. The changed-code denominator is
therefore genuinely empty — not zero-because-unmeasured, but zero-because-there-is-nothing-to-measure.
A changed-code coverage percentage cannot be computed, because it would divide by zero.

## No part of the delta is attributable to this feature's tests

This is the point that must not be misread in the PR body. The repository line-rate rose by
0.1732 pp and the branch-rate by 0.2501 pp, but:

- `QuickFiler/Controllers/QfcCollectionController.cs` carries `[ExcludeFromCodeCoverage]` and
  contributes nothing to either denominator. The 28 tests this feature adds exercise that file
  exclusively — they drive `TryGetMoveReadiness`, `ShrinkByRows`, `ReconcileInsertionCount`,
  `GetMoveDiagnostics`, `PromoteFirstChild`, `SetVisualDigits`, `DrainBackgroundLoadingTasksAsync`,
  `RemoveSpecificControlGroupAsync`, `MoveEmailsAsync`, and `EliminateSpaceForItems`, all of which
  live in that one excluded file. **Every line those tests cover is outside the measurement.**
- The denominator grew by 318 lines and the numerator by 380. Both movements come from the two merges
  of `origin/epic/quickfiler-bug-family-integration`, which brought in sibling features 498 and 446 —
  new production code in `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`,
  `UtilitiesCS/OutlookObjects/Folder/*.cs` and elsewhere, together with 71 tests covering it.
- The `QuickFiler` package rate moved from 76.8497% to 77.6691% (line) and 72.6905% to 74.0135%
  (branch). That movement is likewise sibling-derived for the same reason.

The plan states this directly in its `### Coverage scope note`: **no acceptance condition in this plan
claims a coverage increase attributable to this feature**, because such a condition could not fail.
The delta is reported because the coverage-evidence contract requires numeric baseline and
post-change values; it is not offered as evidence that this feature works.

## What carries the per-defect proof instead

Named MSTest methods, indexed in `evidence/qa-gates/p14-t8-fail-before-index.2026-08-26T16-30.md`:
fifteen genuine red-then-green pairs across `#474-1`, `#286`, `#469-3`, `#473-2`, `#469-1`, `#469-2`,
`#470-2`, `#470-1`, `#470-3`, `#471`, and `#473-1`; three permanent-green tests whose absent red
state is justified item by item in
`evidence/regression-testing/fail-before-exception.2026-08-26T16-24.md`; and `#468`, a removal proven
by compilation, a green suite, and a reflective-caller search over 398 build-input files.

The per-defect map the PR body must use is at
`evidence/other/pr-accuracy-constraints.2026-08-26T16-27.md`, constraint 5.

## Repository thresholds

| Threshold | Source | Post-change value | Meets it? |
|---|---|---|---|
| Line coverage >= 80% | `CLAUDE.md` UT2 | 84.9435% | yes |
| Line coverage >= 85% | `.claude/rules/general-unit-test.md` | 84.9435% | **no — short by 0.0565 pp** |
| Branch coverage >= 75% | `.claude/rules/general-unit-test.md` | 78.9377% | yes |

The two line thresholds in this repository disagree with each other — 80% in `CLAUDE.md` and 85% in
`.claude/rules/general-unit-test.md`. That contradiction is a known, open defect tracked by issue
**#563** (`Coverage threshold contradiction remains: CLAUDE.md/csharp.md say 80%,
general-unit-test.md/quality-tiers.md say 85%/75%, and two live gates disagree`). It is recorded here
rather than resolved, because resolving it is outside this feature's scope.

Against the stricter of the two readings the repository is 0.0565 pp short, and it was 0.2297 pp short
at the baseline. This feature moved the figure **toward** the stricter threshold rather than away from
it, and no line it changed is in the denominator, so the shortfall is neither caused nor worsened
here.

## Acceptance verification

| Clause | Status |
|---|---|
| the artifact exists | met |
| records two baseline numeric values | met — 84.7703%, 78.6876% |
| records two post-change numeric values | met — 84.9435%, 78.9377% |
| records two deltas | met — +0.1732 pp, +0.2501 pp |
| records the changed-code statement | met — quoted verbatim above as a single line |
| no placeholder | met — every value is a measured number read from a committed Cobertura document |
