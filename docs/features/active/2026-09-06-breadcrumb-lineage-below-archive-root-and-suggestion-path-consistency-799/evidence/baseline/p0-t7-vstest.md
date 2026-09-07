# [P0-T7] vstest.console.exe resolution

Timestamp: 2026-09-07T06-53

Command: $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" ;
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1 ;
$vstest ; Test-Path $vstest

EXIT_CODE: 0

VSTEST-PATH: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe

## Verification

- `Test-Path` on the resolved path returned `True`, so `VSTEST-PATH` names an existing file.
- vswhere returned exactly one match for the `-find` pattern, so `Select-Object -First 1` did not discard an
  alternative installation.
- vswhere exit code 0. When the exit code is read directly after the `| Select-Object -First 1` pipeline it comes
  back empty, because `Select-Object -First` stops the upstream pipeline before the native command's exit code is
  published. The value above was therefore read from an equivalent invocation that assigns the full vswhere output
  first and applies `Select-Object -First 1` afterwards; that invocation resolved the identical single path.

Output Summary: vstest.console.exe resolves to the Visual Studio 18 Community Test Platform. This is the one
artifact in the plan exempted from R3 path reduction, because pinning the full resolved path is the task's whole
purpose; reduced per R3, the path is
`<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
Every later task that binds `$vstest` re-runs the two resolution lines above in its own shell, per R11.
