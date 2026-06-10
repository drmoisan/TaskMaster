# Baseline Nullable / TreatWarningsAsErrors Build (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(Invoked via VS18 MSBuild at `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`, dash-switch syntax under git-bash, `-m`.)

EXIT_CODE: 0

Output Summary:
- Build succeeded. 0 Warning(s), 0 Error(s). This is the canonical plan gate (`/t:Build`) and it passes clean.

Diagnostic note (forced Rebuild — not the plan gate):
- A `/t:Rebuild` with the same nullable flags was run as a sanity check to confirm first-party nullable cleanliness when compilation is forced. It surfaced 84 errors, ALL in the two vendored projects only: 34 in `SVGControl/SVGControl.csproj` and 50 in `UtilitiesSwordfish/UtilitiesSwordfish.NET.General.csproj`. Both are vendored projects explicitly excluded from the first-party analyzer/nullable scope (G4; CLAUDE.md analyzer-stack note). ZERO first-party projects produced any nullable error under forced Rebuild.
- The plain Debug build (`/t:Build /p:Configuration=Debug` without nullable flags) was re-run afterward to restore a clean Debug output state (0 Error(s)).

Conclusion: First-party C# is nullable-clean at baseline; the only nullable diagnostics anywhere are pre-existing, out-of-scope vendored-project noise produced when `Nullable=enable` is forced globally on a Rebuild.
