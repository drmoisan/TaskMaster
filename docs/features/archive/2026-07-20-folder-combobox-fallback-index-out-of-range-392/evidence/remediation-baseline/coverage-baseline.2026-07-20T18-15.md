Timestamp: 2026-07-20T18-15
Command: `dotnet-coverage collect -f cobertura -s coverage-exclude-deedle.xml -o remediation-baseline-coverage.cobertura.xml -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation`
(preceded by `MSBuild.exe TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, EXIT_CODE 0, 75 Warning(s), 0 Error(s), after resolving a fresh-session package-restore gap — see note below)
EXIT_CODE: 0
Output Summary:
- Total tests: 541. Passed: 541. Failed: 0. Total time: 7.7568 seconds.
- Class-level coverage for `QuickFiler.Controllers.QfcItemController` sourced from
  `QfcItemController.FolderHandling.cs`: **line-rate 0.918918918918919 (91.89%), branch-rate
  0.7380952380952381 (73.81%)** — matches the expected starting point from
  `policy-audit.2026-07-20T18-00.md` Section 5.2 exactly (re-measured, not assumed).
- Method-level breakdown: `LoadFolderHandler` line 100%/branch 55.56%; `PopulateFolderComboBox`
  line 70%/branch 50%; `AssignFolderComboBox` line 89.29%/branch 87.5%; `PopulateAndSelectFolder`
  line 100%/branch 100%.

## Pre-collection environment note (recorded for auditability)

This is a fresh session turn; the local NuGet `packages/` folder (gitignored) needed
re-verification before the Debug build succeeded. Beyond the three analyzer packages and `log4net`
already documented in the original cycle's baseline evidence, a systematic scan (comparing every
`packages\<name>\` path referenced by any `.csproj` file against what is actually present under
`packages/`) found 15 additional stale-referenced package versions missing:
`AngleSharp.1.4.0`, `Castle.Core.5.1.1`, `FluentAssertions.6.12.0`, `MSTest.TestAdapter.3.1.1`,
`MSTest.TestFramework.3.1.1`, `Microsoft.Extensions.TimeProvider.Testing.9.10.0`,
`Microsoft.Graph.5.105.0`, `Moq.4.20.69`, `OpenTelemetry.PersistentStorage.Abstractions.1.1.0`,
`OpenTelemetry.PersistentStorage.FileSystem.1.1.0`, `Std.UriTemplate.2.0.8`,
`System.Reactive.6.1.0`, `System.Runtime.CompilerServices.Unsafe.6.0.0`,
`System.Threading.Tasks.Extensions.4.5.4`, `altcover.8.6.45`. All 15 were installed via
`nuget.exe install <PackageId> -Version <version> -OutputDirectory packages` without editing any
`.csproj`/`.config` file — the same non-project-file-modifying workaround pattern documented in the
original cycle's `evidence/baseline/analyzer-baseline.2026-07-20T13-25.md`. This is a pre-existing,
out-of-scope, repo-wide packages.config/csproj-hint-path drift from the `1e5ada71 (chore): update
packages` commit, unrelated to this remediation cycle's Scope-Lock.
