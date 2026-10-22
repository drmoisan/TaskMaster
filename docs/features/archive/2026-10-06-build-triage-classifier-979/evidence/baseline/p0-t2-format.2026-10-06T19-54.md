Timestamp: 2026-10-06T19-54
Command: dotnet tool run csharpier check .
EXIT_CODE: 1
Output Summary: CSharpier could not start because the repository-local .NET SDK is missing. No unformatted-file list was available.

Output:

```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application 'tool' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
```
