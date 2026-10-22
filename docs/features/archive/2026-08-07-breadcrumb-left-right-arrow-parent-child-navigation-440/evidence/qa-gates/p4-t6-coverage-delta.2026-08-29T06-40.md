# Phase 4 — Coverage Delta and Threshold Verification (issue #440, plan task P4-T6)

Timestamp: 2026-08-29T06-40

Sources compared:

- Baseline: `<FEATURE>/evidence/baseline/test-coverage.2026-08-29T06-27.md`
  (Cobertura document `coverage\baseline440.cobertura.xml`).
- Post-change: `<FEATURE>/evidence/qa-gates/p4-t5-test-coverage.2026-08-29T06-38.md`
  (Cobertura document `coverage\final440.cobertura.xml`).

Neither run took the Global rule 7 coverage-threshold branch, so both documents are
normal post-processed output and every figure below is read on the same footing.

---

## The three required figures

### 1. Baseline coverage

| Scope | Metric | Value |
| --- | --- | --- |
| Repository-wide | line | 85.2935 % (`line-rate` `0.852935`, `lines-covered` 54755 of `lines-valid` 64196) |
| Repository-wide | branch | 79.2523 % (`branch-rate` `0.792523`, `branches-covered` 13037 of `branches-valid` 16450) |
| `BreadcrumbStateModel.cs` | lines | 119 covered of 121 total; 2 uncovered |
| `BreadcrumbStateModel.cs` | branches | 41 covered of 44 total; 3 uncovered |

### 2. Post-change coverage

| Scope | Metric | Value |
| --- | --- | --- |
| Repository-wide | line | 85.3026 % (`line-rate` `0.853026`, `lines-covered` 54760 of `lines-valid` 64195) |
| Repository-wide | branch | 79.2558 % (`branch-rate` `0.792558`, `branches-covered` 13036 of `branches-valid` 16448) |
| `BreadcrumbStateModel.cs` | lines | 118 covered of 120 total; 2 uncovered |
| `BreadcrumbStateModel.cs` | branches | 39 covered of 42 total; 3 uncovered |

### 3. Changed-region coverage

The changed-region set is derived from a **line span**, not from the diff line list.
The change consists of one deleted conjunct line, which by definition has no
post-change line number, plus a comment rewrite, and comment lines are never emitted
as `line` elements. A set derived from the diff alone would be empty and would gate
nothing.

Span derivation, performed mechanically against the post-change file: the
`public bool LeftArrow()` declaration was located at line **220** and its matching
closing brace, found by brace-depth counting from the declaration, at line **246**.
The span is therefore lines 220 to 246 inclusive.

Every `line` element under the file's `class` element whose `number` falls inside that
span, keyed by line number with the class-level rollup taking precedence over the
method-level view:

```
line 221: hits=1
line 222: hits=1
line 223: hits=1  condition-coverage=100% (2/2)
line 224: hits=1
line 225: hits=1
line 232: hits=1
line 233: hits=1  condition-coverage=100% (6/6)   <- the #440 transition if
line 234: hits=1
line 235: hits=1
line 236: hits=1
line 237: hits=1
line 238: hits=1
line 239: hits=1
line 241: hits=1  condition-coverage=100% (2/2)
line 242: hits=1
line 243: hits=1
line 244: hits=1
line 245: hits=1
line 246: hits=1
```

- Enumerated element count: **19**
- Elements with `hits` of 0: **0**

---

## The four gates

### Gate (1) — repository-wide non-regression

| Metric | Baseline | Post-change | Delta | Tolerance | Result |
| --- | --- | --- | --- | --- | --- |
| Line % | 85.2935 | 85.3026 | **+0.0091 pp** | may fall no more than 0.01 pp | PASS |
| Branch % | 79.2523 | 79.2558 | **+0.0035 pp** | may fall no more than 0.05 pp | PASS |

Neither figure decreased; both rose slightly. There is no decrease whose magnitude
needs stating. The tolerances were stated rather than zero because the change deletes
one covered line from a denominator of roughly 64,000; in the event they were not
needed.

### Gate (2) — per-file uncovered-line non-increase

`FinalFileTotalLines - FinalFileCoveredLines` = 120 - 118 = **2**
`BaselineFileTotalLines - BaselineFileCoveredLines` = 121 - 119 = **2**

2 is at or below 2. **PASS.**

### Gate (3) — per-file uncovered-branch non-increase

`FinalFileTotalBranches - FinalFileCoveredBranches` = 42 - 39 = **3**
`BaselineFileTotalBranches - BaselineFileCoveredBranches` = 44 - 41 = **3**

3 is at or below 3. **PASS.**

Gates 2 and 3 are stated on uncovered counts rather than on rates per decision D5,
because this change deletes a covered source line and removes a `&&` conjunct, which
moves both denominators (121 to 120 lines, 44 to 42 branches) for a measurement reason
rather than a quality reason. The uncovered counts are invariant across that
denominator shift, which is exactly what the gate is designed to detect.

### Gate (4) — changed-region coverage

- Enumerated set contains **19** `line` elements, which is at or above the
  at-least-4 floor. **PASS.**
- Every enumerated element has `hits` greater than 0. **PASS.**

The at-least-4 floor is what makes this gate fail rather than pass vacuously when the
lookup returns nothing. It is the conservative four rather than the observed nine
statement-level sequence points so that an instrumenter merging the multi-line
condition into one element would not fail the gate for a measurement reason. The
observed 19 is comfortably above it.

---

## Branch-level evidence for AC-15's first sentence

The `condition-coverage` of the `#440` transition `if`, before and after the change:

| | Line number | `condition-coverage` |
| --- | --- | --- |
| Before (baseline document) | 232 | `100% (8/8)` |
| After (post-change document) | 233 | `100% (6/6)` |

The line number shifted by +1 because P2-T2's comment rewrite added one line and
P2-T1 removed one line above the `if`. The condition count fell from 8 to 6 because
the guard lost one `&&` conjunct, and each conjunct contributes two conditions. Both
readings are 100 %, so branch coverage of the changed guard is not reduced. This is
the branch-level evidence AC-15's first sentence requires — the sentence requiring
that line and branch coverage for the file is not reduced relative to the pre-change
measurement.

AC-15's second sentence names an evidence location only, and it is discharged by the
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` record at the head of the plan:

```
EVIDENCE_LOCATION_OVERRIDE_REJECTED: <FEATURE>/evidence/coverage/ replaced with <FEATURE>/evidence/baseline/ (baseline coverage) and <FEATURE>/evidence/qa-gates/ (post-change coverage and the coverage delta)
```

---

## Verdict

All four gates pass. The outcome is PASS, not remediation-required.
