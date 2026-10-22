# Fail-before exception dossier: CR-2 assertion fold (remediation cycle 1, P2-T1)

Timestamp: 2026-10-06T20-36
Command: Grep token census (see r1-p2-t1-cr2-assertion-fold)
EXIT_CODE: 0

WhyFailingRunImpossible: the edit replaces a tautological assertion on an empty literal and changes the real assertion's expected operand from the literal 0 to the literal's count, which is 0; no module behaviour and no test outcome changes, so no run can fail before and pass after.

## Alternative proof

- Token census over `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`, recorded in `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p2-t1-cr2-assertion-fold.2026-10-06T20-36.md`:
  - old tautology token `@($expectedDebt).Count | Should -Be 0`: count 1 before the edit, 0 after;
  - new operand token `Should -Be $expectedDebt.Count`: count 0 before the edit, 1 after (line 315);
  - `$expectedDebt` occurrences: 2 before and 2 after (the literal at line 293 is still read);
  - It-block count 16 before and after; examined-count guard and Fizzler/Unsafe exclusion counts 1 after.
- Before-edit run: P0-T7, `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/remediation-baseline/r1-poshqc-test-baseline.2026-10-06T20-29.md`, `JUNIT-SUITE BindingRedirectVerification.Tests.ps1 tests=16 failures=0 skipped=0`.
- After-edit run: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-poshqc-test.md` (P3-T3).

Output Summary:
- No fail-before run exists for this edit by construction; this dossier records why and the census that stands in for it.
