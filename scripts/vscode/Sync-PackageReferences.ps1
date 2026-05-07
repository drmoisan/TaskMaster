param(
    [Parameter(Mandatory = $false)]
    [string]$SolutionRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $SolutionRoot) {
    $SolutionRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
}

# TFM preference order for .NET Framework 4.8.1 projects
$tfmPreference = @(
    'net481', 'net48', 'net472', 'net471', 'net47',
    'net462', 'net461', 'net46', 'net452', 'net451', 'net45',
    'net40', 'net35', 'net20',
    'netstandard2.1', 'netstandard2.0'
)

$packagesDir = Join-Path $SolutionRoot 'packages'
$totalFixed = 0

Get-ChildItem $SolutionRoot -Filter 'packages.config' -Recurse |
    Where-Object { $_.DirectoryName -notmatch '\\packages\\' } |
        ForEach-Object {
            $projectDir = $_.DirectoryName
            $pkgConfigPath = $_.FullName
            $projectName = Split-Path $projectDir -Leaf

            # Find the csproj in the same directory
            $csprojFiles = @(Get-ChildItem $projectDir -Filter '*.csproj' |
                    Where-Object { $_.Name -notmatch '_(BASE|LOCAL|REMOTE)_' })
                if ($csprojFiles.Count -eq 0) { return }

                $csprojPath = $csprojFiles[0].FullName

                # Parse packages.config into a lookup: PackageId -> Version
                [xml]$pkgConfig = Get-Content $pkgConfigPath -Raw
                $pkgMap = @{}
                foreach ($pkg in $pkgConfig.packages.package) {
                    $pkgMap[$pkg.id] = $pkg.version
                }

                # Read the csproj as raw text to preserve formatting exactly
                $csprojText = [System.IO.File]::ReadAllText($csprojPath)

                # Check for merge conflict markers and skip if found
                if ($csprojText -match '<{7}|>{7}|={7}') {
                    Write-Warning "  [$projectName] Merge conflict markers detected, skipping"
                    return
                }

                $fixCount = 0
                $replacements = @{}  # oldHintPath -> newHintPath

                # Find all HintPath values pointing to ..\packages\
                $hintPathRegex = [regex]'<HintPath>(\.\.\\packages\\(.+?)\\lib\\([^\\]+)\\([^<]+))</HintPath>'
                foreach ($m in $hintPathRegex.Matches($csprojText)) {
                    $fullHpValue = $m.Groups[1].Value
                    $folderName = $m.Groups[2].Value
                    $dllFile = $m.Groups[4].Value

                    # Check if the current HintPath resolves on disk
                    $resolvedPath = Join-Path $projectDir $fullHpValue
                    if (Test-Path $resolvedPath) { continue }

                    # HintPath is broken; try to find the correct package version
                    $matchedPkg = $null
                    foreach ($pkgId in $pkgMap.Keys) {
                        if ($folderName.StartsWith("$pkgId.", [StringComparison]::OrdinalIgnoreCase)) {
                            $candidate = $folderName.Substring($pkgId.Length + 1)
                            if ($candidate -match '^\d') {
                                $matchedPkg = $pkgId
                                break
                            }
                        }
                    }
                    if (-not $matchedPkg) { continue }

                    $desiredVer = $pkgMap[$matchedPkg]
                    $newFolder = "$matchedPkg.$desiredVer"
                    $oldTfm = $m.Groups[3].Value

                    # Try same TFM first
                    $newHpValue = "..\packages\$newFolder\lib\$oldTfm\$dllFile"
                    $newResolved = Join-Path $projectDir $newHpValue
                    if (-not (Test-Path $newResolved)) {
                        # TFM might have changed; search with preference order
                        $newPkgLib = Join-Path $packagesDir "$newFolder\lib"
                        $found = $false
                        if (Test-Path $newPkgLib) {
                            foreach ($tryTfm in $tfmPreference) {
                                $tryPath = Join-Path $newPkgLib "$tryTfm\$dllFile"
                                if (Test-Path $tryPath) {
                                    $newHpValue = "..\packages\$newFolder\lib\$tryTfm\$dllFile"
                                    $found = $true
                                    break
                                }
                            }
                        }
                        if (-not $found) {
                            Write-Warning "  [$projectName] Cannot resolve $dllFile from $newFolder"
                            continue
                        }
                    }

                    $replacements[$m.Value] = "<HintPath>$newHpValue</HintPath>"
                    $fixCount++
                }

                if ($fixCount -eq 0) { return }

                # Apply HintPath replacements
                foreach ($old in $replacements.Keys) {
                    $csprojText = $csprojText.Replace($old, $replacements[$old])
                }

                # Update assembly versions in Include attributes for fixed references.
                # Load each new DLL to get its actual assembly version.
                foreach ($newHpTag in $replacements.Values) {
                    $hpMatch = [regex]::Match($newHpTag, '<HintPath>(.+)</HintPath>')
                    if (-not $hpMatch.Success) { continue }
                    $hpValue = $hpMatch.Groups[1].Value

                    $resolved = Join-Path $projectDir $hpValue
                    if (-not (Test-Path $resolved)) { continue }

                    try {
                        $asmName = [System.Reflection.AssemblyName]::GetAssemblyName($resolved)
                    }
                    catch { continue }

                    $simpleName = $asmName.Name
                    $newAsmVer = $asmName.Version.ToString()

                    # Find the Include attribute for this assembly with an old version
                    $escName = [regex]::Escape($simpleName)
                    $includePattern = "(Include=""$escName,\s*Version=)(\d+\.\d+\.\d+\.\d+)"
                    $includeMatch = [regex]::Match($csprojText, $includePattern)
                    if ($includeMatch.Success -and $includeMatch.Groups[2].Value -ne $newAsmVer) {
                        $oldInclude = $includeMatch.Value
                        $newInclude = $includeMatch.Groups[1].Value + $newAsmVer
                        $csprojText = $csprojText.Replace($oldInclude, $newInclude)
                    }
                }

                [System.IO.File]::WriteAllText($csprojPath, $csprojText)
                $totalFixed += $fixCount
                Write-Host "  [$projectName] Fixed $fixCount broken HintPath(s)"
            }

if ($totalFixed -gt 0) {
    Write-Host "Sync-PackageReferences: Fixed $totalFixed HintPath(s) total"
}
else {
    Write-Host 'Sync-PackageReferences: All HintPaths are up to date'
}

