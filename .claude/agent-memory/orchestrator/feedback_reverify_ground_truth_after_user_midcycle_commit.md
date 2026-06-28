---
name: reverify-ground-truth-after-user-midcycle-commit
description: After the user rebases or commits mid-remediation-cycle, re-verify ground truth and re-plan before executing a previously-preflighted plan
metadata:
  type: feedback
---

When the user does their own git work mid-cycle (rebase onto main, or a manual
commit that advances the remediation), do NOT execute the already-preflighted plan
verbatim. Re-verify ground truth first, then revise the plan and re-run validator +
preflight before delegating execution.

**Why:** On issue #218 cycle 2, a plan was authored, validator-passed, and
preflight-ALL-CLEAR against production files at 790/739/1370 lines. The user then
rejected the execute handoff, rebased onto main, and committed `2637e4c1` which split
the production controllers itself (files dropped to 432/454) and half-completed the
test split (created four split test files but left the 1370-line original untrimmed
and the splits unwired in the test csproj). Executing the stale plan verbatim would
have tried to re-extract regions that no longer existed and would have failed.

**How to apply:** On resume / before any execute handoff, run a ground-truth probe:
`git log --oneline`, `git merge-base HEAD main`, current line counts of the target
files, and csproj wiring of any new files. If the baseline shifted, append a
"Ground-Truth Update" section to the cycle `remediation-inputs`, have `atomic-planner`
revise the plan in place to verification-only for the work the user already did and
to completion-only for what they half-finished, then re-run the MCP validator gate and
a fresh `atomic-executor` preflight. Honor the user's own reorg as canonical (see
[[feedback_verify_flat_artifact_layout_after_executor]]); only the plan adapts. The
preflight iteration counter is cumulative across these re-plans.
