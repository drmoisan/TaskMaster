# Restrictive sandbox construction and bounded behavioral preflight for Codex epic children.

function Get-CodexChildSandboxProbeStartInfo {
    [CmdletBinding()]
    [OutputType([System.Diagnostics.ProcessStartInfo])]
    param(
        [Parameter(Mandatory)][string] $WorktreePath,
        [Parameter(Mandatory)][string] $CodexHomePath,
        [Parameter(Mandatory)][string] $CommandPath,
        [Parameter(Mandatory)][string[]] $DeniedPaths,
        [Parameter(Mandatory)][string] $DeniedProbePath
    )
    if (-not (Test-Path -LiteralPath $DeniedProbePath)) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: sandbox deny probe path must exist.'
    }
    $comparison = if ($IsWindows) { [System.StringComparison]::OrdinalIgnoreCase } else { [System.StringComparison]::Ordinal }
    $probeCanonical = [System.IO.Path]::GetFullPath($DeniedProbePath)
    if (-not @($DeniedPaths | Where-Object {
                $probeCanonical.Equals([System.IO.Path]::GetFullPath($_), $comparison)
            })) {
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: sandbox deny probe path is absent from the deny profile.'
    }
    $isShim = $CommandPath.EndsWith('.ps1', [System.StringComparison]::OrdinalIgnoreCase)
    $info = [System.Diagnostics.ProcessStartInfo]::new($(if ($isShim) { 'pwsh' } else { $CommandPath }))
    $info.UseShellExecute = $false; $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true; $info.RedirectStandardError = $true
    $info.WorkingDirectory = $WorktreePath; $info.Environment['CODEX_HOME'] = $CodexHomePath
    if ($isShim) { foreach ($item in @('-NoProfile', '-File', $CommandPath)) { $info.ArgumentList.Add($item) } }
    $projects = Get-CodexChildProjectsOverride -WorktreePath $WorktreePath
    $permissions = Get-CodexChildPermissionOverride -DeniedPaths $DeniedPaths
    foreach ($item in @('sandbox', '-C', $WorktreePath, '-P', 'epic-child-workspace',
            '-c', $projects, '-c', $permissions)) { $info.ArgumentList.Add($item) }
    if ($IsWindows) { $info.ArgumentList.Add('-c'); $info.ArgumentList.Add('windows.sandbox="elevated"') }
    $probeScript = @'
$ErrorActionPreference = 'Stop'
try { Get-Item -LiteralPath $args[0] -ErrorAction Stop | Out-Null } catch { exit 92 }
try { Get-Item -LiteralPath $args[1] -ErrorAction Stop | Out-Null; exit 93 } catch { exit 0 }
'@
    foreach ($item in @('pwsh', '-NoProfile', '-Command', $probeScript, $WorktreePath, $DeniedProbePath)) {
        $info.ArgumentList.Add($item)
    }
    return $info
}

function Invoke-CodexChildSandboxProbe {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][System.Diagnostics.ProcessStartInfo] $StartInfo,
        [scriptblock] $NewProcess = { [System.Diagnostics.Process]::new() }
    )
    $process = & $NewProcess; $process.StartInfo = $StartInfo
    if (-not $process.Start()) { throw 'EPIC_CHILD_LAUNCH_BLOCKED: sandbox preflight process did not start.' }
    $outputTask = $process.StandardOutput.ReadToEndAsync(); $errorTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(15000)) {
        $process.Kill($true); $process.WaitForExit()
        throw 'EPIC_CHILD_LAUNCH_BLOCKED: elevated Windows sandbox preflight timed out after 15 seconds.'
    }
    $output = $outputTask.Result; $errorOutput = $errorTask.Result
    return [pscustomobject]@{ ExitCode = $process.ExitCode; Output = $output; Error = $errorOutput }
}

function Assert-CodexChildSandboxPreflight {
    [CmdletBinding()]
    param([Parameter(Mandatory)][System.Diagnostics.ProcessStartInfo] $StartInfo)
    $result = Invoke-CodexChildSandboxProbe -StartInfo $StartInfo
    if ($result.ExitCode -ne 0) {
        throw "EPIC_CHILD_LAUNCH_BLOCKED: restrictive sandbox preflight failed: $($result.Error.Trim())"
    }
}
