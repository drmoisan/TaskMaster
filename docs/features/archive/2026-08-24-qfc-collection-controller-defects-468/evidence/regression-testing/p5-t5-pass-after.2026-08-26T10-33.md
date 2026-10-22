# [P5-T5] Post-fix green state for issue #473 defect 2

Timestamp: 2026-08-26T10-33

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException|FullyQualifiedName~MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime|FullyQualifiedName~MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow" `
    /Logger:"trx;LogFileName=p5-t5.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p5-t5
```

The three clauses are joined with `|`; vstest 18.8.0 rejects the `OR` keyword.

EXIT_CODE: 0

## Output Summary

```
Passed MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException [199 ms]
Passed MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime [44 ms]
Passed MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow [< 1 ms]

Test Run Successful.
Total tests: 3
     Passed: 3
 Total time: 1.6154 Seconds
```

TRX `<Counters>`:

```
total="3" executed="3" passed="3" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | **0** |
| Passed count | exactly 3 | **3** |
| Failed count | exactly 0 | **0** |

### Fail-before / pass-after pairing

| Test | Red evidence | Green evidence |
|---|---|---|
| `MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException` | P5-T1, failed 1, "but no exception was thrown" | this run, passed |
| `MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime` | P5-T2, failed 1, "should never have been performed, but was 1 times" | this run, passed |
| `MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow` | none — this test covers the new guarded path introduced by P5-T3 and did not exist before it | this run, passed |

`MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow` is added by P5-T4 after the fix, per the
plan, and carries no fail-before artifact because the plan does not tag it `[expect-fail]`: it
exercises the true null-group path through the boundary guard, which before the fix was contained by
the broad catch rather than throwing, so it would have passed for the wrong reason.

### Production change that closes the two red states

`TryMoveEmailByGroupAsync` now reads, comments elided:

```csharp
private static async Task TryMoveEmailByGroupAsync(QfcItemGroup group)
{
    try
    {
        await group.ItemController.MoveMailAsync();
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (System.Exception e)
    {
        logger.Error($"Error moving message. Continuing execution.\n{e.Message}", e);
        return;
    }
}
```

Structural verification against the P5-T3 acceptance conditions:

| Condition | Measured |
|---|---|
| Cancellation clause declared before the broad clause | true (index of `catch (OperationCanceledException)` precedes `catch (System.Exception e)`) |
| No statement in the method executes after the broad catch other than the return | the broad catch block ends with `return;`, and the only text following that block is the method's closing brace |
| Possibly-null group guarded at the `TryMoveEmailByGroupIndexAsync` boundary | that method now returns after a single `logger.Error` when `TryGetItemGroupByIndex` yields `null` |
| Catch clauses in the method body | **2** (down from 3: the nested subject-lookup catch is gone) |
| `group.MailItem.Subject` present in the method body | **false** |

Log-and-proceed is retained per D5: a failed move for one message still logs and lets the batch
continue. The change is the *number* of entries (two per root failure to one) and the exclusion of
cancellation from the error path.

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS.
