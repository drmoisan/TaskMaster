# Subfolder Accessibility Failure-Before Gate

Timestamp: 2026-07-23T02:27:25.1125458Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:'FullyQualifiedName~FolderBreadcrumbAssetContractTests.ExpandedSubfolders_UseOneAccessibleStableIdentityActivationPath' /Logger:'console;Verbosity=normal'`

EXIT_CODE: 1

Output Summary: Expected failure confirmed. Exactly one test was discovered and failed because the compiled HTML resource did not give expanded subfolders an accessible option role or a shared stable-identity activation path. The pre-remediation resource still posted the legacy `selectionChange` row-index message from mouse clicks and provided no Enter/Space subfolder activation handler.
