---
name: nullable-build-gate-is-vacuous-incremental
description: The plan-standard `msbuild /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` gate returns EXIT 0 without type-checking, because MSBuild's up-to-date check ignores /p: changes; verify with an isolated /t:Rebuild
metadata:
  type: project
---

The repo-standard type-check gate `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Nullable=enable /p:TreatWarningsAsErrors=true` reports `Build succeeded. 0 Error(s)` in ~1.6 s whenever a prior build (e.g. the analyzer gate) already produced current outputs. MSBuild's up-to-date check compares timestamps only and **ignores `/p:` property changes**, so `CoreCompile` never runs and no nullable analysis happens. The gate is symmetric between baseline and post-change, so it always "passes" — vacuously.

Measured on #503 (2026-08-08): the `/t:Build` form gave 0 errors. A forced
`msbuild TaskMaster\TaskMaster.csproj /t:Rebuild /p:Configuration=Debug /p:Platform='AnyCPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`
gave **223 errors** (`CS8600/8601/8602/8603/8604/8618/8619/8625`), concentrated in `TaskMaster\AppGlobals\*` (AppOlObjects 58, AppAutoFileObjects 52, AppToDoObjects 48, AppOlObjects.FolderTreeService 48, AppStagingFilenames 40, ApplicationGlobals 40, AppItemEngines 18). Exactly 3 of the 223 were in newly-authored code.

Do NOT add `/p:OutputPath=<scratch>` to isolate the probe: it also redirects project-reference resolution and produces bogus `CS0006 Metadata file '...QuickFiler.dll' could not be found`. Use `/p:BuildProjectReferences=false` against the normal output path instead, and re-run the full solution build afterwards because `Rebuild` cleans the output.

**Why:** a plan can require "new files must be nullable-clean" and the plan's own command will confirm it without ever checking. Reporting PASS on that basis is an unmeasured claim.

**How to apply:** when a plan's type-check task uses `/t:Build`, execute it verbatim and record its result as the task's stated gate, then run the isolated `/t:Rebuild` as *supplementary verification* scoped to the projects the change touched. Attribute the resulting errors to files (`grep -oE "[A-Za-z0-9_.\\\\]+\.cs\([0-9]+,[0-9]+\): error CS[0-9]+" | sed 's/(.*//' | sort | uniq -c | sort -rn`) and fix only those in authored code; record the rest as pre-existing debt. Related: [[project_incremental_build_vacuous_baseline]].
