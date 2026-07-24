<#
.SYNOPSIS
    Resolves the non-workspace authority store used by Codex provenance hooks.

.DESCRIPTION
    Epic-entry receipts and routed-subagent attestations must not be writable by
    workspace-scoped agents. These helpers derive a repository- and session-bound
    directory under CODEX_HOME and exact receipt/attestation paths within it.
#>
[CmdletBinding()]
param()

function Get-CodexAuthoritySha256 {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Text)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    return [Convert]::ToHexString(
        [System.Security.Cryptography.SHA256]::HashData($bytes)
    ).ToLowerInvariant()
}

function Get-CodexCanonicalAuthorityPath {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd(
        [System.IO.Path]::DirectorySeparatorChar,
        [System.IO.Path]::AltDirectorySeparatorChar
    )
    if ($IsWindows) {
        return $fullPath.ToLowerInvariant()
    }
    return $fullPath
}

function Get-CodexAuthorityRepositoryKey {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $RepositoryRoot)

    return Get-CodexAuthoritySha256 -Text (
        Get-CodexCanonicalAuthorityPath -Path $RepositoryRoot
    )
}

function Get-CodexResolvedAuthorityPath {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $Path)

    $candidate = [System.IO.Path]::GetFullPath($Path)
    $suffix = [System.Collections.Generic.Stack[string]]::new()
    while (-not (Test-Path -LiteralPath $candidate)) {
        $leaf = Split-Path $candidate -Leaf
        if ([string]::IsNullOrWhiteSpace($leaf)) {
            break
        }
        $suffix.Push($leaf)
        $candidate = Split-Path $candidate -Parent
    }
    if (-not (Test-Path -LiteralPath $candidate)) {
        return Get-CodexCanonicalAuthorityPath -Path $Path
    }
    $item = Get-Item -Force -LiteralPath $candidate
    $resolved = if ($null -ne $item.LinkTarget) {
        $target = [string]$item.LinkTarget
        if (-not [System.IO.Path]::IsPathFullyQualified($target)) {
            $target = Join-Path (Split-Path $candidate -Parent) $target
        }
        [System.IO.Path]::GetFullPath($target)
    } else {
        [string](Resolve-Path -LiteralPath $candidate).ProviderPath
    }
    while ($suffix.Count -gt 0) {
        $resolved = Join-Path $resolved $suffix.Pop()
    }
    return Get-CodexCanonicalAuthorityPath -Path $resolved
}

function Assert-CodexAuthorityOutsideRepository {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $AuthorityPath,
        [Parameter(Mandatory)][string] $RepositoryRoot
    )

    $authority = Get-CodexResolvedAuthorityPath -Path $AuthorityPath
    $repository = Get-CodexResolvedAuthorityPath -Path $RepositoryRoot
    $prefix = $repository + [System.IO.Path]::DirectorySeparatorChar
    $comparison = if ($IsWindows) {
        [System.StringComparison]::OrdinalIgnoreCase
    } else {
        [System.StringComparison]::Ordinal
    }
    if ($authority.Equals($repository, $comparison) -or $authority.StartsWith($prefix, $comparison)) {
        throw 'EPIC_INVOCATION_ORIGIN_BLOCKED: the Codex authority store must be outside the repository workspace.'
    }
}

function Get-CodexAuthorityHome {
    [CmdletBinding()]
    [OutputType([string])]
    param()

    if (-not [string]::IsNullOrWhiteSpace($env:CODEX_HOME)) {
        return [System.IO.Path]::GetFullPath($env:CODEX_HOME)
    }
    $profileRoot = [Environment]::GetFolderPath(
        [Environment+SpecialFolder]::UserProfile
    )
    if ([string]::IsNullOrWhiteSpace($profileRoot)) {
        throw 'EPIC_INVOCATION_ORIGIN_BLOCKED: CODEX_HOME and the user profile are unavailable.'
    }
    return Join-Path $profileRoot '.codex'
}

function ConvertTo-CodexAuthorityPathSegment {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][string] $Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw 'EPIC_INVOCATION_ORIGIN_BLOCKED: authority path identity is empty.'
    }
    return $Value -replace '[^A-Za-z0-9._-]', '_'
}

function Get-CodexAuthorityStateRoot {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $SessionId
    )

    $repositoryKey = Get-CodexAuthorityRepositoryKey -RepositoryRoot $RepositoryRoot
    $safeSession = ConvertTo-CodexAuthorityPathSegment -Value $SessionId
    $stateRoot = Join-Path (
        Join-Path (Join-Path (Get-CodexAuthorityHome) 'authority/epic-entry') $repositoryKey
    ) $safeSession
    Assert-CodexAuthorityOutsideRepository -AuthorityPath $stateRoot -RepositoryRoot $RepositoryRoot
    return $stateRoot
}

function Get-CodexAuthorityReceiptPath {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $SessionId,
        [Parameter(Mandatory)][string] $TurnId
    )

    $stateRoot = Get-CodexAuthorityStateRoot `
        -RepositoryRoot $RepositoryRoot `
        -SessionId $SessionId
    $safeTurn = ConvertTo-CodexAuthorityPathSegment -Value $TurnId
    return Join-Path $stateRoot "epic-root-invocation.$safeTurn.json"
}

function Get-CodexAuthorityAttestationPath {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $SessionId,
        [Parameter(Mandatory)][string] $AttestationKey
    )

    $stateRoot = Get-CodexAuthorityStateRoot `
        -RepositoryRoot $RepositoryRoot `
        -SessionId $SessionId
    $safeKey = ConvertTo-CodexAuthorityPathSegment -Value $AttestationKey
    return Join-Path $stateRoot "codex-routing-attestation.$safeKey.json"
}

if ($MyInvocation.InvocationName -ne '.') {
    throw 'codex-authority-store.ps1 is a helper and must be dot-sourced.'
}
