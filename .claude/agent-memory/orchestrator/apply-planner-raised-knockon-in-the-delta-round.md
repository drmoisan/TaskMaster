---
name: apply-planner-raised-knockon-in-the-delta-round
description: When the delta-applying planner flags a gap in the reviewer's own delta, fix it in the same round as a declared knock-on; it saved a preflight round on #968
metadata:
  type: feedback
---

When the planner applying a reviewer's verbatim delta reports that the delta itself leaves a gap (on #968 round 5, `RESTART-CORRECTED:` named only the latest restart correction, so a two-restart run still failed P6-T2), apply the widening in the SAME round as a declared knock-on edit, then disclose it in the next preflight prompt. Round 6 confirmed the union wording and cleared with 0 defects.

**Why:** a "verbatim" instruction plus a known residual gap almost guarantees the confirming round reports it, costing a round. The coordinator's brief also asked for a knock-on check on the restart path, which covers this.

**How to apply:** relaunch the planner with a narrow brief (exact old/new text, update the revision bullet). Do not, conversely, elect a reviewer's OPTIONAL observation after ALL CLEAR: it changes the cleared blob ([[do-not-elect-reviewer-declined-optional-changes]]).
