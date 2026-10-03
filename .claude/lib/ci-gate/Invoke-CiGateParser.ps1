<#
.SYNOPSIS
    Parses `gh pr checks` JSON into the orchestrator `ci_gate` object defined by
    Step S9 (CI Green Gate) of .claude/skills/orchestrate/SKILL.md.

.DESCRIPTION
    Consumes the JSON array emitted by
    `gh pr checks --required --json bucket,name,state,link,workflow` and produces
    the `ci_gate` object the orchestrator writes to its checkpoint. The object
    carries the five contract fields: head_sha, pr_pipeline_run_id,
    pr_pipeline_run_url, conclusion, and verified_at.

    This script is the single source of truth for `ci_gate.conclusion`
    derivation. It does NOT invoke `gh`. The orchestrator runs `gh` and passes
    the resulting JSON to this script via the -ChecksJson parameter or the
    pipeline, keeping all network/executable I/O outside this derivation step so
    the logic is deterministically testable.

    Conclusion derivation (per the documented bucket mapping):
      - Any required check with bucket 'fail' or 'cancel'  -> conclusion 'failure'
      - Else any required check with bucket 'pending'       -> conclusion 'pending'
      - Else (all checks 'pass' or 'skipping', or empty set) -> conclusion 'success'
    An empty required-check set is treated as 'success' (vacuously satisfied: no
    required check can fail or be in progress). An unrecognized bucket value is a
    fail-fast error so the parser never silently passes an enum it does not
    understand.

    The verified_at timestamp is produced through an injectable clock delegate
    (-NowProvider) so tests can assert an exact deterministic value without
    reading the wall clock.

.PARAMETER ChecksJson
    The JSON array string emitted by `gh pr checks --required --json ...`. Each
    element is expected to expose at least a `bucket` property. Accepts pipeline
    / stdin input so the orchestrator can pipe the `gh` output directly.

.PARAMETER HeadSha
    The PR head SHA the required checks were observed against. Recorded verbatim
    as ci_gate.head_sha.

.PARAMETER PrPipelineRunId
    Optional GitHub Actions run id for the PR Pipeline. Recorded verbatim as
    ci_gate.pr_pipeline_run_id (may be empty when not supplied).

.PARAMETER PrPipelineRunUrl
    Optional URL of the PR Pipeline run. Recorded verbatim as
    ci_gate.pr_pipeline_run_url (may be empty when not supplied).

.PARAMETER NowProvider
    Optional clock delegate (ScriptBlock) returning the value used for
    ci_gate.verified_at. Defaults to a UTC Get-Date formatted as ISO-8601. Tests
    inject a fixed delegate to make verified_at deterministic.

.PARAMETER AsJson
    When set, emits the ci_gate object as a JSON string instead of a PowerShell
    object. The default (object) output is convenient for in-process callers; the
    JSON output is convenient for checkpoint serialization.

.EXAMPLE
    gh pr checks --required --json bucket,name,state,link,workflow |
        ./.claude/lib/ci-gate/Invoke-CiGateParser.ps1 -HeadSha $sha

