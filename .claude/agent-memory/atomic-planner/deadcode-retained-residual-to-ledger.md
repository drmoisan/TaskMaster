---
name: deadcode-retained-residual-to-ledger
description: When dead prod code blocking a coverage floor CANNOT be deleted (deletion routed to another issue/child), plan a measured residual routed to the ratifying ledger — never deletion, exclusion, or reflection-invoking private statics
metadata:
  type: feedback
---

When unreachable dead production code blocks a per-file coverage floor **and the orchestrator has
routed its deletion out of scope** (a separate issue, or a later child, to avoid a same-file conflict),
the plan must:

1. state the floor risk explicitly in a named plan note before the affected phase;
2. cover only the members that are *honestly* reachable (e.g. an `internal static` pure helper the
   test assembly can call directly through `InternalsVisibleTo`);
3. measure the real per-file rate in a task; and
4. if the rate misses the floor, write a residual dossier **requesting** ratification from the
   authoritative ledger, naming the retained region and the sequenced remedy issue.

**Why:** this is the inverse of [[project_deadcode_removal_vs_coverage_exclusion]]. That memory's
answer — shrink the denominator by deleting the dead code — is unavailable when deletion is owned by
someone else. The three tempting substitutes are all policy violations: adding
`[ExcludeFromCodeCoverage]` (the epic treats that as Blocking on a testable file), reflection-invoking
caller-free `private static` members purely to inflate the numerator (noise, not assurance), and
deleting the region anyway (creates the exact merge conflict the routing was designed to avoid).
A child may not ratify its own exemption; only the ledger owner can.

**How to apply:** epic #136 child F6 (issue #435), `QfcExplorerController.cs` — the dead
`#region Email Sorting To Rewrite` (lines 183-321) stays because issue #449 sequences its deletion
after F6 merges. Reach for this shape whenever an orchestrator fact says "do NOT plan the deletion"
about code that is simultaneously the reason a coverage gate cannot pass.
See also [[named-coverage-exception-verify-member-body]].
