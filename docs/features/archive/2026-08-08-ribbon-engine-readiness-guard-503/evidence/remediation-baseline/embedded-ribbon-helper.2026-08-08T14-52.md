# Phase 0 — Embedded Ribbon Resource Helper and Pre-Remediation State (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P0-T3]
Command: `pwsh -NoProfile -File <SCRATCH>\Assert-EmbeddedRibbon.ps1 -RepoRoot 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'`
EXIT_CODE: 0

## Why this helper exists

`TaskMaster\Ribbon\RibbonExplorer.xml` is an **embedded resource**. `RibbonExplorerXmlTests` reads it through `assembly.GetManifestResourceStream("TaskMaster.Ribbon.RibbonExplorer.xml")` on the `TaskMaster.dll` copied into `TaskMaster.Test\bin\Debug\`. An edit to the `.xml` on disk is invisible to the test until the assembly is rebuilt and re-copied. This helper asserts the byte content actually embedded in the built assembly, so the Phase 1 fail-proof cannot be a false negative caused by a stale assembly. It loads the assembly from a byte array rather than by path so it never holds a file lock that would block the next rebuild.

## Verbatim script text

The script is a session-throwaway file held outside the working tree. Its verbatim text is recorded here so the assertion is auditable and reproducible without the scratchpad.

```powershell
param([Parameter(Mandatory = $true)][string]$RepoRoot)
$ErrorActionPreference = 'Stop'
$dll = Join-Path $RepoRoot 'TaskMaster.Test\bin\Debug\TaskMaster.dll'
$asm = [System.Reflection.Assembly]::Load([System.IO.File]::ReadAllBytes($dll))
$stream = $asm.GetManifestResourceStream('TaskMaster.Ribbon.RibbonExplorer.xml')
$text = (New-Object System.IO.StreamReader($stream)).ReadToEnd()
$count = ([regex]::Matches($text, 'getEnabled="EngineCommand_GetEnabled"')).Count
$single = '<button id="TriageSetA" onAction="TriageSetA_Click" getEnabled="EngineCommand_GetEnabled" label="Set A" />'
Write-Output ("EMBEDDED_GETENABLED_COUNT={0}" -f $count)
Write-Output ("EMBEDDED_TRIAGESETA_SINGLELINE={0}" -f $text.Contains($single))
Write-Output ("EMBEDDED_ASSEMBLY_WRITETIME={0}" -f (Get-Item $dll).LastWriteTimeUtc.ToString('o'))
```

## Output Summary

```text
EMBEDDED_GETENABLED_COUNT=8
EMBEDDED_TRIAGESETA_SINGLELINE=False
EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T17:48:38.5907327Z
```

- `EMBEDDED_GETENABLED_COUNT=8` — the pre-remediation assembly carries all eight `getEnabled="EngineCommand_GetEnabled"` attributes, matching the state delivered by the implementation cycle.
- `EMBEDDED_TRIAGESETA_SINGLELINE=False` — expected at this point. The `TriageSetA` button is currently in the six-line form that F2 collapses. This flag becomes `True` after P2-T1 and is the P2-T4 gate.
- `EMBEDDED_ASSEMBLY_WRITETIME=2026-08-08T17:48:38.5907327Z` — the write time of the currently built `TaskMaster.dll`, recorded so a later rebuild can be distinguished from a stale artifact. (The recorded value is the file-system timestamp as reported; no interpretation of clock skew is made here.)

Binary outcome satisfied: `EMBEDDED_GETENABLED_COUNT=8` and `EMBEDDED_TRIAGESETA_SINGLELINE=False`.
