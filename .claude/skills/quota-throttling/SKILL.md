---
name: quota-throttling
description: 'Quota throttling and account-switching policy for multi-agent orchestration. Use when pacing agent work against five-hour and seven-day usage limits, deciding whether and when to switch accounts, setting concurrency caps, forecasting a rate-limit wall, handling total weekly exhaustion, or designing a usage monitor. Covers account viability, failover-dependent pacing targets, the measured cost model, commit-before-the-wall, and the coordinator/agent division of labour.'
---

# Quota Throttling and Account-Switching Policy

Policy for keeping long-running, multi-agent orchestration inside account usage limits without
losing work. Derived from about five days of continuous multi-agent orchestration (TaskMaster runs
`bugs-2026-09-11` and `bugs-2026-09-17`).

Every figure marked **MEASURED** was observed; figures marked **ESTIMATE** are extrapolations.
Cost figures are machine- and repository-specific. Re-measure before relying on them elsewhere.

## When to Use This Skill

Use this skill when:

- a coordinating session runs orchestrators, epic runs, or parallel runs that spawn subagents;
- deciding whether to start, stagger, hold, or stop agent work because of usage limits;
- choosing whether and when to switch between accounts;
- a rate-limit wall is forecast, or every account is exhausted;
- designing a monitor that watches usage or CI state.

## Reading Quota

The coordinator reads usage from the account switcher, for example `cswap list --json`. The
fields each rule below depends on:

| Field | Meaning |
|---|---|
| `accounts[].active` | Whether the account currently supplies credentials. |
| `accounts[].usageStatus` | Must be `ok`; any other value makes the account non-viable. |
| `usage.fiveHour.pct` | Five-hour window utilization, an integer percentage. |
| `usage.fiveHour.resetsAt` | Authoritative ISO-8601 UTC reset time. `null` means the window has not started. |
| `usage.sevenDay.pct` | Seven-day utilization, an integer percentage. |
| `usage.sevenDay.resetsAt` | Authoritative ISO-8601 UTC weekly reset time. |
| `usage.sevenDay.willLastToReset` | The switcher's own projection of whether the weekly budget lasts. |
| `usage.*.clock`, `usage.*.countdown` | Human-readable display values. `clock` is LOCAL time; do not use it for arithmetic. |

Some switchers switch automatically on the five-hour threshold alone and do not consult the
seven-day window. Such a switcher can hand over to an account whose five-hour window has reset
but whose weekly budget is spent. Apply section 1 yourself rather than relying on the switcher.

## 1. Account Viability

An account is a viable target only if BOTH hold:

- seven-day usage < 95%
- five-hour usage < 90%

The weekly gate dominates. An account at >= 95% weekly is NOT a viable target regardless of its
five-hour state, and must be excluded from all pacing math.

The binding budget is `min(five-hour headroom to 90%, weekly headroom to 95%)`. Compute both every
time. Late in a week the weekly headroom is usually the smaller number, and the five-hour window
is then irrelevant.

## 2. Pacing Target Depends on Whether a Failover Exists

**Two or more viable accounts: front-load.** Target about 90% of the five-hour window in about
2 hours (about 45% per hour). Reaching 90% triggers a switch, so the unused remainder of the
window costs nothing.

**Exactly one viable account: spread.** Target about 90% across the WHOLE five-hour window (about
18% per hour). Front-loading here is wrong: reaching 90% at the 2-hour mark produces a stall of
about 3 hours with no account to switch to.

These are different policies, not different settings of one policy. Applying the two-account
target to a one-account situation is the most expensive mistake observed (see section 12,
item 1).

## 3. Cost Model (MEASURED, Serialized Single Items)

| Activity | Five-hour points | Weekly points | Wall clock |
|---|---|---|---|
| Coordination only (API calls, git, checkpoint writes, no agents) | ~3 total | negligible | — |
| One item, full cycle (child + executor + toolchain + coverage run) | ~17 | ~3 | ~43 min |
| One plan revision, or a low-cost item | ~4-5 | — | — |
| Three items entering Phase 0 simultaneously | 96-108 per hour | — | a full five-hour window in 62 min |

Builds and test runs consume wall-clock time but zero quota. A long quiet period at 0% per hour
with agents alive usually means they are inside a build, not stalled.

Ratio (ESTIMATE, two data points): one full five-hour window is about 14 weekly points.

## 4. Concurrency Control

A concurrency cap governs an orchestrator's DIRECT children. Each child spawns its own
grandchildren (executors, planners, researchers), and those are the dominant consumers. Always
express a cap as a budget on TOTAL AGENTS IN THE SUBTREE, and require that it be propagated
verbatim to every level.

A cap change only restricts NEW starts. It does nothing to work already running. Cutting a cap
from 6 to 2 mid-flight changed the burn rate by zero, because the cost was in agents already
alive.

For in-flight burn there are exactly two controls:

- **Commit-and-hold** — finish the current task, commit, start nothing new.
- **Stopping agents outright** — destructive; use only as a last resort.

