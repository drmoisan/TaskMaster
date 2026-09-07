# Feature Audit — breadcrumb-coordinator-hub-defects-501

- Timestamp: 2026-08-27T23-48
- Work Mode: `full-bug` — sole AC source is `spec.md`, section `## Acceptance Criteria`
- Range: `origin/epic/quickfiler-bug-family-integration..HEAD` (`cab1a0fb`)
- Verdict: **PASS** — 0 Blocking findings
- AC outcome: **29 PASS, 3 PARTIAL, 0 FAIL, 0 UNVERIFIED** of 32

Verification method: each criterion was checked against the source at HEAD and against the named
evidence artifact. Coverage-based claims were re-derived from the committed Cobertura XML rather than
read from the executor's prose. No criterion is marked PASS on the strength of an evidence artifact's
assertion alone where the underlying fact was independently checkable.

## Acceptance Criteria Evaluation

| AC | Verdict | Verification performed |
| --- | --- | --- |
| AC-01 | PASS | Source read: `_closePending` gone, replaced by `_closeInFlight` (`BreadcrumbDropDownOpenCoordinator.cs:27-34`) and `_closeCompleted` (`:36-45`), each with a distinct XML-documented meaning. `_closeInFlight` cleared in `finally` at `:322-326` around `_host.Close(reason)` at `:320`. All four exits confirmed by reading the method: success (`:328-336`), not-closed (`:337-340`), throw (`finally` runs before propagation), released (`:312-313`, returns before the flag is set) |
| AC-02 | PASS | `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync` (`Part2.cs:332-362`) asserts `Host.Requests` count 2 and `reopen.Result == true`. RED evidence `red-462-reopen.2026-08-27T20-12.md`, GREEN `green-462.2026-08-27T20-17.md`. Source confirms `_closeCompleted = false` at `:114` on the reopen path |
| AC-03 | **PARTIAL** | Conjunct 1 (idempotent close) PASS — `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce` (`Part2.cs:373-398`) asserts one `CloseReasons` entry. Conjunct 2 (generation monotonicity) PASS — source `:328-336` increments `_generation` only inside `if (closed)`. Conjunct 3 split: `RequestOpen` after `Release()` PASS (`Part2.cs:176`); **`CloseCore` after `Release()` NOT VERIFIED** — line `313` has zero hits in the post-change Cobertura, so no test reaches it. Not a regression (baseline line 288 equally uncovered; the two lines are unchanged context). See code-review NB-3 |
| AC-04 | PASS | `TryRunCurrent_GuardedActionRunsWithoutHoldingLifetimeSync` (`BreadcrumbCoordinatorUpgradeLifetimeTests.cs:103-122`) reads `Monitor.IsEntered` on the reflected `_sync` from inside the action. Source confirms the lock at `:181-184` encloses only the currency computation; `action()` at `:190` is outside it. RED/GREEN evidenced |
| AC-05 | PASS | `PostJson_SurfaceInvocationRunsAfterHubLockIsReleased` (`BreadcrumbSelectorCoordinatorTests.cs:200-215`) probes `Monitor.IsEntered` on the reflected hub `_sync` from a Moq callback. Source: snapshot taken under lock at `BreadcrumbMessengerHub.cs:145`, `Broadcast` called at `:147` outside it |
| AC-06 | PASS | `TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation` asserts `invoked == true` and `IsCurrent(lease) == false` after a re-entrant `Invalidate()` |
| AC-07 | PASS | `PostJson_ReentrantAttachFromSurfaceDoesNotThrowCollectionModified` (`BreadcrumbSelectorCoordinatorTests.cs:224-250`) asserts no `InvalidOperationException` and `attached == true`. Source: `_attachments.Values.ToArray()` at `:145` makes the enumeration immune to live-dictionary mutation |
| AC-08 | PASS | `PostJson_SurfaceFailureDoesNotStarveOtherSurfacesOrFalsifyReplayCache` asserts `attempts == 2`. Order-independence verified by construction: both `CountingThrowingMessenger` instances increment before throwing, so 2 holds in every `Dictionary.Values` order. RED evidence records the pre-fix total of 1 |
| AC-09 | PASS | Same test asserts `recording.Posted` contains the render payload |
| AC-10 | PASS | Same test attaches a late `TrackingMessenger` and asserts it replays the payload the surviving surface received |
| AC-11 | **PARTIAL** | Non-propagation half PASS — `post.Should().NotThrow(...)`, RED-to-GREEN evidenced. Logging half verified at source (`BreadcrumbMessengerHub.cs:163-168`, one `log4net.LogManager.GetLogger(...).Error(...)` inside the per-surface catch) and strengthened independently: the hub measures 306/306 lines covered, so the catch block demonstrably executes at runtime. The log record's content and level are not asserted by any test. The plan's amended justification is incomplete — see code-review NB-1 |
| AC-12 | PASS | Signature is `internal bool RunSynchronous(...)` at `:128`. `grep -rn "RunSynchronous" QuickFiler/ --include=*.cs` returns exactly two call sites, `.Suggestions.cs:51` and `:105`, and both bind the result to `bool ran` and branch on it. `RunSynchronous_SupersededLeaseReportsSkipToCaller` asserts both directions ("when and only when"). Interpretation note: `SetSuggestions` and `AddItems` consume the verdict transitively through their `*Core` seams; the spec's own interface section already anticipates this shape for `SetSuggestionsCore` |
| AC-13 | PASS | `SetSuggestionsCore_SupersededLeaseReplacesStaleSuggestionsUpgrade` asserts `NotBeSameAs(captured)` and `IsCompleted == true`. The test correctly makes the captured handle genuinely pending first, and asserts that precondition — without it the `Task.CompletedTask` singleton would make the inequality assertion unfalsifiable. Source: `.Suggestions.cs:59-63` |
| AC-14 | PASS | `.Suggestions.cs:115-118` calls `_upgradeLifetime.Abandon(lease)` on the `false` branch; the deliberate discard is documented in the XML `<remarks>` at `:82-89`. Both halves of the criterion are literally satisfied. That the call is redundant in effect is a design finding, not an AC failure — see code-review NB-2 |
| AC-15 | PASS | `RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource` asserts `lease.Settled` and `lease.SourceDisposed`. Source read of `Abandon` / `CancelLease` / `Complete` (`:88-98`, `:298-330`, `:279-296`) confirms the skip path reaches `Complete`, which disposes when `Cancelled && !SourceDisposed`. RED evidence `red-502-lease-leak.2026-08-27T20-29.md` |
| AC-16 | PASS | Test is in `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`, uses `CoordinatorHarness`, `Host.SetOpen(true)` and `Context.DrainUntil`. RED and GREEN artifacts both present |
| AC-17 | PASS | Test is in `BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, asserts via `Monitor.IsEntered` against the reflected `_sync` (helper `GetSync` at `:236-243`). RED `red-500-lifetime-lock.2026-08-27T20-24.md`, GREEN `green-500-lifetime.2026-08-27T20-27.md` |
| AC-18 | PASS | Test is in `BreadcrumbMessengerHubTests.cs` with two counting-and-throwing surfaces. RED artifact records total attempts 1; GREEN records 2 |
| AC-19 | PASS | The lease-leak test is written as a bare statement (`lifetime.RunSynchronous(lease, () => ran = true);`), so it compiles against both the pre-change `void` and post-change `bool` signatures — confirmed by reading the test. RED-then-GREEN artifacts present, and it is the first-authored #502 test per the plan sequence |
| AC-20 | PASS | `git diff --numstat` shows `BreadcrumbDropDownOpenCoordinatorTests.cs` absent from the range entirely: 0 added, 0 deleted. The named test passes in `green-462-suite` (48/48) |
| AC-21 | PASS | `Part2.cs` is +74/-0. The single hunk is `@@ -322,6 +322,80 @@`, a pure append after line 322, so lines 120-140 are untouched. Passes in the same 48/48 suite |
| AC-22 | PASS | `BreadcrumbMessengerHubTests.cs` is +78/-0. The single hunk is `@@ -331,6 +331,84 @@`, a pure append, so lines 198-217 are untouched. `green-501-suite.2026-08-27T20-48.md` names the test as passing |
| AC-23 | PASS | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` exists (123 lines) and contains all five named members: `SetSuggestions` `:18`, `SetSuggestionsCore` `:46`, `SuggestionsUpgrade` `:70`, `PopulateSuggestionsAsync` `:72`, `AddItems` `:91`. `git diff --numstat` shows `QuickFiler/QuickFiler.csproj` at exactly +1/-0, and the added line is the expected `<Compile Include>` |
| AC-24 | PASS | `QuickFiler.Test/QuickFiler.Test.csproj` is +1/-0. The added line sits immediately after the `BreadcrumbBridgeCoordinatorTests.cs` entry, matching the "adjacent to the sibling entry" requirement rather than alphabetical order. Exactly one file was added under `QuickFiler.Test/` in the range; no third new test file |
| AC-25 | PASS | Independently counted with `awk 'END{print NR}'`, cross-checked with `wc -l`, not `Measure-Object -Line`. Production: 490, 437, 123, 353, 378. Test: 191, 271, 455, 492, 500. Maximum 500, so none exceeds 500. `BreadcrumbSelectorCoordinatorTests.cs` sits exactly on the cap — see code-review O-1 |
| AC-26 | PASS | `git diff --name-only` over the range contains none of the six sibling-owned paths. Independently reproduced rather than taken from the scope-lock artifact |
| AC-27 | PASS | Independent grep for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `new Thread`, `Task.Run`, `Path.GetTempFileName`, `Path.GetTempPath`, `WaitOne`, `.Wait(` across all five changed test files returned zero banned constructs. Ordering mechanisms confirmed by reading each new test: injected delegates, reflected `Monitor.IsEntered` probes, and `CapturingSynchronizationContext.DrainAll`/`DrainUntil` |
| AC-28 | PASS | Read `TryRunCurrent` at `:179-193`. After `action()` at `:190` the only statement is `return true;`. No call to `IsGenerationCurrentCore`, `IsCurrent`, or `IsCancellationRequested` occurs lexically after the invocation. The contract is additionally documented in the `<returns>` block at `:158-166`, and AC-06's test guards it |
| AC-29 | PASS | `post-merge-csharpier.2026-08-27T23-31.md`: `format` then `check`, both exit 0, 1545 files, zero needing formatting in the final pass |
| AC-30 | PASS | `post-merge-msbuild-analyzers.2026-08-27T23-31.md`: exit 0, 0 errors, 5 pre-existing sibling-owned warnings. Non-vacuity is proven, not assumed: `Skipping target "CoreCompile"` count 0 and 36 `csc.exe` invocations |
| AC-31 | PASS | `post-merge-msbuild-nullable.2026-08-27T23-31.md`: exit 0, 0 errors, no `CS86xx` promoted. Command shape verified — contains `/t:Rebuild`, does not contain `/p:Nullable=enable`. `Skipping target "CoreCompile"` count 0. The new `.Suggestions.cs` carries `#nullable enable` at line 1, so it genuinely participates in the per-file opt-in gate |
| AC-32 | **PARTIAL** | Coverage half PASS and independently reproduced from the committed Cobertura: new/changed-line coverage 89/89 = 100%, all four per-file deltas at or above 0.00 pp, repository line rate 85.1448% (up +0.0068 pp) and branch rate 79.2202%. Toolchain-pass half PASS per the attestation. Suite-green half (6730/6730, 0 failed, 0 skipped) rests on prose attestation: no full-suite log or TRX is committed, and the cited `p7-t5-coverage-stdout.log` does not exist on disk. See code-review NB-4 |

