---
name: executed-phases-drift-the-plans-own-citations
description: A plan's line citations decay from its OWN executed phases, not just from merging main; whether it matters depends on append-past vs insert-before
metadata:
  type: feedback
---

A merge is not the only thing that invalidates a plan's line citations. The plan's own
already-executed phases move them too, and that source is easy to rule out by mistake.

**Why:** On issue #796 I authored a re-anchor delta that told the planner the project-file
citations should stand, reasoning that the merge had touched neither `.csproj`. The planner
tested the claim instead of accepting it and found both files *had* moved — because the
plan's own executed Phase 1 had inserted `<Compile Include>` entries into each. My framing
pointed away from the finding; only the instruction to re-derive rather than assume recovered
it. Later the same run, a preflight reviewer flagged four files as probably drifted; I
measured and only two had.

**How to apply:** After any phase executes, treat every citation in the *unexecuted* tasks as
suspect, from both causes. Then discriminate by mechanism, because it halves the work:

- An edit that **appended past** the cited region leaves every earlier citation intact. On #796
  the new interface member landed at line 243 and the new test method at line 255, both after
  everything cited, so two of the four suspected files were fine.
- An edit that **inserted before** it shifts everything after. A 12-line pure move shifted
  `FinishClose` from 439 to 426.

Check whether any *acceptance condition* reads a drifted number before deciding severity. On
#796 the five `[ExcludeFromCodeCoverage]` line numbers that `P9-T7`'s acceptance reads had not
moved, which downgraded the whole finding from blocking to instruction-position-only. Never
edit a citation inside an already-executed, checked-off task: it was correct when it ran and
rewriting it falsifies an audit record. Disclose the current value elsewhere instead.

The durable fix is to stop citing by line at all. See [[preflight-catches-vacuous-gates]] for
the related class where a gate reads a figure that cannot fail.
