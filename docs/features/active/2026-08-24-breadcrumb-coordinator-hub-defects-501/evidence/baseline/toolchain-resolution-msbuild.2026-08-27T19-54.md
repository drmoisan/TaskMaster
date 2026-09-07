# Toolchain Resolution — MSBuild (P0-T4)

Timestamp: 2026-08-27T19-54

Command: `& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'`

EXIT_CODE: 0

Output Summary: exactly one path resolved. Visual Studio 18 Community full-framework MSBuild. This is
the path every later analyzer and nullable build in this plan invokes through the call operator `&` as
`$msbuild`.

MSBUILD: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe

The recorded path ends `Bin\MSBuild.exe`. It contains no user-profile segment, no account name, and no
machine name, so it does not violate the no-absolute-host-path rule; the workspace itself is recorded
throughout this plan as the literal token `WS`.
