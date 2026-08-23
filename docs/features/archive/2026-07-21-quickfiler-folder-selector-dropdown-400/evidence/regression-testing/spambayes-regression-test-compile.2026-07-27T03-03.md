# P8-T47 SpamBayes regression test compile

Timestamp: 2026-07-27T03-03Z

Command: `msbuild UtilitiesCS.Test/UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 1

Output Summary: The prescribed standalone project command did not reach compilation. MSBuild reports that `UtilitiesCS.Test.csproj` has no `BaseOutputPath` or `OutputPath` for `Configuration='Debug'` and `Platform='Any CPU'` when built outside the solution. No source, test, project, coverage, or policy file was changed after P8-T46.

## Diagnostic

`Microsoft.Common.CurrentVersion.targets(843,5): error : The BaseOutputPath/OutputPath property is not set for project 'UtilitiesCS.Test.csproj'. Please check to make sure that you have specified a valid combination of Configuration and Platform for this project. Configuration='Debug' Platform='Any CPU'.`

P8-T47 remains unchecked. The plan must be revised in place to supply a command that compiles the P8-T46 integration in this repository's solution/project configuration without expanding the P8-T46 edit scope.
