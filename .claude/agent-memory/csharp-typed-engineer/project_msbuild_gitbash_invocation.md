---
name: msbuild-gitbash-invocation
description: How to invoke MSBuild and vstest.console.exe from the Bash tool in this Windows/Git Bash environment, and the CSharpier subcommand form
metadata:
  type: project
---

Running the C# toolchain from the Bash tool (Git Bash on Windows) requires non-obvious invocation forms.

**Why:** `msbuild` and `vstest.console.exe` are not on the Bash `which` PATH. MSBuild lives under the VS install. Git Bash (MSYS) also mangles MSBuild `/switch` arguments into Windows paths (e.g., `/t:Build` becomes a path), producing `MSB1008: Only one project can be specified`. CSharpier in this repo uses the newer `format`/`check` subcommand CLI, not a bare `.` argument.

**How to apply:**
- MSBuild path: `/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe`
- vstest path: `/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe`
- Prefix MSBuild/vstest commands with `MSYS2_ARG_CONV_EXCL='*' MSYS_NO_PATHCONV=1` and quote each switch, e.g. `"/t:Build" "/p:Platform=Any CPU"`.
- CSharpier: `dotnet tool run csharpier format <path>` and `dotnet tool run csharpier check <path>` (a bare `csharpier .` errors with "Required command was not provided").
- The vendored projects `SVGControl` and `UtilitiesSwordfish.NET.General` do not opt into nullable. Forcing the policy step-3 `/p:Nullable=enable /p:TreatWarningsAsErrors=true` as a global override with `/t:Rebuild` injects nullable into them and yields ~34 (SVGControl) + ~50 (UtilitiesSwordfish) pre-existing CS86xx errors. These are environmental/vendored, not first-party regressions; confirm by re-running on a stashed baseline. Standalone-building a single test `.csproj` fails with `BaseOutputPath/OutputPath not set` because the platform mapping is solution-level — build the `.sln`, not the project, when a non-default platform is needed.
- Standalone build/test the UtilitiesCS.Test DLL at `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`; see [[vstest-binding-redirect-flakiness]] for why to filter to specific classes rather than running the full assembly.
