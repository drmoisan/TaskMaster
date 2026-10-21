# P3-T3 — Corroborating Measurement Against A Committed Real Cobertura Document

Timestamp: 2026-09-09T11-09
Task: [P3-T3]
Command: `pwsh -NoProfile -File aggregate-compare-815.ps1 -RepoRoot <repository root> -RelativeDocumentPath docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml`
EXIT_CODE: 0

`aggregate-compare-815.ps1` is the single throwaway measurement helper plan decision D7 authorizes.
It was created in the agent session scratchpad directory, outside the repository worktree, and is
deleted by P4-T1. Its absolute path and the repository's absolute root are deliberately not recorded
here: an absolute path would record the operator's account name and machine name in a committed
artifact. The helper's file name and the repository-relative document path above are the whole of
its identification.

The helper takes the repository root and the document path as explicit parameters and resolves every
repository file through them, so its result does not depend on the working directory the process
starts in.

## Document identity

| Field | Value |
| --- | --- |
| Document | `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml` |
| SHA-256 | `803C4030F4FAD13352B694EEB7CBF9819D91CEF49A23ABBFDD77221E5D8E46BA` |

This SHA-256 **equals** the value recorded for the same document in
`evidence/baseline/p0-t11-threshold-and-fixture-baseline.md`, so both aggregations demonstrably ran
over the same bytes that baseline pinned.

## Resolved allowlist

`Get-KoverageProjectAllowlist` was called exactly once and the resulting nine names were bound to
both sides of the comparison, so the retained package set is identical on both sides by construction
rather than by assumption:

```
QuickFiler, SVGControl, Tags, TaskMaster, TaskTree, TaskVisualization, ToDoModel, UtilitiesCS, VBFunctions
```

A third party re-running the measurement with this allowlist retains the identical package set.

## Both computations

```
NAIVE LinesValid=133485 LinesCovered=112865 BranchesValid=33624 BranchesCovered=26642 LinePercent=84.55 BranchPercent=79.24
DEDUP LinesValid=66158  LinesCovered=55940  BranchesValid=16812 BranchesCovered=13321 LinePercent=84.56 BranchPercent=79.24
STRICTLY_GREATER_LINESVALID=True
```

| Quantity | Descendant-axis `.//line` | De-duplicated | Delta |
| --- | --- | --- | --- |
| LinesValid | 133,485 | 66,158 | +67,327 |
| LinesCovered | 112,865 | 55,940 | +56,925 |
| BranchesValid | 33,624 | 16,812 | +16,812 |
| BranchesCovered | 26,642 | 13,321 | +13,321 |
| Line percentage | 84.55% | 84.56% | -0.01 points |
| Branch percentage | 79.24% | 79.24% | 0.00 points |

## The strict inequality AC7 requires

The descendant-axis `LinesValid` of 133,485 is **strictly greater** than the de-duplicated
`LinesValid` of 66,158. The helper asserted this directly and printed
`STRICTLY_GREATER_LINESVALID=True`.

## Observations

The branch counters are **exactly** doubled: 33,624 is twice 16,812 and 26,642 is twice 13,321, so
the branch percentage is bit-for-bit identical at 79.24% under both computations. The line counters
are not exactly doubled — 133,485 exceeds twice 66,158 by 1,169 — which is the intra-class
repeated-line-number shape, and it moves the line percentage by one hundredth of a point in the
**pessimistic** direction. This reproduces on a real 66,158-line first-party population exactly the
property the fixture demonstrates and that `spec.md` records: **the defect is a count defect, not a
rate defect.** The counts are roughly twice the true population, which invalidates every
absolute-count assertion built on them, while the derived percentages barely move.

## Scope statement

This criterion does **not** require re-deriving the 79.38% or 77.03% figures from issue 809, and no
attempt to do so was made. The report that produced those figures is not committed: it was written
under `coverage/`, which `.gitignore` excludes, and every tracked file under the issue 809 feature
folder is a `.md` file. The provenance of 77.03% therefore cannot be reconstructed from the
repository, and `spec.md` records it as unknown.

Output Summary: Both aggregations ran over the same committed Cobertura document, pinned by the
SHA-256 `803C4030...5D8E46BA` recorded in P0-T11, using one resolved nine-name allowlist bound to
both sides. The descendant-axis computation reports 133,485/112,865 lines and 33,624/26,642
branches; the de-duplicated computation reports 66,158/55,940 lines and 16,812/13,321 branches. The
descendant-axis `LinesValid` is strictly greater than the de-duplicated `LinesValid`. Line
percentages are 84.55% and 84.56%; branch percentages are 79.24% under both.
