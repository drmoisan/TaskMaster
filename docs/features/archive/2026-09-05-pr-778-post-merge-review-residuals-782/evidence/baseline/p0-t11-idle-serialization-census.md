# Baseline — ApplicationIdleTimer Serialization Census (P0-T11, SD7)

Timestamp: 2026-09-05T19-39

Command:

```powershell
foreach ($f in 'UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs',
               'UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs',
               'UtilitiesCS.Test/Threading/ApplicationIdleTimer_Tests.cs') {
    Get-Content -LiteralPath $f |
        Select-String -Pattern 'TestClass|DoNotParallelize|^\s*(public|internal).*class '
}
```

EXIT_CODE: 0

Output Summary:

| Test class | File | Carries `[DoNotParallelize]` | Line |
|---|---|---|---|
| `IdleActionQueue_Tests` | `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` | **No** | n/a |
| `IdleAsyncQueue_Tests` | `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | Yes | 29 |
| `ApplicationIdleTimer_Tests` | `UtilitiesCS.Test/Threading/ApplicationIdleTimer_Tests.cs` | Yes | 17 |

The matched class-declaration regions, verbatim with line numbers:

```text
UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs
  24:     [TestClass]
  25:     public class IdleActionQueue_Tests

UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
  28:     [TestClass]
  29:     [DoNotParallelize]
  30:     public class IdleAsyncQueue_Tests

UtilitiesCS.Test/Threading/ApplicationIdleTimer_Tests.cs
  16:     [TestClass]
  17:     [DoNotParallelize]
  18:     public class ApplicationIdleTimer_Tests
```

Every expectation in the acceptance condition holds exactly: `IdleAsyncQueue_Tests` carries
`[DoNotParallelize]` at line 29, `ApplicationIdleTimer_Tests` carries it at line 17, and
`IdleActionQueue_Tests` does not carry it, with its `[TestClass]` at line 24 and its class
declaration at line 25.

## Justification this census supplies

`IdleActionQueue_Tests` is the only one of the three classes sharing `ApplicationIdleTimer`
process-global state that is not serialized. The `[TestCleanup]` that P4-T1 adds calls
`ApplicationIdleTimer.Unsubscribe`, which calls `Stop()` when the invocation list empties, touching
process-global `System.Windows.Forms.Application.Idle` and `ApplicationIdleTimer.Guard` state shared
with the two sibling classes. Adding the cleanup without also adding `[DoNotParallelize]` would let
that global mutation run concurrently with the siblings' tests. This finding is the stated
justification for the SD7 attribute addition in P4-T1 and is repeated in the code-review artifact.
