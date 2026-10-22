[CmdletBinding()]
param(
    [string]$SourceRoot = "",
    [string]$WorktreeRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function ConvertTo-NormalizedRootPath {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return $Path.Replace('\', '/').TrimEnd('/')
}

function Join-NormalizedPath {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string]$ChildPath
    )

    $normalizedRoot = ConvertTo-NormalizedRootPath -Path $Root
    $normalizedChildPath = $ChildPath.Replace('\', '/').TrimStart('/')
    return "$normalizedRoot/$normalizedChildPath"
}

function Get-NormalizedParentPath {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    return (Split-Path -Parent $Path).Replace('\', '/').TrimEnd('/')
}

function Get-RelativeCustomizationPath {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceFolder,

        [Parameter(Mandatory = $true)]
        [string]$SourcePath
    )

    $normalizedSourceFolder = ConvertTo-NormalizedRootPath -Path $SourceFolder
    $normalizedSourcePath = $SourcePath.Replace('\', '/')
    return $normalizedSourcePath.Substring($normalizedSourceFolder.Length).TrimStart('/')
}

function Test-TransientCustomizationPath {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $normalizedRelativePath = $RelativePath.Replace('\', '/').TrimStart('/')
    $firstSegment = ($normalizedRelativePath -split '/')[0]
    $blockedSegments = @(
        '.cache',
        '.venv',
        'cache',
        'caches',
        'credentials',
        'dist',
        'logs',
        'node_modules',
        'out',
        'sessions',
        'tmp',
        'venv'
    )

    if ($blockedSegments -contains $firstSegment) {
        return $true
    }

    return $normalizedRelativePath -match '(^|/)(.*\.log|.*\.tmp|.*\.cache|credentials(\..*)?)$'
}

function Resolve-CodexCustomizationSourceRoot {
    [CmdletBinding()]
    param(
        [string]$SourceRoot = "",

        [Parameter(Mandatory = $true)]
        [string]$WorktreeRoot,

        [string]$ScriptRoot = $PSScriptRoot,

        [scriptblock]$InvokeGit = {
            param([string[]]$GitArgs)
            git @GitArgs 2>$null
        }
    )

    if (-not [string]::IsNullOrWhiteSpace($SourceRoot)) {
        return ConvertTo-NormalizedRootPath -Path $SourceRoot
    }

    $normalizedWorktreeRoot = ConvertTo-NormalizedRootPath -Path $WorktreeRoot
    $commonDir = [string]::Join("`n", @(& $InvokeGit -GitArgs @('-C', $normalizedWorktreeRoot, 'rev-parse', '--git-common-dir'))).Trim()
    if (-not [string]::IsNullOrWhiteSpace($commonDir)) {
        $normalizedCommonDir = ConvertTo-NormalizedRootPath -Path $commonDir
        if ($normalizedCommonDir.EndsWith('/.git', [System.StringComparison]::OrdinalIgnoreCase)) {
            return Get-NormalizedParentPath -Path $normalizedCommonDir
        }
    }

    $worktreeList = @(& $InvokeGit -GitArgs @('-C', $normalizedWorktreeRoot, 'worktree', 'list', '--porcelain'))
    foreach ($line in $worktreeList) {
        if ($line -match '^worktree\s+(.+)$') {
            return ConvertTo-NormalizedRootPath -Path $Matches[1]
        }
    }

    $fallbackPath = [System.IO.Path]::GetFullPath((Join-Path -Path $ScriptRoot -ChildPath '../..'))
    return ConvertTo-NormalizedRootPath -Path $fallbackPath
}

function Get-CodexCustomizationCopyPlan {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceRoot,

        [Parameter(Mandatory = $true)]
        [string]$WorktreeRoot,

        [scriptblock]$TestPath = {
            param([string]$LiteralPath)
            Test-Path -LiteralPath $LiteralPath -PathType Container
        },

        [scriptblock]$GetChildItem = {
            param([string]$LiteralPath)
            Get-ChildItem -LiteralPath $LiteralPath -File -Recurse
        }
    )

    $normalizedSourceRoot = ConvertTo-NormalizedRootPath -Path $SourceRoot
    $normalizedWorktreeRoot = ConvertTo-NormalizedRootPath -Path $WorktreeRoot
    if ($normalizedSourceRoot.Equals($normalizedWorktreeRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return
    }

    foreach ($customizationFolder in @('.codex', '.agents')) {
        $sourceFolder = Join-NormalizedPath -Root $normalizedSourceRoot -ChildPath $customizationFolder
        if (-not (& $TestPath -LiteralPath $sourceFolder)) {
            continue
        }

        foreach ($file in @(& $GetChildItem -LiteralPath $sourceFolder | Sort-Object -Property FullName)) {
            $fullNameProperty = $file.PSObject.Properties['FullName']
            if ($null -eq $fullNameProperty) {
                throw "Customization file object is missing FullName."
            }

            $sourcePath = [string]$fullNameProperty.Value
            $relativePath = Get-RelativeCustomizationPath -SourceFolder $sourceFolder -SourcePath $sourcePath
            if (Test-TransientCustomizationPath -RelativePath $relativePath) {
                [pscustomobject]@{
                    Action              = 'Skip'
                    CustomizationFolder = $customizationFolder
                    SourcePath          = $sourcePath.Replace('\', '/')
                    DestinationPath     = ''
                    Reason              = 'transient or machine-local path'
                }
                continue
            }

            [pscustomobject]@{
                Action              = 'Copy'
                CustomizationFolder = $customizationFolder
                SourcePath          = $sourcePath.Replace('\', '/')
                DestinationPath     = Join-NormalizedPath -Root $normalizedWorktreeRoot -ChildPath "$customizationFolder/$relativePath"
                Reason              = ''
            }
        }
    }
}

function Invoke-CodexCustomizationCopyPlan {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$CopyOperation,

        [scriptblock]$NewDirectory = {
            param([string]$LiteralPath)
            New-Item -ItemType Directory -Force -Path $LiteralPath | Out-Null
        },

        [scriptblock]$CopyFile = {
            param(
                [string]$SourcePath,
                [string]$DestinationPath
            )
            Copy-Item -LiteralPath $SourcePath -Destination $DestinationPath -Force
        }
    )

    foreach ($operation in @($CopyOperation | Where-Object { $_.Action -ne 'Skip' })) {
        $destinationDirectory = Get-NormalizedParentPath -Path $operation.DestinationPath
        & $NewDirectory -LiteralPath $destinationDirectory
        & $CopyFile -SourcePath $operation.SourcePath -DestinationPath $operation.DestinationPath
    }
}

function Write-CodexCustomizationCopySummary {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceRoot,

        [Parameter(Mandatory = $true)]
        [string]$WorktreeRoot,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$CopyOperation,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$SkippedOperation,

        [scriptblock]$WriteLog = {
            param([string]$Message)
            Write-Information -MessageData $Message -InformationAction Continue
        }
    )

    & $WriteLog -Message "SourceRoot: $(ConvertTo-NormalizedRootPath -Path $SourceRoot)"
    & $WriteLog -Message "WorktreeRoot: $(ConvertTo-NormalizedRootPath -Path $WorktreeRoot)"
    & $WriteLog -Message "Copied: $(@($CopyOperation).Count)"
    foreach ($operation in @($CopyOperation | Select-Object -First 5)) {
        & $WriteLog -Message "  copied $($operation.CustomizationFolder): $($operation.DestinationPath)"
    }

    & $WriteLog -Message "Skipped: $(@($SkippedOperation).Count)"
    foreach ($operation in @($SkippedOperation | Select-Object -First 5)) {
        & $WriteLog -Message "  skipped $($operation.CustomizationFolder): $($operation.Reason)"
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    $effectiveWorktreeRoot = if ([string]::IsNullOrWhiteSpace($WorktreeRoot)) {
        (Get-Location).Path
    }
    else {
        $WorktreeRoot
    }

    $effectiveSourceRoot = Resolve-CodexCustomizationSourceRoot -SourceRoot $SourceRoot -WorktreeRoot $effectiveWorktreeRoot
    $plan = @(
        Get-CodexCustomizationCopyPlan -SourceRoot $effectiveSourceRoot -WorktreeRoot $effectiveWorktreeRoot
    )
    $copyOperations = @($plan | Where-Object { $_.Action -ne 'Skip' })
    $skippedOperations = @($plan | Where-Object { $_.Action -eq 'Skip' })
    Invoke-CodexCustomizationCopyPlan -CopyOperation $copyOperations
    Write-CodexCustomizationCopySummary `
        -SourceRoot $effectiveSourceRoot `
        -WorktreeRoot $effectiveWorktreeRoot `
        -CopyOperation $copyOperations `
        -SkippedOperation $skippedOperations
}
