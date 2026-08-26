---
name: msbuild-invocation-via-bash
description: How to invoke msbuild/vstest from the Bash tool in this repo — PATH and the "Any CPU" platform-quoting gotcha
metadata:
  type: project
---

The Bash tool's shell does not have `msbuild` or `vstest.console.exe` on PATH, and bash word-splitting mangles `/p:Platform="Any CPU"`.

**Why:** `msbuild TaskMaster.sln ... /p:Platform="Any CPU"` from bash either returns `command not found` (exit 127, PATH) or splits the space and fails with `MSB1008: Only one project can be specified`. Passing `Platform=AnyCPU` (no space) fails differently: `MSB4126: solution configuration "Debug|AnyCPU" is invalid` — the solution requires the literal `Any CPU` with the space.

**How to apply:** Resolve the tool via vswhere, then invoke through a `/tmp` `.cmd` wrapper so Windows (not bash) parses the quotes:
- msbuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
- vstest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
- Pattern: write a `.cmd` containing the full command with `/p:Platform="Any CPU"`, then `cmd.exe //c "$(cygpath -w /tmp/foo.cmd)"`.

A bare `msbuild ...` that returns exit 127 has NOT validated anything — do not record it as a passing toolchain step. Verified working in the #211 review (analyzers/nullable both `Build succeeded, 0 warnings, 0 errors`; vstest 4109/4109 on TaskMaster.Test + UtilitiesCS.Test with `/TestCaseFilter:"TestCategory!=LiveOutlook"`). Contrast with [[csharp-local-fullsuite-coverage-blocked]], which is about a Moq binding redirect on the *full-assembly coverage* run, not the two-assembly local run used here.

**Simpler and faster than the `.cmd` wrapper (verified #614 review, 2026-08-26):** the wrapper is unnecessary. Export `MSYS_NO_PATHCONV=1` and `MSYS2_ARG_CONV_EXCL='*'` in the same Bash call, then invoke the exe directly with the CLAUDE.md command verbatim:

```
export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*'
MSBUILD="/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe"
"$MSBUILD" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:m > "$LOG" 2>&1
```

Without those two variables MSYS rewrites every `/switch` into a path — the failure signature is `MSB1008: Only one project can be specified` with a `Full command line:` that shows `t:Rebuild`, `M:/`, and `"C:/Program Files/Git/nologo"`. That is a path-conversion artifact, NOT the `Any CPU` space problem. The same export makes `vstest.console.exe ... /Settings:... /InIsolation "/TestCaseFilter:..." "/ResultsDirectory:..."` work directly too. Also: `cmd.exe /c foo.cmd` from the Bash tool can drop into an interactive shell and return nothing — prefer the direct-exe form. Full-solution `/t:Rebuild` for both gates plus the three changed test assemblies (6093 tests) completed comfortably inside one review.

**Scoped single-project build for fast independent re-verification (issue #240 cycle-2 re-audit, 2026-07-06):** when targeting a single `.csproj` (not the `.sln`), `Platform=AnyCPU` (no space) works fine — the `MSB4126`/space-quoting problem is a solution-configuration-mapping issue, not a single-project one. Building just `UtilitiesCS.Test\UtilitiesCS.Test.csproj` (which pulls in its project-reference graph, including `UtilitiesCS`) with `/t:Rebuild /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` or `/p:Nullable=enable /p:TreatWarningsAsErrors=true` finishes in ~10-15s and is a legitimate, much faster substitute for a full `TaskMaster.sln` rebuild when the review only needs to confirm zero new diagnostics on a handful of touched files — grep the build log for the touched file names and for `: error `/`: warning ` lines rather than eyeballing the whole log. Likewise `vstest.console.exe <dll> /TestCaseFilter:"FullyQualifiedName~<ClassName>"` gives a fast targeted re-run (39 tests in ~2s) instead of the full 4000+ test suite, sufficient to independently corroborate an executor's full-suite pass claim for the specific class under review.
