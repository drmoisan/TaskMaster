---
name: barrier-hook-resolves-the-longest-active-path-token
description: The cohort-barrier hook resolves the target item from the LONGEST docs/features/active token in the prompt, so the plan path wins over the folder path — and one trailing comma after it defeats the .md strip and denies the launch
metadata:
  type: feedback
---

Never let a `docs/features/active/...` path token in a delegation prompt be followed immediately by
punctuation. Re-word so every such token is followed by whitespace.

**Why:** On run `bugs-2026-09-06` (2026-09-08) the pre-launch probe for item 812 returned

> `PARALLEL_COHORT_BARRIER_BLOCKED: 'plan.2026-09-07T22-12.md,' cannot start until every conflicting
> item in a strictly prior current-generation cohort is durably confirmed merged or worktree_removed`

which reads exactly like a real barrier block and was not one. The checkpoint was correct, every
conflicting neighbour was merged, and the item was genuinely eligible. Two mechanisms in
`.claude/hooks/enforce-parallel-cohort-barrier.ps1` compose:

- `Find-ParallelCohortBarrierFeatureFolderFromPrompt` matches
  `docs[\\/]+features[\\/]+active[\\/]+[^\s"''`]+` and **returns the LONGEST unique match**, not the
  first and not the bare folder token. A prompt that names both the feature folder and the committed
  plan path always resolves through the plan path, because the plan path is strictly longer. That is
  by design — `Get-ParallelCohortBarrierFolderBasename` strips a trailing `/<name>.md` to recover the
  parent folder.
- The character class excludes only whitespace, double quote, single quote and backtick, so **a
  trailing comma is captured into the token**. The strip is anchored `'\.md$'`, so `....md,` does not
  match it, no strip happens, and the basename becomes the file name rather than the folder. It then
  matches no `items[].feature_folder`, and the hook fails closed on an unresolvable target.

The identical hazard exists on the drift gate, which the skill documents as resolving the target the
same way, and on any future hook that scans a prompt for a feature-folder token.

**How to apply:**

- Write `... from the committed plan at <path> and begin at the first unchecked P#-T# task`, never
  `... at <path>, beginning at ...`. Any of `,` `;` `)` `.` immediately after the token breaks it.
- **Probe every prompt before the spawn.** This cost one round to find and would have cost a failed
  launch. The probe is read-only and cheap; see [[parallel-run-execution-playbook]].
- **The deny reason quotes the resolved token.** When it names a FILE rather than a folder, the
  defect is in the prompt text, not in the checkpoint. Do not go rewrite `cohorts[]` or
  `merge_status` in response — that is the same misdiagnosis trap as
  [[keyed-issue-num-in-delegation-prompts]], where the fix also belonged in the prompt.

Same authoring family as [[keyed-issue-num-in-delegation-prompts]] and
[[never-cd-before-a-file-operand-in-delegation-prompts]]: prompt prose is an interface to mechanical
matchers, and ordinary English punctuation can defeat one.
