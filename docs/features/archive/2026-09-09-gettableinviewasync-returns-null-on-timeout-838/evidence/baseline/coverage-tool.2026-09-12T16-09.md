# P0-T12 — Coverage collector global tool availability

Timestamp: 2026-09-13T02-18

## Invocation 1 — ensure present

Command: `pwsh -NoProfile -Command 'if (Get-Command dotnet-coverage -ErrorAction SilentlyContinue) { "INSTALL=already-present"; exit 0 } else { & ".\.dotnet-sdk\dotnet.exe" tool install --global dotnet-coverage; exit $LASTEXITCODE }'`

EXIT_CODE: 0

INSTALL=already-present

## Invocation 2 — fresh-session resolution check

Command: `pwsh -NoProfile -Command 'if (Get-Command dotnet-coverage -ErrorAction SilentlyContinue) { "RESOLVED=True"; exit 0 } else { "RESOLVED=False"; exit 1 }'`

EXIT_CODE: 0

RESOLVED=True

Output Summary: the collector was already installed as a global tool, so no install was performed, and the separate fresh session resolved it on PATH and printed `RESOLVED=True`. Both invocations exited 0, satisfying both acceptance clauses. The second invocation is deliberately separate because a global tool installed inside one session is not necessarily resolvable on that same session's PATH.
