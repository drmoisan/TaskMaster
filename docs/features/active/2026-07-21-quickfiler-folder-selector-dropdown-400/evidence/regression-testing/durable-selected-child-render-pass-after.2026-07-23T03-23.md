# Durable Selected-Child Render Pass-After Gate

Timestamp: 2026-07-23T03:23:21.4017962Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest) { Write-Error 'VSTest was not resolved.'; exit 1 }; & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:'FullyQualifiedName~BreadcrumbBridgeMessagesTests|FullyQualifiedName~BreadcrumbSubfolderActivationTests|FullyQualifiedName~FolderBreadcrumbAssetContractTests' /Logger:'console;Verbosity=normal'; exit $LASTEXITCODE`

EXIT_CODE: 0

Output Summary: VSTest resolved successfully and discovered exactly 43 tests across the three approved classes in both assemblies. All 43 passed with zero failures or skips. Runtime assertions cover selected-child render round trip and legacy defaults; one activation render/event/close/focus outcome carrying index `0` and the canonical child path; durability through Enter, Escape, native close, reopen, and Down navigation; invalid activation with zero output; and preservation of committed parent identity while pending identity moves. Compiled-resource assertions cover collapsed canonical child-path display with parent probability and no parent affordance, stable child option id, exclusive child-versus-pending-row active/ARIA ownership, list `aria-activedescendant`, normalization of invalid render child state, and unchanged Left/Right handlers. Explicit activation emitted no legacy `selectionChange`.
