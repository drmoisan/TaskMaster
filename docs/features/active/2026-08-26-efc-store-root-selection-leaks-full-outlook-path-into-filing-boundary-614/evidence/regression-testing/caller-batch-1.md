Timestamp: 2026-08-27T03-22-31Z
Command: `MSBuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`
EXIT_CODE: 0
Output Summary: Batch 1 build succeeded with 0 errors. The three-class scoped run passed 12/12 tests with 0 failures.

VSTest command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.PeopleScoConverter_Tests|FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.ScDictionaryConverter_Tests|FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.ScoDictionaryConverterTests" "/Logger:trx;LogFileName=p3-t4.trx" "/ResultsDirectory:coverage\trx\p3-t4"`
VSTest exit code: 0
Test result: 12 total, 12 passed, 0 failed.
