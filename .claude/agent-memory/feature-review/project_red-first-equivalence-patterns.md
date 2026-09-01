---
name: red-first-equivalence-patterns
description: What counts as valid RED-first (test-before-fix) evidence across several reviewed PRs, and one trap where a "corrected" test was actually defect-neutral
metadata:
  type: project
---

Consolidated from individual review-residuals findings:

- **#489**: A RED TRX (failing test result) committed in one commit, with the fix landing in the next
  commit, is provable RED-first evidence — the failing run is on disk, not just narrated.
- **#677**: A compile-red state (the new test doesn't compile until the fix lands) is an equally valid
  RED-first equivalent to a runtime-red test — don't require a runtime failure specifically.
- **#440**: A "corrected" defect-encoding test can be defect-NEUTRAL. Before crediting a test with
  proving a defect existed, check that it actually failed (Totals show a failure) before the fix —
  some "regression tests" pass both before and after because the encoded defect was never reachable
  by the test as written.
- **#680**: A fresh `vstest` TRX is born unsanitized (host tokens, absolute paths). This recurred three
  times in one review cycle set, including in the QA agent's own freshly-generated output. Sanitize in
  the same task that generates the TRX, every cycle — don't assume a prior sanitize pass covers a new
  run.

**How to apply:** When verifying a bugfix's RED-first claim, look for either a genuinely failing test
run artifact or a compile failure prior to the fix commit, not just a plan's narrative claim. When a
"regression test" is presented as proof a bug existed, check its pre-fix Totals/pass-fail line, not
just its presence.
