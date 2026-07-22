---
name: deadcode-removal-vs-coverage-exclusion
description: when a coverage gate is blocked by provably-unreachable dead production code, plan removal (shrink the denominator), never an exclusion/carve-out/forced-rethrow
metadata:
  type: project
---

When a below-threshold coverage unit is blocked by provably-unreachable dead production code (e.g. #400 `BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` at 24/28, whose inner recovery `catch` at lines 153-156 can never execute because `HandleOpenFailureAsync` routes every failure to `BreadcrumbUiDispatcher.Report`, which cannot throw for a non-null exception), the repo-policy-favored resolution is to REMOVE the dead code so the denominator shrinks to 24/24.

**Why:** `.claude/rules/general-unit-test.md` forbids excluding any production file from coverage and requires untestable lines be refactored out; the simplicity-first design principle favors removing dead defensive code. Adding a coverage exclusion, documenting a `24/28` acceptance carve-out, or making a collaborator rethrow to force the catch reachable are all rejected.

**How to apply:** Plan a bounded one-production-file batch (authorization/unreachability-proof inventory → delegate edit → csharpier/analyzer/nullable/focused-vstest gates → behavior-preservation+scope+anti-masking ledger → re-run the coverage gate). State in the plan text that dead-code removal is expressly NOT a masking action and distinguish it from the [[plan-validator-task-id-sequential-constraint]] anti-masking prohibitions (no weakened/deleted assertions, sleeps, retries, [DoNotParallelize]/[Ignore], filter narrowing, coverage/threshold changes) with a one-line unreachability justification. Removal is production-only, so the instrumented case total is unchanged unless an optional comment-only test touch is authorized; verify a test that appears to target the dead lines actually asserts behavior on the reachable path (its report may come from an inner catch elsewhere) so its assertions stay byte-identical.