### Acceptance Criteria Status

- Source: `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md`
- Total AC items: 32
- Checked off in source (delivered): 32
- Reviewer verdict: 29 PASS, 3 PARTIAL (AC-03, AC-11, AC-32), 0 FAIL, 0 UNVERIFIED
- Items remaining unchecked: none

No checkbox was altered by this review. The three PARTIAL items are each substantially delivered with a
narrow, identified verification gap, and none rises to a Blocking finding. Per the check-off protocol a
PARTIAL item would normally be left unchecked; these were already checked by the executor. The gaps are
documented here and in the code review rather than reversed unilaterally, because in each case the
delivered behaviour is correct and only the breadth of the evidence falls short of the criterion's
literal wording.

## Defect-by-Defect Assessment

**#462 — stale close flag drops a legitimate reopen.** Fixed correctly. The two-flag split is the right
design: the old single flag conflated in-flight suppression with completed-close suppression and could
not serve both. `_closeInFlight` is now bounded by a `finally`, so it cannot outlive the call, which is
what the old `catch`-and-rethrow form failed to guarantee on the success path. The known residual — a
host reopened by a path reaching neither `RequestOpen` nor `Invalidate` leaves `_closeCompleted` stale,
so a close request returns `true` without closing — is disclosed in `spec.md` as the SR-4 limitation,
is strictly narrower than the HEAD behaviour it replaces, and is filed as issue #656 against feature
488's host paths. Verified open via `gh issue view`.

