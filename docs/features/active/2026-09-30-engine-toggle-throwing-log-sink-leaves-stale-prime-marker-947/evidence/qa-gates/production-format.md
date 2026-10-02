# QA Gate: Scoped Format of the Edited Production File (P1-T8)

Timestamp: 2026-10-01T17-53
Task: P1-T8
Command: dotnet tool run csharpier format TaskMaster\Ribbon\EngineToggleStateCoordinator.cs
EXIT_CODE: 0

Output Summary:
- CSHARPIER_EXIT_CODE: 0
- PRODUCTION-FORMAT-REWROTE: False (the two production hashes are identical)
- Partial hash identical before and after (BEB9C785...61FE), and equal to the after-hash of P1-T2: True
- PrimeFaultOrdering hash equals BASE-HASH-PFO: True
- Production hash after (EFC6F0DB...05BF) differs from BASE-HASH-PROD: True (the edit is present)
- Porcelain span prints exactly the three expected lines (below; git lists them in path order).
- The console line `Formatted 1 files` is not used as a rewrite count.
- Result: P1-T8 acceptance holds.

## Hashes (CMD-HASH)

Before:

```
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = BEB9C785536242293C3069E05BE54A355E2E030AE1AC06398C08AEAC188361FE
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB
```

After:

```
HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = BEB9C785536242293C3069E05BE54A355E2E030AE1AC06398C08AEAC188361FE
HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = AA88DC05B45CE2E0D935014778779C5B500025BCFD237AEE05A184CED7D6F8DB
```

PROD-HASH-AFTER-P1-T8: EFC6F0DB3766495223014357FEAEE8D9AF530FA5A4C73717E42454D4FB3A05BF

## Porcelain (after)

Command: git status --porcelain --untracked-files=all -- TaskMaster TaskMaster.Test

```
 M TaskMaster.Test/TaskMaster.Test.csproj
 M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
?? TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs
```

Observation: the formatted production file has 476 lines with 476 CRLF terminators.
