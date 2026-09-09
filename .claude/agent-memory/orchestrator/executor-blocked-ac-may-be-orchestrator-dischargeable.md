---
name: executor-blocked-ac-may-be-orchestrator-dischargeable
description: An acceptance criterion the executor reports as PARTIAL for a missing tool is often one the orchestrator can discharge itself - check your own tool surface before accepting the gap
metadata:
  type: feedback
---

When `atomic-executor` returns an acceptance criterion as PARTIAL because a tool is **absent from its
tool surface**, check your own surface before accepting the gap. The executor holds only
`mcp__drm-copilot__run_poshqc_*`; the orchestrator additionally holds the whole promotion lifecycle
(`new_potential_bug_entry`, `potential_to_issue`, `new_active_feature_folder`) and
`collect_pr_context`. A criterion requiring "a pointer to a separate promotion or issue raised for
it" is therefore unreachable for the executor and routine for you.

**Why:** the executor was right to refuse. Plan task P6-T14 on issue #815 would have *permitted* the
check-off on a `POSTING BLOCKED` artifact, but the criterion's own text demanded a pointer that did
not exist, and marking it delivered would have asserted a fact the run could not verify. The failure
mode to avoid is the opposite one: reading the executor's PARTIAL as a settled limitation of the run
and shipping a 13/14 when 14/14 was one MCP call away.

**How to apply.**

1. Read the blocked artifact. A well-written one records the intended text in full precisely so you
   can act on it — treat that as a work order addressed to you, not as documentation.
2. Check the footprint criterion before promoting. A promotion writes under
   `docs/features/potential/`, which normally *breaks* a scope-boundary AC — see
   [[footprint-ac-forbids-onbranch-followup-promotion]]. But a plan author who anticipated the
   promotion may have admitted that prefix deliberately: on #815 the plan's decision D6 listed
   `docs/features/potential/` as one of five permitted prefixes with the stated reason that AC14
   requires a promotion and the lifecycle is file-based. When the plan already admits the prefix,
   on-branch promotion is sanctioned and the usual bind does not apply. Re-verify the scope gate
   after committing regardless.
3. Fill every canonical template heading before `potential_to_issue`, then confirm the body carries
   zero `not provided in potential file` placeholders — see
   [[potential-to-issue-keeps-only-summary-section]].
4. Update the blocked artifact with a `## RESOLUTION` section carrying the issue number and URL,
   leave the original `POSTING BLOCKED` text intact as the audit trail, then check the criterion off
   and correct the AC-status artifact's counts.

**A related asymmetry worth remembering:** a PR based on an epic integration branch does **not**
auto-close its issue, even with a correct `Closes #NNN` bullet. GitHub honours a pull-request closing
keyword only when the PR targets the repository default branch. Expect the issue to stay OPEN after
the child merge and leave it to the epic parent's integration-to-main merge; do not force-close it,
because the work is not yet on main.
