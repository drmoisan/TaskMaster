# [P5-T2] [expect-fail] Pre-fix red state for issue #473 defect 2 — one root failure, two log entries

Timestamp: 2026-08-26T10-27

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime" `
    /Logger:"trx;LogFileName=p5-t2.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p5-t2
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

```
Failed MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime

Test Run Failed.
Total tests: 1
     Failed: 1
 Total time: 2.0901 Seconds
```

TRX `<Counters>`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Recorded failure message:

```
Moq.MockException: issue #473 defect 2 requires the catch to log once and return rather than
dereferencing the same failed group a second time to look up its subject
Expected invocation on the mock should never have been performed, but was 1 times: mail => mail.Subject

Performed invocations:

   Mock<MailItem:1> (mail):

      _MailItem.Subject
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| Failed count in the `p5-t2` TRX | exactly 1 | **1** |
| Exit code | non-zero, declared `ExpectedExitCode: 1` | **1** |

`but was 1 times` is the defect made observable. The `NotThrowAsync` assertion that precedes the
`VerifyGet` **passed**, so the batch did complete — the recorded failure is the second-dereference
assertion alone. Execution path before the fix: `group.ItemController` is `null`, so
`await group.ItemController.MoveMailAsync()` raises `NullReferenceException`; the broad
`catch (System.Exception)` handles it and then evaluates `group.MailItem.Subject` **inside that same
catch**, which raises a second exception into the nested `catch (System.Exception e2)`; both catches
emit a `logger.Error`, so one root cause produces two misleading entries.

Host-identifier sanitisation was applied to the committed TRX exactly as recorded in the P2-T6
artifact. A post-substitution scan for the bare account name, the machine name in either casing, the
workspace absolute path, and the user-profile path returns zero hits.

Result: PASS (expected failure observed).
