# P2-T5 — Repo-Wide Floor Escalation Finding (FLOOR-BELOW) (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-50

## Decision routed to: orchestrator (authority-scoped exception decision)

This is a scoped escalation finding. The repo-wide first-party testable-denominator coverage floor (`>= 80%`) is NOT met. The gate is NOT weakened, no exemption is widened, and no test is altered. The disposition (accept as pre-existing debt under the ratified COM/VSTO/WinForms exemption initiative, or require additional first-party tests before merge) is an authority-scoped decision for the orchestrator/maintainer, not the executor.

## Measured figures
- Repo-wide first-party testable-denominator: 73.35% (authoritative #197 per-`<line>`, 39585/53969); 74.11% by Cobertura root aggregate (71654/96685); 76.08% vendored-excluded per-`<line>` (38607/50745).
- Floor: `>= 80%`. Gap: approximately 6.65 pp (authoritative) to 3.92 pp (vendored-excluded).

## Evidence the shortfall is PRE-EXISTING, not introduced by this change
1. New code is fully covered: `QfcFormKeyHandler.IsAltKeyCommand` is 2/2 = 100% (>= 90% new-code floor). Source: coverage-delta.2026-06-28T20-52.md.
2. Changed type improved, no regression: `QfcFormController` went 39.24% (301/767) -> 51.86% (363/700), +12.62 pp. Source: coverage-delta.2026-06-28T20-52.md.
3. The change is a testability refactor that ADDS tests and moves Form-bound code under `[ExcludeFromCodeCoverage]`; it cannot lower repo-wide first-party coverage. The instrumented denominator confirms the Form-derived/Designer exempt classes (QfcFormViewer, QfcFormViewerDark, QfcFormViewerExpanded, Designer) are absent — the exemption is applied as documented, not weakened.
4. The repo-wide first-party shortfall is a known, separately-tracked initiative. Prior `feature/csharp-coverage-uplift` (#197) figures were in the 59.03%-76.08% range; the 73.35% measured this cycle is consistent with that pre-existing baseline. The low-coverage packages (QuickFiler 32.2%, ToDoModel 27.0%, Tags 37.9%, TaskMaster 53.4%, TaskVisualization 18.3%) are predominantly COM/Outlook-Interop-bound code whose untested portions are not marked `[ExcludeFromCodeCoverage]` and therefore still count in the denominator; raising them is out of scope for this testability refactor (issue #223) and would require the separate coverage-uplift effort.

## What this remediation DID accomplish
- Resolved Finding 1's artifact-existence sub-claim: `artifacts/csharp/coverage.xml` now exists (well-formed Cobertura, first-party packages, `.Test` stripped).
- Resolved Finding 1's measurement sub-claim: the repo-wide first-party testable-denominator figure is now measured and recorded (73.35% / 74.11%), replacing the prior UNMEASURED state and the disclaimed 12.86% single-assembly process-wide number.

## What remains for orchestrator decision
- AC5's "repo-wide coverage stays >= 80%" sub-claim cannot be confirmed at 73.35%/74.11%. AC5 remains `[ ]` (unchecked) pending the orchestrator's authority-scoped exception decision. See P3-T3 deferral artifact `evidence/issue-updates/issue-223-ac5-deferred.2026-06-28T21-30.md`.

Output Summary:
FLOOR-BELOW escalation: measured repo-wide first-party coverage is 73.35% (authoritative) / 74.11% (root), below the `>= 80%` floor by ~6.65/5.89 pp. The shortfall is demonstrably pre-existing (new code 100%, changed type +12.62 pp, no regression; exemptions applied not weakened; consistent with the prior #197 coverage-uplift baseline). The floor is not weakened and the cycle does not silently pass. The accept-vs-uplift decision is routed to the orchestrator; AC5 stays unchecked.
