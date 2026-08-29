---
name: project-637-selectrow-rooted-path-plan-seams
description: Issue #637 breadcrumb SelectRow plan — preflight R2/R3 seams: docs/features/active as a command operand reaches 121 sibling evidence files; the untracked-440 claim was false for the agent worktree; a blanket -F rule breaks every regex; pre-format line ranges consumed post-format.
metadata:
  type: project
---

Preflight round-2 seams from the issue #637 atomic plan (breadcrumb `SelectRow` emits a rooted path).

**Never use `docs/features/active` as a command operand.** A token scan with that directory operand
returned matches from **121 distinct evidence Markdown files** belonging to other feature folders
(re-measured 2026-08-29 in the agent worktree: still exactly 121, none under this feature's folder),
so a zero-hit gate over it can never pass. That reason is git-tracking-independent — `rg` reads the
working tree — and it is the only sibling-contribution verdict that survives round 3.

**CORRECTED (R3): the untracked-440 claim was false for the agent worktree.** Round 2 asserted
`docs/features/active/2026-08-07-...-440` was "present and untracked" and made that the justification
for narrowing every git pathspec. In the agent worktree
`.claude/worktrees/agent-a68051a23e4479267` the 440 folder is **tracked**: `.../440/spec.md` and
`.../440/evidence/baseline/phase0-instructions-read.md` are both present in that worktree's git index,
while this feature's own `.../637/spec.md` is absent from it. The claim came from a harness-supplied
git status describing the *session* worktree `TaskMaster-wt/2026-08-29T00-11`, where 440 genuinely was
untracked. See [[harness-git-status-may-describe-another-worktree]].
Keep the narrowing; state it forward-looking: a parent-directory `git add` would stage any untracked
sibling folder that a concurrent run creates between planning and execution, and the plan cannot
assume the tree it observed is the tree the executor meets.
**Why:** the parent directory is shared with every other in-flight feature, none of which this plan owns.
**How to apply:** scope every `rg` directory operand, `git add`, `git status --porcelain` and `git diff`
pathspec to the feature's own folder spelled in full. A FEATURE_DIR convention that says "this path is
spelled once" must be amended — commands cannot carry a placeholder.

**A pre-format line range consumed by a post-format assertion is unsound.** The plan recorded the new
helper's line range in Phase 4, ran the write-mode formatter in Phase 7, then evaluated coverage
against the Phase-4 range. Fix: re-derive the range in the consuming task against the post-format
tree, record both ranges, state whether they differ, and evaluate every assertion against the
re-derived one. Sweep every line number/hunk range captured before the first format pass.

**A convention sentence that generalizes a flag is a defect generator.** "Where the pattern contains a
backslash, `-F` is used" attached `-F` to every regex in the plan, because most backslashes were
regex escapes (`\s`, `\(`, `\[`) — `-F` matches them literally and returns zero matches. Correct form:
name the two sites that genuinely need `-F` and state that every other pattern is a regex issued
without it. See also [[zero-hit-grep-gates-need-carveouts]].

**Exact-count gates versus the same task's authoring instruction.** Beyond the known `MoveToFolder`
case, three more surfaced in one file: a replacement comment naming `IsFullOutlookPath` (count 1),
XML documentation phrasing AC12's contract (`IsFullOutlookPath` again), and a doc sentence saying
"never throws" against an `rg -n "throw"` before/after set-equality gate. Also `out string stem`
(count 2), `out _` (count 0), and `Globals.Ol.ArchiveRootPath` (count 4). Sweep every asserted token
against every authoring instruction in the same *and* later tasks over the same file.
See also [[single-numeral-gates-must-name-the-role]] and [[absolute-counts-in-shared-files-go-stale]].

**A probe must state what each outcome means.** An `ON_PATH`/`NOT_ON_PATH` `msbuild` probe was added
to justify a vswhere substitution, but the plan supplied no branch for `ON_PATH` — which is the
outcome that actually occurs in this worktree — so the artifact would have recorded an observation
refuting the justification it supports. Write both branches.
