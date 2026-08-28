# Acceptance-Criteria Inventory (P0-T2)

Timestamp: 2026-08-27T19-53

Source: `FF/spec.md` -> `## Acceptance Criteria` (the sole AC source for work mode `full-bug`).
Documents read in full for this task: `FF/spec.md` (1037 lines) and
`FF/research/2026-08-24T09-12-breadcrumb-ordering-invariants-research.md` (1032 lines).

Line numbers below are the physical `spec.md` line on which the `- [ ] AC-nn` marker appears.
All 32 criteria are unchecked at the time of this inventory.

| AC ID | spec.md line | Group | Subject |
| --- | ---: | --- | --- |
| AC-01 | 884 | Defect fixes | #462 I-462.1 two-flag split, `_closeInFlight` cleared in a `finally` |
| AC-02 | 888 | Defect fixes | #462 I-462.2 reopen after a successful close reaches `_host.OpenAsync` |
| AC-03 | 891 | Defect fixes | #462 I-462.3/4/5 idempotent close, generation monotonicity, released terminality |
| AC-04 | 895 | Defect fixes | #500 I-500.1 `Monitor.IsEntered(lifetime._sync)` is `false` inside the guarded action |
| AC-05 | 898 | Defect fixes | #500 I-500.2 `Monitor.IsEntered(hub._sync)` is `false` inside a surface `PostJson` |
| AC-06 | 901 | Defect fixes | #500 I-500.3 re-entrant `Invalidate` still yields `TryRunCurrent == true` |
| AC-07 | 904 | Defect fixes | #500 I-500.4 re-entrant `Attach`/`Detach` during broadcast does not throw |
| AC-08 | 907 | Defect fixes | #501 I-501.1 two counting-and-throwing surfaces produce exactly 2 attempts |
| AC-09 | 910 | Defect fixes | #501 I-501.2 recording surface receives the payload despite a throwing sibling |
| AC-10 | 912 | Defect fixes | #501 I-501.3 fresh `Attach` replays a state a surviving surface received |
| AC-11 | 914 | Defect fixes | #501 I-501.4 + SR-3 per-surface failure logged; `PostJson` does not propagate |
| AC-12 | 916 | Defect fixes | #502 I-502.1 `RunSynchronous` returns `bool`; both call sites consume it |
| AC-13 | 918 | Defect fixes | #502 I-502.2 superseded `SetSuggestions` replaces `SuggestionsUpgrade` |
| AC-14 | 920 | Defect fixes | #502 I-502.4 superseded `AddItems` settles its lease; discard documented |
| AC-15 | 925 | Companion defect | I-502.3 every lease reaches `Settled` and `SourceDisposed`; no CTS leak |
| AC-16 | 931 | Failing-first tests | #462 regression test RED before, green after |
| AC-17 | 935 | Failing-first tests | #500 lifetime lock probe RED before, green after |
| AC-18 | 939 | Failing-first tests | #501 starvation test RED (1 attempt) before, green (2) after |
| AC-19 | 942 | Failing-first tests | #502 companion lease-leak test compiles on HEAD, RED there, green after |
| AC-20 | 949 | Must pass unmodified | `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` |
| AC-21 | 951 | Must pass unmodified | `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` |
| AC-22 | 954 | Must pass unmodified | `Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry` (I-501.5) |
| AC-23 | 959 | Structure/budget | SR-1 new partial part exists; exactly one `<Compile Include>` in `QuickFiler.csproj` |
| AC-24 | 964 | Structure/budget | New supersession test file; exactly one `<Compile Include>` in the test csproj |
| AC-25 | 968 | Structure/budget | No file in the change set exceeds 500 lines after the change |
| AC-26 | 970 | Ownership | The diff writes none of the six sibling-owned files |
| AC-27 | 973 | Determinism | No banned determinism construct in any added or modified test |
| AC-28 | 977 | Cross-cutting NFR | `TryRunCurrent`'s `bool` is the entry-time verdict only |
| AC-29 | 982 | Toolchain | CSharpier format applied and check reports no differences |
| AC-30 | 984 | Toolchain | Analyzer build completes with no analyzer errors |
| AC-31 | 986 | Toolchain | Nullable build clean, `/t:Rebuild`, no `/p:Nullable=enable` |
| AC-32 | 988 | Toolchain | Coverage-enabled test run green with no changed-line coverage regression |

Row count: 32 (AC-01 through AC-32, contiguous, no gaps).
