<#
.SYNOPSIS
    Portable discovery-artifact validation for the Claude enforcement hooks (#475).
.DESCRIPTION
    Validates the domain-profile document and the seven schema-governed discovery
    artifact types WITHOUT invoking a Python interpreter. Both discovery hooks
    previously shelled out to `python -m scripts.dev_tools.validate_discovery_artifacts`;
    the `.claude/**` payload ships to destinations with no guaranteed Python, Poetry,
    or `scripts/dev_tools`, where that call fails obscurely or blocks everything.

    REQUIRES POWERSHELL 7.4 OR LATER. Schema validation uses `Test-Json -SchemaFile`,
    whose JSON Schema Draft 2020-12 support was added in PowerShell 7.4. All seven
    schemas under `schemas/discovery/v1/` declare Draft 2020-12 (verified 7 of 7), so
    the floor is unavoidable. Verified present in PowerShell 7.6.3 in this environment.
    DESTINATION RISK: on PowerShell 7.0-7.3 every entry point fails CLOSED with an
    explicit, actionable message naming the required version, the `Test-Json -SchemaFile`
    Draft 2020-12 reason, and issue #475. It never degrades silently and never fails
    open. The repo standard in `.claude/rules/powershell.md` is "PowerShell 7+", which
    is below this floor; that rule file is NOT modified by this change.

    This help is the destination-visible statement of that requirement: the module ships
    inside `.claude/**`, and the pushed-down pack holds only `config/` and
    `pack-manifests/` beside it, with no pack README to carry the statement.
.NOTES
    Issue #475.

    RESULT CONTRACT (defect D-2 avoidance). Success is SILENT: a passing validation
    returns `ExitCode = 0` with EMPTY `Output`. Both hooks deny on a non-zero exit code
    OR on non-empty output, so the previous Python CLI's success line
    ("<type> validation passed: <path>") turned a PASSING validation into a DENY.

    PARITY with `validate_discovery_profile.py` / `validate_discovery_schema_artifacts.py`.
    Error families are preserved: `invalid JSON (...)`,
    `JSON root must be an object for validation`, `schema resolution failed (...)`,
    `Profile document is empty.`, `Profile document root must be a mapping.`,
    `Missing required field: <field>.` Schema location resolves solely from each
    artifact's own `$schema`, as in the reference; the type-to-file table below is
    documentation and artifact-type validation only. Deliberate divergences: an
    `http(s)://` `$schema` is NOT fetched (no guaranteed destination network, so a
    non-`file` scheme is reported fail-closed in the `schema resolution failed (...)`
    family), and `file://` resolves through `[uri]::LocalPath` rather than the
    reference's `Path(parsed.path)`, which mishandles a Windows `file:///C:/...` path.
    Unavoidable divergence: per-violation wording comes from `Test-Json`, not
    `jsonschema`, so violation strings differ while family and verdict match. The
    profile check reproduces the placeholder contract without a YAML parser (PowerShell
    ships none); see `Get-DiscoveryProfileValidationError` for what is and is not
    detected.
.EXAMPLE
    Invoke-DiscoveryArtifactValidation -ValidatorArgs @('evidence-reference', $path)

    Returns @{ ExitCode = 0; Output = '' } when the artifact conforms.
#>

Set-StrictMode -Version Latest

# Schema-governed artifact types mapped to their filenames under `schemas/discovery/v1/`.
# Verified against `validate_discovery_artifacts.py`: `runtime-scenario`,
# `product-decision`, and `unspecified-behavior` do NOT share their token's stem.
$script:DiscoverySchemaArtifactFile = [ordered]@{
    'feature-contract'     = 'feature-contract.schema.json'
    'coverage-ledger'      = 'coverage-ledger.schema.json'
    'runtime-scenario'     = 'runtime-characterization-scenario.schema.json'
    'parity-matrix'        = 'parity-matrix.schema.json'
    'unspecified-behavior' = 'unspecified-behavior-record.schema.json'
    'product-decision'     = 'product-decision-record.schema.json'
    'evidence-reference'   = 'evidence-reference.schema.json'
}

# `Test-Json -SchemaFile` gained Draft 2020-12 support in PowerShell 7.4 and all seven
# schemas declare Draft 2020-12, so this floor is unavoidable. Verified on 7.6.3 here.
$script:MinimumPowerShellVersion = [version]'7.4'

# TODO(#9001): replace with the finalized field contract once #9001 ships. Mirrors
# `_PLACEHOLDER_REQUIRED_FIELDS` in `validate_discovery_profile.py`, whose own
# TODO(#9001) marks it as the single seam that changes when #9001 lands.
$script:ProfileRequiredField = @('legacy_source_path')

function Get-DiscoveryRuntimeVersionError {
    <#
    .SYNOPSIS
        Returns the fail-closed message when the host PowerShell is below 7.4, or
        $null when the host meets the floor.
    .DESCRIPTION
        The destination version floor (issue #475). `Test-Json -SchemaFile` gained
        JSON Schema Draft 2020-12 support in PowerShell 7.4, and all seven discovery
        schemas declare Draft 2020-12, so an older host cannot validate them.

        This runs BEFORE any schema validation so an unsupported host fails with one
        explicit, actionable message instead of a raw `Test-Json` parameter error. It
        never degrades silently: no partial validation, no fail-open path.

        `PowerShellVersion` is the injectable seam, defaulting to the real
        `$PSVersionTable.PSVersion` and only ever READ; nothing here writes
        `$PSVersionTable`, so tests pass a version rather than mutating host state.
    .OUTPUTS
        [string] The fail-closed message, or $null when the host is supported.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $false)]
        [version]$PowerShellVersion = $PSVersionTable.PSVersion
    )

    if ($PowerShellVersion -ge $script:MinimumPowerShellVersion) {
        return $null
    }

    return (
        "Discovery-artifact validation requires PowerShell $($script:MinimumPowerShellVersion) or later " +
        "(this host is PowerShell $PowerShellVersion). Reason: schema validation uses " +
        "'Test-Json -SchemaFile', whose JSON Schema Draft 2020-12 support was added in PowerShell 7.4, " +
        'and every schema under schemas/discovery/v1/ declares Draft 2020-12. ' +
        'See issue #475. Upgrade the destination host to PowerShell 7.4+ to run this gate.'
    )
}

