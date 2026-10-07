Timestamp: 2026-10-06T19-54
Command: dotnet tool restore
EXIT_CODE: 1
Output Summary: The repository-local .NET SDK is missing. The dotnet tool command could not load; it directed use of `scripts/vscode/Install-RepoDotNetSdk.ps1` before retrying.

Output:

```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application 'tool' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
```
