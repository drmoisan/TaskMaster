Timestamp: 2026-03-14T13-18
Command: dotnet format TaskMaster.sln --verify-no-changes --no-restore
EXIT_CODE: 1
Output Summary:
- dotnet format failed during workspace loading
- Primary failure: Microsoft.CodeAnalysis.MSBuild.RemoteInvocationException
- Nested failure: System.TypeInitializationException for Microsoft.Build.Shared.XMakeElements
- No formatting results were produced because project loading failed before verification completed
