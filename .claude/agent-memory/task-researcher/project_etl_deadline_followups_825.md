---
name: etl-deadline-followups-825
description: "Issue #825 research (2026-09-08): CreateCancellationTokenSource VERIFIED in Bcl.TimeProvider 10.0.11 net462 via shipped XML doc; the timeoutSourceFactory seam is NOT reachable from DfDeedle; the '#825 ten tests' and OlTableExtensions_Tests comment claims are both wrong (four/one)"
metadata:
  type: project
---

Issue #825 (etl-deadline-mechanics-follow-ups) research findings that cost real effort to derive.

**Why:** #825 is six residuals deliberately excluded from #811. Three of its own claims turned out
to be wrong, and two of those inversions change which fix option is viable.

**How to apply:** when planning or reviewing #825, or any future work on the ETL deadline path.

## Package-surface verification without a decompiler
NuGet `packages/` is gitignored (`.gitignore:190`) so agent worktrees have NO restored packages dir.
The main checkout at `<repo-root>/packages/` does. The `.xml` doc file shipped beside each assembly
is generated from the same compilation and is authoritative for the public surface — reading it is a
legitimate substitute for a decompiler. `Microsoft.Bcl.TimeProvider.10.0.11/lib/net462/Microsoft.Bcl.TimeProvider.xml:199`
declares `TimeProviderTaskExtensions.CreateCancellationTokenSource(TimeProvider, TimeSpan)`, so that
method IS available on net481. Note the csproj HintPath pins **net462**, not netstandard2.0, even
though all three TFM folders ship. The XML also warns (lines 211-214) that on pre-.NET 8 a later
`CancelAfter` does not cancel the original delay timer.

Tooling note: the Bash tool was fully disabled and `pwsh` refused in this session. Glob/Read/Grep
DO work on absolute paths outside the worktree (Glob needs a real `path` arg plus pattern `*`).

## The seam that looks threaded but is not
`OlTableExtensions.GetTableInViewAsync` has a `Func<int, CancellationTokenSource>? timeoutSourceFactory`
seam, but `DfDeedle.GetEmailDataInViewAsync` calls it as `GetTableInViewAsync(token, 0)` — two args,
and it has no parameter able to carry a factory. So the "just pass a never-cancelling factory from
the test" option cannot reach any test that goes through DfDeedle. Verify reachability, not just
existence, of a seam before costing an option around it.

## Count claims in #825 that are wrong
- "ten of its tests drive the 2000 ms GetTableInViewAsync window" (both the issue AND the in-code
  comment at `OlTableExtensions_Tests.cs:18-20`): FOUR tests reference it, and only ONE arms a real
  `CancellationTokenSource(2000)`. The other three throw early or inject a factory.
- `OlTableExtensions_Tests` has NO shared mutable state (no statics, no lifecycle hooks, no
  Console.SetOut). Its parallelism hazard is a wall-clock deadline under thread-pool contention,
  which a soak cannot retire — fix the deadline first, then drop `[DoNotParallelize]`.

Everything else in #825 (line numbers 84/149, 824/862/924/949, 1011, 1846, QfcColumns.cs:96,
TableAccess.cs:79/97, TimeOutTask_Tests.cs:197/210) checked out exactly.

Related: [[qfc424-high-confidence-startup-stall]] for the FakeTimeProvider/deadline pattern.
