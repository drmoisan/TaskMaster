# P8-T47 corrected SpamBayes regression test compile

Timestamp: 2026-07-27T03-09Z

Command: `msbuild UtilitiesCS.Test/UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 0

Output Summary: The corrected standalone platform value, `AnyCPU`, compiled `UtilitiesCS.Test` with analyzers enabled. The build completed with six existing warnings and zero errors. The P8-T46 tests were discovered through VSTest before P8-T48.

## Preserved failed-command evidence

- Cited immutable artifact: `evidence/regression-testing/spambayes-regression-test-compile.2026-07-27T03-03.md`
- Preserved artifact SHA-256: `69B8B6377281881E1A43D12B4C5D608E11034EADEF8D0E5EFCED89529A715D57`
- Prior command platform: `Any CPU`; prior exit code: `1`; prior failure occurred before compilation because the standalone project had no corresponding output path.

## VSTest discovery

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll /ListTests /TestCaseFilter:FullyQualifiedName~SpamBayesActionsRegressionTests`

EXIT_CODE: 0

Discovered tests:

- `SpamBayesActionsRegressionTests.GetDestinationFolder_WhenSpamTrueAndJunkCertainExists_ReturnsConfiguredJunkCertain`
- `SpamBayesActionsRegressionTests.GetDestinationFolder_WhenSpamTrueAndJunkCertainIsNull_ReturnsCurrentParent`
