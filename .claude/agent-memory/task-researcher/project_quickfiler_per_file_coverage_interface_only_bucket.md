---
name: quickfiler-per-file-coverage-interface-only-bucket
description: Epic #136 per-file coverage — recommended a third ledger bucket "interface-only" distinct from ratified-exempt, plus a 0/0 harness requirement on F1's per-file report.
metadata:
  type: project
---

For epic `quickfiler-per-file-coverage` (#136), F3 research (#430, 2026-08-07) recommended that F1's
`coverage-ledger.md` carry **three** buckets, not two: `testable`, `ratified-exempt`, and a third
**`interface-only — zero executable lines — not in the denominator`**.

**Why:** `.claude/rules/general-unit-test.md:29` treats interface-only files as a *measurement-scope
clarification*, whereas `CLAUDE.md` § UT2 `ratified-exempt` means "an irreducible untestable remainder
was accepted after a refactor attempt." Filing ~24 QuickFiler interface files as `ratified-exempt`
would misrecord "nothing to cover" as "an accepted defeat" and inflate the epic's exempt count against
its own leading indicator (`epic.md:14`).

Two harness requirements were raised against F1's upstream contract at the same time:
1. A file that emits **no `<class>` element** in the Cobertura report must be reported `N/A`, never
   `0%` — otherwise ~24 interface files become permanently-failing false gate failures, and silently
   dropping them makes F16's "all 121 files accounted for" check unverifiable.
2. The harness must attribute results by the `<class>` element's `filename` attribute, not by type-name
   substring match. Interface type names appear in the report only inside consumers' `signature`
   attributes (e.g. `QuickFiler.IItemControler` in `ItemViewer`'s `set_Controller`), so name matching
   mis-attributes consumer lines to the interface file.

**How to apply:** if a later session reads or authors F1's ledger or harness, check these three points
first. If F1 shipped only two buckets, the correct move is to record a dissent note in the child's
`spec.md`, not to fabricate tests for a 0/0 file. See
[[feedback-exemption-audit-check-proven-techniques]] for the related rule that exemption claims must be
verified against already-proven test techniques.
