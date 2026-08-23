# HTML Resource Wiring

Timestamp: 2026-07-21T17:04:00Z

## Resource and Project References

Command:

```powershell
[xml]$resx = Get-Content -LiteralPath 'QuickFiler/Properties/Resources.resx' -Raw; $resourceMatches = @($resx.root.data | Where-Object { $_.name -eq 'FolderBreadcrumb' -and $_.value -like '..\Resources\FolderBreadcrumb.html;*' }); [xml]$project = Get-Content -LiteralPath 'QuickFiler/QuickFiler.csproj' -Raw; $contentMatches = @($project.Project.ItemGroup.Content | Where-Object { $_.Include -eq 'Resources\FolderBreadcrumb.html' }); $sourceExists = Test-Path -LiteralPath 'QuickFiler/Resources/FolderBreadcrumb.html'; "RESX_MATCHES: $($resourceMatches.Count)"; "PROJECT_CONTENT_MATCHES: $($contentMatches.Count)"; "SOURCE_EXISTS: $sourceExists"; if ($resourceMatches.Count -ne 1 -or $contentMatches.Count -ne 1 -or -not $sourceExists) { exit 1 }
```

EXIT_CODE: 0

RESX_MATCHES: 1

PROJECT_CONTENT_MATCHES: 1

SOURCE_EXISTS: True

## Generated Designer Guard

Command:

```powershell
git diff --exit-code df5ad49c909f6b739edef45d0336151f44e827a6 -- QuickFiler/Properties/Resources.Designer.cs
```

EXIT_CODE: 0

Output Summary: `QuickFiler/Properties/Resources.resx` references `..\Resources\FolderBreadcrumb.html` exactly once, `QuickFiler/QuickFiler.csproj` includes `Resources\FolderBreadcrumb.html` exactly once, the edited source exists, and `QuickFiler/Properties/Resources.Designer.cs` is unchanged from the captured baseline.
