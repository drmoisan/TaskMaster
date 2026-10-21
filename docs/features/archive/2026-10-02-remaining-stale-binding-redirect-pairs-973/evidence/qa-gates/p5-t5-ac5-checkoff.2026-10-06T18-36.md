# P5-T5 AC5 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC5 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifacts and of the test file's current diff at PLAN-START-HEAD
EXIT_CODE: 0
Output Summary: AC5 (as amended, D1) met and checked off. It (a) is present with the 16 corrected names, the ConvertTo-ReferenceVersionMap clause, the record-count guard and the all-names-observed guard. It was red in the AC1 run (`but got 152.`) and green in the AC3 run.

Artifacts read:
- evidence/baseline/p0-t3-spec-verification.2026-10-03T10-42.md (EXIT_CODE 0): the amended AC5 text was verified at Phase 0.
- evidence/regression-testing/p1-t3-range-guard.2026-10-03T10-58.md (EXIT_CODE 0): It (a) inserted, 0 parse errors.
- evidence/regression-testing/binding-redirect-gate-fail-before.md: It (a) JUNIT-NOTPASSED with message ending `, but got 152.`.
- evidence/regression-testing/binding-redirect-gate-pass-after.md: suite 16/0 (It (a) passes).
- Current test file diff (P4-T12 capture): the It titled 'bounds every corrected redirect range at its newVersion across the repository app.config files' carries the 16-name `$correctedName` list, `ConvertTo-ReferenceVersionMap` over every root-level csproj, `$record.Count | Should -BeGreaterThan 0` and `$missingName.Count | Should -Be 0`.

SPEC-LINE: `- [x] AC5 (range guard).` (criterion text unchanged)
