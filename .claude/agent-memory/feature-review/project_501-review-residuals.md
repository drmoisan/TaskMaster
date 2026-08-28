---
name: 501-review-residuals
description: "#501 breadcrumb coordinator/hub PASS 0 blocking; residuals NB-1 MemoryAppender placement premise, NB-2 redundant Abandon = coverage without assertion power, NB-4 missing full-suite logs; post-501 baseline 85.1448/79.2202"
metadata:
  type: project
---

Feature `breadcrumb-coordinator-hub-defects-501` (epic child of `quickfiler-bug-family`, base
`origin/epic/quickfiler-bug-family-integration`) reviewed 2026-08-27 at HEAD `cab1a0fb`.
Exit: **PASS, 0 Blocking**, 29/32 AC PASS + 3 PARTIAL (AC-03, AC-11, AC-32).

**Why:** three self-declared deviations were put up for independent scrutiny and each survived, but
two of the three justifications were overstated in ways only a disk check exposed.

**How to apply:** reuse these three verification moves on later QuickFiler breadcrumb reviews.

## Reusable verification moves that paid off here

1. **"No compliant test placement exists" — always enumerate ALL existing candidate files, not just
   the obvious one.** #501 argued AC-11's log assertion had to stay source-level because
   `BreadcrumbMessengerHubTests.cs` is 492/500 and AC-24 bans a third new file. Both facts true; the
   conclusion still failed, because `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs`
   is **478 lines**, already has a `<Compile Include>` (`QuickFiler.Test.csproj:97`), is hub-cohesive,
   and AC-24 does not reach it (not a new file). A reusable `MemoryAppender` fixture also already
   exists at `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs:338-345`. Note the
   executor had ALREADY caught and amended an earlier false premise (ruling PD-2 claimed the test
   csproj has no log4net ref; it does, at `QuickFiler.Test.csproj:209-210`) — so a corrected premise
   is not automatically a sound one. Check the replacement too.
2. **A 100%-covered file can still under-verify.** AC-11 stayed Non-blocking, not Blocking, because
   `BreadcrumbMessengerHub.cs` measures 306/306 lines covered, which proves the `catch` block's
   log statement EXECUTES at runtime. That is stronger evidence than the source-inspection artifact
   itself claimed. Conversely, coverage does not assert log content or level. Separate "the line ran"
   from "the behaviour is asserted" in both directions.
3. **Coverage remediation can raise the number without raising assurance.** The mid-run `AddItemsCore`
   seam was added to cure a -1.03 pp per-file delta. But `RunSynchronous` already calls
   `Abandon(lease)` on every `false` return, so the caller's `if (!ran) { Abandon(lease); }` in
   `BreadcrumbBridgeCoordinator.Suggestions.cs:115-118` is unobservable — the new test
   `AddItemsCore_SupersededLeaseSkipsAppendAndSettlesTheLease` asserts `dead.Settled`, a state the
   INNER Abandon set, so it would pass with the branch deleted. Verified idempotency by reading
   `Abandon`/`CancelLease`/`Complete`: second pass is a no-op (`CancellationStarted` early-return,
   `SourceDisposed` guard). Correct but redundant; lease-settlement ownership is now duplicated across
   two layers with only prose holding it together.

## Evidence-integrity residuals (not blocking, worth curing)

- **NB-4:** `evidence/qa-gates/final-test-coverage.2026-08-27T21-02.md` cites
  `p7-t5-coverage-stdout.log` (498107 bytes) and `-stderr.log` as its primary record. **Neither is
  committed.** No full-suite TRX either — the `trx/` dirs are scoped per-task runs only. So 6711/6711,
  6730/6730, and the whole 13-failure flake narrative rest on prose. The committed Cobertura
  corroborates the coverage numbers but not the pass/fail counts.
- **The flake claim's weakest leg:** "the identical merged tree ran 6729/6729 green before the
  two-file edit". The string `6729` appears exactly ONCE across all 93 evidence files — in the
  sentence asserting it. Arithmetically consistent (6711 + 18 siblings), not independently evidenced.
  Corroboration that DID hold: `GateTimeoutMs = 60000` is real at
  `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:33`, and the 13
  failing files are provably not written by this branch. Verdict: more likely than not, inadequately
  evidenced. **Retain the FAILING run's log — that is the artifact that proves a flake, and it is the
  one that always gets discarded.**
- **NB-5:** `evidence/other/handoff-index.2026-08-27T23-39.md` "Where to start" table cites 5 paths at
  `T23-39` that do not exist (real ones are `T23-31`), while the same doc asserts "Artifacts listed
  but missing from disk: 0". The 92-row inventory table IS accurate. Diff `find evidence -type f`
  against the index's backticked paths with `comm` — cheap and it caught this immediately.

## Other residuals

- **NB-3:** `BreadcrumbDropDownOpenCoordinator.cs:313` (`return false;` under `if (_released)` in
  `CloseCore`) has ZERO hits, so AC-03's "after Release(), CloseCore returns false" is unexercised.
  Structural: all three CloseCore call sites (`:167`, `:182`, `:277`) sit behind an earlier
  `IsReleased()` gate. Not a regression — baseline line 288 maps to it and was equally uncovered.
- Post-501 repo baseline: **line 85.1448% (54439/63937), branch 79.2202% (12943/16338)**. Clears the
  85% floor by only 0.1448 pp (~93 lines).
- Files at/near the 500 cap after this change: `BreadcrumbSelectorCoordinatorTests.cs` **exactly 500**,
  `BreadcrumbDropDownIntegrationTests.cs` 500, `BreadcrumbDropDownHostTests.cs` 499,
  `BreadcrumbMessengerHubTests.cs` 492.
- Follow-ups filed and verified OPEN via `gh`: **#655** (non-re-entrant upgrade-lifetime guard — the
  enforcement gap left by relaxing `TryRunCurrent`'s lock) and **#656** (SR-4 `_closeCompleted`
  residual, owned by feature 488).

Related: [[441-review-residuals-and-494-handoff]], [[csharp-coverage-constants-nondeterministic]],
[[same-commit-differing-outcome-flake-check]], [[review-worktree-differs-from-session-cwd-mirror-artifacts]],
[[verify-zero-own-effect-coverage-noise-491]]
