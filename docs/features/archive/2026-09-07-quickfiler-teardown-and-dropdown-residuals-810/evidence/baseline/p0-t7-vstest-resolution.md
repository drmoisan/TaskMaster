# [P0-T7] vstest.console.exe Resolution

Timestamp: 2026-09-08T09-17
Command: `$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"`; `$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1`
EXIT_CODE: 0
Output Summary: `vswhere.exe` was found at its fixed installer location and resolved a single `vstest.console.exe` under the latest Visual Studio 18 Community installation. `Test-Path` on the resolved value returned `True`, and the value is non-empty.

VSTEST-PATH: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe

## Probe results

```
VSWHERE-EXISTS: True
VSTEST-NONEMPTY: True
VSTEST-TESTPATH: True
```

## D3 and D14 notes

Neither `vstest.console.exe` nor `vswhere.exe` is on `PATH` in this environment, so every task in this plan that needs the test runner re-binds `$vstest` inside its own command block using the same two resolution lines rather than carrying a variable across tasks (D3).

D14 exempts this one artifact from the absolute-path reduction rule, because pinning the resolved path is this task's entire purpose. The recorded path is a `Program Files` installation path: it carries no user-profile segment and no machine name.

Each later task that binds `$vstest` prints the bound value and confirms it is non-empty before use, because an invocation that ran against an empty path is not evidence.
