# Toolchain Resolution — vstest.console.exe (P0-T5)

Timestamp: 2026-08-27T19-54

Command: `& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`

EXIT_CODE: 0

Output Summary: exactly one path resolved. Visual Studio 18 Community test platform. This is the path
every scoped test run in this plan invokes through the call operator `&` as `$vstest`.

VSTEST: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe

The recorded path ends `vstest.console.exe`. It contains no user-profile segment, no account name, and
no machine name.
