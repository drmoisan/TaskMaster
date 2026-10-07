---
name: project-950-r2-self-matching-process-probe-and-bom-numstat-seams
description: Issue 950 preflight round 2 seams - a CommandLine-scanning stray-process gate counts its own pwsh; a BOM-dropping edit adds +1/-1 to numstat; a failed set cannot show aborted tests
metadata:
  type: project
---

Five round-2 defects on the #950 plan (2026-10-01), all applied verbatim from executor deltas:

- A `Get-CimInstance Win32_Process` gate that filters on `CommandLine.Contains(<leaf>)` matches the checking pwsh itself once the filter includes `pwsh*`. Exclude `$PID` and assemble the runner token from two literals. Include `testhost*` and a surviving runner pwsh, not just vstest/dotnet-coverage.
- A trx `FAILED-SET` lists only `Failed` outcomes; a no-new-failures gate also needs Total equal, executed not lower, and error/timeout/aborted/notExecuted not higher than baseline (`Format-TrxRunSummary` prints all six).
- Reason literals named in task acceptance text must also be counted by the census task that "records" them.
- A span anchor ending at a continuation line (`.BeginTransactionAsync()`) includes the statement's first line; say so.
- QfcDatamodel.cs carries a UTF-8 BOM; a whole-file rewrite drops it and numstat moves from `14 2` to `15 3`. Gate the BOM bytes and state in-place edits.

**Why:** each was a gate that could pass or fail for a reason unrelated to the change.
**How to apply:** when authoring process-count probes, numstat expectations on BOM files, or no-new-failure comparisons, apply these forms up front. See [[project-950-r1-inline-continuation-and-reindent-width-seams]].
