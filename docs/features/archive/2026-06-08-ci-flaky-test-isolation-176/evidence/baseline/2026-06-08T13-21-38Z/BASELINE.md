# Baseline — Issue #176 (ci-flaky-test-isolation)

- Timestamp (UTC): 2026-06-08T13-21-38Z
- Branch: bug/ci-flaky-test-isolation-176
- Commit: 3b379f600a91d415d1efaaee4a4188c88ef54b4c
- Scope: test-only fix in two files.

## Toolchain baseline

1. CSharpier: repository formats clean (pinned local tool `dotnet tool run csharpier format .`).
2. Analyzers (`msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`):
   Build succeeded, 0 Error(s). Pre-existing warnings present in the test project
   (CS8632/CS0067), none in the two scoped files.
3. Nullable + TreatWarningsAsErrors (`msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`):
   When the test project recompiles under this command-line override it reports
   905 Error(s) total. These are pre-existing CS8618/CS8600/CS8625/CS0067 diagnostics
   surfaced only because `UtilitiesCS.Test.csproj` does not opt into nullable and the
   override forces it. The only scoped-file diagnostic is the pre-existing
   `OlFolderClassifierGroup_Tests.cs(29,49)` `_originalDialogInvoker` (untouched code).
   This is the project baseline under the forced override, not a regression.
4. MSTest with coverage (affected classes): 14/14 passed.

## Per-file coverage baseline (cobertura line-rate)

- OlFolderClassifierGroup.cs: 0.3889
- BayesianClassifierGroup.cs: 0.1783
- PhysicalFileInfoAdapter.cs: 0.8909 (write-mode lines 75/103/114 = hits 1)
- PhysicalDirectoryInfoAdapter.cs: 0.8659
- FileInfoWrapper.cs: 1.0

## Full-assembly MSTest baseline (local sandbox)

- Full `UtilitiesCS.Test` run: 882 failed / 2931 passed / 3813 total (863 unique
  failing test names).
- 863 of the failures are the environmental `System.Threading.Tasks.Extensions,
  Version=4.2.0.1` Moq binding-redirect `TypeInitializationException`, which occurs in
  the local vstest host independent of any source change. The remaining failures are
  flaky Win32-handle / filesystem-under-load tests outside the change scope.
- Artifacts: baseline.coverage, baseline.cobertura.xml, fullrun/baseline-full.trx
