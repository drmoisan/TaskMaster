# P2-T12 Focused Coverage Checkpoint

- Timestamp: 2026-03-22T17:46:06.6717089-04:00
- Task: `P2-T12`
- Scope: `BayesianPerformanceMeasurement.cs`, `BayesianSerializationHelper.cs`

## Commands

1. Format
   - `dotnet tool run csharpier format .`
   - Exit: 0
   - Note: repo emitted the existing warning that `TaskMaster_BACKUP_1250.csproj` is invalid XML and was skipped.

2. Fresh focused build to alternate output
   - `MSBuild.exe UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU /p:OutputPath=bin\DebugP2\ /m`
   - Exit: 0
   - Note: used alternate output path because an earlier interactive PowerShell session had stale locks on the default `bin\Debug` outputs.

3. Focused tests with coverage
   - `dotnet-coverage collect --output coverage/p2t12-focused-20260322.cobertura.xml --output-format cobertura --settings coverage.config -- vstest.console.exe UtilitiesCS.Test\bin\DebugP2\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~BayesianPerformanceMeasurement_Tests|FullyQualifiedName~BayesianSerializationHelper_Tests" /InIsolation`
   - Exit: 0
   - Result: 37 tests passed, 0 failed

## Coverage

- `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs`: 87.59% line rate
- `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianSerializationHelper.cs`: 95.18% line rate

## Artifacts

- Coverage XML: `coverage/p2t12-focused-20260322.cobertura.xml`
- Test source: `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs`

## Acceptance

- Focused test classes exist and execute successfully.
- Both `P2-T12` target files are above the 80% line-rate threshold.
