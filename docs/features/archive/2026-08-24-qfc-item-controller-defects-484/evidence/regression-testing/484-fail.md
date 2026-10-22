# Issue #484 — Fail-before regression run (Cleanup timer disposal and stale collaborators)

Timestamp: 2026-08-26T09-58
Task: [P4-T4] `[expect-fail]`

## Step 1 — Build the test project (not a gate; decision D2)

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0

`Platform=AnyCPU` is used for the project-level build because the standalone project defines the
`AnyCPU` platform, not the solution-level `Any CPU` alias; this matches the convention already used by
the `[P3-T4]` and `[P2-T3]` fail-before runs on this branch. This build is not an analyzer or nullable
gate (decision D2).

## Step 2 — Run the three new #484 regression tests against the unfixed production code

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Cleanup_DisposesEmailIsReadTimerBeforeNullingIt|FullyQualifiedName~ApplyReadEmailFormat_AfterCleanup_IsInertAndDoesNotSave|FullyQualifiedName~Cleanup_NullsMailActions_AndSaveParametersRebindsIt" "/Logger:trx;LogFileName=484-fail.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\484-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Results

| Test | Outcome | Failure reason |
|---|---|---|
| `Cleanup_DisposesEmailIsReadTimerBeforeNullingIt` | **Failed** | `Expected a <System.ObjectDisposedException> to be thrown, but no exception was thrown.` — `Cleanup()` nulls `_emailIsReadTimer` without disposing it, so the injected timer is still usable after teardown. |
| `ApplyReadEmailFormat_AfterCleanup_IsInertAndDoesNotSave` | **Failed** | `Did not expect any exception, but found System.NullReferenceException` thrown from `QfcItemController.ApplyReadEmailFormat(Object state)` at `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:319` — the method has no early-return guard. |
| `Cleanup_NullsMailActions_AndSaveParametersRebindsIt` | **Failed** | `Expected afterCleanup to be <null>, but found Mock<IMailItemActions:1>.Object.` — `Cleanup()` does not null `_mailActions`. |

```
Total tests: 3
     Failed: 3
Test Run Failed.
 Total time: 1.7349 Seconds
```

TRX artifact: `docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/484-fail/484-fail.trx`.

Output Summary: 3 of 3 tests Failed against the unfixed code, exactly as `[P4-T4]` expects. Each failure
names the specific missing behaviour that `[P4-T5]`, `[P4-T6]`, and `[P4-T7]` deliver: the timer is not
disposed, `ApplyReadEmailFormat` has no guard, and `_mailActions` is not released.
