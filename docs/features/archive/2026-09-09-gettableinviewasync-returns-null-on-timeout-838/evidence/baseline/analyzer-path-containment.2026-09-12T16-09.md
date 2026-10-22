# P0-T14 — Analyzer-path containment inside the gitignored package directory

Timestamp: 2026-09-13T02-19

Command: a single pwsh payload that captures the anchored name-listing diff `git -C . diff --name-only 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- . ":(exclude).claude/agent-memory" ":(exclude)docs/features/potential"` and the porcelain span `git -C . status --porcelain --untracked-files=all -- . ":(exclude).claude/agent-memory" ":(exclude)docs/features/potential"` immediately before the branch action, resolves the main working tree from `git -C . worktree list --porcelain`, performs the branch action, re-probes the previously unresolved include, then captures the same two listings again and compares them.

EXIT_CODE: 0

BRANCH=B1

The count P0-T13 reported is 15, above zero, and every one of the fifteen unresolved includes contains the case-sensitive fixed literal `Meziantou.Analyzer.3.0.203`, which selects Branch B. Sub-branch B1 was taken because the first `worktree ` line of the porcelain worktree listing resolves the main working tree and a directory named `Meziantou.Analyzer.3.0.203` exists under that tree's packages directory. It was copied recursively into this worktree's packages directory. B1 is the preferred sub-branch because it supplies the analyzer bits CI actually compiles with. No package tool was invoked. B2 was not reached.

BACKFILL_PROBE_RESOLVES=True

The repository-relative include path `packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll`, which every one of the fifteen project files names and which did not exist before this task, exists after the copy.

## Before-action listings, verbatim

Anchored name-listing diff:

```
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/ac12-amendment-confirmed.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/diff-anchor.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/phase0-instructions-read.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/scratch-root.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/sdk-bootstrap.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/issue.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/research/2026-09-12T15-30-gettableinviewasync-null-contract-research.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/spec.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/user-story.md
```

Porcelain span:

```
 M docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/analyzer-path-reconciliation.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/coverage-tool.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/nuget-restore.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/tool-restore.2026-09-12T16-09.md
```

## After-action listings, verbatim

Anchored name-listing diff:

```
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/ac12-amendment-confirmed.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/diff-anchor.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/phase0-instructions-read.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/scratch-root.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/sdk-bootstrap.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/issue.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/research/2026-09-12T15-30-gettableinviewasync-null-contract-research.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/spec.md
docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/user-story.md
```

Porcelain span:

```
 M docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/analyzer-path-reconciliation.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/coverage-tool.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/nuget-restore.2026-09-12T16-09.md
?? docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/tool-restore.2026-09-12T16-09.md
```

DIFF_LISTINGS_IDENTICAL=True
PORCELAIN_LISTINGS_IDENTICAL=True
AFTER_CSPROJ_ENTRIES=0

Output Summary: the containment was performed entirely inside the gitignored package directory. No tracked file was edited: the before and after anchored diff listings are byte-identical to each other, the before and after porcelain listings are byte-identical to each other, and the count of entries across both after-listings whose path ends with `.csproj` is 0. The copied directory is not visible to either listing because the package directory is gitignored, which is the mechanism that keeps the fifteen stale tokens out of this change's anchored diff. The porcelain span is paired with the name-listing diff because a name-listing diff cannot see an untracked addition. All three acceptance parts hold and the payload exited 0.
