# P0-T10 External tool resolution (leaf names only)

Timestamp: 2026-09-29T08-54
Command: pwsh -NoProfile -Command '$vw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; "VSWHERE=" + (Test-Path $vw); $vt = & $vw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; "VSTEST-LEAF=" + (Split-Path -Leaf $vt); $mb = & $vw -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find "MSBuild/**/Bin/MSBuild.exe" | Select-Object -First 1; "MSBUILD-LEAF=" + (Split-Path -Leaf $mb); "ACTIONLINT=" + (Test-Path "actionlint-bin/actionlint.exe")'
EXIT_CODE: 0
Output Summary:
- VSWHERE=True
- VSTEST-LEAF=vstest.console.exe
- MSBUILD-LEAF=MSBuild.exe
- ACTIONLINT=True
- No full tool path is recorded (convention C4).