function Get-DiscoverySchemaArtifactType {
    <#
    .SYNOPSIS
        Returns the seven schema-governed artifact-type tokens, in dispatch order.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param()

    return , @($script:DiscoverySchemaArtifactFile.Keys)
}

function Get-DiscoverySchemaFileName {
    <#
    .SYNOPSIS
        Returns the schema filename an artifact-type token corresponds to, or $null
        when the token is not a schema-governed type. Documentation and artifact-type
        validation only: schema RESOLUTION is driven by the artifact's `$schema`.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$ArtifactType
    )

    if ($script:DiscoverySchemaArtifactFile.Contains($ArtifactType)) {
        return [string]$script:DiscoverySchemaArtifactFile[$ArtifactType]
    }
    return $null
}

function Get-DiscoveryProfileTopLevelKey {
    <#
    .SYNOPSIS
        Extracts the top-level mapping keys of a YAML profile document by line
        inspection, without a YAML parser.
    .DESCRIPTION
        A top-level key is an unindented `key:` line. Blank lines, comment lines, and
        the `---`/`...` document markers are skipped. Nested keys are indented and are
        therefore ignored, which is exactly the scope the placeholder contract needs.
        Returns $null when the document presents no top-level mapping key, which the
        caller reports as a non-mapping root.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $keys = [System.Collections.Generic.List[string]]::new()
    $sawContent = $false

    foreach ($line in ($Text -split '\r?\n')) {
        $trimmed = $line.Trim()
        if ($trimmed.Length -eq 0 -or $trimmed.StartsWith('#')) {
            continue
        }
        if ($trimmed -eq '---' -or $trimmed -eq '...') {
            continue
        }
        $sawContent = $true

        # Only unindented lines can carry a top-level key.
        if ($line -match '^(?<key>[A-Za-z_][A-Za-z0-9_.-]*)\s*:(\s|$)') {
            $keys.Add($Matches['key'])
        }
    }

    # Content that never presented an unindented `key:` is not a mapping root
    # (a sequence root, a bare scalar, or a flow document).
    if (-not $sawContent -or $keys.Count -eq 0) {
        return $null
    }
    return , $keys.ToArray()
}

function Get-DiscoveryProfileValidationError {
    <#
    .SYNOPSIS
        Validates domain-profile document text against the placeholder contract,
        mirroring `validate_profile_text` in `validate_discovery_profile.py`.
    .DESCRIPTION
        Reproduced checks: empty/whitespace-only document; non-mapping root; and the
        placeholder required-field set (`legacy_source_path`), reported one error per
        absent field.

        NOT reproduced: detection of arbitrary YAML syntax errors. PowerShell ships no
        YAML parser and this module must not add a dependency, so the reference's
        `Profile document is not valid YAML: <exc>` branch has no direct analogue. The
        substitute is fail-CLOSED: any document not presenting an unindented `key:`
        line is reported as a non-mapping root rather than accepted, so a malformed
        document is still rejected and only the diagnostic wording differs.
    .OUTPUTS
        [object[]] Zero or more error strings; empty when the document conforms.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text,

        [Parameter(Mandatory = $false)]
        [version]$PowerShellVersion = $PSVersionTable.PSVersion
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # The floor is enforced at EVERY validation entry point, not only the schema path,
    # so an unsupported host can never receive a partial pass from this module.
    $versionError = Get-DiscoveryRuntimeVersionError -PowerShellVersion $PowerShellVersion
    if ($null -ne $versionError) {
        $errors.Add($versionError)
        return , $errors.ToArray()
    }

    if ([string]::IsNullOrWhiteSpace($Text)) {
        $errors.Add('Profile document is empty.')
        return , $errors.ToArray()
    }

    $keys = Get-DiscoveryProfileTopLevelKey -Text $Text
    if ($null -eq $keys) {
        $errors.Add('Profile document root must be a mapping.')
        return , $errors.ToArray()
    }

    # Report every absent field so a maintainer can fix the document in one pass.
    foreach ($field in $script:ProfileRequiredField) {
        if ($keys -notcontains $field) {
            $errors.Add("Missing required field: $field.")
        }
    }

    return , $errors.ToArray()
}

