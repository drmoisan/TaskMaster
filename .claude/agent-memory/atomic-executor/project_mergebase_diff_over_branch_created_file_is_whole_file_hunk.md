---
name: mergebase-diff-over-branch-created-file-is-whole-file-hunk
description: A `git diff <MERGE_BASE> -- <path>` clause asserting "no hunk touches X" is unsatisfiable when <path> was created on the branch — the diff is one whole-file addition hunk containing every line, including X
metadata:
  type: project
---

A plan that anchors every diff to a pinned `MERGE_BASE` (the correct rule for gates over files that
exist on the base) produces an **unsatisfiable** clause the moment the same idiom is applied to a
file the branch itself created. Feature-folder documents — `spec.md`, `issue.md`, `plan.*.md`,
`research/*.md` — are exactly that class: the promotion commit adds them, so they are absent at the
merge base.

Measured on issue #911, task P1-T1:

```
git cat-file -e <MERGE_BASE>:docs/features/active/<feature>/spec.md
  fatal: path '...' exists on disk, but not in '<MERGE_BASE>'
git ls-tree <MERGE_BASE> -- docs/features/active/<feature>/    ->  0 entries
git diff --numstat <MERGE_BASE> -- .../spec.md                 ->  701  0
```

One hunk, `@@ -0,0 +1,701 @@`. The acceptance read "shows this task's own changes confined to the
Write Set section, with no hunk touching any criterion line" — all 26 criterion lines sit inside
that single hunk. The edit was correct (12 added, 0 deleted, three hunks, all inside the target
section, confirmed by `git diff HEAD`), and no possible edit could satisfy the clause.

**Why:** the merge-base anchor is chosen for reproducibility against a moving `origin/main`, and
that reasoning is sound — but it silently assumes the file has a base-side version to diff against.
For a branch-created file the anchor degenerates and the gate stops measuring the task.

**How to apply:**
- In preflight, for every `git diff <BASE> -- <path>` clause, check `git ls-tree <BASE> -- <path>`.
  If it returns nothing, flag the clause: it cannot isolate a task's edit.
- The correct anchor for "this task's own change" over a branch-created file is `git diff HEAD --`
  (working tree against the last commit), paired with `git status --porcelain --untracked-files=all`
  per gate rule 8. `HEAD` is a ref, so it satisfies the anchored-diff rule; it is stable within a
  run because it moves only at the plan's own commit tasks.
- The defect is per-path, not per-plan. In the same plan the merge-base gates over `*.csproj`,
  `*/packages.config`, `*/app.config`, `.csharpierignore` and `.github/workflows/*.yml` were all
  correct, because those files exist at the base.

Related: [[project_baseline_sha_diff_conflates_merged_base]],
[[project_preflight_moving_base_two_dot_diff_inertness_test]],
[[project_preflight_mergebase_diff_gates_need_commit_cadence]].
