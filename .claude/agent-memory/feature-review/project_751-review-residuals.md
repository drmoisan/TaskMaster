---
name: 751-review-residuals
description: "#751 sync-barrier review: PASS/0 blocking, 10/10 AC; the plan's exactly-one-.coverage precondition is unsatisfiable under /InIsolation, and the reviewer recovered the blocked numeric pair with dotnet-coverage"
metadata:
  type: project
---

Issue #751 (terminal-hook test sync barrier, 3 test-only lines) closed **PASS / 0 blocking / 10-of-10 AC**, with 7 non-blocking findings.

**Why:** three things in this review generalize.

1. **A `COVERAGE_CAPTURE_BLOCKED` pair is usually recoverable — recover it instead of adjudicating around it.** The plan required the `*.coverage` search under a run's `/ResultsDirectory` to return exactly one file. Under `/InIsolation` vstest emits two (the published attachment plus the in-run `In\<machine>\` copy, byte-identical), so the precondition is unsatisfiable and *no* executor could pass it. Both the baseline and final capture tasks therefore recorded `Rung: 3` with no figures. The raw attachments were still on disk, so `dotnet-coverage merge <attachment> -f cobertura -o <scratch>.xml` produced the pair in ~2 min: all-module 75.890%→75.891% (vendor-inflated), first-party production 85.081%→85.059%, `TaskMaster.Test` +1 valid/+1 covered matching the diff's net +1 line. That turned a "blocked, adjudicate on the empty-subject argument" verdict into a closed record. See [[csharp-coverage-independent-verification-via-raw-coverage-conversion]] and [[csharp-repowide-coverage-below-80]].
2. **A synchronization barrier can trade a named failure for a hang.** The inserted `await run.Terminal` closes the race genuinely (increment → `Interlocked.Exchange` → `TrySetResult` on the *captured* generation gives a real happens-before edge), but a future "hook never invoked" regression now blocks forever instead of failing `Expected ... to be 1, but found 0`. Nothing bounds it: no `[Timeout]` on the method, no assembly-level attribute, and `_mstest-coverage.yml` invokes vstest with no `/Settings`, so `TaskMaster.runsettings` never applies in CI. Always check for a timeout bound when a review approves an added await in a test.
3. **TRX `notExecuted` + total-count parity is the cheapest integrity check on a "green-after" series.** Reading the TRXs directly gave 408/408 pre and post and 6984/6984 solution-wide, `notExecuted=0` everywhere — which falsifies `[Ignore]`, a narrowed filter, and a removed test in one pass, without trusting any executor artifact.

**How to apply:** on any TaskMaster review whose coverage evidence reads BLOCKED, look for `coverage/trx/**/*.coverage` before writing the verdict. Record the canonical-artifact-absence FAIL row separately from the substantive numeric verdict; the two have different dispositions.

Residual/owed (none blocking): `[Timeout(5000)]` recommended on `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`; the plan's locate step still carries the unsatisfiable precondition; the fail-before route-2 rationale led with the weak "line headroom" condition when the sound reason was that an instrumented red restates rather than reproduces the race. `artifacts/pr_context.*` in the item worktree were stale from issue #565 — see [[pr-context-artifacts-are-tracked-not-gitignored]].
