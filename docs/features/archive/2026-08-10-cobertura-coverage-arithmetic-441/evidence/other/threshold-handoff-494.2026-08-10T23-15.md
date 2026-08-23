# Threshold Handoff to Child Feature #494 (P5-T4)

Timestamp: 2026-08-10T23-15

This is a **record of fact and a handoff. It is not a proposal, and nothing in it is acted upon by
this feature.**

## The measured fact

The corrected repository-wide line rate for the #424 committed sample
(`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
reprocessed through `ConvertTo-KoverageCoberturaXml` with the fix applied) is:

| Quantity | Value |
| --- | --- |
| `lines-covered` | **53013** |
| `lines-valid` | **62345** |
| **`line-rate`** | **0.850317 = 85.0317%** |

Measured in P5-T2; the pre-change figures for the same input were 94937 / 110849 / 0.856453
(85.6453%), measured in P0-T12.

## The threshold it is measured against

`.claude/rules/general-unit-test.md` § Coverage Requirements states a **uniform line-coverage floor
of >= 85% across all tiers (T1-T4)**. `.claude/rules/quality-tiers.md` repeats the same figure in
its uniform gate matrix. (`CLAUDE.md` § UT2 states a laxer repository-wide floor of >= 80%; where
the two differ, the stricter 85% figure is the one recorded here.)

**Margin: 85.0317% versus 85% = 0.03 percentage points.**

The corrected figure is materially closer to the floor than the reported one: the defect was
inflating the reported rate by 0.61 pp for this document. The inflation is not uniform across
assemblies, because line duplication is not uniform across classes, so per-assembly margins will
differ from this repository-wide figure and must be re-measured rather than extrapolated.

## Owner

**Child feature #494 (epic `build-ci-coverage-gate-fidelity`, wave 2) owns threshold
reconciliation.** #494 runs after #457 (wave 1), which itself runs after this feature (wave 0).

## What this feature does about it: nothing

This feature **proposes no threshold change and makes no threshold edit.** Enforced mechanically by
P4-T9, which recorded:

- `git diff --name-only edf3d34c -- CLAUDE.md .claude/rules coverage.config` returns **empty
  output** — no threshold-bearing file is touched;
- the single added line in the two changed source files matching `\b(85|90|75)\b` is a fixture rate
  value (`0.75`, the defective per-file `line-rate` that fixture F3 exists to detect), not a
  threshold.

`spec.md` § Scope & Non-Goals item 1 and the plan's § Non-Goals item 1 both record this as a hard
scope boundary, and `spec.md` § Risks & Mitigations names "someone lowers the threshold to create
margin" as an identified risk whose mitigation is exactly this handoff.

## Additional inputs #494 may find useful

1. **Historical evidence is non-comparable.** Every committed Cobertura coverage artifact in the
   repository produced by `ConvertTo-KoverageCoberturaXml` carries the inflated root attributes.
   Twenty-one unmerged branches from epic #136 gate on per-file line rates computed by the defective
   code. None touches any file this feature modifies, so there is no merge conflict, but their
   committed coverage evidence will not reproduce against the corrected arithmetic.
2. **Branch coverage has no instrument for PowerShell here.** Pester 5.6.1 emits no `BRANCH`
   counter, so the `>= 75%` branch floor cannot be evaluated for PowerShell modules in this
   repository. Recorded as an auditable negative-evidence claim in
   `<FEATURE>/evidence/qa-gates/coverage-delta.2026-08-10T23-10.md` § 2.
3. **Package-level rates remain stale.** `ConvertTo-KoverageCoberturaXml` writes only root and
   merged-class attributes, so every `<package line-rate=...>` is stale after package filtering and
   class merging. Filed as follow-up candidate 1; it is a separate defect from #441/#478 and is not
   fixed here. Any #494 gate that reads a package-level rate is reading a stale value.
