# Shared Fixture File Created (P1-T1)

Timestamp: 2026-08-27T10-32
Task: [P1-T1]
Command: `Select-String -SimpleMatch -Pattern 'typeof(UiThread)' -Path 'QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs'` and a line count of the same path
EXIT_CODE: 0
Output Summary: The new file exists at
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`. The
`typeof(UiThread)` search returns 1 match, satisfying the "at least one match" condition. The file
measures 278 lines, which is within the 500-line ceiling.

## Acceptance verification

| Check | Value |
| --- | --- |
| File exists at the stated path | yes |
| `typeof(UiThread)` match count | 1 |
| Line count | 278 |

## Contents delivered, against § Fixture Contract

- Namespace `QuickFiler.Controllers.Tests`.
- Using directives: `System`, `System.Reflection`, `System.Threading`, `System.Threading.Tasks`,
  `System.Windows.Threading`, `FluentAssertions`, `UtilitiesCS`. `UtilitiesCS` and **not**
  `UtilitiesCS.Threading` is the namespace that declares `UiThread`, so the folder-implied spelling
  was not used. `UtilitiesCS.Threading` is not referenced because this file uses neither
  `IUiDispatcher` nor `WpfUiDispatcher`.
- `internal static class UiThreadDispatcherFixture` with all five field declarations carrying an
  initializer at the declaration: `FieldLock`, `TransactionGate`, `ParkedDispatcherLock`,
  `DispatcherField`, and `_parkedDispatcher`. No field is declared without one, so
  `/p:TreatWarningsAsErrors=true` cannot promote `CS0649` or `CS0169` to an error.
- Members: `Current` (get-only property with an explicit block-bodied accessor holding `FieldLock`,
  not an auto-property), `Exchange`, `CompareExchange`, `ReleaseTransactionGate`, `EnsureDispatcher`,
  `BeginTransactionAsync`, `ResolveDispatcherField`, `GetParkedDispatcher`, and the nested
  `private sealed class EnsureScope : IDisposable` whose disposer is declared `public void Dispose()`.
- `internal sealed class UiThreadDispatcherTransaction : IDisposable` with the four instance fields
  `_previous`, `_installedValue`, `_hasInstalled`, `_disposed`, all four definitely assigned in the
  single constructor `internal UiThreadDispatcherTransaction()`. `Install` throws
  `InvalidOperationException` on a second call; `Dispose` restores strictly before releasing the gate
  and is idempotent.
- `EnsureDispatcher` calls `GetParkedDispatcher()` **before** taking `FieldLock`, and never touches
  `TransactionGate`.
- No `init` accessor, no `record`, and no `record struct`, per the net481 constraint.

## Renames applied

The three members reproduced from `QfcItemController.TestSupport.cs` were renamed as the task
requires. This task writes only the new file; it does not delete the originals, which `P2-T1` does.

| Original in `QfcItemController.TestSupport.cs` | Name in the new fixture |
| --- | --- |
| `_dedicatedDispatcher` | `_parkedDispatcher` |
| `_dedicatedDispatcherLock` | `ParkedDispatcherLock` |
| `GetDedicatedDispatcher` | `GetParkedDispatcher` |

The parked STA background thread is renamed from
`QfcItemControllerTestSupport.ParkedDispatcher` to `UiThreadDispatcherFixture.ParkedDispatcher`.

`StartRunningDispatcher` and `ShutdownDispatcher` were **left in** `QfcItemControllerTestSupport` and
are not reproduced here, because three unowned test files call them: `WpfUiDispatcherTests.cs`,
`QfcItemController.FolderHandlingTests.cs`, and `QfcItemController.ViewerSetupTests.cs`.

## Files not touched by this task

`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` was read but not edited, so the
absolute line citations that `P0-T14` and `P2-T1` rely on remain valid, and `P1-T4` observes a tree
whose only source-naming compile errors are the ones in the new regression-test file.
