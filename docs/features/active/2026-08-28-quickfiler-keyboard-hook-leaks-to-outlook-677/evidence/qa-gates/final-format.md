# Final QA Gate 1 — Formatting (P5-T1)

Timestamp: 2026-08-28T16-06
Command: `./.dotnet-sdk/dotnet.exe tool run csharpier format .` (repo-wide, from repo root)
EXIT_CODE: 0

## Branch taken: repo-wide

P0-T5 recorded a **clean** formatting baseline (`csharpier check .`, EXIT_CODE 0, 1554 files, no
violation list). The task's condition for the scoped branch — "If P0-T5 recorded pre-existing
violations" — is therefore not met, so the repo-wide branch applies and was taken.

## Output Summary

```
Formatted 1558 files in 5077ms.
```

`1558` is the number of files CSharpier **processed**, not the number it rewrote. It is 4 higher
than the 1554 processed at the P0-T5 baseline, matching exactly the four new `.cs` files this plan
adds (`QfcFormController.Deactivate.cs` plus the three new test files).

## No file was rewritten by this pass

The pass was a content no-op, established by two facts:

1. At the P0-T5 baseline `csharpier check .` returned EXIT_CODE 0 over the whole tree, so every
   file this plan does not touch was already formatter-clean and cannot have been changed.
2. Every file this plan does touch was individually verified with
   `dotnet tool run csharpier check <file>` immediately before this pass and reported clean. The
   four new files were explicitly formatted at the end of Phase 3 (the only rewrite CSharpier has
   performed in this plan, which corrected the new files' line endings to the repository's CRLF
   convention) and re-checked clean afterwards.

Consequently the Phase 5 loop does not restart from T1 on account of this step.

`*.csproj`, `*.props` and `*.targets` are excluded by `.csharpierignore`, as are `**/evidence/**`,
`*.trx`, `*.cobertura.xml` and `*.coverage`, so neither the two project-file edits nor any evidence
artifact was subject to formatting.

## Working-tree scope after the pass

`git status --porcelain` lists no modified file outside this plan's change set. The modified and
untracked C# and project files are exactly the fourteen production/test/project files enumerated in
`evidence/baseline/scope-lock.md` plus the four new files. The remaining entries are the
pre-existing dirty `.claude/agent-memory/**` paths and `docs/features/potential/promoted/**`
entries recorded verbatim at the P0-T2 baseline, none of which is C# source.
