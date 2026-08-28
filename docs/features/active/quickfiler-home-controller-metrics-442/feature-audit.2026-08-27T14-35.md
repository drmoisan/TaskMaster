# Feature Audit — quickfiler-home-controller-metrics-442

- Timestamp: 2026-08-27T14-35 (UTC)
- Branch: `bug/quickfiler-home-controller-metrics-442`, HEAD `6a1b9ca4` vs base `origin/epic/quickfiler-bug-family-integration` @ `0ddab410`
- Work mode: `full-bug` — `spec.md` is the sole acceptance-criteria source. `user-story.md` is correctly absent by mode rule.
- AC source: `docs/features/active/quickfiler-home-controller-metrics-442/spec.md`, `## Acceptance Criteria`, 25 items.
- Check-off state on entry: 24 of 25 checked; AC-19 unchecked. Per the recorded disposition, AC-19 must remain unchecked; this reviewer flipped no checkbox.

## Verification method

Evidence-first. Rows marked "reviewer-verified" were re-run or re-read directly by this reviewer this session (git greps from the branch worktree, source reads, line counts, `gh issue view 645`). Rows marked "evidence + corroboration" rest on the committed evidence artifacts of record (the `2026-08-27T14-18`/`14-19` set, per the reconciliation record) plus the launching session's independently measured toolchain/coverage facts, cross-checked for internal arithmetic consistency where counters are given.

## Per-criterion evaluation

