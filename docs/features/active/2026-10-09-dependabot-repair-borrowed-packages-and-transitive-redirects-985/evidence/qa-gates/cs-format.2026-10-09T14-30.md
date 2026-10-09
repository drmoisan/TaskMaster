# C# Format Gate (P5-T1)

Timestamp: 2026-10-09T14-30
Command: git status --porcelain; pwsh -NoProfile -File CMDDIR\985-csharpier.ps1 -WorkspaceRoot WORKSPACE-ROOT -Mode format (dotnet tool run csharpier format .); git status --porcelain; pwsh -NoProfile -File CMDDIR\985-csharpier.ps1 -WorkspaceRoot WORKSPACE-ROOT -Mode check (dotnet tool run csharpier check .)
EXIT_CODE: 0
ITERATION: 1
Output Summary:
- format: EXIT_CODE 0; Formatted 1650 files in 4650ms.
- Porcelain before and after format: identical (21 lines); `git diff --numstat BASE-SHA` for the Write Set unchanged (QuickFiler.Test.csproj 0 5; the three manifests 2 0, 1 0, 1 0).
- check: EXIT_CODE 0; Checked 1650 files in 4582ms.
- Result: PASS (no rewrite; no loop restart).
