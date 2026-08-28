# GREEN AFTER THE FIX — #500 Hub Half and #501 (P5-T7)

Timestamp: 2026-08-27T20-47

## Step 1 — analyzer build

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: `5 Warning(s)`, `0 Error(s)`, `Time Elapsed 00:00:19.57`.
Count of lines matching `Skipping target "CoreCompile"`: **0** — the gate is non-vacuous.

## Step 2 — scoped test run

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~PostJson_SurfaceFailureDoesNotStarveOtherSurfacesOrFalsifyReplayCache|FullyQualifiedName~PostJson_SurfaceInvocationRunsAfterHubLockIsReleased|FullyQualifiedName~PostJson_ReentrantAttachFromSurfaceDoesNotThrowCollectionModified'
    '/Logger:trx;LogFileName=p5-t7.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p5-t7'
```

EXIT_CODE: 0

Output Summary:

```
  Passed PostJson_SurfaceFailureDoesNotStarveOtherSurfacesOrFalsifyReplayCache [72 ms]
  Passed PostJson_SurfaceInvocationRunsAfterHubLockIsReleased [170 ms]
  Passed PostJson_ReentrantAttachFromSurfaceDoesNotThrowCollectionModified [4 ms]

Test Run Successful.
Total tests: 3
     Passed: 3
```

| Metric | Value |
| --- | ---: |
| Total | 3 |
| Passed | 3 |
| Failed | 0 |
| Skipped | 0 |

## Red-to-green transition — three tests, one edit

| Test | Before | After | Invariants | AC |
| --- | --- | --- | --- | --- |
| `PostJson_SurfaceFailureDoesNotStarveOtherSurfacesOrFalsifyReplayCache` | FAILED — propagated `InvalidOperationException` (P5-T2) | PASSED | I-501.1, I-501.2, I-501.3, SR-3 | AC-08, AC-09, AC-10, AC-11 (containment half) |
| `PostJson_SurfaceInvocationRunsAfterHubLockIsReleased` | FAILED — probe observed `True` (P5-T5) | PASSED | I-500.2 | AC-05 |
| `PostJson_ReentrantAttachFromSurfaceDoesNotThrowCollectionModified` | FAILED — `Collection was modified` (P5-T5) | PASSED | I-500.4 | AC-07 |

All three moved from RED to GREEN off a **single** rewrite of `PostJson`, which is the point the
research document's section 8.2 makes: the #500 hub narrowing and the #501 broadcast containment are one
change, not two. Containing the throw inside the existing lock would have left the lock probe RED;
narrowing the lock without containment would have left the starvation test RED. The combined shape —
cache and snapshot under the lock, contained per-surface broadcast outside it — satisfies all three
simultaneously.

TRX artifact: `FF/evidence/regression-testing/trx/p5-t7/p5-t7.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0` with 3 passed, 0 failed, 0 skipped. PASS.
