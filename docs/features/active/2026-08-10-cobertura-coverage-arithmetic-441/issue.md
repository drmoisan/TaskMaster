# 2026-08-10-cobertura-coverage-arithmetic-441 (Issue)

- Work Mode: full-bug
- **Primary Issue:** #441
- **Also Closes:** #478
- **Epic:** `build-ci-coverage-gate-fidelity` (wave 0, no dependencies)
- **Integration Branch:** `epic/build-ci-coverage-gate-fidelity-integration`
- **Type:** bug
- **Complexity Band:** C3 (`cross_module_contract_change`)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-10T14-07
- **Status:** Prepared (planning complete, execution deferred to epic-orchestrator)
- **AC Source:** `spec.md` (full-bug work mode)

## Summary

Two compounding defects in the Cobertura post-processing arithmetic in
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. Both live in the same rate-recomputation
path and must be fixed as a single change.

### Issue #441 — descendant-axis double count

`Get-CoberturaCoverageSummary` selects over the XPath descendant axis `.//lines/line`. In the
Cobertura documents this pipeline actually produces, each `<class>` carries its `<line>` nodes twice
— once nested under each `<method>`, and once again as a class-level rollup. The descendant axis
matches both sets, so every line is counted twice.

**Corrected site references (research-verified 2026-08-10; supersedes the line numbers in the
GitHub issue text).** The issue text cites `:98` and `:167`; both are function *declaration* lines,
not selections. The verified situation is:

| Site | Expression | Verdict |
| --- | --- | --- |
| `Invoke-MSTestWithCoverage.Helpers.ps1:122` | `$cls.SelectNodes('.//lines/line')` | **The one and only defective selection in the repository.** |
| `Invoke-MSTestWithCoverage.Helpers.ps1:219` | `$classNode.SelectNodes('./lines/line')` | **Already correct** (child axis). This is the merge union builder and must NOT be changed. |
| `Invoke-MSTestWithCoverage.Helpers.ps1:270-273` | `Get-CoberturaCoverageSummary -XmlDocument $classSummaryXml` | **Indirect.** This delegation is how the `:122` defect reaches the merged per-class rate. |
| `Invoke-MSTestWithCoverage.ps1` | — | **No line-axis selection at all.** No change required in this file. |

`Merge-CoberturaClassesByFilename` does not itself select over the descendant axis; it inherits the
defect through the `$classSummaryXml` delegation. Editing `:219` instead of `:122` would destroy the
correct union and leave both defects in place.

Confirming evidence: in the committed sample
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
the header attribute `lines-valid="110849"` equals exactly the raw count of `<line number=`
elements in the file, not the distinct-line count.

### Issue #478 — blended merge denominator

`Merge-CoberturaClassesByFilename` unions the class-level `<lines>` of all `<class>` elements
sharing a `filename` correctly (`:217-268`, max hits per line number), but never merges the
corresponding `<methods>` subtrees into the primary class element. It then recomputes `line-rate`
over the same `.//lines/line` descendant axis, which sees the correct union **plus** only the
primary class's method-level lines. The emitted per-file `line-rate` therefore blends two
denominators and matches neither.

## Reproducible Arithmetic (regression fixture assertion)

For `QfcHomeController.Iteration.cs` in the committed report above:

| Quantity | Value |
| --- | --- |
| True per-file figure from the class-level union alone | 45 / 56 = 0.8036 |
| Emitted `line-rate` attribute (blended) | 0.8625 = 69 / 80 |

Independent cross-check of the correct recipe:
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-delta.2026-08-07T00-48.md`
lines 10 and 41.

## Impact / Severity

- #441: High. Reported repository-wide and per-assembly line-coverage figures are computed over an
  inflated denominator. The error is not uniform across assemblies because duplication is not
  uniform across classes. Every coverage gate and every committed coverage baseline in the
  repository consumes this figure.
- #478: High. Any consumer reading the per-file `line-rate` attribute gets a wrong number, and the
  error direction depends on how much of a file's coverage sits in the primary class versus its
  siblings. Epic #136 gates each of its fifteen children on a per-file line rate.

## Expected Behavior

1. `Get-CoberturaCoverageSummary` counts each source line exactly once per class, deduplicating by
   line number with `max(hits)`, so `lines-valid` equals the distinct-line count.
2. The emitted per-file `line-rate` equals the rate computed from the merged class-level `<lines>`
   set alone: distinct line numbers, max hits per number, hit count over total count.
3. `branches-valid` / `branches-covered` are deduplicated on the same basis (one
   `condition-coverage` fraction per class per distinct line number). Branch arithmetic is
   necessarily in scope: the branch accumulator sits physically inside the defective loop, and
   correcting lines while leaving branches doubled would emit an internally inconsistent report.

## Correctness Oracle (research-verified)

`dotnet-coverage`'s own Cobertura writer defines `lines-valid` / `lines-covered` /
`branches-valid` / `branches-covered` from the **class-level rollup only**. Verified against the
committed *raw* generator output
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`:
its class-level `<line>` count is exactly `79957` = its own `lines-valid`, and class-level minus
uncovered is exactly `56124` = its own `lines-covered`, while the both-axes count is `161086`.

Therefore the corrected `Get-CoberturaCoverageSummary`, run over that raw document, must reproduce
that document's own root attributes exactly: **79957 / 56124 / 23109 / 13472**. This is the primary
acceptance oracle for the fix.

The current code does not merely count differently — it unconditionally *overwrites a correct root
summary with an incorrect one*.

## In Scope

- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — line-selection and rate-recomputation
  arithmetic.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — only if it independently selects over the same
  descendant axis.
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` — regression tests.
- Repository-wide coverage baseline re-capture (pre-change and post-change figures, recorded
  numerically in evidence).

## Out of Scope (explicit)

- **Re-tuning any coverage threshold.** Threshold reconciliation is owned by child feature #494,
  which runs after this one. If a corrected figure would fail an existing threshold, record the
  fact in evidence and state it as a handoff to #494. Do not lower the threshold.
- **`[ExcludeFromCodeCoverage]` nested-lambda suppression (#457).** That is a separate child
  feature that depends on this one. It changes *which* lines enter the denominator; this feature
  changes only *how* they are counted.
- **`CLAUDE.md` and anything under `.claude/rules/`.** Those edits belong to sibling features #512
  and #494.

## Known Environment Note

`CLAUDE.md`'s documented `/p:Nullable=enable` type-check command is a known defect (issue #522)
that produces roughly 200-414 spurious errors on a clean `main`. It is not a gate for this change:
this feature is PowerShell-only and the applicable toolchain is PoshQC format → PSScriptAnalyzer →
Pester per `.claude/rules/powershell.md`.

## Research

Authoritative research for this feature:
`docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/research/2026-08-10T14-20-cobertura-arithmetic-research.md`

**Prior-research availability correction.** The two documents named in the epic kickoff are NOT
present on this branch and must not be cited as readable:

- `docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/research/2026-08-07T22-15-quickfiler-coverage-ledger-research.md`
  — absent (feature folder `-432` is not on this base).
- `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/coverage-harness-contract.md`
  — absent (feature folder `-454` is not on this base).

The root-cause analysis was therefore re-derived independently from source and from the committed
sample reports, and is stronger than the issue text (it identifies one defective site rather than
two, and establishes the generator-parity oracle above).

The one cross-check document named in the kickoff **is** present and was read:
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-delta.2026-08-07T00-48.md:10`
records the correct recipe verbatim.

## Acceptance Criteria

Authoritative acceptance criteria for this `full-bug` feature live in `spec.md` § Acceptance
Criteria. This section is a pointer only and must not be treated as a second AC source.
