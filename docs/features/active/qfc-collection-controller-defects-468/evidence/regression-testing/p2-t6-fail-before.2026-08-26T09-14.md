# [P2-T6] [expect-fail] Pre-fix red state for issue #474 defect 1

Timestamp: 2026-08-26T09-14

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build     # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~ParentFieldAndConstructorParameterAreTypedIQfcFormController" `
    /Logger:"trx;LogFileName=p2-t6.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p2-t6
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

```
Results File: <repo-root>\docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p2-t6\p2-t6.trx

Test Run Failed.
Total tests: 1
     Failed: 1
 Total time: 1.4722 Seconds
```

TRX `<Counters>`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Recorded failure message (verbatim from the TRX `<ErrorInfo><Message>`):

```
Expected fieldTypeName to be a match with the expectation because issue #474 defect 1 requires the
_parent field to be declared as QuickFiler.Controllers.IQfcFormController so the runtime downcast to
the internal concrete QfcFormController is removed, but it differs at index 11:
            ↓ (actual)
"…uickFiler.Interfaces.IFilerFormController"
"…uickFiler.Controllers.IQfcFormController"
            ↑ (expected)
```

### Acceptance verification

| Condition | Required | Measured |
|---|---|---|
| TRX count in `evidence/regression-testing/p2-t6` | exactly 1 | **1** (`p2-t6.trx`) |
| Failed count for the named test | exactly 1 | **1** |
| Failure message names the observed type | `IFilerFormController` | present: the actual value is `QuickFiler.Interfaces.IFilerFormController` |
| Exit code | non-zero, declared as `ExpectedExitCode: 1` | **1** |

This is a genuine pre-fix red state. The fix (P2-T7) has not been applied: at the time of this run
`QuickFiler/Controllers/QfcCollectionController.cs` still declares `IFilerFormController parent` at
`:35`, `private IFilerFormController _parent;` at `:64`, and
`await ((QfcFormController)_parent).SkipGroupAsync();` at `:1025`.

### Host-identifier sanitisation applied before committing the TRX

vstest embeds the operator account and machine name in a TRX regardless of `LogFileName=`. The
following substitutions were applied to `p2-t6.trx` after the run and before it was staged:

| Attribute / content | Original form | Committed form |
|---|---|---|
| `<TestRun name=...>` | `<user>@<host> 2026-08-26 09:14:15` | account and machine replaced with `<user>` and `<host>` |
| `<TestRun runUser=...>` | `<host>\<user>` | `<host>\<user>` |
| `<Deployment runDeploymentRoot=...>` | `<user>_<host>_2026-08-26_09_14_15` | `<user>_<host>_...` |
| `computerName` on every `<UnitTestResult>` | the machine name | `<host>` |
| Absolute paths in `<TestRun>`, `<Deployment>`, `codeBase`, `storage`, and the stack trace | the workspace root | `<repo-root>` |
| Any remaining user-profile path prefix | the profile directory | `<user-profile>` |

`/InIsolation` also created an empty deployment scratch directory whose name embedded the account
and machine name (`Deploy_<user> 20260826T091415_58560`, containing only empty `In/<host>` and `Out`
subdirectories and no files). It was removed after the run. It carried no evidence.

A post-substitution scan of the committed TRX for the bare account name, the machine name (either
casing), the workspace absolute path, and the user-profile path returns zero hits.

Result: PASS (expected failure observed).