**Stagger starts.** The dominant cost driver is not item count; it is simultaneous Phase 0
cycles. Bring items in one at a time and let each settle before starting the next.

## 5. Measurement Discipline

- The usage percentage is an INTEGER. At 5-minute sampling the computed rate quantizes to
  multiples of about 12% per hour. Do not react to a single sample; measure over spans of at
  least 15 minutes.
- The FIRST sample after an account switch is inflated by batched accounting. Take a second
  reading before acting on it, but do not dismiss it either; it has been partly real more than
  once.
- Reset times: the human-readable `clock` field is LOCAL time. The ISO `resetsAt` field is
  authoritative. Convert before doing arithmetic; a four-hour error from this changed a plan.
- Five-hour window elapsed time = 300 − (minutes until reset). The window starts on first use,
  not on a fixed clock boundary.
- VERIFY a reset directly before resuming. Never infer it from the clock. A reset timestamp can
  pass while the counter still reads the old value for a minute or two.

## 6. Approaching a Wall

Commit BEFORE the wall, not at it. A rate-limit error terminates an agent immediately; the agent
does not wait out the gap and resume. The duration of the outage does not change this risk: even
a 60-second gap terminates in-flight work.

Sequence when a wall is forecast:

1. Order all in-flight children to COMMIT NOW, in whatever state, without finishing the current
   task. A committed partial result is recoverable; an uncommitted complete one is not.
2. Let them keep working until the wall actually arrives.
3. Set the commit checkpoint with real margin, and recompute it as the rate moves. A checkpoint
   that lands ON the forecast wall protects nothing.

Git operations are LOCAL and consume no model quota; they succeed on a fully exhausted account.
If errors start before committing finishes, keep retrying the COMMIT specifically and nothing
else.

## 7. Total Weekly Exhaustion

When no account is viable:

1. **Serialize** — stop all parallel activity; zero new starts.
2. **Commit ALL working documents across ALL worktrees** — every item worktree, the plan-home
   worktree, and every in-flight child, not only the current item.
3. **Report and hold.**

This differs from a routine pause: the outage may last days, so nothing may be left uncommitted
on the assumption of a quick return.

## 8. Switching

- Switch at 90% of the five-hour window IF a viable target exists.
- If no viable target exists, do NOT switch; hold until the earliest reset. Switching onto an
  exhausted account produces immediate failures.
- Verify the switch landed (active account, both windows) before releasing work.
- A mid-flight credential change is transparent to running agents. Instruct them explicitly not
  to treat the change, or a brief error around it, as a failure signal.

## 9. Usage Credits and Overage

Never spend credits on work. The ONLY sanctioned use is completing a non-destructive
serialization and commit so that work is not lost. If any path offers to continue past a limit
by consuming credits, stop and ask the user.

## 10. Reserve Policy

Holding an account's last few points as an emergency commit reserve is correct ONLY while
something is uncommitted. Verify first: if every branch is pushed and every worktree is clean,
the reserve protects nothing and should be spent on useful work instead. Check before assuming.

## 11. Monitor Design

Report on thresholds, not on every sample. An idle monitor that fires every ten minutes costs a
turn each time and hides the signal among routine readings.

**A check for the ABSENCE of a bad state is satisfied by the absence of ALL state.** A CI waiter
polling for "no pending checks" reported SETTLED during the window between one run being
superseded and the next registering, when no checks existed at all. Require a POSITIVE assertion
instead: a conjunction that an empty set cannot satisfy (for example, "at least one required check
exists AND none is pending").

Alert on:

- threshold crossings;
- a sustained rate above target;
- a non-`ok` status;
- account-viability changes;
- window rollovers.

Stay silent otherwise.

## 12. Observed Failure Modes

1. Applying the two-account front-load target with only one viable account.
   Cost: about 95 minutes of idle time.
2. Cutting a concurrency cap to control burn from work already running.
   Cost: none directly, but the effective control went unused while the window drained.
3. Launching multiple items into simultaneous Phase 0.
   Cost: a full five-hour window in 62 minutes.
4. Assuming a fallback account was fresh without checking.
   Cost: about 1 hour, and seven items' preparation output stranded uncommitted when agents were
   terminated.
5. Setting a commit checkpoint that landed on the forecast wall instead of before it.
6. Reacting to single 5-minute samples inside the quantization band, producing cap oscillation
   that achieved nothing.

## 13. Division of Labour

An orchestrating agent generally CANNOT see quota. Do not ask it to estimate usage. The workable
split is:

- the **agent** reports its COST PROFILE: how many agents are alive, what kind of work they are
  doing, and what it is about to start;
- the **coordinator** supplies the MEASUREMENT and the decision.

Have the agent announce before starting each item, and reply with a live reading.

Corollary: if the coordinator is slow to respond, the agent should proceed on its own rather than
stall. A four-hour idle period occurred because an agent correctly held for a ruling that was
never sent. Absorbing a small pacing error is preferable to repeating that.
