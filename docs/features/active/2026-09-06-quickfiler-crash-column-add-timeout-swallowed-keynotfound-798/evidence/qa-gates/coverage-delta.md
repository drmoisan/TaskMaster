# P8-T6 — Change-scoped coverage obligations verified against the Phase 0 baseline

Timestamp: 2026-09-07T05-47
Task: [P8-T6]
Issue: #798

Sources: `evidence/baseline/coverage-baseline.cobertura.xml` and
`evidence/baseline/per-file-coverage-baseline.md` (P0-T9, P0-T10) for the baseline side;
`evidence/qa-gates/coverage-final.cobertura.xml` and `evidence/qa-gates/per-file-coverage-final.md`
(P8-T4, P8-T5) for the post-change side. Both sides were measured with the same extractor, which was
validated by reproducing the P0-T10 figures from the baseline document before being applied to the
post-change document.

## 1. Baseline document-level line rate

`0.8582705126774282` — 169184 covered of 197122 valid lines.

## 2. Post-change document-level line rate

`0.8586876295435013` — 169857 covered of 197810 valid lines.

## 3. Per-file rate for each new module

| New module | covered | valid | rate | obligation | verdict |
|---|---|---|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` | 164 | 168 | 0.976190476190476 | >= 0.90 | PASS |
| `TaskMaster/Ribbon/RibbonCommandBoundary.cs` | 55 | 61 | 0.901639344262295 | >= 0.90 | PASS |

These two files carry all three new modules named in spec.md: the column partial, the validator that
lives inside it, and the ribbon boundary type.

## 4. Covered-line comparison for each existing production file

| Path | baseline covered | post-change covered | verdict |
|---|---|---|---|
| `QuickFiler/Controllers/QfcDatamodel.cs` | NOT INSTRUMENTED | NOT INSTRUMENTED | NOT APPLICABLE |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | NOT INSTRUMENTED | NOT INSTRUMENTED | NOT APPLICABLE |
| `TaskMaster/Ribbon/RibbonViewer.cs` | NOT INSTRUMENTED | NOT INSTRUMENTED | NOT APPLICABLE |
| `UtilitiesCS/Extensions/DfDeedle.cs` | 226 | 157 | see relocation adjustment below |

## Acceptance — all four clauses

### Clause 1 — `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` rate at or above 0.90

Observed `rate=0.976190476190476` (164 covered of 168 valid). 0.976190476190476 >= 0.90.
**PASS.**

### Clause 2 — `TaskMaster/Ribbon/RibbonCommandBoundary.cs` rate at or above 0.90

Observed `rate=0.901639344262295` (55 covered of 61 valid). 0.901639344262295 >= 0.90.
**PASS.** The margin is narrow: 55 of 61 clears the threshold, whereas 54 of 61 would read
0.885245901639344 and would not.

### Clause 3 — no covered-line loss on the three existing non-relocating production files

All three are `[ExcludeFromCodeCoverage]` at the type level: `RibbonViewer` on its own declaration,
and both `QfcDatamodel` partials through the single class-level attribute on the `QfcDatamodel`
declaration. All three read `NOT INSTRUMENTED` on both the baseline side and the post-change side, so
this clause records `NOT APPLICABLE` for each rather than a numeric comparison, which is the outcome
the plan states is expected and not a defect.

The `NOT INSTRUMENTED` readings are attribute-driven rather than assembly-driven, verified on both
sides: the `QuickFiler` package contributes 282 `class` elements under `QuickFiler\Controllers\` in
both documents, and the `TaskMaster` package contributes 13 `class` elements under
`TaskMaster\Ribbon\` at baseline and 15 post-change, the two added being the new
`RibbonCommandBoundary` type and its compiler-generated async state machine. Both owning assemblies
are therefore measured.

**PASS (NOT APPLICABLE for all three paths).**

### Clause 4 — relocation-adjusted comparison for the file that lost lines to the new partial

`UtilitiesCS/Extensions/DfDeedle.cs` loses lines to `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`,
so a per-file comparison on the source file alone would compare different denominators. The clause
therefore compares the sum across the pair against the single baseline value:

```
post-change covered(DfDeedle.cs)             = 157
post-change covered(DfDeedle.QfcColumns.cs)  = 164
                                        sum  = 321

baseline covered(DfDeedle.cs)                = 226

321 >= 226
```

**PASS**, with 95 covered lines more than the baseline across the pair. The increase is consistent
with the change: the new partial carries the AC1 timeout loop, the AC2 timing instrumentation and the
AC3 validator, all of which the Phase 2 through Phase 4 tests exercise, whereas the four methods
carried only partial coverage before relocation.

## Overall verdict

**All four clauses PASS.** No coverage gap remains for P8-T7 to close.

## Repository-wide figure — record and report, not a blocking gate

Baseline document-level line rate `0.8582705126774282`; post-change `0.8586876295435013`. Direction
of movement: **up**, by 0.0004171168660731 in absolute terms, over a denominator that itself grew
from 197122 to 197810 valid lines as the new production and test code entered measurement.

No repository-wide floor is asserted for this item, for the three reasons the plan records:

- No merge-base repository baseline exists in this feature folder. The Phase 0 baseline was captured
  on this worktree, not against a merge base, so it is a same-tree reference for the delta and not a
  repository gate.
- The repository floor applies to the **testable denominator**, that is, production-only first-party
  code after the COM, VSTO and WinForms exemptions. The document-level rate reported here is over the
  raw denominator, which includes test assemblies and host-bound code, so it is not the figure the
  floor is defined against.
- This pipeline's repository-wide line rate is not reproducible run-to-run on an identical tree, so a
  small movement in either direction would not be attributable to the change.

The change-scoped obligations in clauses 1 through 4 above are the blocking coverage gates for this
item, and all four pass.

Output Summary: All four acceptance clauses pass. New modules read 0.976190476190476 and
0.901639344262295, both at or above 0.90. The three `[ExcludeFromCodeCoverage]` paths read
NOT INSTRUMENTED on both sides and are recorded NOT APPLICABLE. The relocation-adjusted sum is
157 + 164 = 321 against a baseline of 226. The repository-wide line rate moved up from
0.8582705126774282 to 0.8586876295435013 and is reported, not gated.

---

## P8-T7 — Gap closure

Timestamp: 2026-09-07T05-48
Task: [P8-T7]

**GAP CLOSURE: NOT REQUIRED**

P8-T6 identified no coverage gap: all four of its acceptance clauses were satisfied on the first
measurement, recorded above. P8-T7's action is conditional on a gap existing, so no test was added,
no test file was edited, and P8-T4, P8-T5 and P8-T6 were not re-run.

Gap-closure tests added: **none**. There is therefore no fully-qualified test name to list.

This artifact in its final state satisfies all four clauses of P8-T6, which is P8-T7's acceptance
condition.

Two consequences worth recording, because both were live risks entering this task:

- **The 500-line cap was not approached.** `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs`
  stands at exactly 500 lines, the repository cap, so it has zero headroom and a single added line
  would breach it. Had a gap needed closing in `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`, the
  test would have been routed to `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs`
  instead, which is the other owning test file named by P8-T7 and has headroom. No such routing was
  needed.
- **The write set was not widened.** No file was added, so the sixteen-path write set fixed by AC13
  is unchanged, and the P8-T9 re-verification of the write-set diff is unaffected by this task.

