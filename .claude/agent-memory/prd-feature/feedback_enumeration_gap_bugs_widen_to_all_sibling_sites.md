---
name: enumeration-gap-bugs-widen-to-all-sibling-sites
description: For a defect whose root cause is "a guard applied at one site and not carried to its siblings", put every sibling site the research enumeration found into the write set instead of filing them as out-of-scope follow-ups
metadata:
  type: feedback
---

When the defect class is an *enumeration gap* — a guard, idiom or fix applied at one site and not
carried to its siblings — the spec's write set must include every additional site the research
enumeration identified, not just the sites the GitHub issue happens to name. File as out-of-scope
only findings of a genuinely different defect class (resource leaks, dead code, unrelated null
paths), and preserve those verbatim in Non-Goals with citations so they survive the merge.

**Why:** Fixing only the issue-named sites reproduces, inside the fix for an enumeration-gap defect,
exactly the enumeration gap the issue exists to close. On #821 (2026-09-09) the research record filed
`EfcHomeController.cs` and `ProgressPane.cs` as out-of-scope; the orchestrator overrode that and put
both in scope. `ProgressPane` was additionally the *live* production surface while the issue-named
`ProgressViewer` was the dormant one, so the narrow write set would have shipped a fix for the less
reachable of the two identical defects.

**How to apply:** Before accepting a research file's out-of-scope table, check each row against the
defect class in the Root Cause Analysis. Widening requires four verified preconditions, which the
spec must state: no sibling feature owns the file; the corresponding test file is not in another
feature's population; every added file already has a `Compile Include` entry so no project file is
edited; and no fan-in conflict is possible. Record the override explicitly as superseding the
research record on the named rows, with the rationale, so a reviewer does not read it as scope creep.
Related: [[ac-gates-verify-satisfiability]], [[backticked-paths-are-the-change-footprint]].
