# P1-T7 — New-file size and scope audit

Timestamp: 2026-09-01T19-56
Command: `(Get-Content -LiteralPath 'QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs').Count`, `git status --porcelain -- QuickFiler`, and `git diff --name-status 988d35a8f8eb7436cc46a9f6424db917ed93807a -- QuickFiler`
EXIT_CODE: 0

## Base-ref substitution

The plan's stated command names `2b85134b42872e405602e6064e02dc9cda6c319b`. That SHA is superseded and is a stale ancestor rather than the current merge base, so `988d35a8f8eb7436cc46a9f6424db917ed93807a` — the merge base of this branch with `origin/main` — was used instead. The full rationale, including the measurement showing the superseded SHA already reports 17 contaminating paths under `QuickFiler` and `QuickFiler.Test` before any edit in this delivery run, is recorded in `evidence/baseline/p0-t7-base-ref.md`.

## Line count

    QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs = 41 lines

The acceptance ceiling for this task is 60, and the repository-wide ceiling is 500. Both hold with wide margin. The count is taken after the P1-T5 formatter normalization, so it is the count of the formatter-stable file rather than of a pre-format draft.

## File set under QuickFiler/

`git status --porcelain -- QuickFiler`:

    AM QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs
    M  QuickFiler/QuickFiler.csproj

`git diff --name-status 988d35a8f8eb7436cc46a9f6424db917ed93807a -- QuickFiler`:

    A	QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs
    M	QuickFiler/QuickFiler.csproj

The combined file set is exactly the two required paths and no others:

- `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` — added
- `QuickFiler/QuickFiler.csproj` — modified

The acceptance condition holds. In particular `QuickFiler/Controllers/QfcItemController.Initialization.cs` does not yet appear, which is correct at this point: the three call-site substitutions are Phase 2 work and have not been made. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` does not appear and must not appear at any later point either, per AC8.

## Why both spans are required

The two commands are complementary and each alone would be wrong in one state. `git diff --name-status` against a ref enumerates tracked changes only, so it cannot see an untracked file; the created file is visible to it here solely because P1-T2 staged it. `git status --porcelain` sees the untracked and unstaged state but goes empty once everything is committed. Running both is what makes the file-set claim complete at this point in the plan, where the tree carries a staged addition and an unstaged formatter rewrite simultaneously — the `AM` code records exactly that pair of states.
