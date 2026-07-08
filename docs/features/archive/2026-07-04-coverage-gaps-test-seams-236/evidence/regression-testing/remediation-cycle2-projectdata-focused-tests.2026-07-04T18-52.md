Timestamp: 2026-07-04T20:04:22-04:00
Task: [P2-T6]

Command: dotnet tool run csharpier format "ToDoModel.Test/Data Model/Project/ProjectDataCoverageExpansionTests.cs"
EXIT_CODE: 0
Output Summary: Formatted 1 file successfully.

Command: (Get-Content 'ToDoModel.Test/Data Model/Project/ProjectDataCoverageExpansionTests.cs').Count
EXIT_CODE: 0
Output Summary: 238 lines; the new test file is under the 500-line repository limit.

Command: msbuild ToDoModel.Test\ToDoModel.Test.csproj /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
Output Summary: Build succeeded. Existing warnings remained in ToDoModel.Test\Data Model\People\PeopleScoDictionaryNewTests.cs for unused fields `mockApplication`, `_mockPrefix`, and `_peopleScoDictionaryNew`; no new errors were reported.

Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' ToDoModel.Test\bin\Debug\ToDoModel.Test.dll /TestCaseFilter:"FullyQualifiedName~ProjectDataCoverageExpansionTests" /InIsolation
EXIT_CODE: 0
Output Summary: Test Run Successful. Total tests: 8. Passed: 8. Failed: 0.

Command: & 'C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe' collect --settings 'coverage.config' --output 'docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\regression-testing\remediation-cycle2-projectdata-focused-coverage.cobertura.xml' --output-format cobertura -- 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' ToDoModel.Test\bin\Debug\ToDoModel.Test.dll /TestCaseFilter:"FullyQualifiedName~ProjectDataCoverageExpansionTests" /InIsolation
EXIT_CODE: 0
Output Summary: Test Run Successful. Total tests: 8. Passed: 8. Failed: 0. Cobertura output written to docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\regression-testing\remediation-cycle2-projectdata-focused-coverage.cobertura.xml. The existing coverage.config was used to avoid instrumenting third-party Deedle/FSharp assemblies; no coverage configuration was modified.

Command: Parse remediation-cycle2-projectdata-focused-coverage.cobertura.xml and compare ToDoModel\Data Model\Project\ProjectData.cs against remediation-cycle2-baseline-coverage.cobertura.xml.
EXIT_CODE: 0
Output Summary: Target file `ToDoModel\Data Model\Project\ProjectData.cs`; baseline covered lines 7; focused valid lines 216; focused covered lines 94; focused line rate 43.52%; newly covered lines versus baseline 91.

Acceptance Summary:
- Construction defaults are covered by the default, IList, and IEnumerable constructor tests.
- Project-entry transitions are covered by SetIdUpdateAction propagation, IsCorrupt state checks, and UpdateProjectID duplicate/new-ID checks.
- Duplicate and missing project behavior is covered by project name, project ID, program name, and Programs_ByProjectNames query tests.
- Invalid input behavior is covered by Programs_ByProjectNames(null) returning an empty string.
- Frame filtering and project category conversion are covered with deterministic in-memory Deedle frames.
- No filesystem dependencies, Outlook dependencies, external services, coverage exemptions, or coverage configuration changes were added.
