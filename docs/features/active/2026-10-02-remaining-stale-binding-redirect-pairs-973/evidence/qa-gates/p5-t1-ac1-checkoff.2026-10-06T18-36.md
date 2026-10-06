# P5-T1 AC1 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC1 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read and Grep over the named artifacts
EXIT_CODE: 0
Output Summary: AC1 met and checked off. The fail-before run was taken after the test edits and before any config, manifest or csproj edit. It exited 1 with ExpectedExitCode 1, and the main It failed listing exactly the 15 stale pairs.

Artifacts read:
- evidence/regression-testing/p1-t1-literals.2026-10-03T10-58.md (EXIT_CODE 0): both literals replaced (`$expectedDebt = @()`, `$expectedUnverifiable = @('netstandard')`).
- evidence/regression-testing/p1-t2-assertions.2026-10-03T10-58.md (EXIT_CODE 0): `$actualDebt.Count | Should -Be 0` with a -Because joining the observed pairs; the examined-count guard and the Fizzler/Unsafe exclusion are intact.
- evidence/regression-testing/binding-redirect-gate-fail-before.md: EXIT_CODE 1, ExpectedExitCode 1; OBSERVED-PAIRS lists the 15 section 9 pairs with no 16th entry; the main It message ends `, but got 15.`.

Clauses verified: literals; count assertion with -Because; guard and exclusion unchanged; red run before any config, manifest or csproj edit; failure text lists the 15 pairs.
SPEC-LINE: `- [x] AC1 (regression test, red first).` (criterion text unchanged)
