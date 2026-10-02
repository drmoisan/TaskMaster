---
name: preflight-scratch-reconstruction-with-cached-csharpier
description: During C# plan preflight, rebuild the post-edit files in the scratchpad from the plan's delivered text and format them with the CSharpier 1.2.6 binary in the NuGet cache to verify census rows; plus two gate gaps found that way on #950 round 2
metadata:
  type: project
---

Census and span gates written against "prescribed final source text" can be checked mechanically
without touching the worktree: read the worktree files, apply the plan's delivered blocks by their
pre-edit line ranges bottom-up in a scratchpad copy, then run
`dotnet <USERPROFILE>\.nuget\packages\csharpier\1.2.6\tools\net8.0\any\CSharpier.dll format <scratch dir>`
(the cached binary runs without `dotnet tool restore`). Recompute every token/span gate on both the
pre-format and post-format copies. On #950 round 2 this confirmed all 14-column rows and showed
which lines CSharpier reflows (re-indented member chains, a lengthened lambda).

Reconstruction pitfalls: pwsh `Set-Content -Encoding UTF8` drops a BOM, so a BOM-bearing target
shows one extra changed line in numstat; a PowerShell function returning a `List[string]` must
`return ,$x` or the list unrolls. CSharpier 1.2.6 preserves a BOM when it rewrites a file.

Two gate gaps found on #950 round 2:
- `Get-TrxRunSummary.FailedTestName` lists only `outcome=Failed`, so a baseline-relative
  "new failures" gate built on it cannot see aborted, timed-out or not-executed tests; compare the
  summary's Total/executed/error/timeout/aborted/notExecuted figures to baseline as well.
- A stray-process gate filtering only `vstest*`/`dotnet-coverage*` misses the surviving pwsh runner
  that respawns test hosts. Filter pwsh by a runner token assembled from two literals and exclude
  `$PID`, or the checker counts its own command line (joined literal read 1 on an idle machine).

**How to apply:** use the reconstruction whenever a plan asserts post-edit counts, spans or numstat.
Related: [[preflight-csc-probe-for-mandated-csharp-shapes]], [[timedout-mstest-leaves-detached-runner]].
