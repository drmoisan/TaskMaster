# P1-T4 Scope Part File

Timestamp: 2026-09-29T09-17
Task: P1-T4
Command: Write tool (new file scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1); git add -N -- scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1; Grep over the new file; git status --porcelain -uall -- scripts/vscode
EXIT_CODE: 0

Output Summary:
- New part file created per the Production Specification: `Test-CoverageRunIsScoped` with CmdletBinding, OutputType bool, two mandatory string parameters, GetFullPath normalisation, trailing-separator trim, ordinal case-insensitive comparison. A header comment above the function names issue #928, states that the file is pure, and states that it is dot-sourced by the entry point rather than by the helpers chain because of the three-production-file cap.
- Grep `function Test-CoverageRunIsScoped`: 1 (line 8) (required at least 1).
- Grep `OrdinalIgnoreCase`: 1 (line 48) (required at least 1).
- Grep `GetFullPath`: 3 lines (14, 42, 43) (required at least 2).
- Grep `TrimEnd`: 2 lines (42, 43) (required at least 2).
- Grep `issue #928`: 2 lines (3, 20) (required at least 2). A first draft of the header comment spelled it with a capital I, which gave a case-sensitive count of 1; the comment was reworded to the lowercase form before this record was written.
- Grep `\[OutputType\(\[bool\]\)\]`: 1 (line 32) (required exactly 1).
- Grep count 0 for each of `Get-ChildItem`, `Test-Path`, `Resolve-Path`, `Get-Content`, `Set-Content` (a single alternation over the five tokens returned 0), so the file is pure.
- Grep pattern `[^\x00-\x7F]`: 0 (pure ASCII).
- First line: `Set-StrictMode -Version Latest`. First three bytes 53 65 74 (no byte-order mark).
- Line count: 49 newline-terminated lines (git grep -c -e "" --untracked); Read renders the final numbered line at 50 (empty), at most 60 as required.
- git add -N: exit 0, no output.
- git status --porcelain -uall -- scripts/vscode (verbatim, one line):
  ` A scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1`
