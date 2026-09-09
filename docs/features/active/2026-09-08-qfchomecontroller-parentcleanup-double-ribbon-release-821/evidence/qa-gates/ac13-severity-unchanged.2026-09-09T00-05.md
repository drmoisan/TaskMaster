# Phase 5 — AC13 severity unchanged

Timestamp: 2026-09-09T13-22
Task: [P5-T2]

Two paths carry analyzer and compiler severity configuration in this repository:
`.editorconfig` and `BannedSymbols.txt`. Neither may appear in this change.

## Span 1 — anchored name-only diff

Command: `git diff --name-only (git merge-base HEAD origin/main)`
EXIT_CODE: 0

| Path searched for | Matches in span |
|---|---|
| `.editorconfig` | **0** |
| `BannedSymbols.txt` | **0** |

## Span 2 — porcelain status companion

Command: `git status --porcelain --untracked-files=all`
EXIT_CODE: 0

| Path searched for | Matches in span |
|---|---|
| `.editorconfig` | **0** |
| `BannedSymbols.txt` | **0** |

Neither path is reported modified or added. The porcelain companion is required because a
name-listing diff enumerates tracked changes only and cannot report an untracked path.

## Span 3 — authored-change diff

Command: `git diff --name-only HEAD`
EXIT_CODE: 0

| Path searched for | Matches in span |
|---|---|
| `.editorconfig` | **0** |
| `BannedSymbols.txt` | **0** |

This third span isolates the change this feature authored from the inherited integration history the
anchored two-dot diff also lists; see `[P5-T7]` for the full treatment of that distinction. Here it
is redundant confirmation: all three spans agree at zero.

## No severity was lowered by any other mechanism

- No `#pragma warning disable` was introduced in any of the four production files — `[P5-T1]`
  records that search at 0 matches.
- No `NoWarn`, `WarningsNotAsErrors` or `TreatWarningsAsErrors` element was added to any project
  file, because no project file is in this change at all — `[P5-T6]` records zero `.csproj` paths.
- The two msbuild gates in Phase 6 run with the repository's own property sets, unmodified:
  `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` for analyzers and
  `/p:TreatWarningsAsErrors=true` for the type check. Neither was relaxed, and `/t:Rebuild` was used
  for both so the gates actually compile rather than skipping `CoreCompile`.

Output Summary: `.editorconfig` and `BannedSymbols.txt` each return **0 matches** in all three spans —
the anchored diff, the porcelain status, and the authored-change diff. No analyzer or compiler
diagnostic severity is lowered anywhere in this change.
