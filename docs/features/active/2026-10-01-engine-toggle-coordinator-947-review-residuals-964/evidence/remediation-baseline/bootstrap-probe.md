# Toolchain bootstrap probe (P0-T5)

Timestamp: 2026-10-03T09-23 (host clock read at correction; the label first written was composed, not read)
Command: pwsh -NoProfile -Command (Set-Location to the worktree; SDK marker test; dotnet --version; dotnet tool list --local; package directory count; dotnet-coverage resolution)
EXIT_CODE: 0
Output Summary: SDK marker present, SDK 8.0.205 active, csharpier 1.2.6 restored, 172 package directories, dotnet-coverage resolved.

SDK_MARKER=True
dotnet --version: 8.0.205
Local tools (Package Id, Version): csharpier 1.2.6
PACKAGE_DIRS=172
DOTNET_COVERAGE_RESOLVED=True
