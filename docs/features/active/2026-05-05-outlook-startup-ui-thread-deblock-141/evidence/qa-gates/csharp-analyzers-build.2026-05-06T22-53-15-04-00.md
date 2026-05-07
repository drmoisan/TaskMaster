Timestamp: 2026-05-06T22:53:15.6526838-04:00
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build succeeded after package restore and restoration of the in-scope `TaskMaster.Test.csproj` compile includes. MSBuild reported `5 Warning(s)` and `0 Error(s)` in `00:00:01.47`. The remaining warnings are CS8632 nullable-annotation warnings in test files (`TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs`, `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs`, and `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`).
