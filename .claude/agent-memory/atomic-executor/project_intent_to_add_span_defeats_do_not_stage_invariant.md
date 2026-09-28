---
name: intent-to-add-span-defeats-do-not-stage-invariant
description: A plan invariant of "never stage directory X" is defeated by an unscoped `git add --intent-to-add -- .` in an earlier task, because -N is a staging span that most reviewers read as diff plumbing
metadata:
  type: project
---

When a plan carries an invariant of the form "this delivery must neither stage nor be failed by
paths under `<dir>`", enforcing it only on the explicit `git add -- <pathspec>` spans and on the
terminal `git status --porcelain` spans is not sufficient. `git add --intent-to-add -- . ":(exclude).claude"`
is itself a staging span: it writes an index entry for every untracked path under the root except
the one exclusion, including another item's queued promotion file under `docs/features/potential/`.

**Why:** the `-N` span is normally authored as plumbing for a later `git diff --numstat`, whose
blindness to untracked files is what the companion span exists to fix (rule G8b). Because its stated
purpose is diff visibility rather than staging, a reviewer checking "which tasks stage what" reads
the explicit `git add` spans and the commit span and skips it. The `-N` entry does not reach the
commit — `git commit` ignores `CE_INTENT_TO_ADD` entries — so the contamination is index-only and no
gate in the plan reports it. That is precisely why it survives a review round.

**How to apply:** during preflight, grep the plan for every `git add` occurrence, including
`--intent-to-add`, and check each one against every "do not stage X" clause in the plan, not only the
commit tasks. The fix is one pathspec per span: append `":(exclude)<dir>"` to each `-N` span and to
its companion numstat span. Adding it to the numstat span is inert for the comparison, because a path
excluded from both the before and the after run contributes the same figure to each.

Related: [[project_preflight_blanket_assertion_and_forward_dependency]],
[[project_revision_bullet_negates_earlier_clause_left_standing]],
[[project_agent_memory_tracked_breaks_unscoped_git_gates]].
