# RED — #501 Starvation, Containment and Cache Truthfulness (P5-T2) [expect-fail]

Timestamp: 2026-08-27T20-40

ExpectedExitCode: 1

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~PostJson_SurfaceFailureDoesNotStarveOtherSurfacesOrFalsifyReplayCache'
    '/Logger:trx;LogFileName=p5-t2.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p5-t2'
```

The test project was rebuilt (`/t:Rebuild`, `BUILD_EXIT=0`, zero compiler errors) immediately before
this run.

EXIT_CODE: 1

Output Summary:

```
  Failed PostJson_SurfaceFailureDoesNotStarveOtherSurfacesOrFalsifyReplayCache [176 ms]
Total tests: 1
     Failed: 1
Test Run Failed.
```

| Metric | Value |
| --- | ---: |
| Tests run | 1 |
| Failed | 1 |
| Passed | 0 |

The observed exit code equals the declared `ExpectedExitCode`, so this gate is a PASS: a failing test
is the intended outcome of this task, and only of this task.

## Verbatim failure text

```
Did not expect any exception because PostJson must not propagate a surface throw to its caller, but found System.InvalidOperationException: Surface delivery rejected
```

This is the **propagated surface exception** form of RED evidence, which P5-T2's acceptance admits as an
alternative to an observed attempt count of 1. The containment assertion is the first assertion in the
test, so it fails before the attempt-count assertion is evaluated; on the pre-fix code both would fail.

The mechanism is `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:126-135`: the `foreach` over
`_attachments.Values` at `:131` wraps `PostToSurface` at `:133` in no `try`/`catch`, so the first
surface throw propagates out of `PostJson` AND aborts the loop, starving every attachment later in
enumeration order while `CacheState` at `:130` has already recorded the message as delivered.

## Order-independence of the pre-fix result

Both throwing surfaces increment the shared attempt counter BEFORE throwing, so on the pre-fix code the
total is 1 in every `Dictionary.Values` enumeration order: whichever throwing surface is reached first
increments once and then aborts the loop. The expected post-fix total is 2 in every order. The
assertion therefore cannot pass vacuously, which is the risk `spec.md`'s edge-case list and AC-08
explicitly call out.

TRX artifact: `FF/evidence/regression-testing/trx/p5-t2/p5-t2.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: 1 run, 1 failed, 0 passed, and the failure text shows either an observed attempt count of 1
or the propagated surface exception (the latter is what was observed). PASS.
