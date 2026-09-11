---
name: project_issue_751_five_round_preflight_detached_launch_convention
description: Issue #751 plan needed 5 preflight rounds because it required a novel detached-launch/durable-exit-code PowerShell convention for full-suite vstest runs exceeding the agent tool timeout
metadata:
  type: project
---

Preparation-mode delivery for GitHub issue #751 (TaskMaster,
`bug/terminal-notification-hook-test-lacks-sync-barrier-751`, prepared under
`parallel_slug: bugs-2026-09-02`) took 5 atomic-executor preflight rounds to clear, well past
the atomic-plan-contract's 2-round target.

**Why:** The plan's actual code fix (one line awaiting an existing `run.Terminal` signal, plus a
counter-synchronization hardening) was simple and cleared in round 1's citation review. What drove
the extra rounds was that the plan also had to invent a PowerShell mechanism for running full-suite
`vstest.console.exe` under `/EnableCodeCoverage` — those runs exceed the agent tool's call timeout,
so the plan needed a detached-launch pattern (`Start-Process` without `-Wait`, poll for completion
in separate short invocations, write the exit code durably to disk via try/finally so it survives
the launching shell exiting). Each of rounds 2-4 found a new defect specifically in that mechanism
(unguarded exit-code write that could silently record a false 0, no bounded inter-poll wait so a
healthy long run got killed as "hung", stale TRX files surviving a relaunch and satisfying a
completion witness with the wrong run's data, wrong-pid termination hazards from pid reuse). Once
the mechanism itself was correct, round 5 confirmed everything (including a dozen already-correct
prior-round fixes) and cleared.

**How to apply:** When a plan requires inventing a new execution-infrastructure mechanism (not just
applying an existing repo convention), budget for 3-5 preflight rounds concentrated on that one
mechanism, not the 2-round target that applies to plans using only established patterns. The
round-over-round convergence signal (`CONVERGENCE: FURTHER ROUNDS LIKELY` narrowing to "same
convention, not the whole plan") is a reliable signal that the process is converging even when the
round count is high — each round after round 1 found defects in a strictly shrinking region of the
plan (5 blocking region-wide -> 3 blocking in one convention -> 4 blocking in the same convention ->
3 blocking in the same convention -> 0).

Also: two defects were found not by the reviewer's stated review scope but by the planner's own
adversarial self-review noticing a sibling problem while fixing something else (round 3: research's
"six sibling sites" was itself an undercount, real count is seven; round 4: P1-T1 didn't produce the
`Timestamp:` field P1-T3's gate required). Both were real and both were caught before the confirming
round, validating that the mandatory self-review pass (not just the external preflight) catches
distinct defect classes.

See also [[preparation-child-cwd-is-session-root-not-item-worktree]] for an unrelated environment
issue hit in the same run.
