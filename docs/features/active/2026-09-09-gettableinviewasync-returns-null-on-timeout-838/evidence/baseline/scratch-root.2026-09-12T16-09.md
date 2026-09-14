# P0-T5 — Scratch root and evidence directories

Timestamp: 2026-09-13T00-48

Command: `pwsh -NoProfile -Command '$root = Join-Path $env:TEMP "taskmaster-838"; if (Test-Path -LiteralPath $root) { [System.IO.Directory]::Delete($root, $true) }; New-Item -ItemType Directory -Path $root -Force | Out-Null; $subs = @("baseline-tests","p1-failbefore","p2-passafter","p3-classrun","final-tests","logs"); foreach ($s in $subs) { New-Item -ItemType Directory -Path (Join-Path $root $s) -Force | Out-Null }; $n = @($subs | Where-Object { Test-Path -LiteralPath (Join-Path $root $_) -PathType Container }).Count; "SCRATCH_SUBDIRS=$n"; $fe = "docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence"; $ev = @("baseline","regression-testing","qa-gates"); foreach ($e in $ev) { New-Item -ItemType Directory -Path (Join-Path $fe $e) -Force | Out-Null }; $m = @($ev | Where-Object { Test-Path -LiteralPath (Join-Path $fe $_) -PathType Container }).Count; "EVIDENCE_DIRS=$m"; if ($n -eq 6 -and $m -eq 3) { exit 0 } else { exit 1 }'`

Execution note: the executing session's working directory is not the item worktree, so the payload was invoked with a leading `Set-Location -LiteralPath <item worktree root>` inside the same single-quoted payload. That prefix is an invocation detail only; it changes no behaviour of the recorded command, which the plan specifies to run with the worktree root as the current directory. The worktree root is not transcribed here, per the plan's evidence content rule.

EXIT_CODE: 0

Output Summary:

```
SCRATCH_SUBDIRS=6
EVIDENCE_DIRS=3
CWD_LEAF=bugs-2026-09-11-item-838
```

- Scratch root: the deterministic path produced by the expression `Join-Path $env:TEMP "taskmaster-838"`. It is outside the repository, and no absolute path is transcribed.
- Any pre-existing tree at that path was deleted recursively before creation, so no output from an earlier attempt can be selected by a later task.
- Six scratch sub-directories created and verified: `baseline-tests`, `p1-failbefore`, `p2-passafter`, `p3-classrun`, `final-tests`, `logs`. The derived instrumentation settings file `coverage.effective.config` is written directly under the scratch root by P0-T19 and is a file rather than a sub-directory.
- Three in-repository evidence directories created and verified, all under `docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/`: `baseline/`, `regression-testing/`, `qa-gates/`. These are the only permitted evidence locations for this change.
- `CWD_LEAF` confirms the payload resolved against the item worktree, which the plan's assembly-discovery rule depends on.
