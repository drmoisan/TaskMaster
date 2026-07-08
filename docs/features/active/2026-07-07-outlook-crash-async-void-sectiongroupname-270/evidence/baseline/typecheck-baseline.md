# Nullable / Type-Check Baseline (Issue #270)

Timestamp: 2026-07-07T22-05

Command: `MSBuild.exe TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true` (VS18 Community MSBuild 18.7.8)

EXIT_CODE: 1

Output Summary: Build FAILED with 84 pre-existing nullable errors, ALL confined to vendored projects: `UtilitiesSwordfish.NET.General.csproj` (50) and `SVGControl.csproj` (34). Error codes: CS8625 (26), CS8618 (26), CS8603 (9), CS8600 (8), CS8602 (6), CS8601 (5), CS0649 (2), CS8619 (1), CS8604 (1). Zero errors reference `AppEvents.ReadinessHookup.cs` or `AppEventsTests`. Forcing `Nullable=enable` on a global Rebuild surfaces long-standing vendored debt that halts the build before the `TaskMaster`/`TaskMaster.Test` projects are reached; this is a pre-existing baseline condition unrelated to issue #270.

No-regression method for P3-T3: the touched-project source (`AppEvents.ReadinessHookup.cs`, `AppEventsTests.cs`, new `AppEventsTests.Helpers.cs`) must not add any nullable diagnostic. This is verified two ways in Phase 3: (1) the solution-level command must yield the identical 84-error vendored-only diagnostic set (no new error citing a touched file), and (2) a targeted nullable build of `TaskMaster.csproj` and `TaskMaster.Test.csproj` against the already-built dependencies must report zero nullable errors from the touched files.
