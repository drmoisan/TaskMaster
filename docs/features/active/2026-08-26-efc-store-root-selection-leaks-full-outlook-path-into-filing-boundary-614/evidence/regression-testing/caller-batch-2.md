Timestamp: 2026-08-27T03-23-24Z
Command: `MSBuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`
EXIT_CODE: 0
Output Summary: Batch 2 build succeeded with 0 errors. The three-class scoped run passed 34/34 tests with 0 failures.

VSTest command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.WrapperScoDictionaryTest|FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.WrapperScDictionaryTest|FullyQualifiedName~UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableLoader_Tests" "/Logger:trx;LogFileName=p3-t8.trx" "/ResultsDirectory:coverage\trx\p3-t8"`
VSTest exit code: 0
Test result: 34 total, 34 passed, 0 failed.
