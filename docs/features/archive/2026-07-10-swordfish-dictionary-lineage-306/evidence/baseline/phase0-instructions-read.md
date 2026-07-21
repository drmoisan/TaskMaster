# Phase 0 — Policy Instructions Read (P0-T1)

Timestamp: 2026-07-11T03-02

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific toolchain and coding standards)

Files Read (absolute paths, current feature worktree):
- C:/Users/DanMoisan/repos/TaskMaster-wt/swordfish-dictionary-lineage-306/CLAUDE.md
- C:/Users/DanMoisan/repos/TaskMaster-wt/swordfish-dictionary-lineage-306/.claude/rules/general-code-change.md
- C:/Users/DanMoisan/repos/TaskMaster-wt/swordfish-dictionary-lineage-306/.claude/rules/general-unit-test.md
- C:/Users/DanMoisan/repos/TaskMaster-wt/swordfish-dictionary-lineage-306/.claude/rules/csharp.md

No policy document was modified.

Notes:
- The caller directive specified reading policy files from the current feature worktree (not any stale preparation-worktree path). The four files above were read from the current worktree.
- CSharpier resolves to the globally-installed CSharpier v1.3.0 (no `.config/dotnet-tools.json` local manifest exists in this worktree, so `dotnet tool run csharpier` is unavailable). CSharpier v1 uses subcommand syntax: `csharpier check .` (verify) and `csharpier format .` (write). These are the semantic equivalents of the plan's `csharpier --check .` / `csharpier .`.
- Toolchain binaries resolved for this session:
  - MSBuild: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
  - vstest.console.exe: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
  - Repo-local .NET SDK 8.0.205 installed to .dotnet-sdk via scripts/vscode/Install-RepoDotNetSdk.ps1; NuGet packages restored via scripts/vscode/Invoke-Restore.ps1 (169 packages, restore succeeded).