| AC | Verdict | Basis |
|---|---|---|
| AC-1 regression-test-first | PASS | Red observations recorded per defect family: `evidence/regression-testing/efc-metrics-red.2026-08-26T11-06.md`, `qfc-stopwatch-red.2026-08-26T11-14.md`, `qfc-flush-red.2026-08-26T11-19.md` (reviewer-read: 4 named failures with verbatim messages, exit 1 expected), `efc-reentrancy-pin.2026-08-26T11-10.md`, plus the fail-before-exception dossier for the guard pin. Matching green records follow each. |
| AC-2 flush occurs | PASS | `WriteMetricsAsync_InvokesInjectedMetricsFileWriterOnce` present (reviewer-read, asserts filename, folder root, and lines captured exactly once); green record `qfc-flush-green.2026-08-26T11-23.md`; suite of record 6701/6701 passed. |
| AC-3 flush-timing invariant | PASS | `WriteMetricsAsync_CompletesWriterTaskBeforeReturning` (reviewer-read; `Task.Yield` suspension, flag asserted after await). Reviewer-run `git grep -nE "NonBlockingProducer|TimedConsumerAsync|_metricsConsumers|_lockObject|_fileName" QuickFiler/Controllers/` — zero matches. |
| AC-4 flush survives cancellation | PASS | `WriteMetricsAsync_PassesUncancelledTokenToWriter` (reviewer-read; cancels `TokenSource` first, asserts captured token uncancelled). Production passes `CancellationToken.None` with in-code rationale (reviewer-read at `QfcHomeController.Metrics.cs:175-179`). |
| AC-5 no blank CSV line | PASS | `WriteMetricsAsync_FiltersNullDiagnosticLinesBeforeWriting` (reviewer-read; trailing `null` and whitespace entries dropped); production filter at `Metrics.cs:171-174`. |
| AC-6 correct stopwatch | PASS | `WriteMetricsAsync_ReadsMovedStopwatchForDuration` (reviewer-read; `_stopWatchMoved` populated, `_stopWatch` fresh, `It.Is<double>(d => d > 0)`); production reads `_stopWatchMoved.Elapsed.TotalSeconds` (reviewer-read at `:136`). |
| AC-7 no seconds truncation | PASS | Reviewer-run `git grep -n "Elapsed.Seconds" QuickFiler/Controllers/` — zero matches. `BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration` asserts `,90,1.50,` (reviewer-read). |
| AC-8 calendar span agrees | PASS | Reviewer-run grep shows `QfcHomeController.Metrics.cs:141: OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);`; no `(int)Duration` cast remains. Verified by inspection, as the criterion itself provides (GetCalendar returns null in unit fixtures). |
| AC-9 EFC stopwatch started | PASS | Reviewer-run grep returns `Stopwatch.StartNew()` at `EfcHomeController.cs:76` and `:225`. `StopWatch_AfterControllerConstruction_IsRunning` (reviewer-read; `withMail: true` reaches the `:76` site through the real constructor) — the spec's fallback for `:76` unreachability was not needed. |
| AC-10 signature widening | PASS | Reviewer-read: both writer declarations take `double elapsedSeconds`; reviewer-run `git grep -n "int elapsedSeconds" QuickFiler/` — zero matches; nullable gate exit 0. Spec line numbers (`:35`, `:57`) are stale (now `:63`, `:85`); the substance is met and the staleness is recorded in the acceptance-criteria status artifact. |
| AC-11 rounding pinned | PASS | `BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding` pins `,3,0.04,` for 8s/3 items (reviewer-read); PR-body statement §5 records the change. |
| AC-12 CSV separator | PASS | `BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields` and the updated `..._FormatsMetricLine` assert 12 fields and `,Recipient,Sender,` (reviewer-read); reviewer-run `git grep -n "RecipientSender" QuickFiler.Test/` — zero matches. |
| AC-13 CSV sanitization | PASS | `BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields` (reviewer-read); `xComma` applied at all four free-text sites (reviewer-read in the diff). |
| AC-14 atomic re-entrancy | PASS | Reviewer-read: `Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0` take, `Interlocked.Exchange` release, `private int _isExecuting`; reviewer-run `git grep -n "volatile" QuickFiler/Controllers/EfcHomeController.cs` — zero matches. Sequential tests true/false/true-after-reset present; no concurrent assertion attempted, per the criterion. |
| AC-15 interface overload implemented | PASS | Reviewer-run `git grep -n "NotImplementedException" QuickFiler/Controllers/EfcHomeController.Metrics.cs` — zero matches (the file's remaining reference set is empty; the class-level `Loaded => throw` lives in `EfcHomeController.cs` and is out of the criterion's scope). Both replacement tests present (reviewer-read: absent-prerequisites no-op, present-prerequisites delegation observed through the writer seam). `IFilerHomeController.cs` absent from the branch diff. |
| AC-16 culture invariance | PASS | Six `CultureInfo.InvariantCulture` sites confirmed in the diff (4 QFC + 2 EFC); both `de-DE` tests use `try`/`finally` restore and assert the invariant separator and field count (reviewer-read). |
| AC-17 test determinism | PASS | Reviewer-run banned-construct grep over both owned test files — zero matches; no filesystem access in either file; clock reads via `FakeTimeProvider`/injected factory (reviewer-read). |
| AC-18 deliberate test updates recorded | PASS | Dispositions for all four named tests, including the deletion of `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`, recorded in `evidence/other/pr-body-statements.2026-08-26T11-31.md` §6 and the addendum; deleted tests confirmed absent from the run of record. No pinning assertion for a fixed defect survives (the concatenated-form assertion was replaced). |
| AC-19 ownership boundary | **FAIL — documented, ratified deviation; correctly left unchecked** | Reviewer-verified: the three-dot diff contains `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` (one line, commit `889fa298`) plus two `.claude/agent-memory/orchestrator/**` paths (parent-orchestrator commit `2b3760eb`) outside the criterion's allowed set. The six named must-not-touch files all show zero diff lines. The deviation is disclosed in `evidence/qa-gates/ownership-gate.2026-08-27T14-03.md` with parent ratification and a verified fan-in-safety argument; the criterion, [P7-T6], and [P7-T27] are all left unchecked. Evaluated non-blocking. The agent-memory paths are not covered by the deviation record — see policy-audit finding PA-2. |
| AC-20 no project-file edit, no new source file | PASS | Reviewer-verified from the branch diff name list: no `*.csproj`/`*.props`/`*.targets`, no added `.cs` file (the only added files are markdown). |
| AC-21 file-size cap | PASS | Reviewer-measured: all eight touched code files at or below 490 lines; `QfcHomeController.cs` at 449 vs pre-change 487; `QfcHomeControllerMetricsTests.cs` at 453. |
| AC-22 coverage | PASS with the stated exception, judged adequate | Changed-line coverage 39/39 = 100.00%; repo-wide 84.8433% -> 85.1255% line and 78.8181% -> 79.2096% branch (recorded with the baseline, moved up); five of six named members at 100.00%; `QuickFileMetrics_WRITE` at 88.37% with the shortfall wholly in a pre-existing, unchanged (39/49 both sides) Outlook-Interop block outside any injectable seam. Exception adjudication in policy-audit § 8. Note: the spec's `evidence/coverage/` recording path is satisfied in substance by `evidence/baseline/mstest-coverage.2026-08-26T10-42.md` + `evidence/qa-gates/coverage-delta.2026-08-27T14-19.md`. |
| AC-23 full toolchain pass | PASS | Pass-of-record transcript `evidence/qa-gates/toolchain-loop.2026-08-27T14-18.md` (commands, exit codes, timestamps, non-vacuity counters, 6701/6701 tests); corroborated by the launching session's independent measurements. Formatter rewrote zero files in the final pass. |
| AC-24 backward-compatibility decision stated | PASS | All four required statements present in `evidence/other/pr-body-statements.2026-08-26T11-31.md` §1-§4 (11 -> 12 fields, zero -> real EFC durations, untruncated culture-invariant durations, no in-repo reader) plus the addendum. Obligation transfers to the actual PR body at PR-authoring time. |
| AC-25 cross-feature notes filed | PASS | CFN-4 promoted to issue #645 — reviewer-verified `gh issue view 645` returns `OPEN`; the number is written back into the CFN-4 section of `spec.md` (reviewer-verified at `spec.md:904-905`). CFN-1/CFN-3 routed to sibling 446 and CFN-2 to 468 via `evidence/issue-updates/cross-feature-notes-handoff.2026-08-26T11-32.md`; none fixed in this diff (reviewer-verified: the owning files are unchanged). |

