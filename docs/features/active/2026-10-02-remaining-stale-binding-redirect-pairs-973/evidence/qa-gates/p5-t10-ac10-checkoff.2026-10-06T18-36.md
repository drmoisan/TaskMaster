# P5-T10 AC10 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC10 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifacts and of the test file's current diff at PLAN-START-HEAD
EXIT_CODE: 0
Output Summary: AC10 met and checked off. It (b) is present. It matches each `System.Linq.AsyncEnumerable,` Reference through its closing tag in the raw text, asserts the Aliases child on each, and asserts the exact project set `QuickFiler,TaskMaster,ToDoModel,UtilitiesCS,UtilitiesCS.Test`. It was red in the AC1 run and green in the AC3 run.

Artifacts read:
- evidence/regression-testing/p1-t4-alias-guard.2026-10-03T10-58.md (EXIT_CODE 0): It (b) inserted; 0 parse errors.
- evidence/regression-testing/binding-redirect-gate-fail-before.md: It (b) JUNIT-NOTPASSED with the `Expected: 'QuickFiler,TaskMaster,ToDoModel,UtilitiesCS,UtilitiesCS.Test'` / `But was:  ''` message.
- evidence/regression-testing/binding-redirect-gate-pass-after.md: suite 16/0 (It (b) passes).
- Current test file diff (P4-T12 capture): `$expectedProject = 'QuickFiler,TaskMaster,ToDoModel,UtilitiesCS,UtilitiesCS.Test'`, the pattern `(?s)<Reference Include="System\.Linq\.AsyncEnumerable,[^>]*?(/>|>.*?</Reference>)`, `$withoutAlias.Count | Should -Be 0` and `($carrier -join ',') | Should -BeExactly $expectedProject`.

SPEC-LINE: `- [x] AC10 (alias durability guard).` (criterion text unchanged)
