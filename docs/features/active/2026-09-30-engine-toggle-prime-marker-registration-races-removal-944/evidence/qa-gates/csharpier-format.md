# CSharpier Format, Repository-Wide (P3-T1)

Timestamp: 2026-09-30T13-43
Command: dotnet tool run csharpier format . (between CMD-HASH before and after, and git status --porcelain -- . ":(exclude)docs/features" ":(exclude).claude" before and after)
EXIT_CODE: 0
Output Summary: CSHARPIER_EXIT_CODE: 0 (console: "Formatted 1627 files", which counts files processed, not files changed, and is not used as the rewritten count). Rewritten-file count over the two Write Set source paths: 0 (both hashes identical before and after). Scoped porcelain before: empty; after: empty; identical line sets. No FORMAT WIDENED FOOTPRINT and no POST-COMMIT CODE REWRITE. Pass number: 1.

## Hashes (CMD-HASH)

Before:
- HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
- HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = E7582241265ED1838921593C38420CEB1C3C8E9F101DD1405F55945C45A3A73B

After:
- HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
- HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = E7582241265ED1838921593C38420CEB1C3C8E9F101DD1405F55945C45A3A73B

REWRITTEN-COUNT: 0

## Scoped porcelain

Before (verbatim): (no line)

After (verbatim): (no line)

The two outputs are identical line sets.

## PASS-2:

Timestamp: 2026-09-30T15-03
Command: dotnet tool run csharpier format . (between CMD-HASH before and after, and git status --porcelain -- . ":(exclude)docs/features" ":(exclude).claude" before and after)
EXIT_CODE: 0
Output Summary: CSHARPIER_EXIT_CODE: 0 (console: "Formatted 1627 files in 2998ms.", files processed, not used as the rewritten count). Rewritten-file count over the two Write Set source paths: 0. Scoped porcelain before: empty; after: empty; identical line sets. No FORMAT WIDENED FOOTPRINT and no POST-COMMIT CODE REWRITE. Pass number: 2 (restart admitted by the P3-T8 re-run rule, revision round 3 coordinator extension).

Hashes (CMD-HASH), before:
- HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
- HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = E7582241265ED1838921593C38420CEB1C3C8E9F101DD1405F55945C45A3A73B

Hashes (CMD-HASH), after:
- HASH TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086
- HASH TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = E7582241265ED1838921593C38420CEB1C3C8E9F101DD1405F55945C45A3A73B

REWRITTEN-COUNT: 0

Scoped porcelain before (verbatim): (no line)

Scoped porcelain after (verbatim): (no line)

The two outputs are identical line sets. The hashes equal the pass-1 hashes.
