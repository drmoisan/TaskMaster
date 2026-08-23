# Code Review — winforms-message-pump-test-seam (Issue #230)

- Reviewer: feature-review agent
- Date: 2026-08-07T23-00
- Branch: `feature/winforms-message-pump-test-seam-230` @ `8f98264c` vs `main` @ merge-base `74be1964`
- Files reviewed: 10 C# code files (2 production, 7 test, 1 csproj) plus scoping/evidence docs

## Executive Summary

The implementation is disciplined and closely follows both the spec and the repository's established seam precedents. The `WinFormsPumpHost` design is sound: readiness handshake signalled in `finally`, Task-only posting surface (structurally deadlock-free between test and pump threads), three explicit exception channels, a pending-post registry that faults shutdown-raced work instead of leaving it pending, and a stop sequence that retires the lazily created WPF dispatcher before exiting the loop. Two design details deserve specific recognition: (a) `Application.ThreadException` subscription happens on the pump thread, and that event's handler registration is per-thread (`ThreadContext`), so concurrent hosts in parallel test classes cannot cross-record each other's faults; (b) the iteration-1 parallelization failure was root-caused as a real isolation defect and fixed with a deterministic ownership semaphore rather than a retry or timeout hack. No blocking findings. Three low-severity observations are recorded below; none warrants remediation before merge.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | QuickFiler.Test/TestSupport/WinFormsPumpHost.cs | Lines 85, 115, 144, 180 | `ArgumentNullException` constructed with string literals (`"action"`, `"factory"`, `"asyncWork"`) instead of `nameof(...)` | Use `nameof(action)` etc. in a future touch of this file | `nameof` survives renames and is the repo-wide C# convention; net481 fully supports it | Diff hunks for the four guard clauses |
| Low | QuickFiler.Test/TestSupport/WinFormsPumpHost.cs | Lines 383-388 (`Post`), 391-404 (`FaultPendingWork`) | `_pendingFaults` accumulates one delegate per successful post for the host's lifetime; entries are only drained at stop | Acceptable as-is for the one-host-per-test contract; if a long-lived shared host is ever introduced, remove entries on completion | Growth is bounded by posts-per-test today; the registry exists to fault shutdown-raced posts, and `TrySetException` on completed sources is a harmless no-op | Code reading; usage contract in the class remarks |
| Low | QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | File-level coverage | Modified file remains below the uniform per-file coverage floor (81.88% line / 62.96% branch) due to pre-existing untested members untouched by this feature; the feature improved the file from 74.37%/56.00% | Continue drawdown under the #197 repo-wide uplift; no action required in this branch | Changed lines are 100% covered (ResolveControlGroupsAsync 38/38) and both file metrics improved; the residue predates this branch | Independent Cobertura parse of baseline and final XMLs; policy-audit Section 1.2.1 |
| Info | QuickFiler/Controllers/QfcItemController.Initialization.cs | CreateAsync/CreateSequentialAsync signatures | New optional parameters reference `MailItem` (Outlook interop) in a delegate seam type, continuing the file's existing legacy signature pattern | None for this branch; the No-COM architecture rules target new runtime code in the migration architecture, and this is an additive extension of an existing legacy factory | Consistent with every prior review of legacy QuickFiler surfaces; introducing an abstraction here would exceed the feature's charter | `.claude/rules/architecture-boundaries.md` scope ("New runtime code"); factory-seam-verification evidence |
| Info | QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs | `UiThreadDispatcherGate` | Static `SemaphoreSlim(1,1)` serializes pump fixtures across test classes that swap the process-wide `UiThread.Dispatcher` static | Keep; this is the correct deterministic fix for the iteration-1 race | The gate is released by the preceding fixture's `Restore()`, a completion signal, not elapsed time; `Restore()` is idempotent and the builder releases on construction failure | `final-test-coverage.2026-08-08T00-05.md` root-cause section; Part2 lines 32-60 |

## Positive Observations

- Readiness event set in `finally` so a broken pump thread surfaces as a constructor rethrow (original type and stack preserved via `ExceptionDispatchInfo`), never a hang.
- Only Task-returning members are exposed; no synchronous `Invoke` exists, so the test and pump threads cannot block on each other by construction.
- `StopAsync` is idempotent under a lock and returns the same task; `Dispose` is an idempotent bridge; post-after-stop fails fast with `ObjectDisposedException`.
- `RunAsync` unwraps single-exception `AggregateException`s so tests observe the thrown type directly; the null-task delegate case is guarded explicitly.
- The 13 self-tests are the usage contract by example (per the user story), covering all four posting members, both marshal routes, both fault channels, stop-fault surfacing, post-after-stop, and double-dispose.
- The test-partial split (Part2 harness / Part3 tests) respects the 500-line limit without diluting cohesion; Part2 documents each harness decision, including why the WPF dispatcher swap must be serialized.
- Comment quality on the 8 de-exemption sites is high: each rewritten comment names the covering test class/pattern and states precisely what the former barrier was.

## Toolchain Verification

- Formatting: independently re-verified this session (`csharpier check`, 9/9 clean, EXIT 0).
- Analyzers, nullable gate, full test suite: verified from committed Phase 8 iteration-2 evidence (all EXIT 0; 6293/6293; warnings identical to baseline). See policy-audit Sections 3, 6, and Appendix B.
