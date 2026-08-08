---
name: csharpier-formats-xml-print-width
description: CSharpier 1.3.0 formats XML (not just .cs) and enforces its 100-column print width, so an "avoidable reformatting churn" finding on an XML resource can be formatter-mandated and unsatisfiable
metadata:
  type: project
---

CSharpier **1.3.0 in this repo formats `*.xml`, not only `*.cs`**, and enforces its default **100-column print width** on them. `.csharpierignore` excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets` — but **not** `*.xml` generally, so `TaskMaster/Ribbon/RibbonExplorer.xml` is formatter-governed.

**Why:** Issue #503 remediation cycle 1 pinned a finding (F2) asserting that expanding three `<button>` elements from one line to six was "incidental churn with no functional purpose", and required collapsing them back to single-line while keeping a newly added attribute. The collapse passed every scoped gate at 524 lines, then the repo-wide `csharpier check .` failed with CSharpier's *Expected* output showing the six-line form. Arithmetic: the merge-base single-line `<button id="TriageSetA" onAction="TriageSetA_Click" label="Set A" />` is **78 chars**; adding `getEnabled="EngineCommand_GetEnabled"` makes it **116 chars**, over the 100 limit. The multi-line expansion was formatter-mandated, not gratuitous. The plan's own section 3 rule 6 ("CSharpier does not format XML") was false, and the F2 acceptance gate (<= 527 lines) was unsatisfiable while the mandatory format gate must pass.

**How to apply:**
- Before accepting any plan/review finding that an XML (or other non-`.cs`) reformatting is "avoidable churn", measure the resulting line length against 100 columns and run `csharpier check .` on the candidate form. A scoped gate passing proves nothing; only the repo-wide check does.
- When a pinned edit turns out to conflict with a mandatory gate mid-execution: **revert the edit**, restore the gate to green, restart the phase per its own loop semantics, and escalate the finding as *not remediable as specified* with the measured arithmetic. Do **not** add the file to `.csharpierignore`, raise `printWidth`, or accept a red format gate — all three are gate-weakening or scope-widening.
- The only route to shrinking such a file is splitting the resource, which is its own issue.

See also [[project_sln_csproj_edit_crlf_preserve]] and [[project_csharpier_pipefiles_nonenforcing_gate]].
