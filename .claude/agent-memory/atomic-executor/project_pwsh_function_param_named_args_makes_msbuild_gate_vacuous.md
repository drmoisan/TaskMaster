---
name: pwsh-function-param-named-args-makes-msbuild-gate-vacuous
description: A PowerShell helper function whose parameter is named $args binds empty (automatic $args shadows it), so `& msbuild @args` runs with NO arguments — default /t:Build, no /p: — and reports "Build succeeded, 0 Error(s)" while skipping every CoreCompile; always print the bound args and the csc/CoreCompile counters
metadata:
  type: project
---

Naming a function parameter `[string[]]$args` in a `.ps1` helper silently yields an empty array:
`$args` is PowerShell's automatic unbound-arguments variable and takes precedence inside the
function body. `& $msbuildPath @args` then runs msbuild with zero switches, which resolves the
only `.sln` in the working directory and runs the DEFAULT target (`Build`, incremental, no
`/p:EnableNETAnalyzers`, no `/p:TreatWarningsAsErrors`). The console still prints
`Build succeeded.` and the exact `0 Error(s)` line, so a gate that checks only those two signals
passes vacuously.

**Why:** observed 2026-09-17 on #792 [P2-T13]: the "analyzer" pass ran in 12 s with
`CORECOMPILE-SKIPPED: 13`, `PROJECTS-DONE-REBUILD: 0`; the "nullable" pass ran in 1 s with
`CSC-INVOCATIONS: 0`. The only tell was the `MSBUILD-ARGS:` echo line printing empty. A
script-scope `$args = @(...)` (as the Phase 0 helper used) works; the failure appears only when the
name is reused as a function parameter.

**How to apply:** name build-argument parameters `$buildArgs` (or anything but `$args`), echo the
bound arguments before invoking, `throw` when fewer than the expected count are bound, and always
record the non-vacuity counters from the log — lines naming `csc.exe`/`csc.dll`,
`Skipping target "CoreCompile"`, and `Done Building Project "*.csproj" (Rebuild target(s))` — next
to the exit code. A Rebuild gate over TaskMaster.sln legitimately shows 36 csc invocations,
0 CoreCompile skips and 18 projects rebuilt.

Related: [[project_incremental_build_vacuous_baseline]], [[project_nullable_build_gate_is_vacuous_incremental]],
[[project_pwsh_param_name_case_collision_flattens_log_array]]
