# Determinism Tokens (P3-T11)

Timestamp: 2026-09-30T15-15
Command: git diff -U0 ANCHOR-SHA -- TaskMaster.Test/Ribbon (added lines only) with the AC13 token list; git diff --name-only ANCHOR-SHA -- TaskMaster.Test/Ribbon; git status --porcelain -- TaskMaster.Test/Ribbon (ANCHOR-SHA b305903e275b8abf58e8e65831c189f517568fe4, kept per the Phase 3 pass-2 anchor paragraph)
EXIT_CODE: 0
Output Summary: ADDED-LINE-COUNT: 175 (at least 150); ADDED-FILES: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs (exactly the new partial); every ADDED-TOKEN count is 0 (23 tokens); the porcelain span printed no line. Every P3-T11 clause holds. The fresh-harness, strict-mock and FluentAssertions clauses of AC13 are read from the PASS-2: copy of the POST-FORMAT: section of evidence/regression-testing/prime-registration-partial-tokens.md (`var harness = new Harness();` 3, `new Mock<` 0, `MockBehavior` 0, `.ContainSingle(` 3, `.Should()` 13) and the P0-T8 strict-mock row in evidence/baseline/anchor-test-side.md (`new Mock<IAppItemEngines>(MockBehavior.Strict)` 1). Substitution: interpolated output strings rewritten as string concatenation; the git commands and token list are unchanged.

## Details

```
ADDED-LINE-COUNT: 175
ADDED-FILES: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs
ADDED-TOKEN [Thread.Sleep] = 0
ADDED-TOKEN [Task.Delay] = 0
ADDED-TOKEN [SpinWait] = 0
ADDED-TOKEN [while (] = 0
ADDED-TOKEN [for (] = 0
ADDED-TOKEN [Retry] = 0
ADDED-TOKEN [DoNotParallelize] = 0
ADDED-TOKEN [Parallelize] = 0
ADDED-TOKEN [[Timeout] = 0
ADDED-TOKEN [Timeout=] = 0
ADDED-TOKEN [DateTime] = 0
ADDED-TOKEN [Stopwatch] = 0
ADDED-TOKEN [Environment.TickCount] = 0
ADDED-TOKEN [.Wait(] = 0
ADDED-TOKEN [.Result] = 0
ADDED-TOKEN [GetResult(] = 0
ADDED-TOKEN [ManualResetEvent] = 0
ADDED-TOKEN [SemaphoreSlim] = 0
ADDED-TOKEN [GetTempFileName] = 0
ADDED-TOKEN [GetTempPath] = 0
ADDED-TOKEN [File.] = 0
ADDED-TOKEN [TaskScheduler] = 0
ADDED-TOKEN [TimeProvider] = 0
```

Porcelain span (git status --porcelain -- TaskMaster.Test/Ribbon), verbatim: (no line)
