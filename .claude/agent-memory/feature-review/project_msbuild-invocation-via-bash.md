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
