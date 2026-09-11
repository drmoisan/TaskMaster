---
name: project-796-base-anchor-reconciliation-seams
description: "#796 post-clearance merge reconciliation: a merge of origin/main into a cleared branch invalidates both the diff anchor SHA and every csproj line citation; the anchor must be the merge commit, never the cut point or a symbolic ref"
metadata:
  type: project
---

A plan that has already cleared preflight can be invalidated by a merge of `origin/main`
into its own branch. The reconciliation is a planner task, not a text substitution.

**Why:** on #796 the orchestrator merged main (advanced by two sibling bug items of the
same parallel run) as its first execution action. Four gates hardcoded the pre-merge
branch-cut SHA as their diff anchor. Anchored there, those diffs enumerate all 146 files
the sibling merges brought in and bill them to this item, and the changed-code coverage
denominator is computed over sibling-authored lines. Every one of the four gates fails on
work the item did not do.

**How to apply:**

- **The anchor is the merge commit.** It is the state "main-as-merged plus this item's own
  prior commits and nothing else", so a two-dot diff from it isolates exactly this item's
  work. Do NOT use the branch-cut SHA (re-bills the siblings). Do NOT substitute a symbolic
  ref such as `origin/main` (it moves again during execution). Do NOT switch to a three-dot
  form: against a base that is an ancestor it degenerates and re-bills the merge.
- **Keep exactly one occurrence of the cut-point SHA**, on the Branch metadata line, and
  label it unambiguously as the cut point rather than the anchor. State the two facts
  distinctly: cut point is history, anchor is operative.
- **The merge shifts line citations the orchestrator did not flag.** On #796 the merge added
  1 compile entry to `QuickFiler/QuickFiler.csproj` and 2 to
  `QuickFiler.Test/QuickFiler.Test.csproj`. That moved SIX separate line citations in the
  plan by +1 or +2, including an analyzer-block range 600 lines away from the insertion and
  a compile-entry citation in the Decisions record. Re-derive every csproj line citation, not
  just the two the "pattern to follow" sentence names. See
  [[absolute-counts-in-shared-files-go-stale]].
- **Test-population gates are usually safe but must be checked in two categories.** Gates
  scoped by a `FullyQualifiedName~` filter to classes the merge did not touch are unaffected;
  check the merge-added class NAMES against every filter substring for accidental collision.
  A whole-assembly gate is safe only if its acceptance is expressed relative to a baseline
  captured in the post-merge tree, not as an absolute total.
- **The `PRE-EXISTING-DIRTY-SET` reasoning survives** a merge, because the merge is committed
  and contributes nothing to porcelain status — but confirm the tree is clean at the merge
  commit rather than asserting it.
- Do not touch the same SHA in issue.md / spec.md / research.md: there it is a true
  historical statement about when the defect was observed.

## Closing pass (round 6): a supplied "exact replacement text" can still carry a false universal

