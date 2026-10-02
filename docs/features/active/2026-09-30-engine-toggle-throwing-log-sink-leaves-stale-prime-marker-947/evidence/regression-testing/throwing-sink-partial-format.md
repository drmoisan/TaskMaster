# Regression Testing: Scoped Format of the New Partial (P1-T2)

Timestamp: 2026-10-01T17-48
Task: P1-T2
Command: dotnet tool run csharpier format TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs
EXIT_CODE: 0

Output Summary:
- CSHARPIER_EXIT_CODE: 0
- PARTIAL-FORMAT-REWROTE: True (the two partial hashes differ)
- Cause of the rewrite, observed: the file was written with LF line endings and the formatter emitted CRLF; after the format the file has 215 lines and 215 CRLF terminators. The line count (215) equals the delivered text, so no statement was re-laid out.
- Production hash before and after equals BASE-HASH-PROD: True
- PrimeFaultOrdering hash before and after equals BASE-HASH-PFO: True
- Porcelain span (TaskMaster, TaskMaster.Test) prints exactly one line: `?? TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs`
- The console line `Formatted 1 files` is not used as a rewrite count.
- Result: P1-T2 acceptance holds.

## Hashes (CMD-HASH)

Before:

```
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = 7DDDEB5BBD0D9C7A55FFD44661C5632F61CC3AD78418CD90D64169ADC3BBBBCA
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB
```

After:

```
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = BEB9C785536242293C3069E05BE54A355E2E030AE1AC06398C08AEAC188361FE
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB
```

PARTIAL-HASH-AFTER-P1-T2: BEB9C785536242293C3069E05BE54A355E2E030AE1AC06398C08AEAC188361FE

## Porcelain (after)

Command: git status --porcelain --untracked-files=all -- TaskMaster TaskMaster.Test

```
?? TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs
```