## Verdict totals

- PASS: 24 (AC-22 with a stated, adjudicated exception)
- PARTIAL: 0
- FAIL: 1 (AC-19 — documented, parent-ratified deviation; deliberately and correctly left unchecked)
- UNVERIFIED: 0

No checkbox was flipped by this review: every PASS row was already checked by the executor, and AC-19 must remain unchecked to reflect the recorded deviation.

### Acceptance Criteria Status
- Source: docs/features/active/quickfiler-home-controller-metrics-442/spec.md
- Total AC items: 25
- Checked off (delivered): 24
- Remaining (unchecked): 1
- Items remaining: AC-19 (ownership boundary) — `git diff --name-only <merge-base>..HEAD` lists only the five owned production files, the two owned test files, and files under `docs/features/active/quickfiler-home-controller-metrics-442/`; not met because of the ratified one-line write to `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` (and, additionally observed by this review, two parent-orchestrator agent-memory paths).

## Plan reconciliation

`plan.2026-08-24T09-40.md`: 108 tasks; 106 checked, 2 unchecked — exactly [P7-T6] (ownership gate) and [P7-T27] (AC-19 check-off), both annotated as the documented deviation. Reviewer-verified by grep; no other unchecked task exists. This matches the intended, recorded state.

## Residual items for the PR body / epic close-out

1. Enumerate the two `.claude/agent-memory/orchestrator/**` paths alongside the EfcHomeControllerTests.cs deviation in the PR body's changed-file accounting (policy-audit PA-2).
2. Carry the AC-22 exception statement and the four AC-24 statements from `evidence/other/pr-body-statements*` into the actual PR body verbatim at authoring time.
3. Code-review follow-ups (all non-blocking): empty-lines write guard (CR-1), `FileIO2.WriteTextFileAsync` silent-failure/uncancellable-retry promotion candidate (CR-2), culture-sensitive date/time separators adjacent to #645 (CR-3).
