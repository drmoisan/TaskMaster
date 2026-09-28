---
name: delegation-brief-silently-overrides-recorded-deviation
description: A delegation brief that contradicts an entry in your own checkpoint's recorded_deviations will be obeyed by the delegate without challenge; re-read recorded_deviations before briefing on the same artifacts, and verify the commit file list afterwards
metadata:
  type: feedback
---

Before writing a delegation brief that touches an artifact class you have already ruled on, re-read
`recorded_deviations` in your own checkpoint. A brief that contradicts a prior ruling will be carried
out. The delegate has no way to know the ruling exists, and will not push back.

**Why:** on item 871 the checkpoint already carried a deviation `evidence-hygiene-cobertura` whose
resolution was explicit: the raw Cobertura document is written to the plan's stated path so every
acceptance condition that reads it stays satisfiable, but it is **never staged**, and it is deleted
after its last consumer and before the porcelain check that would see it. The run coordinator's brief
said the same thing in one line: commit projections only, never a raw `.trx` or `.cobertura.xml`.

I then wrote a delegation brief containing "The plan's P0-T12 does require the Cobertura XML itself as
a named artifact — that one is mandated by the plan and is committed." That sentence was wrong on both
counts and contradicted both rulings. The executor complied exactly, and the Phase 0 commit carried an
11.58 MB raw Cobertura document containing the full instrumented tree.

The reasoning error is worth naming because it is seductive: the plan DOES name the `.cobertura.xml`
as a required artifact path, so "the plan mandates it" is true — but it mandates *writing* it, not
*committing* it. Producing an artifact and tracking it in git are different acts, and a plan that
names an output path is silent on the second.

**How to apply:**

- Re-read `recorded_deviations` before briefing on evidence artifacts, coverage documents, trx files,
  or anything else you have previously ruled on. Quote the prior ruling into the brief rather than
  re-deriving it, so the delegate enforces it instead of overriding it.
- After every delegated commit, verify the **file list**, not just the summary:
  `git -C <worktree> show --stat --name-only <sha>`. The executor's own report said "11 files, all
  inside the Write Set", which was true and still concealed the problem — the XML *is* inside the
  Write Set. Scope-lock compliance is not evidence-hygiene compliance and one does not imply the other.
- The repair is cheap only while the commit is unpushed. `git rm --cached <path>` then
  `git commit --amend --no-edit` leaves the file on disk for downstream consumers and makes the next
  push a fast-forward, so no history is rewritten and no sibling is disturbed. Check
  `git rev-parse origin/<branch>` before assuming you still have that option.
- An untracked file under a Write Set evidence directory still satisfies a plan's Scope-lock rule, so
  leaving it untracked costs nothing until a task asserts an empty porcelain under the evidence
  directories. Find that task and delete the file before it.

Related: [[cobertura-postprocessing-is-a-zero-exit-proxy-not-a-test-result]] for why the same document
is worth scrutinising rather than trusting, [[feedback_no_helper_scripts_under_evidence]] and
[[gitignore-does-not-cover-trx]] for the rest of the evidence-hygiene surface, and
[[subagent-self-reported-correction-can-be-false]] for the general rule that a delegate's summary is a
claim rather than evidence.
