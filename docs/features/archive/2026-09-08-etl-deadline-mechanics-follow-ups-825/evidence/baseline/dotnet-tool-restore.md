# Baseline — Local Tool Manifest Restore

Timestamp: 2026-09-09T16-34

Command: dotnet tool restore

EXIT_CODE: 0

CSharpierVersion: 1.2.6

Output Summary: The command printed "Tool 'csharpier' (version '1.2.6') was restored. Available
commands: csharpier" followed by "Restore was successful." The version recorded above is read from
the tool manifest at the repository root, dotnet-tools.json, whose tools/csharpier/version field is
1.2.6. The manifest is at the repository root and not under .config/; there is no
.config/dotnet-tools.json in this tree. CSharpier 1.2.6 requires a subcommand, so every invocation in
this plan uses the `dotnet tool run csharpier format .` and `dotnet tool run csharpier check .`
forms rather than the bare-path form.

Deviation recorded. The first invocation of `dotnet tool restore` in this worktree failed with exit
code -2147450725 and the message "The repo-local .NET SDK is missing. Run
./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root". This worktree was created
fresh and carried no .dotnet-sdk directory. The named script was run as a micro-action; it installed
.NET SDK 8.0.205 into .dotnet-sdk at the repository root, which is git-ignored and therefore enters
no diff. The restore was then re-run and succeeded, and the EXIT_CODE recorded above is that of the
successful re-run.
