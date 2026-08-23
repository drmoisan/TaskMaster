# Dependency and Configuration Scope

Timestamp: 2026-07-21T17:08:00Z

BaselineCommitSHA: `df5ad49c909f6b739edef45d0336151f44e827a6`

## Legacy Project Diff

Command:

```powershell
$projectDiff = @(git diff --unified=0 $baselineSha -- QuickFiler/QuickFiler.csproj UtilitiesCS/UtilitiesCS.csproj QuickFiler.Test/QuickFiler.Test.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj); $unexpected = @($projectDiff | Where-Object { $_ -match '^[+-](?![+-])' -and $_ -notmatch '^\+\s*<Compile Include=' }); if ($unexpected.Count) { throw "Unexpected project changes: $($unexpected -join ' | ')" }
```

EXIT_CODE: 0

ADDED_COMPILE_COUNT: 20

UNEXPECTED_PROJECT_CHANGE_COUNT: 0

The first pre-evidence invocation detected final-newline drift on each project closing tag. The four project files were restored to their baseline EOF convention, and the exact gate above was rerun successfully. The final diff contains only the 20 planned added `Compile Include` entries.

## Dependency and Persisted-Configuration Diff

Command:

```powershell
$configChanges = @(git diff --name-only $baselineSha -- '*.config' 'packages.config' 'Directory.Packages.props' 'Directory.Build.props' 'Directory.Build.targets' '*.settings' '*.json'); if ($configChanges.Count) { throw "Unexpected dependency/config changes: $($configChanges -join ',')" }
```

EXIT_CODE: 0

CONFIG_CHANGE_COUNT: 0