The orchestrator closed #796 with a bounded three-replacement directive that quoted the
exact prose to write and forbade any sweep. Two of the three replacements were verifiable
verbatim. The third replaced a false universal ("every other reference is a Moq mock or a
comment") with a narrower one naming four categories — and the narrower universal was still
incomplete, because a grep sweep for an interface name also returns **the interface's own
declaration**, which is neither a consumer, a mock, a region marker, nor a doc comment.

**Why:** an implementor sweep and a name sweep return different populations. The sentence's
first clause was correctly scoped ("the only declaration that names the interface in a base
list" — the definition site names it in the declaring position, not a base list), but the
second clause silently widened to all references and inherited the definition site.

**How to apply:** when a caller hands down exact prose containing a universal over search
results, run the sweep before writing it and check three residues that categories routinely
miss: the definition site itself, references in sibling test projects (a `<c>`/`<see cref>`
doc comment in another assembly's fakes), and non-code mentions in `docs/` and
`.claude/agent-memory/`. Bound the universal to the file type actually swept ("every other
reference in a .cs file") and carve out the definition site by path and line. Applying a
verbatim-but-false sentence is worse than a disclosed one-clause amendment; the directive's
own "do not write a new false statement" clause outranks its exactness clause.

## Second merge (round 7): after execution starts, the anchor set stops being uniform

A SECOND merge of `origin/main` landed mid-execution, after Phase 0 and Phase 1 had been
committed. The round-6 rule "the anchor is the merge commit" does **not** generalise to
"replace every anchor with the newest merge commit". Anchors must now be stated per gate.

**Why:** by the second merge the branch already contains this item's own committed work, so
the newest merge commit is no longer a clean "everything before my work" boundary. Three
distinct dispositions were correct simultaneously on #796:

- **Already-executed gate → leave the old anchor.** `P1-T14` ran at `c7ae69f1` and was
  correct then. Editing it rewrites an audit record. Record its historical status on the
  Branch metadata line instead.
- **Unexecuted whole-tree gate → move to the new merge commit.** `P7-T4` and `P9-T10`
  enumerate all paths; anchored at the older merge they bill the 59 incoming files to this
  item. Note in the task that the newer anchor lists *strictly fewer* paths, which is safe
  only because the acceptance is an upper bound on which paths may appear, never a lower
  bound on how many must.
- **Path-scoped changed-line gate → RETAIN the old anchor and say so.** `P9-T7` diffs
  `-- QuickFiler`, which the merge did not touch, so the old anchor is already exact. Moving
  it would drop this item's own committed Phase 1 instrumentation out of the changed-line
  denominator that the same task requires to be counted. Add an explicit "retained
  deliberately, do not sweep this" sentence, or a later stale-anchor sweep silently breaks it.

**The bigger sweep finding: a plan's own executed phases move its unexecuted citations.**
The orchestrator scoped the sweep to merge-induced drift and to *project-file* citations. The
project files had indeed moved — but by the plan's own `P1-T3` / `P1-T6` compile-entry
insertions, not by the merge (+1 in each file, shifting an analyzer block 180 lines away and
two later compile-entry citations). Far larger drift sat in the `.cs` citations: `P1-T2`'s
12-line pure move shifted every `BreadcrumbDropDownHost.cs` citation at or after 426 by −13
(`FinishClose` 439→426), and `P1-T4`'s instrumentation shifted every
`QfcFormController.Deactivate.cs` citation by roughly +46 (cancel loop 52→105, catch 58→117),
invalidating citations in five unexecuted tasks.

**How to apply:** on any mid-execution revision, sweep line citations in *unexecuted* tasks
against the tree, not just the ones the caller names, and partition by execution status —
correct unexecuted-task and reference-prose citations, leave executed-task citations alone,
and record the arithmetic once in the constraints bullet. Refer out-of-scope drift to the
orchestrator instead of widening the revision, especially when later phases will move the
same citations again.

## Round 8: an "append past the cited region" edit causes NO drift — verify before referring

The round-7 referral listed four files as probably-stale. The orchestrator re-derived and
upheld only two. The other two were false alarms, and the mechanism is worth keeping: an
executed task that **appends past every cited position** (`P1-T4` added a member at line 243,
below all cited positions 60–224; `P1-T7` appended a test at line 255, the end of file)
shifts nothing. Only an insertion *before* a citation, or a deletion, moves it.

**How to apply:** before referring a citation as stale, find the executed edit's insertion
point and compare it to the lowest cited line. If the insertion is below, the citations are
intact — say so, and ask that the finding be recorded in the plan, so a later reader who
notices the phase touched that file does not reopen the question. Half a referral's blast
radius can evaporate this way.

Two further seams from the same round:

- **A "which member is documented" defect is not detectable by a whole-file count.** `P1-T4`
  inserted methods *beneath* an existing `<summary>`/`<remarks>` pair, stranding it on the
  wrong member: one member ended up with two `<summary>` elements and another with none. The
  file holds five `<summary>` tags before the repair and five after, so a whole-file count is
  invariant and gates nothing. The failable form is **positional**: count `<summary>` in the
  maximal run of consecutive `///` lines immediately above each named declaration
  (0 and 2 before, 1 and 1 after). Locate the declarations by name, never by line, so the
  command survives the earlier same-phase task that edits the same file.
- **A comment-only repair must be carved out of a changed-line coverage denominator.**
  `git diff -U0` enumerates moved `///` lines as changed, but a comment line carries no
  Cobertura `line` node, so it can enter neither numerator nor denominator. Without an
  explicit clause it deflates the figure. Scope the carve-out to comment lines and state that
  it is not charged against the plan's named-exclusion allowance.

Related: [[diff-gates-need-a-commit-task]], [[never-pin-head-sha-as-plan-expectation]],
[[verify-citations-in-the-assigned-worktree]],
[[acceptance-edits-must-be-false-before-true-after]].