function Resolve-DiscoverySchemaFilePath {
    <#
    .SYNOPSIS
        Resolves an artifact's `$schema` URI to a local schema file path.
    .DESCRIPTION
        Mirrors the Python reference's resolution rules for the cases a destination
        can service: only a `file://` URI resolves. A scheme-less or non-`file` URI is
        rejected, and an `http(s)://` URI is deliberately NOT fetched.
    .OUTPUTS
        [hashtable] `@{ Path = <string>; Error = <string> }`; exactly one is non-null.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object]$SchemaUri
    )

    if ($SchemaUri -isnot [string] -or [string]::IsNullOrEmpty([string]$SchemaUri)) {
        return @{ Path = $null; Error = 'schema resolution failed (missing $schema)' }
    }

    $uri = $null
    if (-not [System.Uri]::TryCreate([string]$SchemaUri, [System.UriKind]::Absolute, [ref]$uri)) {
        # A scheme-less value cannot resolve: these pure text validators receive no
        # document path to resolve a relative reference against.
        return @{ Path = $null; Error = 'schema resolution failed (Unsupported schema URI scheme: missing)' }
    }

    if ($uri.Scheme -ine 'file') {
        $message = "schema resolution failed (Unsupported schema URI scheme: $($uri.Scheme))"
        return @{ Path = $null; Error = $message }
    }

    # LocalPath, not the raw URI path, so a Windows `file:///C:/...` URI resolves.
    $localPath = $uri.LocalPath
    if (-not (Test-Path -LiteralPath $localPath -PathType Leaf)) {
        return @{ Path = $null; Error = "schema resolution failed (Schema file not found: $localPath)" }
    }

    return @{ Path = $localPath; Error = $null }
}

