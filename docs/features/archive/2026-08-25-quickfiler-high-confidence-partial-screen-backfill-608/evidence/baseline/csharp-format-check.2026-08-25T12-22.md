Timestamp: 2026-08-25T12-22
Command: dotnet tool run csharpier check .
EXIT_CODE: 1
Output Summary: The repository-local .NET SDK is missing, so the manifest CSharpier command could not load. Output directs running scripts/vscode/Install-RepoDotNetSdk.ps1 before retrying.

Output:
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application 'tool' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
