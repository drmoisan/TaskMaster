# `vstest.console.exe` Resolution — Remediation Cycle 2

- Task: `[P0-T6]`
- Timestamp: 2026-08-04T23-32
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`

## Command

Run from the repository root. This is the same resolution `scripts/vscode/Invoke-MSTest.ps1` performs.

```powershell
& (Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe') -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'
```

```
EXIT_CODE: 0
```

## Resolved path

Exactly one path was returned:

```
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
```

## Existence confirmation

```
Command: ls -la "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe"
EXIT_CODE: 0
Output:  -rwxr-xr-x 1 DanMoisan 197121 337264 Jul 17 13:09 .../vstest.console.exe*
```

The file exists, is 337264 bytes, and is executable.

## Fidelity to the wrapper's own resolution

Re-measured at the point of writing this artifact rather than transcribed from the plan. The plan cites
`scripts/vscode/Invoke-MSTest.ps1:102`. The actual resolution in that script reads:

```powershell
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
...
$vstestPath = & $vswherePath -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
```

The `vswhere` executable path, the four arguments, and the `-find` pattern are byte-identical to the
command run above. The script additionally pipes through `Select-Object -First 1`; that is immaterial
here because `vswhere` returned exactly one line. The plan's line-number citation (`:102`) matches the
`$vstestPath = ...` assignment as read.

## `<VSTEST>` binding for the remainder of this plan

For `[P0-T7]`, `[P0-T8]`, `[P1-T5]`, `[P1-T6]`, and `[P2-T9]`:

```
<VSTEST> = C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
```

All five tasks invoke it with the identical switch set — **no** `/EnableCodeCoverage`, **no**
`/InIsolation`, **no** `/Settings` — so the before/after comparison is like-for-like. Per Design
Decision 7 the order-proof runs use this bare invocation rather than
`scripts/vscode/Invoke-MSTest.ps1`, which throws under `Set-StrictMode` when a single assembly matches
its search and therefore cannot express a single-assembly or a two-assembly ordered run.

## Output Summary

`vswhere` resolved a single absolute path,
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`,
with `EXIT_CODE: 0`; the file was confirmed present on disk. The resolution command was verified
byte-identical to the one `scripts/vscode/Invoke-MSTest.ps1` uses. That path is `<VSTEST>` for every
order-proof run in this plan.