function Get-DiscoverySchemaArtifactValidationError {
    <#
    .SYNOPSIS
        Validates a schema-governed discovery artifact against its declared `$schema`,
        mirroring `_validate_against_schema` in `validate_discovery_schema_artifacts.py`.
    .DESCRIPTION
        Order of checks, matching the reference: JSON parse, object-root check,
        `$schema` extraction and resolution, then Draft 2020-12 schema validation via
        `Test-Json -SchemaFile`.
    .OUTPUTS
        [object[]] Zero or more error strings; empty when the artifact conforms.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text,

        [Parameter(Mandatory = $false)]
        [version]$PowerShellVersion = $PSVersionTable.PSVersion
    )

    $errors = [System.Collections.Generic.List[string]]::new()

    # Fail closed before any schema work: on an unsupported host `Test-Json -SchemaFile`
    # would otherwise fail with an opaque parameter error.
    $versionError = Get-DiscoveryRuntimeVersionError -PowerShellVersion $PowerShellVersion
    if ($null -ne $versionError) {
        $errors.Add($versionError)
        return , $errors.ToArray()
    }

    $parsed = $null
    try {
        $parsed = ConvertFrom-Json -InputObject $Text -Depth 100 -ErrorAction Stop
    }
    catch {
        $errors.Add("invalid JSON ($($_.Exception.Message))")
        return , $errors.ToArray()
    }

    if ($parsed -isnot [System.Management.Automation.PSCustomObject]) {
        $errors.Add('JSON root must be an object for validation')
        return , $errors.ToArray()
    }

    $schemaProperty = $parsed.PSObject.Properties['$schema']
    $schemaUri = if ($null -eq $schemaProperty) { $null } else { $schemaProperty.Value }

    $resolution = Resolve-DiscoverySchemaFilePath -SchemaUri $schemaUri
    if ($null -ne $resolution.Error) {
        $errors.Add([string]$resolution.Error)
        return , $errors.ToArray()
    }

    # Test-Json emits one non-terminating error per schema violation; collect them
    # all rather than stopping at the first, matching the reference's behavior.
    $schemaErrors = $null
    $isValid = Test-Json -Json $Text -SchemaFile ([string]$resolution.Path) `
        -ErrorAction SilentlyContinue -ErrorVariable schemaErrors
    if (-not $isValid) {
        foreach ($schemaError in @($schemaErrors)) {
            $errors.Add([string]$schemaError.Exception.Message)
        }
        # Guarantee a non-empty result so a failure is never reported as a pass.
        if ($errors.Count -eq 0) {
            $errors.Add('JSON is not valid with the schema')
        }
    }

    return , $errors.ToArray()
}

function Get-DiscoveryArtifactValidationError {
    <#
    .SYNOPSIS
        Validates artifact text of any supported type, dispatching on the type token.
    .DESCRIPTION
        Dispatches `profile` to the placeholder-contract check and each of the seven
        schema-governed types to the `$schema`-driven check. An unrecognized token is
        reported rather than silently accepted.
    .OUTPUTS
        [object[]] Zero or more error strings; empty when the artifact conforms.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$ArtifactType,

        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text,

        [Parameter(Mandatory = $false)]
        [version]$PowerShellVersion = $PSVersionTable.PSVersion
    )

    # Assign before returning, never `@(Get-...)`. These helpers emit their array as
    # a SINGLE object (the unary-comma no-enumerate idiom), so wrapping the call in
    # `@()` collects that one object into a nested one-element array.
    if ($ArtifactType -ieq 'profile') {
        $profileErrors = Get-DiscoveryProfileValidationError -Text $Text -PowerShellVersion $PowerShellVersion
        return , $profileErrors
    }

    if ($script:DiscoverySchemaArtifactFile.Contains($ArtifactType)) {
        $schemaErrors = Get-DiscoverySchemaArtifactValidationError -Text $Text -PowerShellVersion $PowerShellVersion
        return , $schemaErrors
    }

    return , @("Unsupported artifact type: $ArtifactType")
}

function ConvertTo-DiscoveryValidationResult {
    <#
    .SYNOPSIS
        Shapes an error list into the hook seam's `@{ ExitCode; Output }` result.
    .DESCRIPTION
        The defect D-2 contract in one place: an EMPTY error list yields `ExitCode = 0`
        with an EMPTY `Output`. Both hooks deny on a non-zero exit code OR on non-empty
        output, so success chatter here would turn a passing validation into a deny.
        Kept separate from the on-disk entry point so the contract is unit-testable
        without touching the filesystem.
    .OUTPUTS
        [hashtable] `@{ ExitCode = <int>; Output = <string> }`.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$ValidationError
    )

    if (@($ValidationError).Count -eq 0) {
        return @{ ExitCode = 0; Output = '' }
    }
    return @{ ExitCode = 1; Output = ($ValidationError -join [System.Environment]::NewLine) }
}

function Invoke-DiscoveryArtifactValidation {
    <#
    .SYNOPSIS
        Validates a discovery artifact on disk and returns the hook seam's result shape.
    .DESCRIPTION
        The entry point both discovery hooks call from `Invoke-DiscoveryValidatorExe`.
        `ValidatorArgs` keeps the CLI-style shape the seam already passed:
        `@(<artifact-type>, <path>)`. Success returns `ExitCode = 0` with EMPTY
        `Output` (defect D-2 avoidance); see `ConvertTo-DiscoveryValidationResult`.
    .OUTPUTS
        [hashtable] `@{ ExitCode = <int>; Output = <string> }`.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$ValidatorArgs,

        [Parameter(Mandatory = $false)]
        [version]$PowerShellVersion = $PSVersionTable.PSVersion
    )

    if (@($ValidatorArgs).Count -lt 2) {
        return @{ ExitCode = 1; Output = 'Discovery validation requires an artifact type and a path.' }
    }

    $artifactType = [string]$ValidatorArgs[0]
    $path = [string]$ValidatorArgs[1]

    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        return @{ ExitCode = 1; Output = "Artifact not found: $path" }
    }

    $text = Get-Content -LiteralPath $path -Raw -ErrorAction Stop
    if ($null -eq $text) {
        $text = ''
    }

    $errors = Get-DiscoveryArtifactValidationError -ArtifactType $artifactType -Text $text `
        -PowerShellVersion $PowerShellVersion
    return ConvertTo-DiscoveryValidationResult -ValidationError $errors
}

Export-ModuleMember -Function @(
    'Get-DiscoveryRuntimeVersionError',
    'Get-DiscoverySchemaArtifactType',
    'Get-DiscoverySchemaFileName',
    'Get-DiscoveryProfileTopLevelKey',
    'Get-DiscoveryProfileValidationError',
    'Resolve-DiscoverySchemaFilePath',
    'Get-DiscoverySchemaArtifactValidationError',
    'Get-DiscoveryArtifactValidationError',
    'ConvertTo-DiscoveryValidationResult',
    'Invoke-DiscoveryArtifactValidation'
)
