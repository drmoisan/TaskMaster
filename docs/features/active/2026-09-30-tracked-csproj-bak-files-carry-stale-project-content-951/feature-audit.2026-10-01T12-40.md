# Feature Audit: tracked-csproj-bak-files-carry-stale-project-content (#951)

- Review timestamp: 2026-10-01T12-40
- Reviewer: feature-review agent
- Work mode: `minor-audit` (marker at `issue.md` line 12); AC source is the `## Acceptance Criteria` section of `issue.md`. No `spec.md` or `user-story.md` exists in the feature folder (Glob), consistent with the mode.
- Verdict: **PASS**, 5 of 5 acceptance criteria verified

## Executive Summary

All five acceptance criteria were verified by commands the reviewer ran, not by trusting the evidence
files. Every criterion is a repository-state property checkable with git, so none depends on a build, test
or coverage run. No coverage gate applies because no executable code changed in any coverage language.

## Scope and Baseline

- Audit scope: full branch diff against `origin/main` (`6c710a45dd61710658ea8e60588fb4108cf15b6f`), which is an ancestor of HEAD `cca7ad4438d6e7082a37d50160c53f6ca9cf5c76`, so the two-dot form was used.
- Baseline: before the change, eight `*.csproj.bak` files were tracked on `origin/main` and `.gitignore` had no `*.csproj.bak` rule (per `issue.md` and `evidence/baseline/p0-t5-ac1-baseline` and `p0-t12-check-ignore-baseline`).
- Footprint: `.gitignore` (+1 line), eight deletions, and files under `docs/features/` only.
- Coverage: not applicable; zero changed files in C#, PowerShell, Python and TypeScript.
- No caller narrowing of scope was detected.

## Acceptance Criteria Inventory

Source: `issue.md` `## Acceptance Criteria`.

- AC-1: No `*.csproj.bak` file is tracked; the eight paths are deleted from the index and working tree.
- AC-2: `.gitignore` carries the exact line `*.csproj.bak`, and `git check-ignore -v` reports it for each of the eight paths.
- AC-3: The rule shadows no tracked file; the other three tracked backups remain tracked and unmodified.
- AC-4: Nothing in `scripts/`, `.github/`, `.claude/lib/`, `*.csproj`, `*.sln`, `*.targets` references `.csproj.bak`.
- AC-5: The change set against `origin/main` is only the eight deletions, the `.gitignore` edit, and files under `docs/features/`.

## Acceptance Criteria Evaluation

| AC | Status | Evidence and basis |
|---|---|---|
| AC-1 | **PASS** | Re-derived. `git ls-files -- "*.bak"` returned only `TaskMaster.sln.bak`, `TaskTree/TaskTree.vbproj.bak`, `TaskVisualization/TaskVisualization.vbproj.bak`; no `*.csproj.bak`. `git diff origin/main --name-status` lists `D` for `QuickFiler.Test/QuickFiler.Test.csproj.bak`, `QuickFiler/QuickFiler.csproj.bak`, `Tags/Tags.csproj.bak`, `TaskTree/TaskTree.csproj.bak`, `TaskVisualization.Test/TaskVisualization.Test.csproj.bak`, `TaskVisualization/TaskVisualization.csproj.bak`, `ToDoModel.Test/ToDoModel.Test.csproj.bak`, `ToDoModel/ToDoModel.csproj.bak`. A Glob for `*.csproj.bak` in the worktree found no file. `git status --short` is empty. |
| AC-2 | **PASS** | Re-derived. `git diff origin/main -- .gitignore` shows the single added line `+*.csproj.bak` after `*.rptproj.bak`; `--numstat` is `1 0`. `git check-ignore -v --no-index` on the eight paths printed `.gitignore:258:*.csproj.bak` once per path (8 of 8). |
| AC-3 | **PASS** | Re-derived. `git ls-files -ci --exclude-standard -- "*.bak"` printed nothing. The three remaining backups are listed by `git ls-files`, and `git diff origin/main --stat` over those three paths printed nothing, so they are unmodified. |
| AC-4 | **PASS** | Re-derived. `git grep -n -i -E "csproj\.bak\|\*\.bak" -- scripts .github .claude/lib "*.csproj" "*.sln" "*.targets"` returned no output (a case-insensitive superset of the fixed-string requirement). Control: `git grep -c -F "csproj.bak" -- .gitignore` returned 1. |
| AC-5 | **PASS** | Re-derived. `git diff origin/main --name-status` contains `M .gitignore`, eight `D` entries, and `A` entries only under `docs/features/active/2026-09-30-tracked-csproj-bak-files-carry-stale-project-content-951/` and `docs/features/potential/promoted/`. No other path appears. |

## Acceptance Criteria Check-off

All five items in `issue.md` were already `[x]`. Each was evaluated PASS above, so all remain checked. No
item was unchecked and no criterion text was edited.

| AC | Checkbox state after review |
|---|---|
| AC-1 | `[x]` (confirmed) |
| AC-2 | `[x]` (confirmed) |
| AC-3 | `[x]` (confirmed) |
| AC-4 | `[x]` (confirmed) |
| AC-5 | `[x]` (confirmed) |

Newly checked off by this review: none.

The `## Next Step` item "Move to active fix folder / branch" and the `## Proposed Fix` boxes in
`issue.md` are not acceptance criteria under the `minor-audit` rule and were left unchanged.

## Summary

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-30-tracked-csproj-bak-files-carry-stale-project-content-951/issue.md`
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

Non-blocking observations: three other tracked backup files remain by design (AC-3); the optional #927
hygiene-guard extension was not delivered and an ignore rule does not stop a forced add. No coverage gate
applies because no code changed.

No remediation is required.