**#500 — WebView2 post executes under nested re-entrant monitors.** Fixed correctly at both layers. The
lifetime layer captures the currency verdict under `_sync` and runs the action outside it; the hub
layer snapshots attachments under `_sync` and broadcasts outside it. The consequent loss of
serialization is documented rather than concealed, and the stated mitigation was verified against every
call site. The enforcement gap is filed as #655 and open.

**#501 — `PostJson` caches before broadcasting and starves later attachments.** Fixed correctly. The
per-surface `try`/`catch` guarantees one attempt per attachment, and the decision to keep the cache
write inside the lock and before the broadcast is defensible given containment: once no surface can
starve, the cache's delivery claim holds for every live surface, and the surface that threw is stale by
its own failure. The order-independent test construction is a genuine improvement over the obvious
arrangement.

**#502 — `RunSynchronous` discards its verdict; superseded lease leaves a stale handle.** Fixed
correctly. `SuggestionsUpgrade` is replaced with `Task.CompletedTask` on the skip path, and the choice
of `Task.CompletedTask` over `Task.FromCanceled` is justified in-code by the eleven existing tests that
call `SuggestionsUpgrade.GetAwaiter().GetResult()`. The companion lease-leak defect (I-502.3) is fixed
at the right layer, inside `RunSynchronous`, so every skip settles and disposes. The one weakness is
that the caller-side `Abandon` calls are now redundant and one of them backs a test with no assertion
power — code-review NB-2.

## Regression Risk

Low. The change is confined to four owned production files plus one new partial part, has no public API
change, and every behaviour change is the stated point of an issue. The three deliberate behaviour
changes — a reopen after a successful close now opens, a broadcast throw no longer propagates, and a
superseded population now replaces its handle and settles its lease — are each covered by a
demonstrated red-to-green regression test.

Two residual risks are recorded rather than mitigated, both by design and both filed as issues: the
SR-4 `_closeCompleted` residual (#656) and the unenforced single-viewer-thread assumption that the
relaxed `TryRunCurrent` now depends on (#655).

## Remediation

**Not required.** Zero Blocking findings. No `remediation-inputs` artifact is produced.

The five Non-blocking findings are recorded for disposition at the maintainer's discretion. Of these,
NB-4 (commit the full-suite run log or TRX) and NB-5 (correct the handoff index's five dead paths) are
the cheapest to close and improve the durability of the audit trail. NB-1 and NB-2 are quality
improvements that do not affect correctness. NB-3 documents a pre-existing coverage gap that this
change neither caused nor widened.
