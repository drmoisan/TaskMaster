---
name: project-973-r5-positional-restore-rule-and-enumeration-self-hit-recurrence-seams
description: "#973 revision 5 (preflight round 4, 3 text deltas) - a dictated 'reverse the deletion' restore rule phrased by line POSITION is ambiguous after the deletion shifted lines and silent on the order of an adjacent directive pair (clarify by text identity + ordering, flag the addition); the self-review enumeration self-hit recurred a third time (quoting the superseded literal and the digit I declared absent); re-measure every figure a delta dictates with the delta's own pattern before writing it into a spec amendment."
metadata:
  type: project
---

Seams met while applying the preflight round-4 deltas to the issue #973 plan (revision 1.4 to 1.5, 2026-10-03). Worktree `.claude/worktrees/agent-a24d410b914bcefd7`. Same session shape as R4: no Bash tool, so "no staging" is provable only by construction.

**Why:** each is a place where a verbatim delta would have left a wrong or unverifiable clause.

**How to apply:** when a reviewer dictates a repair-branch Edit by line position, when a delta states repo-wide counts for a spec amendment, and every time the section 14 enumeration is written.

1. **A positional restore rule is ambiguous once the deletion has shifted lines.** The delta said the old_string is "the line that now occupies the line after that directive's fact 10 position". After the deletion the following line sits AT the directive's former position, so a literal reading picks the line one too low and the restore lands one line late (numstat `1<TAB>1`, not an empty diff). The Edit tool matches text, not position, so restate the rule by text identity: the following line fact N names, unique in the file (the forward EDIT's own gate proves uniqueness). Apply the delta verbatim and append a marked `(planner clarification, revision X)` rather than rewriting the reviewer's words; name the addition in the log and the handoff so the next preflight can accept or strike it.
2. **An adjacent directive pair restored one at a time has an order problem.** Inserting the line-11 directive "before the following line" after the line-12 directive is already back yields 12, 11, Outlook: a swap, so the diff is one add plus one delete and the dictated `1<TAB>96` is false. State the second-restore old_string explicitly (the already-restored directive line).
3. **Re-measure dictated figures with the delta's own pattern.** The delta gave 83 (any brace then `#region`) and 77 (class line above); the Grep tool multiline with type cs reproduced both (83/79 files, 77/74 files, blank-line form 0). Record the measurement in the log entry with the patterns so the figures are the planner's, not a transcription.
4. **The enumeration self-hit recurred (third time on this issue).** I wrote "no remaining `<old literal>`" and "the `81` hits: fact 6 only" into the Round 5 block; both statements created the hit they denied. Rule, now mechanical: after writing section 14, re-run the exact superseded-literal Grep and the whole-word digit Grep; describe superseded literals in prose (word forms, "the switch directly after its SyncWindow argument", "eighty-one").
5. **Validator MCP absent again.** `mcp__drm-copilot__validate_orchestration_artifacts` was not in the tool surface; report it and hand to preflight, do not claim validator success.

Related: [[project-973-r4-member-multiset-formatter-blank-lines-and-optional-trailer-seams]], [[project-973-r2-log-entry-self-hit-pester-unroll-and-figure-carrying-artifact-seams]], [[verify-caller-supplied-citation-corrections]], [[plan-self-consistency-sweeps]].