.OUTPUTS
    A PSCustomObject (or JSON string when -AsJson is set) with properties
    head_sha, pr_pipeline_run_id, pr_pipeline_run_url, conclusion, verified_at.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
    [string]$ChecksJson,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$HeadSha,

    [Parameter(Mandatory = $false)]
    [string]$PrPipelineRunId = '',

    [Parameter(Mandatory = $false)]
    [string]$PrPipelineRunUrl = '',

    [Parameter(Mandatory = $false)]
    [ValidateNotNull()]
    [ScriptBlock]$NowProvider = { (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') },

    [Parameter(Mandatory = $false)]
    [switch]$AsJson
)

# Function definitions live in begin so the script is a single explicit
# begin/process unit. This is required because the script-level -ChecksJson
# parameter accepts pipeline input, which forces a named process block; once any
# named block is used, all top-level statements must reside inside named blocks.
begin {
    function Get-CiGateConclusion {
        <#
    .SYNOPSIS
        Pure derivation of ci_gate.conclusion from a parsed required-check set.

    .DESCRIPTION
        Implements the documented bucket precedence with no I/O and no external
        calls, so it is deterministically unit-testable in isolation:
          1. Any 'fail' or 'cancel' -> 'failure'.
          2. Else any 'pending'      -> 'pending'.
          3. Else                    -> 'success'.
        'skipping' is non-blocking: it neither forces failure nor pending and
        contributes to 'success'. An empty set yields 'success' (vacuous). Any
        bucket value outside the known enum throws a fail-fast error naming the
        unrecognized value, preventing a silent pass on an unknown state.

    .PARAMETER Checks
        The parsed required-check collection (each element exposing a `bucket`
        property). May be $null or empty, which yields 'success'.

    .OUTPUTS
        One of the strings 'success', 'failure', or 'pending'.

    .NOTES
        Pure function: no filesystem, network, or `gh` access.
    #>
        [CmdletBinding()]
        [OutputType([string])]
        param(
            [Parameter(Mandatory = $false)]
            [AllowNull()]
            [object[]]$Checks
        )

        # Empty or null required-check set is vacuously satisfied: there is no check
        # that can fail or be in progress, so the gate concludes 'success'.
        if ($null -eq $Checks -or $Checks.Count -eq 0) {
            return 'success'
        }

        # Track whether any check is still in progress so we can defer the 'pending'
        # decision until after confirming nothing failed (failure outranks pending).
        $anyPending = $false

        # Inspect each required check once, classifying by its bucket value. Failure
        # short-circuits immediately because it is the highest-precedence outcome.
        foreach ($check in $Checks) {
            # A check element must expose a bucket; absence is an undefined state and
            # is treated as a fail-fast error rather than silently ignored.
            if ($null -eq $check -or -not ($check.PSObject.Properties.Name -contains 'bucket')) {
                throw "Invoke-CiGateParser: required check is missing a 'bucket' property; cannot derive conclusion."
            }

            $bucket = [string]$check.bucket

            # Routing table for the bucket enum. Ordering inside the switch does not
            # change semantics because failure short-circuits via return; pending is
            # only recorded (not returned) so a later failure can still take priority.
            switch ($bucket) {
                'fail' { return 'failure' }       # a failed required check is terminal-non-success
                'cancel' { return 'failure' }       # a cancelled required check is treated conservatively as failure
                'pending' { $anyPending = $true }     # in-progress; defer until failures are ruled out
                'pass' { }                        # passing check contributes to success; no state change
                'skipping' { }                        # skipped check is non-blocking; neither failure nor pending
                default {
                    # Unknown bucket: fail fast and name the value so the unsupported
                    # enum is visible rather than silently mapped to a pass.
                    throw "Invoke-CiGateParser: unrecognized check bucket '$bucket'; refusing to derive a conclusion for an unknown state."
                }
            }
        }

        # No failure or cancel was seen. If any check was pending the gate is still
        # in progress; otherwise every check was pass/skipping and the gate succeeds.
        if ($anyPending) {
            return 'pending'
        }

        return 'success'
    }

    function ConvertTo-CiGateObject {
        <#
    .SYNOPSIS
        Thin constructor for the ci_gate object from already-derived inputs.

    .DESCRIPTION
        Assembles the five contract fields into a PSCustomObject. Exists as a
        small helper so the wrapper body stays readable and so the field set is
        defined in exactly one place. Performs no derivation and no I/O.

    .PARAMETER HeadSha
        Recorded as head_sha.
    .PARAMETER PrPipelineRunId
        Recorded as pr_pipeline_run_id.
    .PARAMETER PrPipelineRunUrl
        Recorded as pr_pipeline_run_url.
    .PARAMETER Conclusion
        The already-derived conclusion ('success'/'failure'/'pending').
    .PARAMETER VerifiedAt
        The already-resolved ISO-8601 timestamp string.

    .OUTPUTS
        A PSCustomObject with the five ci_gate properties.
    #>
        [CmdletBinding()]
        [OutputType([pscustomobject])]
        param(
            [Parameter(Mandatory = $true)][string]$HeadSha,
            [Parameter(Mandatory = $true)][AllowEmptyString()][string]$PrPipelineRunId,
            [Parameter(Mandatory = $true)][AllowEmptyString()][string]$PrPipelineRunUrl,
            [Parameter(Mandatory = $true)][string]$Conclusion,
            [Parameter(Mandatory = $true)][string]$VerifiedAt
        )

        return [pscustomobject]@{
            head_sha            = $HeadSha
            pr_pipeline_run_id  = $PrPipelineRunId
            pr_pipeline_run_url = $PrPipelineRunUrl
            conclusion          = $Conclusion
            verified_at         = $VerifiedAt
        }
    }

    function Invoke-CiGateParser {
        <#
    .SYNOPSIS
        Wrapper that parses the checks JSON, derives the conclusion, and returns
        the ci_gate object.

    .DESCRIPTION
        Thin orchestration over the pure helpers: it parses -ChecksJson (failing
        fast on malformed JSON), normalizes the result into an array, delegates
        conclusion derivation to Get-CiGateConclusion, resolves verified_at via
        the injected clock delegate, and assembles the object via
        ConvertTo-CiGateObject. The wrapper exists to separate the impure JSON-parsing
        and clock concerns from the pure derivation logic.

    .PARAMETER ChecksJson
        See script-level parameter of the same name.
    .PARAMETER HeadSha
        See script-level parameter of the same name.
    .PARAMETER PrPipelineRunId
        See script-level parameter of the same name.
    .PARAMETER PrPipelineRunUrl
        See script-level parameter of the same name.
    .PARAMETER NowProvider
        See script-level parameter of the same name.
    .PARAMETER AsJson
        See script-level parameter of the same name.

    .OUTPUTS
        PSCustomObject, or JSON string when -AsJson is set.

    .NOTES
        Throws on malformed JSON and on unrecognized bucket values (fail-fast).
    #>
        [CmdletBinding()]
        param(
            [Parameter(Mandatory = $true)]
            [string]$ChecksJson,

            [Parameter(Mandatory = $true)]
            [ValidateNotNullOrEmpty()]
            [string]$HeadSha,

            [Parameter(Mandatory = $false)]
            [string]$PrPipelineRunId = '',

            [Parameter(Mandatory = $false)]
            [string]$PrPipelineRunUrl = '',

            [Parameter(Mandatory = $false)]
            [ValidateNotNull()]
            [ScriptBlock]$NowProvider = { (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') },

            [Parameter(Mandatory = $false)]
            [switch]$AsJson
        )

        # Parse the gh JSON. ConvertFrom-Json throws on malformed input; wrap it so
        # the failure surfaces as an explicit, parser-attributed error rather than a
        # raw deserialization message, satisfying fail-fast-and-explicitly policy.
        try {
            $parsed = $ChecksJson | ConvertFrom-Json -ErrorAction Stop
        }
        catch {
            throw "Invoke-CiGateParser: malformed checks JSON could not be parsed: $($_.Exception.Message)"
        }

        # Normalize to an array. ConvertFrom-Json yields a single object for a
        # one-element payload and $null for the literal 'null'; @() coercion makes the
        # downstream Count/foreach logic uniform for the empty, single, and many cases.
        $checks = @($parsed)

        $conclusion = Get-CiGateConclusion -Checks $checks

        # Resolve the timestamp through the injected delegate so tests can pin an
        # exact value; the default delegate reads UTC wall-clock time.
        $verifiedAt = [string](& $NowProvider)

        $ciGate = ConvertTo-CiGateObject `
            -HeadSha $HeadSha `
            -PrPipelineRunId $PrPipelineRunId `
            -PrPipelineRunUrl $PrPipelineRunUrl `
            -Conclusion $conclusion `
            -VerifiedAt $verifiedAt

        # Emit either the object (default, for in-process callers) or its JSON
        # serialization (for direct checkpoint writing) based on the caller's choice.
        if ($AsJson) {
            return ($ciGate | ConvertTo-Json -Depth 5)
        }

        return $ciGate
    }
}

# Entry point. The script-level -ChecksJson parameter accepts pipeline input, so
# the entry-point execution lives in a process block: each piped JSON payload is
# handled as it arrives. The parser runs only when the file is executed directly;
# when dot-sourced for testing, $MyInvocation.InvocationName is '.', so the guard
# suppresses execution and lets tests invoke the functions with controlled inputs.
process {
    if ($MyInvocation.InvocationName -ne '.') {
        Invoke-CiGateParser `
            -ChecksJson $ChecksJson `
            -HeadSha $HeadSha `
            -PrPipelineRunId $PrPipelineRunId `
            -PrPipelineRunUrl $PrPipelineRunUrl `
            -NowProvider $NowProvider `
            -AsJson:$AsJson
    }
}

