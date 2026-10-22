# Phase 0 — vstest.console.exe resolution

Timestamp: 2026-09-09T13-51

Task: [P0-T7]

Command: `$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"`
Command: `$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1`

EXIT_CODE: 0

VSTEST-PATH: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe

`Test-Path -LiteralPath` against that value returned `True`. `vswhere` itself exited 0; the exit
code was captured by assigning the native command's output to a variable before the
`Select-Object -First 1` filter, because filtering the pipeline directly leaves `$LASTEXITCODE`
unset in this shell.

Neither `vstest.console.exe` nor `vswhere.exe` is on `PATH` in this environment, so every task in
this plan that needs the runner re-binds `$vstest` inside its own command block with the two
resolution lines above (D3). No shell variable survives between tasks.

D14 exempts this one artifact from the absolute-path reduction rule: pinning the resolved path is
this task's entire purpose, and the value is a `Program Files` installation path carrying no
user-profile segment and no machine name.

Output Summary: `vswhere` resolved a single `vstest.console.exe` under Visual Studio 18 Community
at exit 0, and `Test-Path` confirmed the file exists.
