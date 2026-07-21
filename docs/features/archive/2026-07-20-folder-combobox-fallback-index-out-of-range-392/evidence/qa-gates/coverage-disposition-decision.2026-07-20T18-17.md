Timestamp: 2026-07-20T18-17

## R2 maintainer-disposition record (documentation only; no code change)

This artifact records, verbatim, the disposition already made by the orchestrator for the R2 items
identified in `remediation-inputs.2026-07-20T18-00.md`:

**Disposition: SCOPE_CHANGE**

The following two coverage gaps are dispositioned as `SCOPE_CHANGE`, not fixed within this
remediation cycle:

1. `QuickFiler` package-wide coverage: 73.68% line / 64.62% branch (floor 85%/75%).
2. Canonical repo-wide artifact (`artifacts/csharp/coverage.xml`): 16.25% line / 13.60% branch (raw
   six-package aggregate, distorted because only `QuickFiler.Test` ran in this local collection).

**Citations backing this disposition:**

(a) The `human_interaction` `scope_change` entry recorded by the orchestrator in
`orchestrator-state` (session-local, gitignored checkpoint), which names this exact gap and cites
open GitHub issue #136 (*Feature: quickfiler-80-per-file-coverage*) as the tracking vehicle for
closing the `QuickFiler` package-wide floor. This remediation cycle does not reopen or duplicate
that tracked work.

(b) The `#328` `StoreWrapper` branch-floor exception precedent: in that prior cycle, a maintainer
ratified a similar pre-existing, broad, unrelated-to-the-immediate-fix branch-coverage shortfall as
an explicit, documented exception rather than requiring the fix's Scope-Lock to expand to
remediate the entire class/assembly. This remediation cycle's R2 disposition follows the same
ratification pattern: a narrow bug fix's Scope-Lock is not expanded to chase a pre-existing,
package-wide coverage floor unrelated to the two lines the fix touches.

(c) CLAUDE.md's COM/VSTO testable-denominator exemption language (General Unit Test Policy →
Coverage and Scenarios → "COM/VSTO/WinForms coverage exemption"): the 80% floor applies to the
testable denominator after excluding VSTO/WinForms/Interop-bound classes that cannot be
unit-tested without a live Outlook process or a live WinForms message loop. `QuickFiler`'s
package-wide shortfall is concentrated in exactly this category of WinForms/UI surface (per
`remediation-inputs.2026-07-20T18-00.md` item 2's own characterization: "broad, pre-existing
under-coverage across the `QuickFiler` assembly's WinForms/UI surface, unrelated to the two lines
this bug fix touches").

(d) The true all-first-party repo-wide coverage figure is measured by the PR CI full-suite run
(which exercises `Tags.Test`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, and
`QuickFiler.Test` together), not by this single-project local `dotnet-coverage` collection. The
16.25%/13.60% canonical-artifact figure recorded in this cycle is a known local-collection artifact
(only `QuickFiler.Test` ran), not the repo's true PR-gate figure. This is documented, not
cherry-picked: no assembly's evidence is omitted or excluded from the local collection's scope to
inflate the percentage.

## Effect on AC-5

This disposition, combined with the R1 code fix (P1-T2 through P1-T5, verified in Phase 2), fully
satisfies R2. No further R2 task exists in this plan. The `issue.md` AC-5 scope note (amended
2026-07-20 by orchestrator) remains the authoritative, checked-off record; this artifact is the
feature-folder evidence mirror of that same disposition for this remediation cycle's traceability.
