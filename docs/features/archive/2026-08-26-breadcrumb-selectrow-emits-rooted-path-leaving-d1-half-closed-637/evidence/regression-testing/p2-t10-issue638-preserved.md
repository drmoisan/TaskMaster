Timestamp: 2026-08-31T10:35:54-04:00
Command: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; $asm = Join-Path (Get-Location).Path "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll"; & $vstest $asm /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcDataModelArchiveRootTests&TestCategory!=LiveOutlook" /Logger:trx "/ResultsDirectory:coverage\testresults\p2-t10"; "EXIT_CODE=$LASTEXITCODE"'
EXIT_CODE: 0
Output Summary: The filter matched 11 tests; 11 passed and 0 failed. The output did not contain `No test matches the given testcase filter`.

MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce: PASS. Its `Times.Once()` assertion proves P2-T4 passed the existing `olAncestor` local rather than reading `Globals.Ol.ArchiveRootPath` a second time. The failing set is empty, which is a subset of the prior baseline failure set.
