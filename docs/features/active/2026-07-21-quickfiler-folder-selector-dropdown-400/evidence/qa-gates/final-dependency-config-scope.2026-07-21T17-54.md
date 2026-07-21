# Final dependency, configuration, and resource scope gate

Timestamp: 2026-07-21T17-54Z

BaselineCommitSHA: `df5ad49c909f6b739edef45d0336151f44e827a6`

Command 1: `$projectDiff = @(git diff --unified=0 $baselineSha -- QuickFiler/QuickFiler.csproj UtilitiesCS/UtilitiesCS.csproj QuickFiler.Test/QuickFiler.Test.csproj UtilitiesCS.Test/UtilitiesCS.Test.csproj); $unexpected = @($projectDiff | Where-Object { $_ -match '^[+-](?![+-])' -and $_ -notmatch '^\+\s*<Compile Include=' }); if ($unexpected.Count) { throw "Unexpected project changes: $($unexpected -join ' | ')" }`

Command 1 EXIT_CODE: 0

Project diff lines: 41

Added `Compile Include` lines: 20

Unexpected project changes: 0

Command 2: `$configChanges = @(git diff --name-only $baselineSha -- '*.config' 'packages.config' 'Directory.Packages.props' 'Directory.Build.props' 'Directory.Build.targets' '*.settings' '*.json'); if ($configChanges.Count) { throw "Unexpected dependency/config changes: $($configChanges -join ',')" }`

Command 2 EXIT_CODE: 0

Unexpected dependency/config changes: 0

Resource wiring command: `[xml]$resx = Get-Content -LiteralPath 'QuickFiler/Properties/Resources.resx' -Raw; $resourceMatches = @($resx.root.data | Where-Object { $_.name -eq 'FolderBreadcrumb' -and $_.value -like '..\Resources\FolderBreadcrumb.html;*' }); [xml]$project = Get-Content -LiteralPath 'QuickFiler/QuickFiler.csproj' -Raw; $contentMatches = @($project.Project.ItemGroup.Content | Where-Object { $_.Include -eq 'Resources\FolderBreadcrumb.html' }); $sourceExists = Test-Path -LiteralPath 'QuickFiler/Resources/FolderBreadcrumb.html'`

Resource wiring EXIT_CODE: 0

`Resources.resx` matches: 1

Project content matches: 1

HTML source exists: True

Generated resource guard command: `git diff --exit-code df5ad49c909f6b739edef45d0336151f44e827a6 -- QuickFiler/Properties/Resources.Designer.cs`

Generated resource guard EXIT_CODE: 0

Output Summary: PASS. The four legacy project diffs contain only the 20 required `Compile Include` additions, no dependency/configuration file changed, the HTML resource remains wired exactly once through both resource and project metadata, and the generated resource Designer is unchanged.
