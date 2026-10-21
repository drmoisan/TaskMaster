# Remediation Cycle 1 Toolchain Exemption (Issue 930)

Timestamp: 2026-09-29T10-24

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath WORKTREE-ROOT; $names = @(git diff --name-only 39845d4a3f2f38d5c018f41e15e8372d432553c5 -- "*.cs" "*.csproj" "*.props" "*.targets" "*.sln" "*.runsettings" "packages.config" "*.config"); "CODE_OR_CONFIG_TRACKED_CHANGES=$($names.Count)"; $st = @(git status --porcelain --untracked-files=all); $ext = @($st | Where-Object { $_ -match "[.](cs|csproj|props|targets|sln|runsettings|config)$" }); "CODE_OR_CONFIG_PORCELAIN=$($ext.Count)"; $md = @($st | Where-Object { $_ -match "[.]md$" }); "CONTROL_MD_PORCELAIN=$($md.Count)"'

EXIT_CODE: 0

Output Summary:

- CODE_OR_CONFIG_TRACKED_CHANGES=0
- CODE_OR_CONFIG_PORCELAIN=0
- CONTROL_MD_PORCELAIN=24 (at least 1; the porcelain filter sees the markdown artifacts, so zero code paths is not a blind filter)

The CSharpier format, analyzer Rebuild, nullable Rebuild and MSTest-with-coverage steps are not run because the cycle diff holds no C# source, test, project, props, targets, solution, runsettings or configuration file. The footprint proof is r1-footprint.md (OUTSIDE_TRACKED_CHANGES=0).
