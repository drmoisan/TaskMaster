---
name: inherited-ac-from-upstream-sibling
description: When an upstream epic sibling's diff already satisfies a promoted acceptance criterion, write it as inherited-and-verified (verify the site is ABSENT) rather than dropping it or restating it as this feature's work
metadata:
  type: feedback
---

When a feature branches from an epic integration branch that already carries a sibling's merged
change, some of its own promoted acceptance criteria may already be satisfied. Do not silently drop
them, and do not restate them as work items. Write each as an **inherited-and-verified** criterion:
cite the upstream task ID and decision ID that satisfied it, and phrase the check so the executor
verifies the offending code site is **absent** — never "recreate then remove".

**Why:** In `quickfiler-keyboard-action-defects-444` (2026-08-24), upstream #468 `[P1-T2]` deleted
`WireUpKeyboardHandler`, which contained the duplicate `("Collection", Keys.Down)` registration that
#444's promoted document asked to resolve. Copying that criterion verbatim would have made it
unsatisfiable-by-construction (the named code will not exist); dropping it would have lost the audit
trail. #468's own decision `D2` explicitly handed the remainder to #444.

**How to apply:** For every epic child, diff the promoted document's ACs against what the upstream
sibling's committed plan actually deletes or changes. Put the disposition in a small table in
`## Repro & Evidence` (promoted criterion → disposition), then emit the inherited items in
`## Acceptance Criteria` prefixed `**(Inherited from #NNN — verify, do not re-perform.)**` with a
concrete falsifier (for example, a repo-wide identifier search returning zero hits).

Related: a promoted document's stated *coupling* between criteria can also dissolve when the upstream
change removes the only call site the coupling protected — say so explicitly so a reviewer does not
re-impose it. See [[ac-gates-verify-satisfiability]] and [[full-bug-spec-only]].
