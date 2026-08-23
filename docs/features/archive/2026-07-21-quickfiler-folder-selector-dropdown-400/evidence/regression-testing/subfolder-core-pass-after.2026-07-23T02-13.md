# Subfolder Core Pass-After Gate

Timestamp: 2026-07-23T02:13:57.6465143Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7/IDE/Extensions/TestPlatform/vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest 'UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll' /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbSubfolderSelectorSessionTests|FullyQualifiedName~BreadcrumbSelectorMessagesTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterEdgeTests" /Logger:"console;Verbosity=normal"`

EXIT_CODE: 0

Output Summary: VSTest resolved to `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. The exact three-class UtilitiesCS.Test filter discovered 39 tests and passed 39 with 0 failures and 0 skips in 1.5607 seconds. The three durable subfolder-session follow-up cases, typed selector-subfolder message round-trip and constructor cases, four invalid parser cases, valid router atomic commit, invalid identity/index/plain-row no-mutation, durable full-path readback, and legacy bridge/subfolder controls all passed.
