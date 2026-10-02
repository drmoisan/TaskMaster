---
name: project-927-r13-delta-text-violates-own-check-and-quoted-phrase-residue
description: Issue #927 round 13 (revision 1.16) seams - a returned wording delta can itself carry the contiguous segment its own check forbids; quoting a replaced phrase in the plan's own record re-introduces it for a phrase search; the Edit tool preserves CRLF on multi-line inserts; Bash may be disabled entirely so reflogs are read with Read
metadata:
  type: project
---

Round 13 on #927 applied three wording deltas (D1 to D3) that a confirming preflight returned as within the extended standing authority. Three seams surfaced.

**1. A returned delta's text can violate the check the delta prescribes.** D1 supplied replacement prose for constraint C1 and said "check that this new text itself does not carry a contiguous `.claude/worktrees` string". The supplied second sentence ("the hook refuses a command that carries the contiguous .claude/worktrees segment") spelled the segment. Applied verbatim it would have re-created the finding it closed.
**Why:** the atomic-plan contract's "check the delta against its own rule" clause is written for the reviewer, but the planner is the last hand on the text; a delta from a reviewer or coordinator is not exempt.
**How to apply:** before pasting any returned delta, run the delta's own stated check over the delta text. When it fails, reword the offending sentence descriptively (here: "the governance directory name, a slash and the word worktrees as one contiguous segment"), keep the meaning, and record the adjustment in the dispositions and the self-review as a deviation from the returned text.

**2. Quoting the phrase you replaced re-introduces it.** D3 replaced "read after a fetch and before any push" in spec.md. My first draft of the dispositions bullet and the Round 13 record quoted the old phrase verbatim, so a folder-wide search returned two hits in the plan and my own record's claim "zero times in either file after it" was false.
**Why:** a phrase-replacement delta is verified by absence; the plan's own audit trail is inside the search scope.
**How to apply:** describe a superseded wording without quoting it ("the clause no longer says that the read precedes any push"), then re-run the absence search over the whole feature folder, not only the file the delta named.

**3. Tooling facts for command-less sessions.** Bash was disabled for the whole session (not only isolation-filtered), so no git command ran. The worktree reflog (`.git/worktrees/<id>/logs/HEAD`) and `.gitignore` were read with Read to re-derive the revision commit (a6f59d260) and the six-line P2-T2 hunk (three comments, `*.trx`, `*cobertura*.xml`, blank). The Edit tool preserved the plan's CRLF convention on a multi-line insert (verified 1152 `\r$` matches over 1152 lines), so multi-paragraph insertions are safe in a CRLF file; verify with a `\r$` count against a `^` count after every round.

Related: [[project-927-r12-three-dot-anchor-and-control-ref-ancestry-seams]], [[plan-self-consistency-sweeps]].
