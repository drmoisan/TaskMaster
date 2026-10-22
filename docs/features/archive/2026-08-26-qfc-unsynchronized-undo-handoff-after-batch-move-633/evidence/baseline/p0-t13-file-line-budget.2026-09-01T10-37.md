# In-scope file line budget (P0-T13)

Timestamp: 2026-09-01T10-37
Task: [P0-T13]
Working directory: WORKTREE

Command: `(Get-Content -LiteralPath <path>).Count` for each of the six in-scope files.
EXIT_CODE: 0

`Measure-Object -Line` was deliberately not used: it counts lines differently from `Get-Content` on a
file whose final line carries no terminator, and every size gate in this plan is written against the
`Get-Content` count.

| File | Lines | Headroom to 500 |
|---|---|---|
| `QuickFiler/Controllers/FilerQueue.cs` | 83 | 417 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 399 | 101 |
| `QuickFiler.Test/Controllers/FilerQueueTests.cs` | 89 | 411 |
| `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 436 | 64 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 512 | -12 |
| `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` | 0 (does not yet exist) | 500 |

Six numeric counts recorded.
`QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` shows **64** lines of headroom,
which is the value the acceptance condition names.

## Agreement with the plan's verified tree facts

Every count reproduces the plan's "Verified tree facts" table exactly: `FilerQueue.cs` 83,
`QfcFormController.EventHandlers.cs` 399, `FilerQueueTests.cs` 89, `SeamFactoryTests.cs` 436. No
citation drifted between plan authoring and execution, including across the `origin/main` merge at
06b1e02e that preceded this run.

## Note on the project file

`QuickFiler.Test/QuickFiler.Test.csproj` is already 512 lines and therefore already over the 500-line
figure, before this change touches it. This is pre-existing and is recorded rather than acted on. The
500-line limit in `.claude/rules/general-code-change.md` is stated for production code, test code, and
reusable script files; an MSBuild project file is none of those. P2-T1 adds exactly one
`<Compile Include>` line, taking it to 513. No acceptance criterion in `spec.md` constrains this file's
size: AC18 constrains only the two changed production files, and the P6-T6 test-file size gate names
only the three test `.cs` files. Reducing the project file is outside the authorized blast radius.

Output Summary: Baseline sizes captured for all six in-scope files. The two production files have 417
and 101 lines of headroom, so the AC18 under-500 condition has ample margin. The tightest constraint in
the change is `QfcItemController.SeamFactoryTests.cs` at 436 lines with 64 to spare; P3-T8 removes lines
from that file rather than adding a net increase, so the margin is not expected to be consumed. The new
test file starts from zero against a 500-line ceiling and must hold five test methods plus a fixture.
