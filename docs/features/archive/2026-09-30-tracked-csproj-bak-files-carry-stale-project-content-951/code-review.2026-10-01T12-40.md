# Code Review: tracked-csproj-bak-files-carry-stale-project-content (#951)

- Review timestamp: 2026-10-01T12-40
- Reviewer: feature-review agent
- Branch: `bug/tracked-csproj-bak-files-carry-stale-project-content-951`, head `cca7ad4438d6e7082a37d50160c53f6ca9cf5c76`
- Base: `origin/main` at `6c710a45dd61710658ea8e60588fb4108cf15b6f` (an ancestor of HEAD; two-dot diff used)
- Work mode: `minor-audit`; acceptance criteria source is `issue.md` `## Acceptance Criteria`
- Verification basis: reviewer-executed git commands plus Read, Grep and Glob. No build or test run was needed because no compiled content changed.

## Executive Summary

Verdict: **PASS**. 0 blocking findings, 2 non-blocking findings.

The change deletes eight tracked `*.csproj.bak` files and adds one line, `*.csproj.bak`, to `.gitignore`
directly after `*.rptproj.bak` (line 258). There is no source code, so code-quality review reduces to
three questions, each verified:

- Is the ignore pattern correctly scoped? `git ls-files -ci --exclude-standard -- "*.bak"` prints nothing,
  so the pattern shadows no tracked file. The pattern is narrower than `*.bak`, which would have matched the
  three remaining tracked backups.
- Does anything read the deleted files? `git grep -n -i -E "csproj\.bak|\*\.bak"` over `scripts`, `.github`,
  `.claude/lib`, `*.csproj`, `*.sln` and `*.targets` returned no output. A control search for
  `csproj.bak` in `.gitignore` returned a count of 1, so the search form detects a hit when one exists.
- Is the footprint minimal? `git diff origin/main --name-status` shows `.gitignore`, eight deletions and
  `docs/features/**` additions only; `.gitignore` is `1 0` in `--numstat`.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low (non-blocking) | `TaskMaster.sln.bak`, `TaskTree/TaskTree.vbproj.bak`, `TaskVisualization/TaskVisualization.vbproj.bak` | repository root and project folders | Three other backup files remain tracked. This is required by AC-3 and consistent with the issue scope, but the same staleness concern may apply to them. | Open a separate follow-up if their removal is wanted; do not widen this fix. | Issue scope names `*.csproj.bak` only; widening the ignore rule to `*.bak` would have shadowed tracked files. | `git ls-files -- "*.bak"` lists exactly these three. |
| Low (non-blocking) | `.gitignore` | line 258 | An ignore rule does not prevent a forced add (`git add -f`), and the optional #927 hygiene-guard extension that would flag tracked backups was not delivered. | Consider the guard extension, with a negative control, as a follow-up. | The extension is listed as optional in `issue.md` and is not an acceptance criterion. | `issue.md` "Proposed Fix / Validation Ideas"; diff contains no `scripts/` path. |

No findings on correctness, naming, error handling, design, tests, security or file size: no code or test
file changed.

## Acceptance Criteria Inventory

Source: `issue.md` `## Acceptance Criteria` (work mode `minor-audit`). Five items, AC-1 to AC-5, all
checked `[x]` by the executor before this review.

## Acceptance Criteria Evaluation

| AC | Code-level result | Basis |
|---|---|---|
| AC-1 | PASS | `git ls-files -- "*.bak"` lists only the three non-csproj backups; Glob `*.csproj.bak` returns no file in the worktree; the diff lists the eight paths as `D`. Re-derived. |
| AC-2 | PASS | `git diff origin/main -- .gitignore` adds exactly `+*.csproj.bak`; `git check-ignore -v --no-index` reports `.gitignore:258:*.csproj.bak` for all eight paths. Re-derived. |
| AC-3 | PASS | `git ls-files -ci --exclude-standard -- "*.bak"` prints nothing; `git diff origin/main --stat` over the three remaining paths prints nothing (unmodified). Re-derived. |
| AC-4 | PASS | `git grep` over the five named locations returns no match; control search matches. Re-derived. |
| AC-5 | PASS | `git diff origin/main --name-status` shows only `.gitignore`, the eight deletions and `docs/features/**` (`active` and `potential/promoted`). Re-derived. |

## Verdict

**PASS.** No blocking findings; no remediation inputs produced.
