# Feature Audit — winforms-message-pump-test-seam (Issue #230)

- Reviewer: feature-review agent
- Date: 2026-08-07T23-00
- Work mode: `full-feature` -> AC sources are `spec.md` (13 items) and `user-story.md` (6 items); 19 total.

## Scope and Baseline

- Base branch: `main`; merge-base `74be19646f0412c6f0eab22999624b9acad91d22` (recomputed this session via `git merge-base HEAD origin/main`; identical to the caller-supplied value and to the PR-context artifact's recorded range).
- Head: `feature/winforms-message-pump-test-seam-230` @ `8f98264c7c31a0afcf18848b28a021a2ba9012e0`; PR-context summary Head ref matches, so the artifacts are fresh.
- Branch diff: 53 files — 10 C# code files (2 production: `QfcItemController.Initialization.cs`, `QfcItemController.ViewerSetup.cs`; 7 test files incl. the new `WinFormsPumpHost` seam and self-tests; 1 csproj wiring change), 13 agent-memory Markdown files, and 30 feature-folder docs/evidence files (including baseline and final Cobertura XMLs).
- Audit scope: full feature-vs-base diff. No caller-supplied narrowing was present or applied.

## Acceptance Criteria Inventory

spec.md (13):

1. S-AC1 — `WinFormsPumpHost.cs` exists as `internal sealed class : IDisposable` with the specified API shape, net481-safe (no `init`/`record`/`record struct`).
2. S-AC2 — Seam unit-tested in its own right (thread-id assertions, fault propagation, post-after-stop `ObjectDisposedException`, double-`Dispose` no-op, `ThreadException` rethrow at `StopAsync`).
3. S-AC3 — Smoke test proving both marshal routes (SyncContext await and WPF `Dispatcher.FromThread(...).InvokeAsync`).
4. S-AC4 — No `Thread.Sleep`/`Task.Delay`/polling/unbounded waits; `[Timeout]` on new test files.
5. S-AC5 — Pre-change census re-baseline (19 sites, ratified-boundary cross-reference, `EnsureBreadcrumbPipeline` flagged post-ratification) plus coverage baseline.
6. S-AC6 — Optional seam parameters on `CreateAsync`/`CreateSequentialAsync`, assigned before `SaveParameters`, non-breaking.
7. S-AC7 — Each of the 8 target members exercised through the pump host with attribute removal in the same change.
8. S-AC8 — `InitializeWebViewAsync` retains its attribute with updated justification.
9. S-AC9 — Post-change census re-baseline (8 removals, retained site, out-of-scope site, resulting count).
10. S-AC10 — Repository line coverage does not regress vs baseline; per-member coverage reported.
11. S-AC11 — Full C# toolchain passes in a single clean final pass in CUT3 order.
12. S-AC12 — No temporary files in any added/modified test.
13. S-AC13 — Every non-markdown changed file <= 500 lines.

user-story.md (6):

14. U-AC1 — Deterministic await of a WinForms-context continuation without touching the MSTest thread's context and without sleep/delay/polling.
15. U-AC2 — Self-test file demonstrates the complete usage contract by example.
16. U-AC3 — Boundary 19 -> 11 with an evidence trail sufficient for maintainer re-ratification.
17. U-AC4 — Every remaining exemption carries a genuine-external-dependency or tracked-follow-up justification.
18. U-AC5 — Unattended full-suite run completes without hangs, dialogs, or external processes.
19. U-AC6 — Existing factory consumers observe no behavior change.

## Acceptance Criteria Evaluation

| # | AC | Verdict | Verification basis |
|---|---|---|---|
| 1 | S-AC1 | PASS | File read in full: `internal sealed class WinFormsPumpHost : IDisposable`, all 8 specified members present with the specified semantics; no `init`/`record` anywhere in the diff; wired into `QuickFiler.Test.csproj` and built by the evidenced net481 solution builds. |
| 2 | S-AC2 | PASS | All 13 self-tests read and matched one-to-one against the listed scenarios (thread-id assertions for `InvokeAsync`/`RunAsync`/`await host.SyncContext`; sync-throw and async-fault propagation; post-after-stop x4 members; double-`Dispose`; raw `SyncContext.Post` throw rethrown by `StopAsync`). Full suite passes. |
| 3 | S-AC3 | PASS | `BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread` read: creates the WPF dispatcher on the pump, drives it via `Dispatcher.FromThread(pump).InvokeAsync`, asserts both routes land on `host.ThreadId`. |
| 4 | S-AC4 | PASS | Independent grep across all 9 changed `.cs` files: zero banned-API hits; 21/21 new tests carry `[Timeout]` (independently counted per file: 13+5+2+1). |
| 5 | S-AC5 | PASS | `exclusion-census-pre.2026-08-07T21-50.md` enumerates 19 sites; independently confirmed via `git show 74be1964:` attribute counts (Initialization.cs 7 + ViewerSetup.cs 3 + other partials 9 = 19). Baseline Cobertura committed with root line-rate 0.856453 (verified by direct parse). |
| 6 | S-AC6 | PASS | Diff read: three optional parameters on each factory, assigned to `_uiDispatcher`/`_webViewInitializer`/`_conversationResolverFactory` before `SaveParameters` (whose `??=` defaults preserve behavior when null); additive-only signature change; zero in-repo callers (evidence P5-T2); full suite green. |
| 7 | S-AC7 | PASS | Diff shows exactly 8 attribute removals, each replaced by a comment naming its covering test; per-member coverage 83.33%-100% (all > 0, aggregate 92.98%) confirms the members actually execute under test; census-post maps each member to phase and covering test. |
| 8 | S-AC8 | PASS | ViewerSetup.cs diff: attribute retained on `InitializeWebViewAsync`; justification rewritten exactly as specified (pump barrier resolved; residual = CoreWebView2/WebView2 runtime; concrete-accessor barrier tracked per #230). |
| 9 | S-AC9 | PASS | `exclusion-census-post.2026-08-07T23-30.md`: COUNT=11, 8 removals tabulated, retained and post-ratification sites documented. Independently recounted at head: 11 attribute sites across the controller partials. |
| 10 | S-AC10 | PASS | Independent parse of both committed Cobertura XMLs: line-rate 85.6453% -> 85.8333% (raw) and 85.8223% denominator-adjusted, both above baseline; per-member table present. The canonical `artifacts/csharp/coverage.xml` is byte-identical to the committed final XML. |
| 11 | S-AC11 | PASS | Phase 8 iteration-2 evidence: all four CUT3 stages EXIT 0 in one pass, restart rule honored after the iteration-1 failure. Formatting independently re-verified clean this session. |
| 12 | S-AC12 | PASS | Independent grep: zero `GetTempFileName`/`GetTempPath`/`GetRandomFileName` hits in changed files; determinism-audit evidence concurs. |
| 13 | S-AC13 | PASS | Independent `awk` line counts of all 10 changed non-markdown files: max 489 (`QfcItemController.Initialization.cs`); all <= 500. |
| 14 | U-AC1 | PASS | `AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread` read; the host installs its context only on the pump thread; no `SetSynchronizationContext` call on the MSTest thread anywhere in the diff; no sleep/delay/polling (see #4). |
| 15 | U-AC2 | PASS | Self-test file read: demonstrates construction, `using`/`finally` release, all four posting members, direct context await, both fault channels, stop-fault surfacing, post-after-stop, double-dispose — the full usage contract by example. |
| 16 | U-AC3 | PASS | 19 -> 11 independently reconfirmed at both endpoints of the diff; census pre/post plus coverage-delta artifacts give the maintainer the complete re-ratification trail without re-derivation. |
| 17 | U-AC4 | PASS | Census-post table read: remaining 11 = 9 ratified non-pump categories + `InitializeWebViewAsync` (external CoreWebView2 process; accessor barrier tracked per #230) + `EnsureBreadcrumbPipeline` (post-ratification #351 follow-up). None cites missing pump infrastructure. |
| 18 | U-AC5 | PASS | Final run evidence: 6293/6293, EXIT 0, unattended, 9 assemblies; all Outlook types Moq'd; `IWebViewCoreInitializer` always mocked so no WebView2 runtime initializes; every new test inside its `[Timeout]` bound. |
| 19 | U-AC6 | PASS | Zero in-repo callers of the factories (so no call site could change), defaults preserved via `??=` ordering, and the full pre-existing suite passes unchanged. |

## Summary

19/19 acceptance criteria PASS. Verdicts rest on direct code reading of every changed C# file, independent recomputation of the census (19 -> 11), coverage figures (repo-wide, per-file, changed-line), determinism/temp-file scans, file-size counts, and a fresh `csharpier check`, supplemented by the committed Phase 8 final-pass evidence for the full-solution build and test stages. The single quality observation that survives review — `QfcItemController.ViewerSetup.cs` remaining below the per-file coverage floor for pre-existing reasons — is dispositioned non-blocking in the policy audit and does not impair any acceptance criterion. Recommendation: **GO** for PR, contingent on the maintainer's re-ratification of the reduced exemption boundary during PR review (the approval step the spec itself defines).

### Acceptance Criteria Status
- Source: `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/spec.md`, `docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/user-story.md`
- Total AC items: 19
- Checked off (delivered): 19
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All 19 checkbox items were already marked `[x]` in the source files by the executor, each with an inline evidence citation. Per the reviewer check-off protocol, every item evaluated PASS above was verified against its cited evidence and, where feasible, re-derived independently; no checkbox state required correction, no criterion text was modified, and no phantom criteria were added. The three `## Seeded Test Conditions` items in spec.md are seeded planning notes, not acceptance criteria (they sit outside the `## Acceptance Criteria` section), and were intentionally left unchecked.
