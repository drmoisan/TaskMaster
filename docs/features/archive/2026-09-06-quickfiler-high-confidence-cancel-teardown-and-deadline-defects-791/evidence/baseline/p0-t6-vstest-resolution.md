# [P0-T6] vstest.console.exe resolution

Timestamp: 2026-09-06T14-26

Command:

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
```

EXIT_CODE: 0

VSTEST-PATH: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe

Output Summary: `vswhere` resolved exactly one candidate, and `Test-Path` on the resolved value
returned `True`, so `VSTEST-PATH` names an existing file. The installation is Visual Studio 18
Community.

R3 exemption: this artifact deliberately records the absolute resolved path in full. R3 requires
absolute host paths to be reduced everywhere else, and names this one value as the single
exception, because pinning the resolved path is the whole purpose of the task. The path contains no
user-profile segment and no machine name; it is a `Program Files` installation path.

Every later task in this plan that uses `$vstest` re-binds it with the same two resolution lines
inside its own command block, per R10, because no variable survives between tasks.
