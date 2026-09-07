# Phase 7 — Changed-File Inventory

Timestamp: 2026-08-26T11-34
Task: [P7-T8]
Command: `git diff --name-only 363bfcdd4da5a24743ee665ea9fd124bc42239ff -- . ":(exclude).claude/agent-memory"` and `git status --porcelain -- . ":(exclude).claude/agent-memory"`
EXIT_CODE: 0

Both commands cover the whole worktree and exclude only `.claude/agent-memory`, which holds tracked
files this feature does not own and which the executing agent writes to during a run. Every other
path in the tree remains observable, so a write outside the owned surface fails this gate.

## Output Summary

### `git diff --name-only`

```
QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs
QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs
QuickFiler/Controllers/EfcHomeController.Metrics.cs
QuickFiler/Controllers/EfcHomeController.cs
QuickFiler/Controllers/QfcHomeController.Metrics.cs
QuickFiler/Controllers/QfcHomeController.cs
docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md
docs/features/active/quickfiler-home-controller-metrics-442/spec.md
```

### `git status --porcelain`

```
 M QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs
 M QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs
 M QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs
 M QuickFiler/Controllers/EfcHomeController.Metrics.cs
 M QuickFiler/Controllers/EfcHomeController.cs
 M QuickFiler/Controllers/QfcHomeController.Metrics.cs
 M QuickFiler/Controllers/QfcHomeController.cs
 M docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md
 M docs/features/active/quickfiler-home-controller-metrics-442/spec.md
?? docs/features/active/quickfiler-home-controller-metrics-442/evidence/
```

## Classification of every listed path

**The acceptance condition holds: every listed path is one of the five owned production files, one
of the two owned test files, or a path under
`docs/features/active/quickfiler-home-controller-metrics-442/`.**

| Path | Classification |
| --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | owned production file 1 |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | owned production file 2 |
| `QuickFiler/Controllers/EfcHomeController.cs` | owned production file 3 |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | owned production file 4 |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | owned production file 5 |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | owned test file 1 |
| `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` | owned test file 2 |
| `docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md` | feature folder; task check-offs |
| `docs/features/active/quickfiler-home-controller-metrics-442/spec.md` | feature folder; AC check-offs and the CFN-4 disposition |
| `docs/features/active/quickfiler-home-controller-metrics-442/evidence/` | feature folder; all evidence artifacts |

All five owned production files and both owned test files were modified, and nothing else outside
the feature folder was.

## One path removed rather than committed

The first execution of this gate listed one additional untracked path:

```
?? .claude/state/
```

It contained a single file, `.claude/state/powershell-batch-budget.default.json`, whose entire
content was a production and test file cap and a list naming the two PowerShell coverage-analysis
scripts created in the session scratchpad:

```json
{
  "prodCap": 3,
  "testCap": 3,
  "prodFiles": [
    "<scratchpad>/Get-CoverageFacts.ps1",
    "<scratchpad>/Get-CoverageFacts2.ps1"
  ],
  "testFiles": []
}
```

It is transient bookkeeping written by a repository hook as a side effect of creating those two
scratchpad scripts, not a deliberate write by this feature. `git check-ignore` confirmed it is not
git-ignored, so it would otherwise have appeared in the commit.

`.claude/**` is outside this feature's owned surface, so committing it was not an option. The file
and its directory were deleted, restoring the tree to exactly the owned surface. The output above is
the post-removal state.

Neither analysis script was written under any `evidence/` directory. Both live in the session
scratchpad outside the repository, so no helper script is retained anywhere under
`docs/features/active/quickfiler-home-controller-metrics-442/evidence/`, which holds Markdown
artifacts only.

## Note on the CRLF advisory

`git diff` emitted one advisory:

> warning: in the working copy of
> 'docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md', LF will be
> replaced by CRLF the next time Git touches it

This is the repository's configured line-ending normalisation acting on a Markdown file inside the
feature folder. It is not a content change and does not affect any gate.
