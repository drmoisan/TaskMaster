<#
.SYNOPSIS
    Maps one pull-request snapshot to a merge-confirmation status (issue #952).

.DESCRIPTION
    Consumes the JSON emitted by
    `gh pr view <n> --json state,mergeCommit,mergedAt,autoMergeRequest`, or a GraphQL
    `pullRequest` node that may also carry `isInMergeQueue`, and reports whether the pull
    request has merged. Merge procedures call this parser while they poll after a merge or
    merge-queue enqueue, and record a merge only after it reports `merged`.

    Mapping (state values are case-sensitive):
      - MERGED with a non-empty mergeCommit.oid             -> merged
      - MERGED with a null mergeCommit or an empty oid      -> error
      - CLOSED                                              -> closed
      - OPEN with isInMergeQueue true, or with a present
        non-null autoMergeRequest                           -> pending
      - OPEN otherwise                                      -> not_queued
      - a null snapshot, a missing state, or any other
        state value                                         -> error

    Every error message starts with 'Invoke-MergeConfirmationParser: '.

    This script does NOT invoke `gh`. The caller runs `gh` and passes the resulting JSON
    through -PrJson or the pipeline, so the mapping is deterministically testable.

.PARAMETER PrJson
    The pull-request snapshot JSON. Accepts pipeline input. Defaults to '' so the file can be
    dot-sourced with no arguments.

.PARAMETER AsJson
    When set, emits the result as compressed JSON instead of a PowerShell object.

.EXAMPLE
    pwsh -NoProfile -File .claude/lib/ci-gate/Invoke-MergeConfirmationParser.ps1 -PrJson $snapshot -AsJson

.OUTPUTS
    A PSCustomObject (or a JSON string when -AsJson is set) with the properties Status
    (merged, pending, not_queued, or closed), MergeCommitSha, and MergedAt. MergeCommitSha and
    MergedAt are empty unless Status is merged.
#>

[CmdletBinding()]
param(
    [Parameter(ValueFromPipeline = $true)]
    [AllowEmptyString()]
    [string]$PrJson = '',

    [Parameter(Mandatory = $false)]
    [switch]$AsJson
)

# Function definitions live in begin so the script is a single explicit begin/process unit;
# the pipeline-bound -PrJson parameter forces a named process block.
begin {
    function Get-MergeConfirmationPropertyValue {
        <#
        .SYNOPSIS
            Return a named property value of a parsed object, or $null when it is absent.
        .DESCRIPTION
            Reads through PSObject.Properties so an absent property is not an error under
            Set-StrictMode -Version Latest.
        #>
        [CmdletBinding()]
        param(
            [AllowNull()]
            $InputObject,

            [Parameter(Mandatory = $true)]
            [string]$Name
        )

        if ($null -eq $InputObject) {
            return $null
        }
        $property = $InputObject.PSObject.Properties[$Name]
        if ($null -eq $property) {
            return $null
        }
        return $property.Value
    }

    function ConvertTo-MergeConfirmationTimeText {
        <#
        .SYNOPSIS
            Render a mergedAt value as text: strings verbatim, dates as UTC ISO-8601.
        #>
        [CmdletBinding()]
        [OutputType([string])]
        param(
            [AllowNull()]
            $Value
        )

        $format = 'yyyy-MM-ddTHH:mm:ssZ'
        $culture = [System.Globalization.CultureInfo]::InvariantCulture
        if ($null -eq $Value) {
            return ''
        }
        if ($Value -is [datetimeoffset]) {
            return $Value.UtcDateTime.ToString($format, $culture)
        }
        if ($Value -is [datetime]) {
            # A Kind of Unspecified carries no offset; JSON timestamps from gh are UTC.
            $utc = if ($Value.Kind -eq [System.DateTimeKind]::Unspecified) { [datetime]::SpecifyKind($Value, [System.DateTimeKind]::Utc) } else { $Value.ToUniversalTime() }
            return $utc.ToString($format, $culture)
        }
        return [string]$Value
    }

    function Get-MergeConfirmationStatus {
        <#
        .SYNOPSIS
            Map one parsed pull-request snapshot to the merge-confirmation result object.
        .PARAMETER Snapshot
            The parsed snapshot (gh pr view JSON or a GraphQL pullRequest node).
        .OUTPUTS
            PSCustomObject with Status, MergeCommitSha, and MergedAt.
        #>
        [CmdletBinding()]
        [OutputType([pscustomobject])]
        param(
            [AllowNull()]
            $Snapshot
        )

        if ($null -eq $Snapshot) {
            throw 'Invoke-MergeConfirmationParser: the pull-request snapshot is null or empty.'
        }
        $state = Get-MergeConfirmationPropertyValue -InputObject $Snapshot -Name 'state'
        if ($null -eq $state) {
            throw 'Invoke-MergeConfirmationParser: the pull-request snapshot has no state field.'
        }
        $stateText = [string]$state

        if ($stateText -ceq 'MERGED') {
            $mergeCommit = Get-MergeConfirmationPropertyValue -InputObject $Snapshot -Name 'mergeCommit'
            $oid = [string](Get-MergeConfirmationPropertyValue -InputObject $mergeCommit -Name 'oid')
            if ([string]::IsNullOrWhiteSpace($oid)) {
                throw 'Invoke-MergeConfirmationParser: state MERGED carries no merge commit oid.'
            }
            $mergedAt = ConvertTo-MergeConfirmationTimeText -Value (Get-MergeConfirmationPropertyValue -InputObject $Snapshot -Name 'mergedAt')
            return [pscustomobject]@{ Status = 'merged'; MergeCommitSha = $oid; MergedAt = $mergedAt }
        }
        if ($stateText -ceq 'CLOSED') {
            return [pscustomobject]@{ Status = 'closed'; MergeCommitSha = ''; MergedAt = '' }
        }
        if ($stateText -ceq 'OPEN') {
            $inQueue = Get-MergeConfirmationPropertyValue -InputObject $Snapshot -Name 'isInMergeQueue'
            $autoMerge = Get-MergeConfirmationPropertyValue -InputObject $Snapshot -Name 'autoMergeRequest'
            $queued = ($inQueue -is [bool] -and $inQueue) -or ($null -ne $autoMerge)
            $status = if ($queued) { 'pending' } else { 'not_queued' }
            return [pscustomobject]@{ Status = $status; MergeCommitSha = ''; MergedAt = '' }
        }

        throw ("Invoke-MergeConfirmationParser: unrecognized pull-request state '{0}'." -f $stateText)
    }

    function Invoke-MergeConfirmationParser {
        <#
        .SYNOPSIS
            Parse a pull-request snapshot JSON string and map it to a merge-confirmation status.
        .PARAMETER PrJson
            The snapshot JSON text.
        .PARAMETER AsJson
            Emit compressed JSON instead of the result object.
        .OUTPUTS
            PSCustomObject, or System.String when -AsJson is set.
        #>
        [CmdletBinding()]
        param(
            [AllowEmptyString()]
            [AllowNull()]
            [string]$PrJson,

            [switch]$AsJson
        )

        $snapshot = $null
        if (-not [string]::IsNullOrWhiteSpace($PrJson)) {
            try {
                $snapshot = $PrJson | ConvertFrom-Json -ErrorAction Stop
            }
            catch {
                throw ('Invoke-MergeConfirmationParser: the pull-request snapshot is not valid JSON: ' + $_.Exception.Message)
            }
        }

        $result = Get-MergeConfirmationStatus -Snapshot $snapshot
        if ($AsJson) {
            return ($result | ConvertTo-Json -Depth 3 -Compress)
        }
        return $result
    }
}

# Entry point. The parser runs only when the file is executed directly; when dot-sourced,
# $MyInvocation.InvocationName is '.', so the guard leaves only the function definitions.
process {
    if ($MyInvocation.InvocationName -ne '.') {
        Invoke-MergeConfirmationParser -PrJson $PrJson -AsJson:$AsJson
    }
}
