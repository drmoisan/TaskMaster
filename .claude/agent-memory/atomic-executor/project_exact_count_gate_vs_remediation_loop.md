---
name: exact-count-gate-vs-remediation-loop
description: An absolute pinned count in an acceptance clause goes stale two ways - an in-plan remediation path that adds tests, and an external PR that grows a READ-ONLY file; derive the count from a recorded command instead.
metadata:
  type: project
---

When a plan hard-pins a test gate to an exact figure (`PassedCount` = `TotalCount` = **19**) and
elsewhere carries a remediation path that says "add further unit tests, then restart the QA loop
from P#-T#", the two clauses contradict each other the moment the remediation fires: the restarted
gate can only be satisfied by recording a false number.

**Why:** Exact counts are added to close the zero-discovery hole (`FailedCount = 0` is also true of
a run that discovered nothing — see [[project-poshqc-pester-mcp-exit-minus1]]). Remediation paths
are added to close a coverage-shortfall hole. Both fixes are correct in isolation; the collision is
only visible when you read them together. Found on the #441 Cobertura plan at preflight iteration 3,
where the fix for SF-6 (coverage remediation) collided with the fix for SF-3 (TotalCount pinning)
introduced in the same round.

**How to apply:** The safe formulation is `PassedCount` = `TotalCount` = **B + N**, where B is the
baseline figure and N is the number of tests added under the named remediation path (N = 0 on the
first pass), with the in-force N recorded in the artifact, plus "a `TotalCount` below B fails this
gate". That keeps the zero-discovery guard intact without forbidding the remediation. Whenever a
preflight round adds a remediation-restart clause, re-scan every downstream numeric acceptance the
restart replays — the executor cannot reconcile a numeric mismatch at runtime without replanning.

## Second cause: an external PR grows a file the plan only READS

The same defect arrives from outside the plan. On the #498 breadcrumb plan, `[P0-T8]` required the
Phase 0 artifact to list "all **seven**" method names in the MUST-NOT-WRITE file
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`. PR #611 then landed on `main`
and added three `Issue609_*` methods (531 -> 694 lines, 7 -> 10 methods). Nothing broke at execution
time, because the plan writes neither file and carries no `file:line` citation into them — but the
acceptance clause became unsatisfiable as written, and the three NEW methods were precisely the
assertions a later phase was most likely to duplicate, which is what the read task exists to prevent.

**How to apply:** an absolute count over a file the plan does not own is a hostage to every sibling
PR. The landed remedy is the right shape and is worth copying: state the current figure as context
(`694 lines, ten test methods`), but make the ACCEPTANCE derive it — "record the command
`grep -c -F "[TestMethod]" <path>` with its `EXIT_CODE:` and numeric result, list exactly that many
names; the listed-name count MUST equal the measured count". Then name verbatim the specific
identifiers whose omission would actually cause harm, so the gate still fails on a silent regression
rather than degrading to a tautology. The same pattern rescues `[P7-T2]`: keep the absolute figures
as advisory and make equality with the in-execution `P0-T16` baseline the binding clause.

When an external PR lands mid-plan, sweep the plan AND the spec for the superseded numerals
(`531`, `983`, `985`, the spelled-out `seven`) and re-measure every row of any file-size table — but
expect legitimate survivors: historical "grew 531 to 694" provenance notes, a "version 1.0 figure"
column, and same-numeral facts about a DIFFERENT file. Confirm which by reading, not by count.

Related: [[project-preflight-selfderived-gate-thresholds-are-blind]],
[[project-418-plan-rationale-clauses-are-evidence]],
[[feedback-verify-line-citations-with-numbered-output]],
[[feedback-confirmatory-preflight-proportionate-bar]].
