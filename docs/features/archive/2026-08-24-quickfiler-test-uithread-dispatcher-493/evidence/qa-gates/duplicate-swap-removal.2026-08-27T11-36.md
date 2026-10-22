# Duplicate #230 Workaround Removal Audit (P4-T4)

Timestamp: 2026-08-27T11-36
Task: [P4-T4]
Command: `Select-String -SimpleMatch` for three patterns against the paths named per row (commands quoted in full per row below)
EXIT_CODE: 0
Output Summary: All three matrix rows hold. `UiThreadDispatcherGate` and `SwapUiThreadDispatcher`
each return 0 matches in `QfcItemController.InitializationTests.Part2.cs`, and `typeof(UiThread)`
returns 0 matches in each of the three named paths. The only in-scope file holding the reflection
swap is `QfcItemController.UiThreadDispatcherFixture.cs`, which `P1-T1` asserts holds it (1 match).

## Row 1

Command:
`Select-String -SimpleMatch -Pattern 'UiThreadDispatcherGate' -Path 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs'`

| Target path | Match count | Required |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 0 | 0 |

The file's own private `SemaphoreSlim UiThreadDispatcherGate` and its 15-line doc block are gone. The
replacement rationale comment `P2-T2` inserted deliberately avoids the identifier, so it cannot
defeat this row.

## Row 2

Command:
`Select-String -SimpleMatch -Pattern 'SwapUiThreadDispatcher' -Path 'QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs'`

| Target path | Match count | Required |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 0 | 0 |

The private `SwapUiThreadDispatcher` method and its doc block are gone, along with both call sites
(the install in `BuildPumpHarnessCoreAsync` and the restore in `PumpHarness.Restore`).

## Row 3

Command:
`Select-String -SimpleMatch -Pattern 'typeof(UiThread)' -Path <each path below>`

| Target path | Match count | Required |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 0 | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 0 | 0 |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | 0 | 0 |

## Uniqueness of the reflection swap

Row 3 establishes that none of the three files above performs the reflection lookup. Combined with
`P1-T1`'s recorded result of exactly **1** match for `typeof(UiThread)` in
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` — re-verified after the
`P3-T1` formatter pass and recorded in
`<FEATURE>/evidence/qa-gates/csharpier-format.2026-08-27T11-08.md` — exactly one implementation of
the reflection swap exists across this feature's in-scope files, which is what AC-4 requires.

The remaining ungated mutator in the assembly,
`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, is outside this feature's owned set and is
accepted residual risk R-1 in spec § Risks. `P5-T12` promotes it as its own follow-up.
