<#
.SYNOPSIS
    Reads and validates the immutable identity fields of a Codex agent profile.
#>
[CmdletBinding()]
param()

function Get-CodexAgentProfileAttestation {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)][string] $RepositoryRoot,
        [Parameter(Mandatory)][string] $AgentType
    )

    if ($AgentType -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]*$') {
        throw "MODEL_ROUTING_ATTESTATION_BLOCKED: invalid agent profile name '$AgentType'."
    }
    $relativePath = ".codex/agents/$AgentType.toml"
    $profilePath = Join-Path $RepositoryRoot ($relativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $profilePath -PathType Leaf)) {
        throw "MODEL_ROUTING_ATTESTATION_BLOCKED: agent profile '$relativePath' does not exist."
    }

    $bytes = [IO.File]::ReadAllBytes($profilePath)
    $sha256 = [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData($bytes)
    ).ToLowerInvariant()
    try {
        $content = [Text.UTF8Encoding]::new($false, $true).GetString($bytes)
    } catch {
        throw "MODEL_ROUTING_ATTESTATION_BLOCKED: agent profile '$relativePath' is not valid UTF-8."
    }

    $values = @{}
    $inMultilineString = $false
    $multilineDelimiter = ''
    $insideTable = $false
    foreach ($line in ($content -split "`r?`n")) {
        if ($inMultilineString) {
            if ($line.Contains($multilineDelimiter)) {
                $inMultilineString = $false
                $multilineDelimiter = ''
            }
            continue
        }
        if ($line -match '^\s*\[') {
            $insideTable = $true
            continue
        }
        if ($insideTable -or $line -match '^\s*(?:#|$)') {
            continue
        }
        if ($line -match '^\s*[A-Za-z0-9_.-]+\s*=\s*' -and
            ($line.Contains("'''") -or $line.Contains('"""'))) {
            $delimiter = if ($line.Contains("'''")) { "'''" } else { '"""' }
            $remainder = $line.Substring($line.IndexOf($delimiter) + 3)
            if (-not $remainder.Contains($delimiter)) {
                $inMultilineString = $true
                $multilineDelimiter = $delimiter
            }
            continue
        }
        if ($line -notmatch '^\s*(?<key>name|model|model_reasoning_effort)\s*=\s*"(?<value>[A-Za-z0-9._-]+)"\s*(?:#.*)?$') {
            continue
        }
        $key = [string]$Matches.key
        if ($values.ContainsKey($key)) {
            throw "MODEL_ROUTING_ATTESTATION_BLOCKED: agent profile '$relativePath' repeats '$key'."
        }
        $values[$key] = [string]$Matches.value
    }

    foreach ($requiredKey in @('name', 'model', 'model_reasoning_effort')) {
        if (-not $values.ContainsKey($requiredKey)) {
            throw "MODEL_ROUTING_ATTESTATION_BLOCKED: agent profile '$relativePath' has no valid '$requiredKey'."
        }
    }
    return [ordered]@{
        profile_path             = $relativePath
        profile_sha256           = $sha256
        profile_name             = [string]$values.name
        profile_model            = [string]$values.model
        profile_reasoning_effort = [string]$values.model_reasoning_effort
    }
}

function Test-CodexAgentProfileBinding {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)] $AgentProfile,
        [Parameter(Mandatory)][string] $AgentType,
        [Parameter(Mandatory)][string] $ActualModel,
        [Parameter(Mandatory)][string] $ExpectedModel,
        [Parameter(Mandatory)][string] $ExpectedReasoningEffort,
        [AllowNull()][AllowEmptyString()][string] $ExpectedProfilePath,
        [AllowNull()][AllowEmptyString()][string] $ExpectedProfileSha256
    )

    if ([string]::IsNullOrWhiteSpace($ExpectedModel) -or
        [string]::IsNullOrWhiteSpace($ExpectedReasoningEffort)) {
        return $false
    }
    if ([string]$AgentProfile.profile_name -cne $AgentType -or
        [string]$AgentProfile.profile_model -cne $ExpectedModel -or
        $ActualModel -cne $ExpectedModel -or
        [string]$AgentProfile.profile_reasoning_effort -cne $ExpectedReasoningEffort) {
        return $false
    }
    if (-not [string]::IsNullOrWhiteSpace($ExpectedProfilePath) -and
        [string]$AgentProfile.profile_path -cne $ExpectedProfilePath) {
        return $false
    }
    if (-not [string]::IsNullOrWhiteSpace($ExpectedProfileSha256) -and
        [string]$AgentProfile.profile_sha256 -cne $ExpectedProfileSha256) {
        return $false
    }
    return $true
}

function Test-CodexRoutedAgentType {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string] $AgentType)

    if ($AgentType -match '-c(?:1|2|3|4|3-elevated)$') {
        return $true
    }
    return @(
        'orchestrator', 'atomic-planner', 'atomic-executor', 'feature-review',
        'feature-reviewer', 'task-researcher', 'prd-feature', 'pr-author',
        'python-typed-engineer', 'powershell-typed-engineer',
        'csharp-typed-engineer', 'typescript-engineer'
    ) -contains $AgentType
}

function Test-CodexRoutingReceiptAgentType {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()] $Receipt,
        [Parameter(Mandatory)][string] $AgentType
    )

    if ($null -eq $Receipt) {
        return $false
    }
    $properties = @($Receipt.PSObject.Properties.Name)
    return $properties -contains 'deployment_agent' -and
    -not [string]::IsNullOrWhiteSpace([string]$Receipt.deployment_agent) -and
    [string]$Receipt.deployment_agent -ceq $AgentType
}

function Find-CodexModelRoutingReceipt {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $AgentType,
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]] $Checkpoints
    )

    $matchingReceipts = [Collections.Generic.List[object]]::new()
    foreach ($checkpoint in $Checkpoints) {
        if ($null -eq $checkpoint) {
            continue
        }
        $checkpointProperties = @($checkpoint.PSObject.Properties.Name)
        foreach ($receiptKey in @('codex_model_routing_receipts', 'model_routing_receipts')) {
            if ($checkpointProperties -notcontains $receiptKey) {
                continue
            }
            foreach ($receipt in @($checkpoint.$receiptKey)) {
                if (Test-CodexRoutingReceiptAgentType -Receipt $receipt -AgentType $AgentType) {
                    $matchingReceipts.Add($receipt)
                }
            }
        }
        if ($checkpointProperties -contains 'features') {
            foreach ($feature in @($checkpoint.features)) {
                if ($null -eq $feature -or
                    @($feature.PSObject.Properties.Name) -notcontains 'model_routing_receipt') {
                    continue
                }
                $receipt = $feature.model_routing_receipt
                if (Test-CodexRoutingReceiptAgentType -Receipt $receipt -AgentType $AgentType) {
                    $matchingReceipts.Add($receipt)
                }
            }
        }
    }
    if ($matchingReceipts.Count -eq 0) {
        return $null
    }
    return $matchingReceipts[$matchingReceipts.Count - 1]
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}
