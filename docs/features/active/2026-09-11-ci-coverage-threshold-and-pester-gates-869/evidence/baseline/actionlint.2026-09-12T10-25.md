# Phase 0 — Baseline workflow lint (P0-T14)

Timestamp: 2026-09-14T18-15

## Item-rooted workflow listing, recorded before the lint run

Tool: Glob
Pattern used: `<repo-root>/.github/workflows/*.yml`

Returned list, verbatim, with the item worktree root shown as `<repo-root>`:

```
<repo-root>\.github\workflows\_actionlint.yml
<repo-root>\.github\workflows\_build-analyzers.yml
<repo-root>\.github\workflows\_build-nullable.yml
<repo-root>\.github\workflows\_format-check.yml
<repo-root>\.github\workflows\ci.yml
<repo-root>\.github\workflows\codex-web-setup-test.yml
<repo-root>\.github\workflows\_mstest-coverage.yml
```

Entry count: **7**, which is exactly the required number. This listing is the named source of the file-count claim: the Glob pattern is item-rooted while the lint run's target is not fixed by its own arguments, so a listing that did not hold exactly seven entries would be the observation that a wrong tree was addressed.

The eighth file in that directory, `.github/workflows/README.md`, is a markdown file and is not returned by a `*.yml` pattern.

## Lint run

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -Command '<worktree prologue>; & "<repo-root>/scripts/dev-tools/run-actionlint.ps1"'`
EXIT_CODE: 0

The prologue is what points actionlint's lint target at this worktree. The mechanism was re-verified in this pass by reading the script, which is 14 lines: its line 4 derives `$repoRoot` from `$PSScriptRoot`, its line 5 joins `actionlint-bin\actionlint.exe` onto it, and its line 11 invokes that binary with **no path argument at all**, so actionlint resolves its lint target from the working directory. An absolute script path alone would therefore fix only which binary is located, leaving the lint target pointed at the coordinator session worktree. The invocation was made with `-Command` plus the prologue plus the call operator, never with `-File`, for that reason.

Output Summary: the lint run printed no diagnostic lines. A clean actionlint run prints nothing, so the absence of output is the pass signal and carries no file count. The file count is therefore read from the recorded Glob listing above, which holds exactly seven entries, rather than from the lint output. The script's own `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` guard at lines 12 to 14 did not fire, and the recorded exit code of 0 is the binary's exit code propagated through it.
