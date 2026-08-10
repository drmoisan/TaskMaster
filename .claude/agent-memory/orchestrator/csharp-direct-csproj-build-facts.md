---
name: csharp-direct-csproj-build-facts
description: Direct single-csproj MSBuild needs /p:Platform=AnyCPU (no space) unlike the solution's "Any CPU"; and CS2002 is NOT promoted by TreatWarningsAsErrors
metadata:
  type: project
---

Two verified facts about building a single legacy non-SDK project in this repo, established while preparing issue #394 (2026-08-10).

**1. The platform spelling differs between a direct-csproj build and a solution build.**

- Direct project: `msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU` — **`AnyCPU`, no space**.
- Solution: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" ...` — **`Any CPU`, with a space**.

**Why:** the csproj declares `<Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>` and keys its Debug/Release PropertyGroups off `'$(Configuration)|$(Platform)' == 'Debug|AnyCPU'`, whereas `TaskMaster.sln`'s `SolutionConfigurationPlatforms` uses `Debug|Any CPU`. Passing the solution spelling to a direct csproj build fails `_CheckForInvalidConfigurationAndPlatform`.

**How to apply:** when a plan or evidence step builds one project rather than the solution, use the no-space spelling, and do not "fix" the apparent inconsistency between the two commands — the asymmetry is correct. A direct `/t:Rebuild` also transitively builds `ProjectReference` dependencies, so no `.sln` context is needed. Pair this with [[bash-tool-mangles-msbuild-switches]] (invoke via `pwsh -NoProfile`, absolute paths) and [[msbuild-analyzer-gate-vacuous-without-rebuild]] (`/t:Build` skips `CoreCompile`).

**2. `/p:TreatWarningsAsErrors=true` does NOT promote CS2002 to an error.**

Verified against a green 2026-08-08 run of CI's exact command (`/t:Rebuild ... /p:TreatWarningsAsErrors=true`, EXIT_CODE 0) that listed CS2002 as a warning while the duplicate `<Compile Include>` was present. No `NoWarn` or `WarningsNotAsErrors` suppression exists in any csproj or `.editorconfig`, and there is no `Directory.Build.props`. The likely mechanism is that CS2002 is a compiler-driver diagnostic emitted before Roslyn applies `/warnaserror` filtering.

**Why it matters:** issues #394 and #510 both assert the duplicate "would break the build if warning-promotion rules changed." That claim is not supported by the evidence, and sibling feature `csharp-toolchain-gate-fidelity-512` changes only the *documentation* of the gate, not diagnostic severities. Justify removing CS2002 noise on warning-signal hygiene, and keep severity Low — do not inflate it into a latent build-breaker.
</content>
