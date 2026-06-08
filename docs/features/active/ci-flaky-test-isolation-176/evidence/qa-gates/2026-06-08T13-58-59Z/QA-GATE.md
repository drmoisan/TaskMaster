# QA Gate — Issue #176 (ci-flaky-test-isolation), Defect 2 re-fix

- Timestamp (UTC): 2026-06-08T13-58-59Z
- Branch: bug/ci-flaky-test-isolation-176
- Scope (this gate): Defect 2 rework — replace rejected scratch-file approach with an
  injectable-delegate production seam on `PhysicalFileInfoAdapter`.

## Files changed in this batch

- `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs` (production seam:
  new `internal` ctor + three private delegate fields; public ctor binds defaults to the
  wrapped `FileInfo`; three write-mode members now invoke the delegates).
- `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` (removed all
  scratch-file code; write-mode members covered via the seam with sentinel streams and
  `BeSameAs` delegation assertions).

Defect 1's file (`UtilitiesCS.Test/EmailIntelligence/OlFolderClassifierGroup_Tests.cs`)
was NOT modified in this batch; its existing `ConcurrentBag` fix is untouched.

## Toolchain results (full solution)

1. CSharpier — `dotnet tool run csharpier format .` (the pinned local tool requires the
   `format` subcommand; bare `dotnet tool run csharpier .` errors). Formatted clean; the
   two scoped files required no rewrite beyond the authored edits.
2. Analyzers — `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug
   /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`:
   Build succeeded, 0 Error(s). On a full rebuild, 19 pre-existing project-wide warnings
   appear (CS0618/CS0649/CS0067/CS8632) in files outside this change; zero warnings in
   either scoped file. Zero analyzer delta versus baseline.
3. Nullable + TreatWarningsAsErrors — `msbuild ... /p:Nullable=enable
   /p:TreatWarningsAsErrors=true`: the command-line override forces nullable on projects
   that do not opt in (`SVGControl`, `UtilitiesSwordfish.NET.General`), surfacing the
   documented pre-existing CS86xx baseline. Zero of these are in the scoped files;
   `UtilitiesCS` (the changed production project) and `PhysicalFileInfoAdapter.cs` are
   nullable-clean. Zero nullable delta versus baseline.
4. MSTest with coverage — `vstest.console.exe UtilitiesCS.Test.dll
   /TestCaseFilter:'FullyQualifiedName~PhysicalFileSystemAdapters_Tests|
   FullyQualifiedName~FileInfoWrapper_Tests' /EnableCodeCoverage`:
   12/12 passed, including `PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo`.
   (Affected-class filtered run with coverage per the documented binding-redirect
   workaround for the local vstest host.)

## Coverage — PhysicalFileInfoAdapter.cs

- Baseline line-rate: 0.8909090909090909
  (`evidence/baseline/2026-06-08T13-21-38Z/baseline.cobertura.xml`).
- Post-change line-rate: 0.9154929577464789
  (`postchange.cobertura.xml`, this folder).
- Write-mode delegation members hit: `AppendText` hits=1, `Open(FileMode)` hits=1,
  `OpenWrite` hits=1.
- New unit (internal seam constructor): line-rate 1.0 (fully covered, >= 90%).
- Result: coverage increased; no per-file regression.

## Determinism / policy

- No temporary or scratch file is created anywhere in the test.
- No real write/append/read-write handle is opened on `TaskMaster.sln` or any shared file.
- Sentinel streams are read-only opens of the test assembly DLL with `FileShare.ReadWrite`;
  the append `StreamWriter` is backed by an in-memory stream. All disposed via `using`.
- No assertions weakened; read-only `.sln` assertions retained.

## Delta summary (zero-regression gate)

- Analyzer delta: 0 new findings in scoped files.
- Compiler/nullable delta: 0 new diagnostics in scoped files.
- MSTest delta: 0 new failing tests (the target test now passes).
- Per-file coverage delta: PhysicalFileInfoAdapter.cs +0.0246 (0.8909 -> 0.9155), >= baseline.
