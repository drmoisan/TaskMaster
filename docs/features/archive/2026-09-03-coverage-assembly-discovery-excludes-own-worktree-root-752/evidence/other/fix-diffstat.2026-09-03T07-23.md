# Fix Diffstat and Blast Radius ([P2-T2])

Timestamp: 2026-09-03T12-06

Command:
1. `git -C <repo-root> add -A -- scripts/vscode/Invoke-MSTestWithCoverage.ps1`
2. `git -C <repo-root> status --porcelain -uall -- scripts/vscode`
3. `git -C <repo-root> diff --stat --cached HEAD -- scripts/vscode`

EXIT_CODE: 0

## Porcelain output (command 2), verbatim

```
M  scripts/vscode/Invoke-MSTestWithCoverage.ps1
```

## Staged diffstat (command 3), verbatim

```
 scripts/vscode/Invoke-MSTestWithCoverage.ps1 | 2 +-
 1 file changed, 1 insertion(+), 1 deletion(-)
```

Output Summary: The staged change against this branch's own tip names exactly one file, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, with 1 insertion and 1 deletion, which is the minimal single-line predicate change this item authorizes. The porcelain output names no other path under `scripts/vscode`. The diff is anchored to `HEAD` rather than to `origin/main` because `origin/main` has advanced past the merge base and already differs from this branch in both files this plan touches, so the one-file, one-insertion, one-deletion shape could never hold against it.
