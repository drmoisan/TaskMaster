# P5-T12 AC12 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC12 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifacts
EXIT_CODE: 0
Output Summary: AC12 met and checked off. After the P4-T6 Rebuild, the five gated BIN lines read `System.Linq.AsyncEnumerable.dll=True`. The P0-T18 negative control read False for all five.

Artifacts read:
- evidence/qa-gates/system-linq-asyncenumerable-bin-presence.md (EXIT_CODE 0): `BIN UtilitiesCS|QuickFiler|ToDoModel|TaskMaster|UtilitiesCS.Test System.Linq.AsyncEnumerable.dll=True System.Linq.Async.dll=True`.
- evidence/baseline/p0-t18-bin-absence.2026-10-03T10-52.md: the same five projects read `System.Linq.AsyncEnumerable.dll=False`.

SPEC-LINE: `- [x] AC12 (assembly deployed).` (criterion text unchanged)
