# Subfolder Core Pass-After Gate

Timestamp: 2026-07-23T02:21:12.1092619Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7/IDE/Extensions/TestPlatform/vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest 'UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll' /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbSubfolderSelectorSessionTests|FullyQualifiedName~BreadcrumbSelectorMessagesTests|FullyQualifiedName~FolderBreadcrumbBridgeRouterEdgeTests" /Logger:"console;Verbosity=normal"`

EXIT_CODE: 0

Output Summary: This corrected gate supersedes `subfolder-core-pass-after.2026-07-23T02-13.md`. VSTest resolved to `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`. The exact three-class UtilitiesCS.Test filter discovered 53 tests and passed 53 with 0 failures and 0 skips in 1.8862 seconds. Fourteen strict-token cases proved numeric/float/bool/null/array/object row identities and string/float/bool/null/array/object/out-of-range subfolder indexes are normalized to `FormatException`, including both 32-bit and large-integer overflow. Durable-session and router cases activated non-current suggestion row 1 at nonzero child index 1 and proved committed identity, exact `\Inbox\Projects\Zeus\Delta` readback, selected model indexes, rendered selected row, rendered subfolder path, and later commit/cancel no-ops.
