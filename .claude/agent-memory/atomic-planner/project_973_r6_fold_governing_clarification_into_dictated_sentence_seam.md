---
name: 973-r6-fold-governing-clarification-into-dictated-sentence
description: Preflight round 5 on #973 — when a planner clarification appended to a dictated sentence is ruled governing, the next round replaces the dictated sentence with the clarification's content as ONE sentence (no trailing "planner clarification, revision N" parenthetical); verify the quoted following-line literal in the file and record count 1
metadata:
  type: project
---

On #973 revision 1.5 the planner kept a reviewer-dictated restore rule phrased by line POSITION and appended a parenthetical "(planner clarification, revision 1.5: ...)" restating it by TEXT identity. Preflight round 5 ruled the clarification correct and governing and sent one delta: replace the dictated span with a single sentence whose primary clause is the text-identity rule, whose exception (the CategoryClassifierGroup.cs line-12 directive line when line 11 is restored second) sits inside the sentence, and whose new_string clause follows after a semicolon.

**Why:** a sentence plus a parenthetical that contradicts it leaves the executor two rules for one Edit; the preflight treats that as a conflicting instruction even when the parenthetical is the right one. The clarification belongs in the body of the rule, with the dictated wording removed, not alongside it.

**How to apply:**
- When a reviewer accepts a planner clarification, fold it in as the rule and delete the superseded wording in the same edit; do not keep both and do not label the surviving text as a "clarification".
- Quote the concrete following-line literal in the task text (here `using Microsoft.Office.Interop.Outlook;`) and re-verify it against the file in the same pass: Grep `^<literal>` count 1 in the target file plus the `-A 1` directive Grep across `*.cs`, and cite the line number in the Round N enumeration.
- Post-edit sweep for the superseded phrase AND for the revision-label phrase (`planner clarification, revision 1.5`) — both must be 0 in the plan.
- Also re-stamp the stale "Revision 4 ... has been applied" parenthetical under the bounded record; it had drifted two rounds and belongs to the handoff record, so updating it is in scope of a "no other change" round. Report it explicitly.
- Keep the earlier log entry that describes the superseded two-part wording; record in the new entry that it is not rewritten.

Related: [[973-r5-positional-restore-rule-and-enumeration-self-hit-recurrence-seams]].
