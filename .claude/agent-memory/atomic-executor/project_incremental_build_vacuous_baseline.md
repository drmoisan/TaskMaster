---
name: incremental-build-vacuous-baseline
description: Invoke-VSBuild.ps1 only runs /t:Build, and legacy non-SDK up-to-date checks ignore property changes, so a Nullable/analyzer "baseline" can return EXIT 0 with 0 CoreCompile and prove nothing
metadata:
  type: project
---

`scripts/vscode/Invoke-VSBuild.ps1` hardcodes `/t:Build` (see `Get-MSBuildBuildArguments`) and exposes
no target parameter. Legacy non-SDK `.csproj` up-to-date checks are **timestamp-based, not
property-based**, so adding `/p:Nullable=enable /p:TreatWarningsAsErrors=true` or
`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` does **not** force a recompile when
`bin`/`obj` are newer than the sources. The build then reports `EXIT_CODE: 0` with **0 `CoreCompile`
targets** and emits no `CS86xx`/analyzer diagnostic at all.

**Why:** On #418 Phase 0, both plan-commanded solution gates returned EXIT 0 in under 7 seconds
(analyzer: 1 CoreCompile, 61 up-to-date notices; nullable: 0 CoreCompile). A supplementary
`/t:Rebuild` with the identical property set exposed the real state: **196 nullable errors** (195 in
`UtilitiesCS.csproj` — CS8766/CS8618/CS8625/CS8600/CS8601/CS8604/CS8602/CS8603/CS8714 — plus 1
`CS8630` in `SVGControl.Test.csproj`, "Invalid 'nullable' value: 'Enable' for C# 7.3"). An
EXIT-0 incremental baseline would have handed the next phase a comparison basis that claimed the
solution was nullable-clean.

**How to apply:** When a plan task says "capture the baseline analyzer/nullable state", run the plan
command verbatim and record its result as authoritative, then run a **supplementary** `/t:Rebuild`
with the identical `/p:` set purely to enumerate diagnostics. Label it supplementary; it is a
recording action, not a substitute for the plan command. Check `grep -c "CoreCompile" <log>` — if it
is 0 or 1 for a whole solution, the inventory is vacuous. Two cautions: a failing nullable Rebuild
leaves outputs cleaned, so follow it with a passing analyzer Rebuild to restore `*.Test.dll` before
any coverage run; and `Invoke-VSBuild.ps1` also runs `Sync-PackageReferences.ps1` on every
invocation, so verify `git status` afterwards.

**This is NOT specific to `Invoke-VSBuild.ps1`.** Re-measured 2026-08-08 (#505 preflight) with a
direct `MSBuild.exe` call — the exact `CLAUDE.md` analyzer command
(`TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true
/p:EnforceCodeStyleInBuild=true`) run immediately after a plain `/t:Build` returned `EXIT_CODE=0`
with **18 `Skipping target "CoreCompile"` notices and 0 `csc.exe` invocations**. The analyzer gate
therefore analyzed nothing. Measure it with
`/fl /flp:"logfile=<log>;verbosity=normal"` then count `Skipping target "CoreCompile"` and
`csc.exe` occurrences — `csc.exe = 0` is the unambiguous vacuity signal.

**Plan-review consequence:** in any final-QC phase the analyzer step almost always follows an
earlier build of the same tree (and, on a loop restart, follows the type-check `/t:Rebuild`), so a
`/t:Build` analyzer gate is vacuous by construction. Require `/t:Rebuild` for the analyzer gate, or
require the acceptance artifact to record a non-zero `csc.exe`/`CoreCompile` count for
`TaskMaster` and `TaskMaster.Test`. CI does not hit this because it always starts from a clean
checkout; `ci.yml` states the same rationale in a comment on its type-check step.

Related: [[project_repo_sdk_and_nullable_rebuild]], [[project_364_nullable_gate_preexisting_blockers]],
[[project_vs18_build_toolchain_paths]], [[project_nullable_build_gate_is_vacuous_incremental]].
